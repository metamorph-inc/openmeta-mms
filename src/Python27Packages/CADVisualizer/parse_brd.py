__author__ = 'robertboyles'

import sys
import xml.etree.ElementTree as ET


def get_board_border(outline, pcb_data, newmethod):
    layer20 = [x for x in outline.getchildren() if x.get("layer") == "20"]
    if newmethod:
        wiretag = '{eagle}wire'
        circletag = '{eagle}tag'
    else:
        wiretag = 'wire'
        circletag = 'circle'

    for el in layer20:
        if el.tag == wiretag:
            x1 = float(el.get("x1"))
            x2 = float(el.get("x2"))
            y1 = float(el.get("y1"))
            y2 = float(el.get("y2"))
            curve = el.get("curve")

            if curve is None:
                wire = {"x1": x1, "x2": x2, "y1": y1, "y2": y2}
                pcb_data["wires"].append(wire)
            else:
                arc = {"x1": x1, "x2": x2, "y1": y1, "y2": y2, "curve": float(curve)}
                pcb_data["arcs"].append(arc)

        elif el.tag == circletag:
            circle = {"x": float(el.get("x")), "y": float(el.get("y")), "radius": float(el.get("radius"))}
            pcb_data["circles"].append(circle)


def get_board_holes(outline, pcb_data):
    return
    #get_vias(outline, pcb_data)

    # Package holes are specified in local csys. To conver this to global csys requires
    # Parsing a lot of the brd file (package instance final position, package bounding box, etc)
    # Too much work for a BRD parser since this will need to be redone in CyPhy2Schematic for
    # adding to layout.json.
    # for package in outline.findall("libraries//library//packages//package"):
    #    get_package_holes(package, pcb_data)


# def get_package_holes(package, pcb_data):

    # return


def get_vias(outline, pcb_data):
    vias = [x for x in outline.iter() if x.tag == "via"]
    for v in vias:
        via = {"x": float(v.get("x")), "y": float(v.get("y")), "radius": float(v.get("drill"))}
        pcb_data["circles"].append(via)


def parse_brd(brdfile):
    tree = ET.parse(brdfile)
    root = tree.getroot()

    pcb_data = {"wires": [], "circles": [], "arcs": []}

    try:
        board = root.find("drawing//board")
        plain = board.find("plain")
        newmethod = False
    except:
        board = root.find("{eagle}drawing//{eagle}board")
        plain = board.find("{eagle}plain")
        newmethod = True

    get_board_border(plain, pcb_data, newmethod=newmethod)

    get_board_holes(board, pcb_data)
    print pcb_data["circles"]
    return pcb_data


if __name__ == '__main__':
    parse_brd(sys.argv[1])  # debugging