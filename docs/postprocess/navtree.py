"""
Fix to navtree prior to use of single pages and \section command. This script
is no longer used.

"""
import argparse
import json
import re

START_STR = "var NAVTREE ="
CHAPTER_RE = re.compile(r'^Chapter (?P<num>\d+):')
NAV_PLACEHOLDER = "{NAVTREE_STR}"


def parse_navtree(path):
    template_str = ""
    start, end = False, False
    nav_str = ""
    with open(path) as fp:
        for i, line in enumerate(fp):
            if end:
                template_str += line
                continue
            stripped = line.strip()
            if start:
                nav_str += stripped
                if stripped.endswith('];'):
                    end = True
                    template_str += "\n" + NAV_PLACEHOLDER + "\n"
            elif stripped.startswith(START_STR):
                start = True
                nav_str += stripped[len(START_STR):]
            else:
                template_str += line

    if not start:
        raise Exception('"{}" not found in {}'.format(START_STR, path))
    if not end:
        raise Exception("End of navtree not found -- incorrect formatting")
    return json.loads(nav_str[:-1]), template_str


def _fix_entries(entries):
    """recursive function to collapse entries into correct format"""
    cur_chapter_re, chapter_entry = None, None
    new_entries = []
    for entry in entries:
        title, doxy_path, subentries = entry
        if subentries is not None:
            new_subentries = _fix_entries(subentries)
            new_entries.append([title, doxy_path, new_subentries])
        elif cur_chapter_re and cur_chapter_re.match(title):
            chapter_entry[2].append(entry)
        else:
            new_entries.append(entry)
            chapter_match = CHAPTER_RE.match(title)
            if chapter_match:
                cur_chapter_re = re.compile(
                    chapter_match.group('num') + r'\.\d+:')
                chapter_entry = entry
                chapter_entry[-1] = []
            else:
                cur_chapter_re, chapter_entry = None, None

    return new_entries


def fix_navtree(path):
    # parse current navtree
    navtree, template_str = parse_navtree(path)

    # fix the navtree
    new_navtree = _fix_entries(navtree)
    nav_str = START_STR + ' ' + json.dumps(new_navtree) + ';'
    # write it back
    with open(path, 'w') as fp:
        fp.write(template_str.replace(NAV_PLACEHOLDER, nav_str, 1))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="fixes navigation tree")
    parser.add_argument("-i", "--input", default="out/html/navtree.js",
                        help="path to navtree js file")
    args = parser.parse_args()
    fix_navtree(args.input)
