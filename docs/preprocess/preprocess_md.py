import re
import argparse
import os
import fnmatch

SECTION_RE = re.compile(ur'^(?P<bang>#{2,4})(?P<title>[^#].*)', flags=re.U)
BI_RE = re.compile(ur'(\s)\*\*\*(.*?)\*\*\*', flags=re.U)


def _replace_bang(m):
    substr = u''.join(['sub'] * (len(m.group('bang')) - 2))
    slug = re.sub(ur'[^a-zA-Z0-9]+', u'-', m.group('title').strip().lower(),
                  flags=re.UNICODE).strip(u'-')
    return u'\\' + substr + u'section ' + slug + u' ' + m.group('title')


def preprocess_file(path, outpath):
    newlines = []
    with open(path, 'r') as fp:
        for line in fp:
            line = SECTION_RE.sub(_replace_bang, line)
            line = BI_RE.sub(ur'\g<1><b><em>\g<2></em></b>', line)
            newlines.append(line)
    with open(outpath, 'w') as fp:
        fp.writelines(newlines)


def preprocess_docs(doc_folder, out_folder):
    for root, dirnames, filenames in os.walk(doc_folder):
        for filename in fnmatch.filter(filenames, '*.md'):
            fullpath = os.path.join(root, filename)
            out_root = root.replace(doc_folder, out_folder, 1)
            if not os.path.exists(out_root):
                os.makedirs(out_root)
            outpath = os.path.join(out_root, filename)
            preprocess_file(fullpath, outpath)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description="Converts Bangs to doxygen section commands")
    parser.add_argument("-i", "--input_folder", default="doc",
                        help="path to root folder of markdown files")
    parser.add_argument("-o", "--output_folder", default="processed_doc",
                        help="path to root folder of markdown files")
    args = parser.parse_args()
    if not os.path.exists(args.output_folder):
        os.makedirs(args.output_folder)
    preprocess_docs(args.input_folder.rstrip('/'),
                    args.output_folder.rstrip('/'))

