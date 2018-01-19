#!/usr/bin/env

import json
from string import Template
import sys
import re

def main():
    # Get Testbench Manifest Parameters
    with open("testbench_manifest.json", 'r') as f_in:
        testbench_manifest = json.load(f_in)

    parameters = dict()
    for param in testbench_manifest["Parameters"]:
        parameters[param["Name"]] = str(param["Value"])

    with open("schema.cir.template", 'r') as f_in:
        schema_template = f_in.read()
    
    # Try to generate schema file from template
    schema = Template(schema_template).safe_substitute(parameters)

    unsubstituted_param = re.search("\$\{(.*)\}", schema)
    if unsubstituted_param is not None:
        error_message = "Error: Run Aborted: Parameter '{}' does not exist in testbench_manifest.json.".format(unsubstituted_param.group(1))
        #Pass error message to Manifest
        testbench_manifest["ErrorMessage"] = error_message

        #Save the testbench_manifest
        with open('testbench_manifest.json', 'w') as f_out:
            json.dump(testbench_manifest, f_out, indent=2)
        
        print error_message
        sys.exit(1)

    with open("schema.cir", "w") as f_out:
        f_out.write(schema)
        

if __name__ == "__main__":
    main()
