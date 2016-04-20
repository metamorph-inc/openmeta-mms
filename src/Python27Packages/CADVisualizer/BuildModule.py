__author__ = 'robertboyles'

import os
import json
import argparse
from utility_functions import exitwitherror, setup_logger
import meta_freecad as meta

"""
    Requires:
        - A simplified version of a module, with no PCB components added.
        - Name of template module file.
        - JSON file with keys that are the labels of the elements in the template module.
            - Values are the paths to the CAD files that will replace the template module element.
        - FreeCAD label of component in stock module (JSON file keys).
        - FreeCAD label of component that you are replacing in source file.
            - For PCB, source file will be an assembly, but "PCB" label is fixed per BuildPCB.py
            - For all others, source file should only be a part, so can get away with
              grabbing the active object in source doc after importing.

    The element can be replaced based on the fact that its bounding box tells you where it's final
    position is. The part that we are wanting to swap out has the same bounding box dimensions
    as that of the part in the module. By taking the difference between the minimum values of the
    two bounding boxes, you know how much the object needs to be translated in each direction. In
    the case of the PCB, there are more components than just the PCB object. However, this approach
    still works so long as the bounding box of the PCB is the largest of all of the other elements.

    Example JSON:
    {
     "template_cadpath": "ara_1x2_template.step",
     "PCB": "Template_Module_1x2.FCStd",
     "SHELL": "stock"
    }

    This script will replace the "PCB" element of the template module with the assembly in
    Template_Module_1x2.FCStd. The word "stock" tells the program to leave the SHELL element
    in the template alone.

    The program will export the template module with replaced components to a FC file and either
    a STEP or STL file, depending on if there are any STL components in the design.
"""


def get_args():
    parser = argparse.ArgumentParser()
    parser.add_argument('json', type=str, help='Map of parts within Ara module.')
    return parser.parse_args()


def parse_partfile(partfilepath):
    partdata = None
    try:
        with open(os.path.join(os.getcwd(), partfilepath), "r") as partfile:
            partdata = json.load(partfile)
    except IOError:
        exitwitherror("Unable to open Ara parts list file, {0}".format(partfilepath))
    return partdata


def check_file_exists(filein):
    if not os.path.exists(filein):
        exitwitherror("Unable to find file {0}".format(filein))


def replace(aradata):
    module_file = aradata["template_cadpath"]
    check_file_exists(module_file)
    logger.info("Ara template module: {0}".format(module_file))

    # Create new Document and Assembly object to store template module information.
    module = meta.Assembly(module_file.rsplit('.')[0])
    module.doc = meta.Document("AraModule")
    module.doc.new()
    (module.cadpath, module.format) = module.doc.load(module_file)

    # Loop through assembly and create a Part object for each element. Add these parts to the list of objects in doc.
    module.get_parts()
    module.doc.objects = module.parts

    # We are only interested in replacing parts that are not stock components.
    for rep_part in [p for p in aradata.items() if p[1] != "stock" and p[0] != "template_cadpath"]:
        assembly_file = rep_part[1]  # Path to component
        module_label = rep_part[0]   # FreeCAD internal label of component in Ara template file.
        check_file_exists(assembly_file)
        logger.info("Replacing part {0}...".format(assembly_file))
        logger.info("FreeCAD part label: {0}".format(module_label))

        # Create new Document and Part/Assembly object for the replacing part we are about to load.
        if module_label == "PCB":
            replace_obj = meta.Assembly(module_label)
        else:
            replace_obj = meta.Part(module_label)
        replace_obj.doc = meta.Document("ReplacingObject")
        replace_obj.doc.new()
        try:
            (replace_obj.cadpath, replace_obj.format) = replace_obj.doc.load(assembly_file)
        except IOError:
            # Already checked for existence, so it means file is already open. Grab by label.
            logger.debug("FreeCAD file {0} already open! Grabbing existing document.".format(assembly_file))

        # Grab object in module that we wish to replace, along with its bounding box in its assembled position.
        module_prt = module.grab_object_by_name(module_label)
        module_prt_bb = module_prt.get_bounding_box()

        # The only assembly that should be replaced is that of the PCB.
        if replace_obj.of_type() == "assembly":
            # Grab all objects in the assembly that will need to be translated.
            replace_obj.get_parts()
            replace_obj.doc.objects = replace_obj.parts
            replace_prt = replace_obj.grab_object_by_name("PCB")  # This is the element whose bounding box we want.
        else:
            # Otherwise, the document only has the one part that will be copied.
            replace_obj.doc.objects.append(replace_obj)
            replace_obj.set_object(meta.get_active_object(replace_obj.doc))
            replace_prt = replace_obj

        replace_prt_bb = replace_prt.get_bounding_box()

        # Grab list of parts to copy into module doc, provided it is a part or mesh part (no sketches).
        objs = [x for x in replace_obj.doc.objects if (x.parent() == "Part::Feature" or
                                                       x.parent() == "Mesh::Feature")]

        module_objs_before = len(module.doc.objects)
        for obj in objs:
            new_obj = module.doc.copy_object(obj)  # Copy object from replace doc to module doc
            new_obj.set_colors(obj.get_colors())   # Color is lost in copying, so add color back to object.

        # Now copied objects are in module doc, but at the coordinates as they appear in replace doc.
        module_objs = module.doc.get_objects(module_objs_before, len(module.doc.objects))
        for obj in module_objs:
            logger.info("Translating object: {0}".format(obj.name))
            logger.debug("starting placement: {0}".format(obj.fc.Placement))
            logger.debug("deltaX: {0}".format(str(module_prt_bb.XMin - replace_prt_bb.XMin)))
            logger.debug("deltaY: {0}".format(str(module_prt_bb.YMin - replace_prt_bb.YMin)))
            logger.debug("deltaZ: {0}".format(str(module_prt_bb.ZMin - replace_prt_bb.ZMin)))
            logger.debug("ending placement: {0}".format(obj.fc.Placement))
            obj.translate([module_prt_bb.XMin - replace_prt_bb.XMin,
                          module_prt_bb.YMin - replace_prt_bb.YMin,
                          module_prt_bb.ZMin - replace_prt_bb.ZMin])

        # Update assembly list, as parts have changed.
        module_prt.delete()
        module.parts = []
        module.get_parts()
        module.doc.objects = module.parts

        # Close current "replace" part and delete it's corresponding objects.
        replace_obj.doc.close()
        del replace_obj
        del replace_prt

    # Export to FreeCAD and STEP/STL
    meta.set_active_doc(module.doc)
    meta.export("FC", [])
    if len([x for x in module.doc.objects if x.parent() == "Mesh::Feature"]) > 0:
        meta.convert_parts_to_meshes(meta.get_all_objects(), logger.name)
        meta.export("STL", meta.get_all_objects())
    else:
        meta.fuse_components(meta.get_all_objects(), module.doc.Name)  # For cad.js
        meta.export("STEP", [meta.get_active_object()])


def build_module(template_file):
    # This is how the program will be called if BuildPCB.py was just executed.
    global logger
    logger = setup_logger("BuildModule.log", "module_log")
    logger.info("Replacing stock Ara template components...")
    ara_data = parse_partfile(template_file)
    replace(ara_data)


if __name__ == '__main__':
    # This is how the program will be called from __main__.py if args.replace = True
    # and args.assembly = False or if debugging through FreeCAD console.
    args = get_args()
    jfile = args.json
    build_module(jfile)