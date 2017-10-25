#!/usr/bin/env python

from sys import exit
from get_eagle_path import find_eagle


if __name__ == "__main__":
    eagle_path = find_eagle()
    if eagle_path:
        print(eagle_path)
        exit(0)
    else:
        exit(1)
