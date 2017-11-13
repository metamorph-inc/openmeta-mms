from __future__ import print_function, unicode_literals

import argparse
import json
import os
import os.path
import shutil

import win32con
import win32ui

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("viz_config")
    parser.add_argument("meta_path")
    args = parser.parse_args()

    print(args.viz_config)
    print(args.meta_path)

    dialog = win32ui.CreateFileDialog(0, ".csv", None, win32con.OFN_HIDEREADONLY|win32con.OFN_OVERWRITEPROMPT, "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*||")
    dialog.SetOFNInitialDir(os.getcwd())
    result = dialog.DoModal()
    print(result)

    if result == win32con.IDOK:
        print("OK clicked")
        print("Filename:", dialog.GetFileName())
        print("File path:", dialog.GetPathName())

        config_file_path = args.viz_config
        destination_path = dialog.GetPathName()

        with open(config_file_path, "r") as viz_config_file:
            viz_config = json.load(viz_config_file)

        dataset_basedir = os.path.dirname(config_file_path)
        raw_data_csv_path = os.path.join(dataset_basedir, viz_config["raw_data"])

        shutil.copyfile(raw_data_csv_path, destination_path)

if __name__ == "__main__":
    main()
