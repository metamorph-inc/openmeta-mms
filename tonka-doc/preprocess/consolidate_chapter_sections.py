import argparse
import os
import fnmatch
import glob


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description="Consolidates all markdown files in a chapter folder into a single markdown md_file.")
    parser.add_argument("-f", "--folder", default="processed_doc",
                        help="Path to root folder of markdown documents. "
                             "Each folder with a number as a name prefix "
                             "(e.g.: 02-some-chapter) is treated as a chapter folder.")

    args = parser.parse_args()

    # Recursively walk directory tree
    for root, dirnames, filenames in os.walk(args.folder):
        # Find folders that start with a number, then a dash
        for dirname in fnmatch.filter(dirnames, '[0123456789]*-*'):
            # For each *.md file,
            #   open it,
            #   copy its contents to the accumulator,
            #   and delete it.
            # Finally, save the accumulator as a new file.

            str_accumulator = ""
            for md_file in sorted(glob.glob(os.path.join(root, dirname, "*.md"))):
                path_file = os.path.join(md_file)
                with open(path_file) as f:
                    str_file_content = f.read()
                    str_accumulator += str_file_content + os.linesep

                os.remove(path_file)

            path_newfile = os.path.join(root, dirname, dirname + ".md")
            with open(path_newfile, "w") as f_consolidated:
                f_consolidated.write(str_accumulator)