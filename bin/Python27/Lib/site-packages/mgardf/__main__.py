from mgardf.mgardfconverter import MgaRdfConverter
from mgardf.utilities import xme2mga
from os.path import exists, splitext
import os
from argparse import ArgumentParser
from tempfile import mkstemp
import udm


def main():
    parser = ArgumentParser(description='Extract TTL-formatted RDF from a GME project')
    parser.add_argument('project', type=str, nargs=1,
                        help='the path of the GME project (XME or MGA) from which to extract')
    parser.add_argument('udm_xml', type=str, nargs=1,
                        help='the path of the UDM XML file defining the language')

    args = parser.parse_args()

    path_project = args.project[0]
    path_udm_xml = args.udm_xml[0]

    # Test for existence of file
    if not exists(path_project):
        raise ValueError('Cannot find GME project {}'.format(path_project))

    # Is it an MGA or an RDF?
    path_rootname, extension = splitext(path_project)
    # print (path_rootname, extension)

    if extension == '.xme':
        # Must convert to MGA first
        handle, path_mga = mkstemp(suffix='.mga')
        os.close(handle)
        print ('creating a temporary MGA file at {}'.format(path_mga))

        xme2mga(path_project, path_mga)

    elif extension == '.mga':
        path_mga = path_project
    else:
        raise ValueError('This project file needs to be either a .mga or .xme file: {}'.format(path_project))

    # Now let's load it.
    uml_diagram = udm.uml_diagram()
    meta_dn = udm.SmartDataNetwork(uml_diagram)
    meta_dn.open(path_udm_xml, b'')

    dn = udm.SmartDataNetwork(meta_dn.root)
    dn.open(path_mga, b'')

    g = MgaRdfConverter.convert(dn.root, original_filename=path_project, udm_xml=path_udm_xml)
    with open(path_rootname + '.ttl', 'w') as ttl:
        g.serialize(ttl, format='turtle')

    dn.close_no_update()
    meta_dn.close_no_update()


if __name__ == '__main__':
    main()
