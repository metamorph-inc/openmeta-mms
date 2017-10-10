from __future__ import print_function, unicode_literals

import argparse
import json
import os
import os.path
import subprocess

import _winreg as winreg
with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"Software\META") as software_meta:
    meta_path, _ = winreg.QueryValueEx(software_meta, "META_PATH")

def main():
    parser = argparse.ArgumentParser(description="Run a configuration of a merged PET at the specified points.")

    parser.add_argument("merged_pet_directory", help="Merged PET directory for the PET to run")
    parser.add_argument("cfg_id", help="Configuration ID to run")
    parser.add_argument("csv_file", help="CSV file with design variables to run")

    args = parser.parse_args()

    project_dir = os.path.join(args.merged_pet_directory, "..", "..")
    results_dir = os.path.join(project_dir, "results")

    merged_pet_metadata_filename = os.path.join(args.merged_pet_directory, "metadata.json")

    with open(merged_pet_metadata_filename, "r") as merged_pet_metadata_file:
        metadata = json.load(merged_pet_metadata_file)

    matching_results_folder = find_results_folder_for_config(args.cfg_id, results_dir, metadata)
    print(matching_results_folder)

    csv_abspath = os.path.abspath(args.csv_file)

    meta_python = os.path.join(meta_path, "bin", "Python27", "Scripts", "python.exe")

    result = subprocess.call([meta_python, "-m", "run_mdao", "--append-csv", "--desvar-input", csv_abspath],
                             cwd=matching_results_folder)

    print(result)


def find_results_folder_for_config(config_id, results_dir, metadata):
    for dataset in metadata["SourceDatasets"]:
        # TODO: only considering PETResult datasets for now; should look at recursively searching
        #       PET and MergedPET datasets later to make this more general
        if dataset["Kind"] == 0:
            for folder in dataset["Folders"]:
                tb_manifest_path = os.path.join(results_dir, folder)
                cfg = get_config_id_for_manifest(tb_manifest_path)

                if cfg == config_id:
                    return os.path.abspath(os.path.dirname(tb_manifest_path))

    # Not found
    raise RuntimeError("Configuration not found")


def get_config_id_for_manifest(tb_manifest_path):
    with open(tb_manifest_path, "r") as manifest_file:
        manifest = json.load(manifest_file)

    return manifest["CfgID"]


if __name__ == "__main__":
    main()
