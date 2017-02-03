import fnmatch
import os
import sys
import shutil
import subprocess

os.chdir(os.path.dirname(os.path.abspath(__file__)))

### Delete generated html and preprocessed files
if os.path.exists('out/html_documentation'):
    shutil.rmtree('out/html_documentation', ignore_errors=True)
if os.path.exists('processed_doc'):
    shutil.rmtree('processed_doc', ignore_errors=True)

### Preprocess
subprocess.check_call([sys.executable, 'preprocess/preprocess_md.py'])
subprocess.check_call([sys.executable, 'preprocess/consolidate_chapter_sections.py'])


### Call Doxygen
subprocess.check_call(['../3rdParty/doxygen/doxygen-1.8.5.windows.x64.bin/doxygen.exe', 'Doxyfile'])


### Copy Images
img_path = os.path.join('out', 'html_documentation', 'images')
if not os.path.exists(img_path):
    os.makedirs(img_path)

img_matches = []
for root, dirnames, filenames in os.walk('doc'):
    for filename in filenames:
        if not filename.endswith(('.gif', '.png', '.svg', '.jpg')):
            continue

        org_path = os.path.join(root, filename)
        file_name_only = os.path.basename(filename)
        new_path = os.path.join(img_path, file_name_only)
        if os.path.exists(new_path):
            print "ERROR: " + file_name_only + " isn't a unique filename!"
        else:
            shutil.copyfile(org_path, new_path)
