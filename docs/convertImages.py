from glob import glob
import re

def convert_ref(line):
    if line.startswith("<img"):
        m = re.search('<img src="(.+)" alt="(.+)" style="width: (.+);"/>', line)
        return "\n.. image:: {}\n   :alt: {}\n   :width: {}\n".format(m.group(1), m.group(2), m.group(3))
    elif line.startswith("!["):
        m = re.search('\[(.+)\]\((.+)\)', line)
        return ".. image:: {}\n   :alt: {}\n".format(m.group(2), m.group(1))
    else:
        return line

if __name__ == "__main__":
    with open("full.md", "w") as fout:
        files = glob("*.md")
        files.remove("full.md")
        for file in files:
            print file
            with open(file, "r") as fin:
                for line in fin.readlines():
                    fout.write(convert_ref(line))