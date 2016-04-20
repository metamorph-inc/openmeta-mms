import sys
import os
import copy
import time
from math import cos, sin, pi, atan2, radians, sqrt
import ImportGui
import Part as Partfc
import Mesh
import Draft
import Sketcher
import FreeCAD
import logging
from utility_functions import setup_logger, exitwitherror
import geometry as geom

global logger
logger = logging.getLogger()


def find_freecad_install():
    # Check platform script is running on to account for FreeCAD imports (Mac is for dev testing)
    platform = sys.platform
    if platform == 'Darwin':
        FREECAD = r'/Applications/FreeCAD.app/Contents/bin/FreeCAD'
        sys.path.append('/Applications/FreeCAD.app/Contents/Mod')
    elif platform == 'win32':  # This is win32 for 32 and 64 bit Windows
        import _winreg
        with _winreg.OpenKey(_winreg.HKEY_LOCAL_MACHINE,
                             r'Software\META', 0, _winreg.KEY_READ | _winreg.KEY_WOW64_32KEY) as key:
            META_PATH = _winreg.QueryValueEx(key, 'META_PATH')[0]
        try:
            with _winreg.OpenKey(_winreg.HKEY_LOCAL_MACHINE,
                                 r'Software\Microsoft\Windows\CurrentVersion\Uninstall\FreeCAD 0.14',
                                 0, _winreg.KEY_READ | _winreg.KEY_WOW64_32KEY) as key:
                FREECAD_PATH = _winreg.QueryValueEx(key, 'InstallLocation')[0]
        except WindowsError:
            try:
                with _winreg.OpenKey(_winreg.HKEY_LOCAL_MACHINE,
                                     r'Software\Microsoft\Windows\CurrentVersion\Uninstall\FreeCAD 0.14', 
                                     0, _winreg.KEY_READ | _winreg.KEY_WOW64_64KEY) as key:
                    FREECAD_PATH = _winreg.QueryValueEx(key, 'InstallLocation')[0]
            except WindowsError:
                exitwitherror("Unable to find FreeCAD registry key! \n Message: " + WindowsError.strerror)
        FREECAD_PATH = FREECAD_PATH.strip('"').rstrip('"')
        FREECAD = os.path.join(FREECAD_PATH, 'bin', 'FreeCAD.exe')
    else:
        exitwitherror('Unsupported system platform detected: ' + platform)
    
    if not os.path.exists(FREECAD):
        exitwitherror('Unable to find FreeCAD.exe')

    return FREECAD


class Document(object):
    def __init__(self, name):
        self.Name = name
        self.document = None
        self.objects = []  # List of meta class objects in the document.

    def new(self):
        self.document = FreeCAD.newDocument(self.Name)

    def save(self):
        self.document.save()

    def close(self):
        FreeCAD.closeDocument(self.Name)

    def load(self, path):
        """ Loads FC, STEP, or STL file into document object. Will return cadpath and format
            properties to store into Part/Assembly class if desired. """
        cadpath = os.path.join('..', '..', path)  # For test bench runs (..\..\ takes you to project root)
        if not os.path.exists(cadpath):
            cadpath = path
            if not os.path.exists(cadpath):
                exitwitherror('Can\'t find cad file!')
        fileformat = cadpath.rsplit('.')[-1].lower()

        try:
            if fileformat in ["step", "stp"]:
                ImportGui.insert(cadpath, self.Name)
            elif fileformat == 'stl':
                Mesh.insert(cadpath, self.Name)
            elif fileformat == "fcstd":
                # Loading a FC file will always open a new doc, so close the current one and set to the new one.
                self.close()
                self.document = FreeCAD.openDocument(cadpath)
                self.Name = self.document.Name
            else:
                exitwitherror('Unsupported file format {0} for file {1}.'.format(str(fileformat), str(path)))

        except (Exception, IOError) as err:
            logger.error(err)
            exitwitherror('Error attempting to import {0}'.format(cadpath))

        logger.info('Imported CAD file {0}'.format(cadpath))

        return cadpath, fileformat

    def get_objects(self, idx_start, idx_end):
        """
            Return objects in document between idx_start and idx_end
        """
        return self.objects[idx_start:idx_end]

    def copy_object(self, obj_to_copy):
        """ Copies object into this document. A new Part is created to store the object."""
        # Copy Part data from object in other document.
        self.add_part(obj_to_copy.name,
                      ext=obj_to_copy.format,
                      cadpath=obj_to_copy.cadpath,
                      fc=self.document.copyObject(obj_to_copy.fc))
        return self.objects[-1]

    def add_part(self, name, ext=None, cadpath=None, fc=None):
        """ Add a Part object to the list of meta class objects in the document."""
        self.objects.append(Part(name, ext, cadpath, fc))

    def set_active(self):
        """ Set this document to be the active window. """
        FreeCAD.setActiveDocument(self.Name)


# TODO: Remove some of these in a future ticket now that there is a Document class
def new_document(doc):
    return FreeCAD.newDocument(doc)


def open_document(path):
    return FreeCAD.openDocument(path)


def set_active_doc(doc):
    return FreeCAD.setActiveDocument(doc.Name)


def get_active_document():
    return FreeCAD.ActiveDocument


def get_document(doc):
    return FreeCAD.getDocument(doc)


def get_active_object(doc=None):
    if doc is None:
        return FreeCAD.ActiveDocument.ActiveObject
    else:
        return FreeCAD.getDocument(doc.Name).ActiveObject


def get_all_objects(doc=None):
    if doc is None:
        return FreeCAD.ActiveDocument.Objects
    else:
        return FreeCAD.getDocument(doc.Name).Objects


def get_object_label(obj):
    return obj.Label


def make_box(w, h, t, x=None, y=None):
    if x and y:
        return Partfc.makeBox(w, h, t, FreeCAD.Vector(x, y, 0.0))
    else:
        return Partfc.makeBox(w, h, t)


def copy_shape(obj_from, obj_to, del_orig=False):
    """
        Copies 'Shape' definition from one object to another.
        e.g, Copying a Pad's shape to a Part's shape. Most functions
        deal with a Part, so rather than create similar functions for
        objects that are very similar, just create a Part and copy contents.

        del_orig = True will delete the object from which you are copying.
    """
    obj_to.fc.Shape = obj_from.fc.Shape
    if del_orig:
        FreeCAD.ActiveDocument.removeObject(obj_from.fc.Label)


def count_object_types(objects, objtype):
    if objtype == "part":
        typeid = "Part::Feature"
    elif objtype == "mesh":
        typeid = "Mesh::Feature"
    else:
        exitwitherror("Invalid object type specified for querying in count_object_types()" +
                      " Type specified: {0}".format(objtype))
    return len([obj for obj in objects if obj.TypeId == typeid])


def convert_parts_to_meshes(objs, logname):
    """
    Converts any Part::Feature components to Mesh::Feature objects.
    WARNING: Deletes original Part::Feature component.
    """
    log = logging.getLogger(logname)
    for obj in objs:
        if obj.isDerivedFrom("Part::Feature"):
            meshpart = Part(obj.Label)
            meshpart.create_blank('mesh')
            meshpart.part_to_mesh(obj)
            FreeCAD.ActiveDocument.removeObject(obj.Name)


def fuse_components(objects, compoundname):
    """ Fuses all specified components into one object. Shape and color 
        information is stored for each object in tree, then reapplied 
        to a newly created compound FreeCAD object.


        This is separate from the Part class as it does not fuse a set
        of components relative to another component. The entire set of
        objects are fused together to create a new FreeCAD compound.

        This is a function only for STEP files. STL files have not
        hierarchy concept. Any set of objects exported to STL are
        only one level deep.
    """
    shapes = []
    colors = []
    for obj in objects:
        if obj.isDerivedFrom("Part::Feature"):
            shapes.append(obj.Shape)
            color = obj.ViewObject.DiffuseColor
            numfaces = len(obj.Shape.Faces)
            if len(color) == numfaces:
                colors.extend(color)
            else:
                for i in range(numfaces):
                    colors.append(color[0])
            FreeCAD.ActiveDocument.removeObject(obj.Name)

    compound = Partfc.makeCompound(shapes)
    comp = FreeCAD.ActiveDocument.addObject("Part::Feature", compoundname)
    comp.Shape = compound
    comp.ViewObject.DiffuseColor = colors

    return comp


def transform_cad_to_eda(cad_rotation, cad_translation):
    """
    Some parts that are imported into FreeCAD do not conform to the Z-up/Y-forward
    coordinate system definition. Their origins may also differ from that of the EDA
    model's origin. This information comes from the CyPhy model. If the two model's
    match the rotation matrix will be identity and the output translation == input.

    - Calculate the rotation matrix resulting of the yaw-pitch-roll angles.
    - Multiply with the CAD2EDATransform data so that origin corresponds to how it
      is depicted in the EDA model.

    cad_rotation/translation -> arrays of [X, Y, Z] components.
    output: eda_trans -> array of transformed translation.
    """

    # y-p-r angles in radians
    yaw = cad_rotation[2] * (pi/180)
    pitch = cad_rotation[1] * (pi/180)
    roll = cad_rotation[0] * (pi/180)

    # Euler angle rotation matrices
    xmat = FreeCAD.Matrix(1.0, 0.0, 0.0, 0.0, 
                          0.0, cos(roll), -sin(roll), 0.0, 
                          0.0, sin(roll), cos(roll), 0.0)
    ymat = FreeCAD.Matrix(cos(pitch), 0.0, sin(pitch), 0.0, 
                          0.0, 1.0, 0.0, 0.0,
                          -sin(pitch), 0.0, cos(pitch), 0.0)
    zmat = FreeCAD.Matrix(cos(yaw), -sin(yaw), 0.0, 0.0, 
                          sin(yaw), cos(yaw), 0.0, 0.0, 
                          0.0, 0.0, 1.0, 0.0)
    rmat = zmat*ymat*xmat

    cad_trans = FreeCAD.Vector(cad_translation[0], cad_translation[1], cad_translation[2])
    eda_trans = rmat.multiply(cad_trans)   # FreeCAD.Vector -> Convert back to array

    return [eda_trans[0], eda_trans[1], eda_trans[2]]


def import_cad(path, doc):
    """ Imports cad file at path into document doc. May produce more than one FreeCAD object! """
    cadpath = os.path.join('..', '..', path)  # For test bench runs (..\..\ takes you to project root)
    if not os.path.exists(cadpath):
        cadpath = path
        if not os.path.exists(cadpath):
            exitwitherror('Can\'t find cad file!')
    fileformat = cadpath.rsplit('.')[-1].lower()

    newdoc = None
    try:
        if fileformat in ["step", "stp"]:
            ImportGui.insert(cadpath, doc)
        elif fileformat == 'stl':
            Mesh.insert(cadpath, doc)
        elif fileformat == "fcstd":
            # This will create a new doc and ignore doc operator
            newdoc = open_document(cadpath)
        else:
            exitwitherror('Unsupported file format {0} for file to import {1}.'.format(str(fileformat), str(path)))

    except (Exception, IOError) as err:
        logger.error(err)
        exitwitherror('Error attempting to import {0}'.format(cadpath))

    logger.info('Imported CAD file {0}'.format(cadpath))
    return cadpath, fileformat, newdoc


def export(format, parts, exportdir=None, name=None):
    """ 
    Export components back out to given format in their assembled positions. 
    STEP files can be exported to STL, but not the other way around.

    This is not in the Part class since it allows for the export of any number of objects.

    If exportDir is None, the assembly will be exported to one STL.
    If exportDir is specified, the individual components will be exported to STL.
    """
    exportpath = os.getcwd()
    export_individual = False
    if exportdir is not None:
        export_individual = True
        exportpath = os.path.join(os.getcwd(), exportdir)
        if not os.path.isdir(exportpath):
            os.makedirs(exportpath)
    else:
        exportdir = exportpath

    if format.lower() in ['step', 'stp']:
        for obj in parts:
            if obj.isDerivedFrom("Mesh::Feature"):
                logger.warning('Component {0} is derived from an STL file. '.format(obj.Label) +
                               'This can not be exported to the STEP format. This component is being skipped.')
                continue
            ImportGui.export([obj], os.path.join(exportpath, obj.Label + '.step'))
            logger.info('Exported {0}.step to {1}.'.format(obj.Label, exportdir))
    elif format.lower() in ['stl', 'mix']:
        if export_individual:
            for prt in parts:
                Mesh.export([prt], os.path.join(exportpath, prt.Label + '.stl'))
                logger.info('Exported {0}.stl to {1}.'.format(prt.Label, exportdir))
        else:
            Mesh.export(parts, os.path.join(exportpath, FreeCAD.ActiveDocument.Name + '.stl'))
            logger.info('Exported {0}.stl to {1}.'.format(FreeCAD.ActiveDocument.Name, exportdir))
    elif format.lower() == 'fc':
        if name is not None:
            FreeCAD.ActiveDocument.saveAs(os.path.join(exportpath, name + '.FCStd'))
        else:
            FreeCAD.ActiveDocument.saveAs(os.path.join(exportpath, FreeCAD.ActiveDocument.Name + '.FCStd'))
    else:
        exitwitherror('Requested to export parts to unsupported file format {0}.'.format(str(format.lower())))

def interference_check(objects, modelname):
    """
        Checks for any interferences between list of objects.
        Formulates report whether interferences or not.
        Coincidence is not interference.
    """

    interfere_log = setup_logger("interference_report.log", 'interfere_log')
    start_checking = time.clock()
    interfere_log.info("INTERFERENCE CHECK REPORT FOR {0}".format(modelname))
    interfere_log.info("")

    interferences = 0
    checked_components = []
    for obj in objects:
        # Check interferences between this part and any other in the list
        for checkobj in [o for o in objects if o != obj]:

            # Don't want to check interference of identical components multiple times...
            if sorted([obj, checkobj]) in checked_components:
                continue

            objmesh = obj.isDerivedFrom("Mesh::Feature")
            checkobjmesh = checkobj.isDerivedFrom("Mesh::Feature")
            if objmesh and checkobjmesh:
                common = obj.Mesh.intersect(checkobj.Mesh)
            elif objmesh != checkobjmesh:
                logger.warning("Parts {0} and {1} are of different type. Cannot check, skipping.")
                continue;
            else:
                common = obj.Shape.common(checkobj.Shape)

            if common.Volume != 0.0:
                interferences += 1
                interfere_log.info("Interference detected!")
                interfere_log.info("Component 1: {0}".format(obj.Label))
                interfere_log.info("Component 2: {0}".format(checkobj.Label))
                interfere_log.info("Overlapping volume: {0}".format(common.Volume))
                interfere_log.info("")

            checked_components.append([obj, checkobj])

    interfere_log.info("Interference check complete.")
    interfere_log.info("Execution time: {0}".format(time.clock() - start_checking))
    if interferences == 0:
        interfere_log.info("No interferences detected!")
    else:
        interfere_log.info("Total number of interferences: {0}".format(interferences))


def get_assembly_bounding_box(objects):
    """ 
        Calculates overall bounding box given a set of components.
        [Xmin,Ymin,Zmin,Xmax,Ymax,Zmax]
    """
    bb = FreeCAD.BoundBox()
    for object in objects:
        if object.isDerivedFrom("Mesh::Feature"):
            bb.add(object.Mesh.BoundBox)
        else:
            bb.add(object.Shape.BoundBox)
    return bb


class Assembly(object):
    def __init__(self, name, fileformat=None, cadpath=None, docobject=None):
        self.name = name
        self.doc = docobject
        self.format = fileformat
        self.cadpath = cadpath
        self.parts = []

    def get_parts(self):
        """ Assumes file has already been imported. """
        for obj in get_all_objects(self.doc.document):
            self.parts.append(Part(obj.Label, ext=None, cadpath=None, fc=obj, doc=self.doc))

    def grab_object_by_name(self, name):
        parts = [p for p in self.parts if p.name == name]
        if len(parts) == 0:
            logger.error("Parts in assembly {0}: {1}".format(self.name, str([p.name for p in self.parts])))
            exitwitherror("Unable to find part {0} in assembly {1}".format(name, self.name))
        elif len(parts) > 1:
            logger.warning("Multiple parts named {0} in assembly {1}.".format(name, self.name) +
                           "Choosing one at random.")
        return parts[0]

    @staticmethod
    def of_type():
        return "assembly"


class Part(object):
    def __init__(self, component, ext=None, cadpath=None, fc=None, doc=None):
        self.name = component
        self.format = ext
        self.cadpath = cadpath
        self.fc = fc
        self.doc = doc

    @staticmethod
    def of_type():
        return "part"

    def create_blank(self, format='part'):
        """ Create empty FreeCAD object. """
        if format.lower() == 'part':
            self.fc = FreeCAD.ActiveDocument.addObject("Part::Feature", self.name)
        elif format.lower() == 'mesh':
            self.fc = FreeCAD.ActiveDocument.addObject("Mesh::Feature", self.name)
        elif format.lower() == 'pad':
            self.fc = FreeCAD.ActiveDocument.addObject("PartDesign::Pad", self.name)

    def set_object(self, setter):
        self.fc = setter

    def get_shape(self, ext):
        if ext == 'stl':
            return self.fc.Mesh
        else:
            return self.fc.Shape

    def set_shape(self, shape, ext=None):
        if ext == 'stl':
            self.fc.Mesh = shape
        else:
            self.fc.Shape = shape

    def get_colors(self):
        try:
            return self.fc.ViewObject.DiffuseColor
        except AttributeError:
            return [self.fc.ViewObject.ShapeColor]

    def set_colors(self, colors):
        """ Sets color for each individual face of object. """
        try:
            self.fc.ViewObject.DiffuseColor = colors
        except AttributeError:
            if len(colors) == 1:
                self.set_color(colors[0])
            else:
                exitwitherror('Error setting colors for {0}. Colors: {1}'.format(self.name, str(colors)))

    def set_color(self, color):
        """ Assigns all faces one RGB color. """
        self.fc.ViewObject.ShapeColor = color

    def translate(self, trans):
        """ Translates an object by the given <X,Y,Z> vector. Does not place component at <X,Y,Z> position! """
        self.fc.Placement.Base += FreeCAD.Vector(trans[0], trans[1], trans[2])

    def transform(self, trans, rot):
        """ Translate/Rotate current FreeCAD object about X, Y, Z axes (roll-pitch-yaw angles). """
        self.fc.Placement = FreeCAD.Placement(FreeCAD.Vector(trans[0], trans[1], trans[2]), 
                                              FreeCAD.Rotation(rot[2], rot[1], rot[0]))

    def scale(self, scale_vec, copy_orig):
        """
            Scales a component in X, Y, Z directions.
            copy_orig: True -> Creates a new component at the scaled level
                       False -> Modifies given component.
        """
        if len([s for s in scale_vec if float(s) == 0.0]) > 0:
            logger.warning("Component {0} scale data is defined as zero... Canceling scale." +
                           " If component needs to be scaled check CyPhy model.")
            return
            
        self.fc = Draft.scale(self.fc, 
                            delta=FreeCAD.Vector(scale_vec[0], scale_vec[1], scale_vec[2]), 
                            center=self.fc.Placement.Base, 
                            copy=copy_orig, 
                            legacy=True)

        FreeCAD.ActiveDocument.recompute()

    def copy(self):
        """ Create a copy of the current FreeCAD object. """
        typeid = self.fc.TypeId
        newpart = FreeCAD.ActiveDocument.addObject(typeid, self.fc.Label)

        newobj = Part(self.name)
        newobj.cadpath = self.cadpath
        newobj.format = self.format

        if typeid == 'Part::Feature':
            newpart.Shape = self.fc.Shape
            newpart.ViewObject.DiffuseColor = self.fc.ViewObject.DiffuseColor
        elif typeid == 'Mesh::Feature':
            newpart.Mesh = self.fc.Mesh
        
        newobj.fc = newpart
        
        return newobj

    def part_to_mesh(self, obj):
        """
            Tesselates the FreeCAD object to generate a representing mesh.
        """
        faces = []
        shape = obj.Shape
        triangles = shape.tessellate(1)
        for tri in triangles[1]:
            face = []
            for i in range(3):
                vindex = tri[i]
                face.append(triangles[0][vindex])
            faces.append(face)
        self.fc.Mesh = Mesh.Mesh(faces)

    def get_bounding_box(self):
        """ Gets bounding box of part. [Xmin, Ymin, Zmin, Xmax, Ymax, Zmax] """
        if self.format == "stl":
            return self.fc.Mesh.BoundBox
        else:
            return self.fc.Shape.BoundBox

    def parent(self):
        return self.fc.TypeId

    def delete(self):
        self.doc.document.removeObject(self.fc.Name)


class Sketch(object):
    def __init__(self, name):
        self.name = name
        self.fc = None

    def create_blank(self):
        """ Create empty FreeCAD sketch. """
        self.fc = FreeCAD.ActiveDocument.addObject("Sketcher::SketchObject", self.name)

    def set_sketch_plane(self, plane, offset=0.0):
        rot = None
        loc = None
        if any([p for p in ['XY', 'YX'] if plane.upper() == p]):
            rot = FreeCAD.Rotation(0.0, 0.0, 0.0, 1.0)
            loc = FreeCAD.Vector(0.0, 0.0, offset)
        elif any([p for p in ['XZ', 'ZX'] if plane.upper() == p]):
            rot = FreeCAD.Rotation(-0.707107, 0.0, 0.0, -0.707107)
            loc = FreeCAD.Vector(0.0, offset, 0.0)
        elif any([p for p in ['YZ', 'ZY'] if plane.upper() == p]):
            rot = FreeCAD.Rotation(0.5, 0.5, 0.5, 0.5)
            loc = FreeCAD.Vector(offset, 0.0, 0.0)
        if rot is None:
            exitwitherror('Invalid sketch plane specified: {0}'.format(plane.upper()))
        FreeCAD.ActiveDocument.getObject(self.name).Placement = FreeCAD.Placement(loc, rot)

    def sketch_from_coords(self, data):
        for line in data.wires:
            v1 = FreeCAD.Vector(line.x1, line.y1, 0.0)
            v2 = FreeCAD.Vector(line.x2, line.y2, 0.0)
            self.fc.addGeometry(Partfc.Line(v1, v2))

        for arc in data.arcs:
            x1 = arc.x1
            x2 = arc.x2
            y1 = arc.y1
            y2 = arc.y2
            curve = arc.curve
            (xc, yc) = geom.find_circle_center_3pt_arc(x1, y1, x2, y2, radians(curve))
            radius = sqrt((x1-xc)**2 + (y1-yc)**2)
            arc_length = (curve/360) * 2*pi*radius
            center = FreeCAD.Vector(xc, yc, 0)
            if curve < 0:
                curve_start = atan2(y2-yc, x2-xc) # radians
                curve_end = atan2(y1-yc, x1-xc) # radians
            else:
                curve_end = atan2(y2-yc, x2-xc) # radians
                curve_start = atan2(y1-yc, x1-xc) # radians
            self.fc.addGeometry(Partfc.ArcOfCircle(Partfc.Circle(center,
                                                                 FreeCAD.Vector(0, 0, 1),
                                                                 radius),
                                                   curve_start,
                                                   curve_end))


        for circle in data.circles:
            self.fc.addGeometry(Partfc.Circle(FreeCAD.Vector(circle.x, circle.y),
                                              FreeCAD.Vector(0.0, 0.0, 1.0),
                                              circle.radius))

    def pad_sketch(self, length, reverse, midplane, length2, padtype, uptoface):
        """
            Pads a given sketch. Creates/Returns a FreeCAD Pad object and hides sketch.
        """
        pad = Part("pad")
        pad.create_blank("pad")
        pad.fc.Sketch = self.fc
        pad.fc.Length = float(length)
        pad.fc.Reversed = reverse
        pad.fc.Midplane = midplane
        pad.fc.Length2 = float(length2)
        pad.fc.Type = padtype
        pad.fc.UpToFace = uptoface
        FreeCAD.ActiveDocument.recompute()
        self.fc.ViewObject.Visibility = False
        return pad