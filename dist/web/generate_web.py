from __future__ import print_function, unicode_literals, absolute_import

# This script generates the release and previous releases page for a given
# release.  Should be run from the *root* of the tonka repository.

import csv
import io
import os
import os.path
import tempfile
import subprocess
from datetime import date

from jinja2 import Environment, PackageLoader, select_autoescape
env = Environment(
    loader=PackageLoader('generate_web', 'templates'),
    autoescape=select_autoescape(['html', 'xml'])
)

def main(releaseId):
    version = subprocess.check_output(['git', 'describe', '--match', 'v*']).strip()
    if releaseId != version:
        raise ValueError('Version mismatch: {} {}'.format(releaseId, version))
    last_major_version = subprocess.check_output(['git', 'describe', '--match', 'v*', '--abbrev=0']).strip()
    now = date.today().isoformat()
    print(version)
    print(last_major_version)
    print(now)

    try:
        with open(os.path.join("dist", "release_notes", last_major_version + ".html")) as relnotesFile:
            notes = relnotesFile.read()
    except Exception as e:
        print(e)
        notes = "Release notes not available"

    template = env.get_template("release.html")

    with io.open(version + ".html", "w", encoding="utf-8") as outputFile:
        outputFile.write(template.render({
            "version": version,
            "date": now,
            "notes": notes
        }))

if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser(description="generate_web.py")
    parser.add_argument("releaseId")
    args = parser.parse_args()

    main(args.releaseId)
