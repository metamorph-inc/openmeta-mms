__author__ = 'robertboyles'

import sys
import json
import logging
from utility_functions import exitwitherror


class Wire(object):
    def __init__(self, data):
        self.x1 = data.get("x1")
        self.x2 = data.get("x2")
        self.y1 = data.get("y1")
        self.y2 = data.get("y2")
        self.layer = data.get("layer")
        self.width = data.get("width")
        self.curve = data.get("curve")


class Circle(object):
    def __init__(self, data):
        self.x = data.get("x")
        self.y = data.get("y")
        self.radius = data.get("radius")
        self.layer = data.get("layer")
        self.width = data.get("width")


class Border(object):
    def __init__(self):
        self.wires = []
        self.arcs = []
        self.circles = []


class Package(object):
    def __init__(self, data):
        self.guid = data.get("ComponentID")
        self.height = data.get("height")
        self.koheight = data.get("koHeight")
        self.kowidth = data.get("koWidth")
        self.layer = data.get("layer")
        self.name = data.get("name")
        self.originx = data.get("originX")
        self.originy = data.get("originY")
        self.package = data.get("package")
        self.pkgidx = data.get("pkg_idx")
        self.rotation = data.get("rotation")
        self.width = data.get("width")
        self.x = data.get("x")
        self.y = data.get("y")
        self.constraints = data.get("constraints")
        self.relComponentId = data.get("RelComponentID")


class Layout(object):
    def __init__(self):
        self.Border = None
        self.packages = []
        self.height = None
        self.width = None
        self.layers = None

    def get_board_border(self, border):
        if len(border) > 0:
            self.Border = Border()
            for segment in border:
                seg_type = segment.keys()[0]
                if seg_type == "wire" :
                    wire = Wire(segment["wire"])
                    if wire.layer == 20:
                        if wire.curve == 0.0:
                            self.Border.wires.append(wire)
                        else:
                            self.Border.arcs.append(wire)

                elif seg_type == "circle":
                    circle = Circle(segment["circle"])
                    if circle.layer == 20:
                        self.Border.circles.append(circle)

    def get_packages(self, packages):
        for package in packages:
            self.packages.append(Package(package))

    """
    def get_board_holes(outline, pcb_data):
        get_vias(outline, pcb_data)

        # Package holes are specified in local csys. To conver this to global csys requires
        # Parsing a lot of the brd file (package instance final position, package bounding box, etc)
        # Too much work for a BRD parser since this will need to be redone in CyPhy2Schematic for
        # adding to layout.json.
        # for package in outline.findall("libraries//library//packages//package"):
        #    get_package_holes(package, pcb_data)


    # def get_package_holes(package, pcb_data):

        # return
    """


    """
    def get_vias(outline, pcb_data):
        vias = [x for x in outline.iter() if x.tag == "via"]
        for v in vias:
            via = {"x": float(v.get("x")), "y": float(v.get("y")), "radius": float(v.get("drill"))}
            pcb_data["circles"].append(via)
    """


    def parse_layout(self, layoutpath):
        try:
            with open(layoutpath, "r") as layoutfile:
                layout = json.load(layoutfile)
        except IOError:
            exitwitherror('Layout JSON file not found! Message: ' + str(IOError.strerror))

        self.height = layout.get("boardHeight")
        self.width = layout.get("boardWidth")
        self.layers = layout.get("numLayers")

        if "border" in layout:
            self.get_board_border(layout["border"])
        if "packages" in layout:
            self.get_packages(layout["packages"])
        # get_board_holes(board, pcb_data)  # TODO: Vias not in layout yet


if __name__ == '__main__':
    layout = Layout()
    layout.parse_layout(sys.argv[1])  # debugging