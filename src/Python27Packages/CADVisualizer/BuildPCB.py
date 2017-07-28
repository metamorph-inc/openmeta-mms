import json
import sys
import os
import time
import logging
import argparse

try:
    META_CAD = os.path.dirname(os.path.abspath(__file__))
except NameError:
    META_CAD = os.path.dirname(os.path.abspath('__file__'))  # Debugging in FreeCAD console
sys.path.append(META_CAD)
from utility_functions import setup_logger, exitwitherror
import meta_freecad as meta
import build_board as brd
import eda_layout
import geometry as geom
import BuildModule

'''
NOTE: This file should not require any use of the FreeCAD libraries directly.
'''


def read_input_files(component_dict, layoutfile):
    # Parse layout file and populate Layout class
    layout = eda_layout.Layout()
    layout.parse_layout(layoutfile)
    logger.info('EAGLE layout JSON found.')

    try:
        with open(component_dict, 'r') as ct_json:
            CT = json.load(ct_json)
    except IOError:
        exitwitherror('Component Dictionary JSON file not found! Message: ' + str(OSError.strerror))
    logger.info('Component mapping dictionary CT.JSON found.')
    return layout, CT


def create_pcb(layout, boardthick):
    """
        Generate FreeCAD PCB object. If no border segment information is present in
        layout file, generate a plain cube using base width/height/thickness dimensions.
    """
    pcb = brd.BRD(boardthick, layout.height, layout.width, layout.layers)
    if layout.Border is None:
        pcb.create_basic_pcb()
    else:
        pcb.create_detailed_pcb(layout)


def get_cad_srt(cad, component, placeholder):
    """
        Parse component dictionary of component EDA2CADTransform objects.
        Get CAD Scale, Rotation, Translation about part's local coordinate system.
    """
    if cad is not None:
        scale = [component.get("scale")['X'], 
                 component.get("scale")['Y'], 
                 component.get("scale")['Z']]
        rotation = [component.get("rotation")['X'], 
                    component.get("rotation")['Y'], 
                    component.get("rotation")['Z']]
        translation = [component.get("translation")['X'], 
                       component.get("translation")['Y'], 
                       component.get("translation")['Z']]
    else:
        placeholder = True
        scale = [0.0, 0.0, 0.0]
        rotation = [0.0, 0.0, 0.0]
        translation = [0.0, 0.0, 0.0]
    return scale, rotation, translation, placeholder
    

def generate_assembly(doc, component_dict):
    
    [layout, CT] = read_input_files(component_dict, os.path.join(os.getcwd(), 'layout.json'))

    # The component assembly of a subcircuit will have an entry in layout.json. We want to disregard these entries,
    #    as the components that make up the subcircuit will be added individually later on.
    no_relative_id_pkgs = [p for p in layout.packages if p.relComponentId is None]
    subcircuit_pkgs = []
    for p in layout.packages:
        if p.relComponentId is not None:
            for s in no_relative_id_pkgs:
                if p.relComponentId == s.guid:
                    subcircuit_pkgs.append(s.guid)
    subcircuit_pkgs = list(set(subcircuit_pkgs))

    meta.new_document(doc)

    # Create PCB board
    boardthick = 0.6
    chipThick = 0.3
    create_pcb(layout, boardthick)

    # Initialize chip counter
    topchips = 0
    bottomchips = 0
    placeholders = 0
    
    imported_cad = {}
    for p in sorted(layout.packages, reverse=True, key=lambda p: p.width * p.height):
        if p.guid in subcircuit_pkgs:
            continue

        placeholder = False  # Start by assuming component exists, try to prove otherwise.
        component = p.guid

        logger.debug('\n\n')
        logger.debug('-- COMPONENT --')
        logger.debug('Component Key: ' + component)

        if component not in CT:
            logger.warning('Component {0} not found in CT.json, creating placeholder...'.format(component))
            placeholder = True

        ######################################
        ''' PLACEMENT CALCULATION '''

        # Check for relative placement
        xc = None
        yc = None
        relative_package = None    
        if p.relComponentId is not None:
            logger.info("Relative constraints found!")
            
            relative_package = [pkg for pkg in layout.packages if pkg.guid == p.relComponentId][0]
            logger.info('Package relative to: ' + str(relative_package.guid))

            # Need to move the lower left-hand point for rotated reference package
            [xc, yc] = geom.rotate_and_translate(relative_package, p.x, p.y)

            # Cumulative rotation between package and its reference
            rotate = p.rotation + relative_package.rotation
            do_rotate = 1 if rotate == 1 or rotate == 3 or rotate == 5 else 0

            # xc/yc are coordinates of moved LLH point - need to move this so it is back to LLH
            #   --> EG, LLH rotated 180 degrees results in point at URH
            if relative_package.rotation == 1:
                xc -= p.width if not do_rotate else p.height
            elif relative_package.rotation == 2:
                xc -= p.width if not do_rotate else p.height
                yc -= p.height if not do_rotate else p.width
            elif relative_package.rotation == 3:
                yc -= p.height if not do_rotate else p.width
        else:
            logger.info("No constraints present. Using regular X/Y values.")
            xc = p.x
            yc = p.y 
            rotate = p.rotation
            do_rotate = 1 if rotate == 1 or rotate == 3 else 0  

        item = None
        cad = None
        if not placeholder:
            item = CT[component]
            cad = CT[component].get("cadpath")

        [scale, rotation, translation, placeholder] = get_cad_srt(cad, item, placeholder)
        trans = meta.transform_cad_to_eda(rotation, [t*s for t,s in zip(translation, scale)])
        
        # Define placement variables based on top/bottom layer that the component instance is on
        if p.layer == 0:                                # Component is on top layer of PCB
            Z = boardthick                              # Component height on board (Top/Bottom of PCB)
            placeholder_offset = 0.0
            alpha = 90.0                                # 90.0 degree rotation
            board_orient = [0, 0, rotate*alpha]         # Component orientation relative to board
            topchips += 1
        elif p.layer == 1:                              # Component is on bottom layer of PCB
            Z = 0.0
            placeholder_offset = chipThick
            alpha = -90.0
            trans[2] *= -1                              # Flip to handle 'pushing down' the component.
            # Two-way 180 degree flips accounts for switch to bottom
            board_orient = [0,180,180+rotate*alpha]  
            bottomchips += 1
        else:
            exitwitherror('Invalid layer specified for package {0}'.format(component))

        rot = [sum(x) for x in zip(rotation, board_orient)]

        # Component center point. If component is rotated width and height are reversed.
        width = ((1-do_rotate)*p.width + do_rotate*p.height)
        height = ((1-do_rotate)*p.height + do_rotate*p.width)
        
        [originx, originy] = geom.rotate_vector(p.originx, p.originy, rotate)
        X = xc + 0.5*width - originx
        Y = yc + 0.5*height - originy
        
        ######################################
        ''' FREECAD OBJECT CREATION '''

        if not placeholder:
            if cad not in imported_cad:
                solid = meta.Part(component)  # Instantiate new part

                """ Query number of parts before and after import to determine if fuse needs to occur.
                    Some single component may import each SHAPE_REPRESENTATION as its own component.
                    If this happens, solidify these components to get back to one object.
                """
                num_objects_b = len(meta.get_all_objects())
                (solid.cadpath, solid.format, newdoc) = meta.import_cad(cad, doc)
                logger.info(doc)
                logger.info(newdoc)
                logger.info(solid.cadpath)
                logger.info(solid.name)
                num_objects_a = len(meta.get_all_objects())

                if (num_objects_a - num_objects_b) > 1:
                    # Only ever true for STEP files - STL files have no hierarchy concept.
                    objects = [meta.get_all_objects()[x] for x in range(num_objects_b, num_objects_a)]
                    solid.fc = meta.fuse_components(objects, solid.name)
                else:
                    solid.fc = meta.get_active_object()
                
                 # Store object data to avoid having to import a part multiple times (costly for STEP)
                imported_cad[cad] = {'component': solid.name, 
                                     'ext': solid.format,
                                     'cadpath': solid.cadpath,
                                     'geometry': solid.get_shape(solid.format),
                                     'color': solid.get_colors() }
            else:
                # Component has already been imported, grab data from dictionary and create new FC object.
                compdef = imported_cad[cad]
                solid = meta.Part(compdef['component'], compdef['ext'], compdef['cadpath'])
                if compdef['ext'] == 'stl':
                    solid.create_blank('mesh')
                else:
                    solid.create_blank('part')
                solid.set_shape(compdef['geometry'], solid.format)
                solid.set_colors(compdef['color'])

            logger.info("Generating " + component + " object...")
        else:
            placeholders += 1
            logger.warning("Making placeholder (CAD file not found for component {0}!): ".format(component))
            solid = meta.Part(component)
            solid.create_blank()
            if do_rotate:
                solid.set_shape(meta.make_box(p.width, p.height, chipThick, -0.5*height, -0.5*width))
            else:
                solid.set_shape(meta.make_box(p.width, p.height, chipThick, -0.5*width, -0.5*height))
            solid.set_color(brd.PCBColors.PLACEHOLDER) # Red placeholder color

        # Scale/Translate/Rotate component about X,Y,Z axes
        if not placeholder:
            # If scale vector is all ones, no need to scale to identity. This operation is costly.
            if not all([1 if float(s) == 1.0 else 0 for s in scale]):
                solid.scale(scale, False)

            solid.transform([sum(x) for x in zip([X,Y,Z], trans)], rot)  # Translate & Rotate
        else:
            solid.transform([X, Y, Z-placeholder_offset], [0.0, 0.0, rotate*alpha])  # Move component to center origin

        logger.debug('Package LLH <X,Y>: <{0},{1}>'.format(str(xc), str(yc)))
        logger.debug('Component <Width,Height>: <{0},{1}>'.format(str(p.width), str(p.height)))
        logger.debug('Global <X,Y,Z>: <{0},{1},{2}>'.format(str(X), str(Y), str(Z)))
        logger.debug('Translate: ' + str(translation))
        logger.debug('EDARotation: ' + str(rotation))
        logger.debug('Rotation: ' + str(rotate))
        logger.debug('CAD2EDATranslate: ' + str(trans))
        logger.info('Adding FreeCAD object {0} for component {1}.'.format(solid.name, component))
    
    return topchips, bottomchips, placeholders
    

if __name__ == '__main__':
    global logger
    logger = setup_logger("CADAssembler.log", "main_log")
    try:
        if len(sys.argv) == 1:  # For debugging/testing in FreeCAD console
            doc = 'Test'
            ext = 'step'
        elif len(sys.argv) < 6:
            exitwitherror('Missing command-line arguments.')
        elif len(sys.argv) > 6:
            for i in range(0,len(sys.argv)):
                logger.debug(sys.argv[i])
            exitwitherror('Too many command-line arguments.')
        else:
            doc = sys.argv[2].replace('(','_').replace(')','_')
            ext = sys.argv[3]
            replace = sys.argv[4]
            interference_check = sys.argv[5].lower()
            if ext == 'stp':
                ext = 'step'

        t0 = time.clock()

        ## In time these should be replaced by passed-in parameters.
        export_parts = False
        partsdir = 'CAD_Assembled'
        componentdict = 'CT.json'
        ##

        topchips, bottomchips, placeholders = generate_assembly(doc, componentdict)

        mesh_parts = meta.count_object_types(meta.get_all_objects(), "mesh")
        if mesh_parts == 0:
            # Assembler could have been set to 'mix', but with no Mesh parts, no need to 
            #   output a generated assembly of lesser detail. Override extension to STEP.
            ext = 'step'
            if interference_check == "true":
                meta.interference_check(meta.get_all_objects(), doc)
        try:
            meta.export('FC', [])  # Export entire FC file
            if export_parts:
                t_export = time.clock()
                logger.info('Exporting all assembly components to ' + \
                            '{0}. This may take a moment.'.format(partsdir))
                meta.export(ext, meta.get_all_objects(), partsdir)
                logger.info('Done exporting assembly components. Export time = ' +
                            str(time.clock() - t_export))
            
            # Fuse all components into one and export to STEP - Used for cad.js viewer
            if ext == 'step':
                meta.fuse_components(meta.get_all_objects(), doc)
                meta.export(ext, [meta.get_active_object()])
            else:
                meta.convert_parts_to_meshes(meta.get_all_objects(), logger.name)
                if interference_check == "true":
                    meta.interference_check(meta.get_all_objects(), doc)
                meta.export(ext, meta.get_all_objects())

        except:
            exitwitherror("Unable to save FreeCAD/CAD files!")

        logger.info("Visualizer success! File saved to " + doc + ".FCStd.")
        logger.info('Chips on Top: {0}   Chips on Bottom: {1}'.format(topchips, bottomchips) +
                    ' Placeholder Chips: {0}'.format(placeholders))
        logger.info('Execution time: ' + str(time.clock() - t0))
    except Exception as e: 
        import traceback
        logger.error(traceback.format_exc())
        exitwitherror(str(e))
   
    if replace == "False":
        exit(0)
    else:
        BuildModule.build_module("AraTemplateParts.json")
        exit(0)
