#!/usr/bin/env

import json
from string import Template

def main():
    # Get Testbench Manifest Parameters
    with open("testbench_manifest.json", 'r') as f_in:
        testbench_manifest = json.load(f_in)

    parameters = dict()
    for param in testbench_manifest["Parameters"]:
        parameters[param["Name"]] = str(param["Value"])

    with open("schema_template.cir", 'r') as f_in:
        schema_template = f_in.read()
    
    # Try to generate schema file from template
    try:
        schema = Template(schema_template).substitute(parameters)
    except KeyError, Argument:
        error_message = "Error: Run Aborted: {} does not exist in testbench_manifest.json.".format(Argument)
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
