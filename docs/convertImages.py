from glob import glob
import re

def convert_ref(line):
    if line.startswith("<img"):
        m = re.search('<img src="(.+)" alt="(.+)" style="width: (.+);"/>', line)
        return "\n.. image:: {}\n   :alt: {}\n   :width: {}\n".format(m.group(1), m.group(2), m.group(3))
    # elif line.startswith("apple"):
        # return line + "\n\n"
    else:
        return ""

if __name__ == "__main__":
    with open("images.txt", "w") as fout:
        for file in glob("*.md"):
            # print file
            with open(file, "r") as fin:
                for line in fin.readlines():
                    fout.write(convert_ref(line))