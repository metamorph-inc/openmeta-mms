#!/usr/bin/env python

from _winreg import *
from sys import exit


def find_eagle_8():
    major = 8
    minor_nums = range(20, -1, -1)
    patch_nums = range(20, -1, -1)
    versions_desc = list()
    for minor in minor_nums:
        for patch in patch_nums:
            versions_desc.append('{major}-{minor}-{patch}'.format(major=major,
                                                                  minor=minor,
                                                                  patch=patch))

    # Go through descending version numbers until we find a match.
    rk_template = 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{{AUTODESK-EAGLE-{}}}_is1'
    path_eagle = None
    for version in versions_desc:
        try:
            regpath_key = rk_template.format(version)
            key = OpenKey(HKEY_LOCAL_MACHINE, regpath_key, 0, KEY_READ | KEY_WOW64_64KEY)
            path_eagle = QueryValueEx(key, 'DisplayIcon')[0]
            CloseKey(key)

            # This is the highest-numbered version found so far.
            # Take its path and stop.
            break

        except WindowsError:
            pass

    return path_eagle


def find_eagle_6_7():
    path_eagle = None

    try:
        k = OpenKey(HKEY_LOCAL_MACHINE, 'SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\eagle.exe',
                    0, KEY_READ | KEY_WOW64_64KEY)
        path_eagle = QueryValueEx(k, 'Path')[0] + '\\bin\\eagle.exe'
        CloseKey(k)

    except WindowsError:
        # Fail silently -- the BAT calling us will have the message for users.
        pass

    return path_eagle


if __name__ == "__main__":
    # First, let's look for Version 8.x
    path_eagle = find_eagle_8()

    if path_eagle:
        print(path_eagle)
        exit(0)

    # Let's look for 7.x or 6.x
    path_eagle = find_eagle_6_7()

    if path_eagle:
        print(path_eagle)
        exit(0)

    exit(1)
