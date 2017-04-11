#!/usr/bin/env python
from __future__ import print_function, unicode_literals

import os
import json

import numpy as np

def main():
    data = np.genfromtxt("output.csv", delimiter=",")
    plantOutput = data[:, 2]

    metrics = {
        'MaxPlantOutput': {
            'value': np.amax(plantOutput),
            'unit': None
        }
    }

    print(metrics)
    update_metrics_in_report_json(metrics)

def update_metrics_in_report_json(metrics, report_file='testbench_manifest.json'):
    """
    Metrics should be of the form
    :param metrics:
    :param report_file:
    {'name_of_metric' : {value: (int) or (float), unit: ""}, ...}
    """

    if not os.path.exists(report_file):
        raise IOError('Report file does not exits : {0}'.format(report_file))

    # read current summary report, which contains the metrics
    with open(report_file, 'r') as file_in:
        result_json = json.load(file_in)

    assert isinstance(result_json, dict)

    if 'Metrics' in result_json:
        for metric in result_json['Metrics']:
            if 'Name' in metric and 'Value' in metric:
                if metric['Name'] in metrics.keys():
                    new_value = metrics[metric['Name']]['value']
                    new_unit = metrics[metric['Name']]['unit']
                    if new_unit is not None:
                        metric['Unit'] = new_unit
                    if new_value is not None:
                        metric['Value'] = str(new_value)
                else:
                    pass
            else:
                print('Metric item : {0} does not have right format'.format(metric))
                pass
                # update json file with the new values
        with open(report_file, 'wb') as file_out:
            json.dump(result_json, file_out, indent=4)
    else:
        print('Report file {0} does not have any Metrics defined..')
        pass

if __name__ == "__main__":
    main()
