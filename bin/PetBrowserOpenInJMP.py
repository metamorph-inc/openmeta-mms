from __future__ import print_function, unicode_literals

import argparse
import json
import os
import os.path
import shutil

import win32con
import win32ui
import subprocess

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("viz_config")
    parser.add_argument("meta_path")
    args = parser.parse_args()

    print(args.viz_config)
    print(args.meta_path)

    config_file_path = args.viz_config

    with open(config_file_path, "r") as viz_config_file:
        viz_config = json.load(viz_config_file)

    dataset_basedir = os.path.dirname(config_file_path)
    raw_data_csv_path = os.path.join(dataset_basedir, viz_config["raw_data"])

    result = subprocess.call(["start", "jmp.exe", "/r", raw_data_csv_path], shell=True)

    print(result)

if __name__ == "__main__":
    main()
