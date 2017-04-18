#!/usr/bin/env

import json
import string
def main():
    with open("testbench_manifest.json", 'r') as file_in:
        manifestJson = json.load(file_in)

    params = {}

    for param in manifestJson["Parameters"]:
        params[param["Name"]] = str(param["Value"])

    with open("build_simulink.m.in", 'r') as templateFile, open("build_simulink.m", "w") as outputFile:
        for line in templateFile:
            lineTemplate = string.Template(line)
            outputFile.write(lineTemplate.safe_substitute(params))

if __name__ == "__main__":
    main()
