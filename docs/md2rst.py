from glob import glob
import subprocess
import os.path

def change_ext(s):
    return os.path.splitext(s)[0] + ".rst"

if __name__ == "__main__":
    for file in glob("*.md"):
        print "{} ==> {}".format(file, change_ext(file))
        subprocess.call(["pandoc", "-o", change_ext(file), file])