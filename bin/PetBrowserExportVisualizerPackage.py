# -*- coding: utf-8 -*-
from __future__ import print_function, unicode_literals

import argparse
import errno
import json
import os
import os.path
import shutil
import tempfile
import traceback
import zipfile
import win32con
import win32ui

BLOCKED_TABS = ["PETRefinement.R", "SurrogateModeling.R"]

def copy_files(original_dir, new_dir, ignore_exists=False):
    for item in os.listdir(original_dir):
        src = os.path.join(original_dir, item)
        dst = os.path.join(new_dir, item)
        if os.path.isdir(src):
            try:
                shutil.copytree(src, dst)
            except WindowsError as e:
                if ignore_exists and e.errno == errno.EEXIST:
                    pass
                else:
                    raise
        else:
            shutil.copy2(src, dst)

def make_new_visualizer_config(visualizer_config, export_directory):
    visualizer_config["tabs"] = [tab for tab in visualizer_config["tabs"] if tab not in BLOCKED_TABS]

    with open(os.path.join(export_directory, "visualizer.vizconfig"), "w") as new_visualizer_config_file:
        json.dump(visualizer_config, new_visualizer_config_file, indent=2)

def copy_artifacts(merged_directory, project_directory, export_directory):
    copy_artifacts_inner(os.path.join(merged_directory, "metadata.json"), project_directory, export_directory)

def copy_artifacts_inner(metadata_json_path, project_directory, export_directory):
    with open(metadata_json_path, "r") as metadata_file:
        metadata = json.load(metadata_file)
    
    for dataset in metadata["SourceDatasets"]:
        if dataset["Kind"] == 0:
            for folder in dataset["Folders"]:
                tb_manifest_path = os.path.join(project_directory, "results", folder)
                results_dir = os.path.dirname(tb_manifest_path)
                try:
                    copy_files(os.path.join(results_dir, "artifacts"), os.path.join(export_directory, "artifacts"), ignore_exists=True)
                except WindowsError as e:
                    if e.errno == errno.ENOENT:
                        pass
                    else:
                        raise
        else:
            for folder in dataset["Folders"]:
                sub_merged_folder = os.path.join(project_directory, "merged", folder)
                copy_artifacts_inner(os.path.join(sub_merged_folder, "metadata.json"), project_directory, export_directory)

def rename_metadata_json(export_directory):
    shutil.move(os.path.join(export_directory, "metadata.json"), os.path.join(export_directory, "exported_metadata.json"))
    pass

def make_launch_script(export_directory):
    LAUNCH_SCRIPT = r"""
FOR /F "skip=2 tokens=2,*" %%A IN ('%SystemRoot%\SysWoW64\REG.exe query "HKLM\software\Metamorph\OpenMETA-Visualizer" /v "PATH"') DO SET DIG_PATH=%%B
"%DIG_PATH%\Dig\run.cmd" ".\visualizer.vizconfig" "."
"""

    with open(os.path.join(export_directory, "launch.cmd"), "w") as launch_script_file:
        launch_script_file.write(LAUNCH_SCRIPT)

    README_TEXT = r"""To open this dataset in the OpenMETA Visualizer, install the OpenMETA
Visualizer, extract this ZIP file to a location of your choice, and double-click
"launch.cmd" in the extracted folder.

Note that this ZIP *must* be extracted in order to launch in the visualizer.
"""

    with open(os.path.join(export_directory, "INSTRUCTIONS.txt"), "w") as readme_file:
        readme_file.write(README_TEXT)

def zip_directory(source_dir, zip_file):
    relroot = os.path.abspath(source_dir)

    for root, dirs, files in os.walk(source_dir):
        # add directory (needed for empty dirs)
        zip_file.write(root, os.path.relpath(root, relroot))
        for file in files:
            filename = os.path.join(root, file)
            if os.path.isfile(filename): # regular files only
                arcname = os.path.join(os.path.relpath(root, relroot), file)
                zip_file.write(filename, arcname)

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("visualizer_config_path")
    parser.add_argument("project_dir")
    parser.add_argument("meta_install_dir")

    args = parser.parse_args()

    print("Visualizer config path:", args.visualizer_config_path)
    print("Project directory:", args.project_dir)
    print("META install directory:", args.meta_install_dir)

    merged_directory = os.path.dirname(args.visualizer_config_path)

    with open(args.visualizer_config_path, "r") as visualizer_config_file:
        visualizer_config = json.load(visualizer_config_file)

    print("Merged directory:", merged_directory)

    export_directory = tempfile.mkdtemp(prefix="vizexport") # tkFileDialog.askdirectory(initialdir=args.project_dir, title="Export visualizer session to directory")
    
    try:
        # if not export_directory:
        #     parser.exit(status=1, message="No directory selected for export")
        
        print("Exporting to:", export_directory)

        #root.destroy()

        copy_files(merged_directory, export_directory)
        make_new_visualizer_config(visualizer_config, export_directory)
        if os.path.exists(os.path.join(export_directory, "metadata.json")):
            copy_artifacts(merged_directory, args.project_dir, export_directory)
            rename_metadata_json(export_directory)
        make_launch_script(export_directory)

        dialog = win32ui.CreateFileDialog(0, ".zip", None, win32con.OFN_HIDEREADONLY|win32con.OFN_OVERWRITEPROMPT, "ZIP Files (*.zip)|*.zip|All Files (*.*)|*.*||")
        dialog.SetOFNInitialDir(os.getcwd())
        result = dialog.DoModal()
        print(result)

        if result == win32con.IDOK:
            export_zip_filename = dialog.GetPathName()
            with zipfile.ZipFile(export_zip_filename, "w", zipfile.ZIP_DEFLATED) as export_zip:
                zip_directory(export_directory, export_zip)
    finally:
        shutil.rmtree(export_directory)
    

    # raw_input("Press Enter to continue...")

if __name__ == "__main__":
    try:
        main()
    except SystemExit:
        pass
    except:
        win32ui.MessageBox("An error occurred while exporting the visualizer package:\n\n" + traceback.format_exc(), "Error", win32con.MB_OK | win32con.MB_ICONERROR)
