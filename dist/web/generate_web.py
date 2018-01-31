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
from urllib2 import urlopen, URLError, HTTPError

RELEASES_CSV_DOWNLOAD_URL = "https://releases.metamorphsoftware.com/releases.csv"

from jinja2 import Environment, PackageLoader, select_autoescape
env = Environment(
    loader=PackageLoader('generate_web', 'templates'),
    autoescape=select_autoescape(['html', 'xml'])
)

def main():
    version = subprocess.check_output(['git', 'describe', '--match', 'v*']).strip()
    last_major_version = subprocess.check_output(['git', 'describe', '--match', 'v*', '--abbrev=0']).strip()
    now = date.today().strftime("%x")
    print(version)
    print(last_major_version)
    print(now)

    try:
        with open(os.path.join("dist", "release_notes", last_major_version + ".html")) as relnotesFile:
            notes = relnotesFile.read()
    except Exception as e:
        print(e)
        notes = "Release notes not available"

    template = env.get_template("index.html")

    with io.open("index.html", "w", encoding="utf-8") as outputFile:
        outputFile.write(template.render({
            "version": version,
            "date": now,
            "notes": notes
        }))

    # Pull down releases.csv (contains metadata about previous releases)
    download(RELEASES_CSV_DOWNLOAD_URL, "releases.csv")

    releases = []

    try:
        with open("releases.csv", "r") as csvFile:
            reader = csv.DictReader(csvFile)
            for row in reader:
                releases.append(row)
    except Exception as e:
        print(e)
        releases = []

    releases.append({
        "Version": version,
        "Release Date": now,
        "Download Link": "/releases/" + version + "/META_bundle_x64.exe",
        "Info Link": "/releases/" + version + "/index.html"
    })

    with open("releases.csv", "w") as csvFile:
        writer = csv.DictWriter(csvFile, ["Version", "Release Date", "Download Link", "Info Link"])
        writer.writeheader()
        for row in releases:
            writer.writerow(row)

    template = env.get_template("previous_releases.html")

    with io.open("previous_releases.html", "w", encoding="utf-8") as outputFile:
        outputFile.write(template.render({
            "version": version,
            "date": now,
            "releases": releases
        }))

def download(url, filename):
    print('Downloading ' + url)

    try:
        f = urlopen(url)
        print("downloading " + url)

        fd, tmp_path = tempfile.mkstemp()
        with os.fdopen(fd, 'wb') as local_file:
            local_file.write(f.read())

        if os.path.isfile(filename):
            os.remove(filename)

        os.rename(tmp_path, filename)
        print('  => {}'.format(filename))
    #handle errors
    except HTTPError, e:
        print("HTTP Error:", e.code, url)
        raise
    except URLError, e:
        print("URL Error:", e.reason, url)
        raise

if __name__ == "__main__":
    main()
