__author__ = 'robertboyles'

import logging
from utility_functions import exitwitherror
import meta_freecad as meta
import eda_layout

global logger
logger = logging.getLogger()


class BRD(object):
    def __init__(self, thickness, height, width, layers):
        self.boardthick = thickness
        self.height = height
        self.width = width
        self.layers = layers

    def create_basic_pcb(self):
        pcb = meta.Part("PCB")
        pcb.create_blank()
        pcb.fc.Shape = meta.make_box(self.width, self.height, self.boardthick)
        pcb.fc.ViewObject.ShapeColor = PCBColors.BOARD
        logger.info('Board dimensions: h={0}  w={1} lay={2}'.format(self.height, self.width, self.layers))

    def create_detailed_pcb(self, layout):
        """
            Generate the detailed PCB file. Over time this will be expanded to include
            the base structure of the board, traces, component pads, etc.
        """
        self.board_border(layout.Border)

    def board_border(self, border):
        """
            Generate the base structure of the PCB.
        """
        pcb = meta.Sketch("PCB_Border")
        pcb.create_blank()
        pcb.set_sketch_plane('XY', 0.0)
        pcb.sketch_from_coords(border)
        pad = pcb.pad_sketch(self.boardthick, 0, 0, 0.0, 0, None)

        pcb_part = meta.Part("PCB")
        pcb_part.create_blank("part")
        meta.copy_shape(pad, pcb_part, True)
        
        color_array = []
        for i in range(len(pcb_part.fc.Shape.Faces)):
            color_array.append(PCBColors.BOARD)
        pcb_part.set_colors(color_array)


class PCBColors(object):
    """
        Defines a set of colors assigned to various parts of the detailed PCB.
        Each tuple represents a R-G-B color, each value [0, 1].
    """
    BOARD = (0.0, 0.7, 0.0)
    PLACEHOLDER = (1.0, 0.0, 0.0)
