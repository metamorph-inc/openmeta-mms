import argparse
from datetime import datetime
import json
import os
import re

from SpiceVisualizer.spicedatareader import SpiceDataReader

DATE_FMT = '%a %b %d %H:%M:%S  %Y'
NET_RE = re.compile(r'[iv]\((?P<net>\d+)\)')
OBJ_RE = re.compile(r'[iv]\((?P<obj>[\w\.]+)\)')


class SpicePlotJsonConverter(object):
    def __init__(self, spice_plot, siginfo=None):
        self.plot = spice_plot
        self.data = {}
        self.siginfo = siginfo

    def convert(self):
        self.convert_metadata()
        self.convert_data()
        if self.siginfo:
            self.add_legend()
        return self.data

    def convert_metadata(self):
        try:
            plot_date = datetime.strptime(self.plot.date, DATE_FMT).isoformat()
        except ValueError:
            plot_date = self.plot.date
        self.data.update({
            "title": self.plot.title,
            "date": plot_date,
            "name": self.plot.name,
            "type": self.plot.type
        })

    def _convert_data_array(self, arr):
        return [v.real for v in arr]

    def _json_for_vector(self, vector):
        return {
            "name": vector.name,
            "type": vector.type,
            "data": self._convert_data_array(vector.data)
        }

    def convert_data(self):
        self.data.update({
            "signals": {},
            "independent": self._json_for_vector(self.plot.scale_vector)
        })
        for data_vector in self.plot.data_vectors:
            this_data = self._json_for_vector(data_vector)
            self.data["signals"][data_vector.name] = this_data

    ## Parse siginfo
    def _get_obj_names(self, component, test_instance):
        obj_names = [sig['name'] for sig in test_instance['signals']]
        obj_names.append(component['name'])
        return obj_names

    def _get_net_from_vector_name(self, vector_name):
        m = NET_RE.match(vector_name)
        if m:
            return m.group('net')

    def _add_connections_to_netmap(self, obj, net_map, depth=-1):
        for signal in obj["signals"]:
            if "signals" in signal:
                if depth:
                    self._add_connections_to_netmap(signal, net_map, depth - 1)
            elif signal["net"] in net_map:
                net_map[signal["net"]]["connections"].append({
                    "object": obj["name"],
                    "port": signal["name"]
                })

    def _get_obj_for_vector(self, vector_name, obj_names):
        m = OBJ_RE.match(vector_name)
        if m:
            obj_name = m.group('obj')
            matching = [o for o in obj_names if o.lower() in obj_name]
            if matching:
                return max(matching, key=lambda s: len(s))

    def add_legend(self):
        self.data['testbench'] = self.siginfo['name']
        component, test_instance = self.siginfo['signals']
        obj_names = self._get_obj_names(component, test_instance)
        net_map = {}
        obj_map = {}
        for vector_name in self.data["signals"]:
            net = self._get_net_from_vector_name(vector_name)
            if net:
                net_data = net_map.setdefault(
                    net, {"connections": [], "net": net, "signals": []})
                net_data["signals"].append(vector_name)
            else:
                obj_name = self._get_obj_for_vector(vector_name, obj_names)
                if obj_name:
                    obj_data = obj_map.setdefault(
                        obj_name, {"object": obj_name, "signals": []})
                    obj_data["signals"].append(vector_name)
        self._add_connections_to_netmap(component, net_map, 0)
        self._add_connections_to_netmap(test_instance, net_map, 1)
        self.data["legend"] = [n for n in net_map.values() if n["connections"]]
        self.data["legend"].extend(obj_map.values())


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description="Take a raw spice output file and convert to json")
    parser.add_argument('input_file', help='Raw Spice File to convert')
    parser.add_argument('-i', '--info',
                        help='path to siginfo file containing heirarchy data')
    parser.add_argument('-o', '--output',
                        help='Output file (defaults to <INPUT_FILENAME>.json')
    parser.add_argument('-c', '--clean', action="store_true", default=False,
                        help='delete input files after completion')
    args = parser.parse_args()

    output_fname = args.output
    if not output_fname:
        name, ext = os.path.splitext(args.input_file)
        output_fname = name + '.json'

    siginfo = None
    info_path = args.info
    if not info_path:
        info_path = os.path.join(
            os.path.dirname(args.input_file), 'siginfo.json')
    if os.path.exists(info_path):
        with open(info_path) as fp:
            siginfo = json.load(fp)

    data = {"plots": []}
    reader = SpiceDataReader(args.input_file)
    for plot in reader.plots:
        converter = SpicePlotJsonConverter(plot, siginfo=siginfo)
        data["plots"].append(converter.convert())

    with open(output_fname, 'w') as output_file:
        json.dump(data, output_file)

    if args.clean:
        os.remove(args.input_file)
        if siginfo:
            os.remove(info_path)
