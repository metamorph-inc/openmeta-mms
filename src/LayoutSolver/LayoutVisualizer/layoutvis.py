__author__ = 'Sandeep'

import Tkinter as Tk
from Tkinter import *
import json
from json import *

json_data = open('layout-output.json').read()
layout = json.loads(json_data)

master = Tk()

w = Canvas(master, width=400, height=400)
w.pack()

for p in layout["packages"]:
    rot = p["rotation"]
    x1 = p["x"]*10
    y1 = p["y"]*10
    if rot == 1:
        x2 = x1 + p["height"]*10
        y2 = y1 + p["width"]*10
    else:
        x2 = x1 + p["width"]*10
        y2 = y1 + p["height"]*10

    if p["layer"] == 1:
        w.create_rectangle(x1, y1, x2, y2, dash=(3,5))
    else:
        w.create_rectangle(x1, y1, x2, y2)

mainloop()


