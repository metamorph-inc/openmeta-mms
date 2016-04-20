__author__ = 'curator'

import sys
import argparse
import gme
import os
import re
import traceback
import win32com.client
import lxml
from lxml import etree
from collections import namedtuple
import uuid

# Common Functions
def get_fco_name(project, fco):
    project.begin_transaction()
    name = fco.Name
    project.abort_transaction()
    return name


def get_fcos_by_kind(project, kind):
    project.begin_transaction()
    filter = project.project.CreateFilter()
    filter.Kind = kind
    fcos = project.project.AllFCOs(filter)
    project.abort_transaction()
    return fcos


def sanitize_name(cname):
    return re.sub('[/.,\'*?-]', '_', cname)


def create_component_package(out, libname, xv, mv, comp_folder):
    try:
        devName = xv.device.get('name')
        sdevName = sanitize_name(xv.deviceset.get('name')) + sanitize_name(devName)
        print 'Creating Component for Device: {}'.format(devName)

        # create component and schematic object
        comp = comp_folder.CreateRootObject(mv.meta_comp)
        comp.Name = sdevName
        guid = uuid.uuid1()
        comp.SetStrAttrByNameDisp('AVMID','AVM.Component.' + str(guid))
        schematic = comp.CreateChildObject(mv.meta_schematic)
        schematic.Name = 'Sch_' + sanitize_name(devName)
        schematic.SetStrAttrByNameDisp('Package', xv.device.get('package')) #TBD SKN: this might need to be a parameter later
        schematic.SetStrAttrByNameDisp('Device', xv.device.get('name'))
        schematic.SetStrAttrByNameDisp('DeviceSet', xv.deviceset.get('name'))
        schematic.SetStrAttrByNameDisp('Library', libname)

        for sp in schematic.Parts:
            sp.SetGmeAttrs(None, 300, 150)

        userValue = xv.deviceset.get('uservalue')
        if (userValue is not None) and ('yes' in userValue):
            epar = schematic.CreateChildObject(mv.meta_param)
            epar.Name = 'value'
            for pp in epar.Parts:
                pp.SetGmeAttrs(None, 20, 20)



        # now find the gate associated with this device
        my_gates = {}
        for conn in xv.device.xpath('connects/connect'):
            my_gates[conn.get('gate')] = [conn.get('pin'), conn.get('pad')]
        # for each gate find the associated symbol and add pins to schematic
        for g in my_gates.iterkeys():
            gate = xv.gates[g]
            sym = xv.symbols[gate.get('symbol')]
            pins = sym.xpath('pin')
            min_x = 0.0
            min_y = 0.0
            for pin in pins:
                x = float(pin.get('x'))
                y = float(pin.get('y'))
                if x < min_x:
                    min_x = x
                if y < min_y:
                    min_y = y
            for pin in pins:
                epin = schematic.CreateChildObject(mv.meta_pin)
                epin.Name = pin.get('name') # TBD SKN: if there are multiple gates, should we uniqify?
                epin.SetStrAttrByNameDisp('Gate', g)
                epin.SetStrAttrByNameDisp('SymbolLocationX', pin.get('x'))
                epin.SetStrAttrByNameDisp('SymbolLocationY', pin.get('y'))
                epin.SetStrAttrByNameDisp('SymbolRotation', pin.get('rot'))
                for part in epin.Parts:
                    x = float(pin.get('x'))
                    y = float(pin.get('y'))
                    gx = int(40.0 * (x - min_x))
                    gy = int(40.0 * (y - min_y))
                    #print "gx: {}, gy: {}, x: {}, y: {}, min_x: {}, min_y: {}".format(gx, gy, x, y, min_x, min_y)
                    part.SetGmeAttrs(None, gx, gy)

        # create resource object
        resource = comp.CreateChildObject(mv.meta_resource)
        resource.Name = 'Res_' + sanitize_name(devName)
        resource.SetStrAttrByNameDisp('Path', 'Schematic/ecad.lbr' )

        # associate resource object with schematic object
        use_res = comp.CreateSimplerConnDisp(mv.meta_use_res, resource, schematic)
        use_res.Name = 'Uses_' + sanitize_name(devName)


        # write resource file
        root = etree.Element('eagle', nsmap={None:'eagle'})
        root.set('version', '6.5.0')
        resdoc = etree.ElementTree(root)
        resdraw = etree.SubElement(root, 'drawing')
        reslib = etree.SubElement(resdraw, 'library')
        respkgs = etree.SubElement(reslib, 'packages')
        respkgs.append(xv.packages[xv.device.get('package')])
        ressyms = etree.SubElement(reslib, 'symbols')
        resdsets = etree.SubElement(reslib, 'devicesets')
        resdset = etree.SubElement(resdsets, 'deviceset')
        resdset.set('name', xv.deviceset.get('name'))
        pfx =  xv.deviceset.get('prefix')
        if pfx is not None:
            resdset.set('prefix', pfx)
        uv = xv.deviceset.get('uservalue')
        if uv is not None:
            resdset.set('uservalue', uv)
        resgates = etree.SubElement(resdset, 'gates')
        resdevs = etree.SubElement(resdset, 'devices')
        resdevs.append(xv.device)
        for g in my_gates.iterkeys():
            gate = xv.gates[g]
            sym = xv.symbols[gate.get('symbol')]
            ressyms.append(sym)
            resgates.append(gate)

        schdir = os.path.join(out, libname, sanitize_name(xv.deviceset.get('name')), sdevName, 'Schematic')
        try:
            os.makedirs(schdir)
        except OSError:
            pass    # directory exists already
        resfname = os.path.join(schdir, 'ecad.lbr')
        resfile = open( resfname, 'w')
        resdoc.write(resfile, xml_declaration=True, pretty_print=True)
        print 'Created Component Resource File: {0}'.format(resfname)
    except BaseException as ex:
        print 'Exception: {}'.format(ex)
        traceback.print_tb(sys.exc_traceback)





def main(argv=sys.argv):
    parser = argparse.ArgumentParser()
    parser.add_argument("lbr", help="path to library containing component's to be imported", metavar="LBR")
    parser.add_argument("out", help="output folder to create component packages", metavar="OUT")
    parser.add_argument("is_design", nargs='?', type=bool, default=False, help="import from a design file? [default is a library file]")
    args = parser.parse_args()
    print 'Schema Library File: {0}'.format(args.lbr)
    print 'Output Folder: {0}'.format(args.out)
    print 'Is Design: {0}'.format(args.is_design)

    xmldoc = etree.parse(args.lbr)
    if args.is_design is False:
        libs = xmldoc.xpath('/eagle/drawing/library')
    else:
        libs = xmldoc.xpath('/eagle/drawing/schematic/libraries/library')
    for lib in libs:
        if args.is_design is False:
            libname = os.path.basename(args.lbr)[:-len('.lbr')]
        else:
            libname = lib.get('name')
        try:
            os.makedirs(os.path.join(args.out, libname))
        except:
            pass

        mgafile = os.path.join(args.out, libname, libname + '.mga')
        gproject = gme.Project.create(mgafile, 'CyPhyML')
        gproject.begin_transaction()

        # setup GME objects and meta objects
        project = gproject.project
        rf = project.RootFolder
        mrf = rf.MetaFolder
        mcf = mrf.DefinedFolderByName('Components',True)
        comp_folder = rf.CreateFolder(mcf)
        comp_folder.Name = libname
        meta_comp = mcf.DefinedFCOByName('Component',True)
        mmtf = meta_comp.DefinedFCOByName('SchematicModel',True)
        meta_schematic = meta_comp.RoleByName('SchematicModel')
        meta_pin = mmtf.RoleByName('SchematicModelPort')
        meta_param = mmtf.RoleByName('SchematicModelParameter')
        meta_resource = meta_comp.RoleByName('Resource')
        meta_use_res = meta_comp.RoleByName('UsesResource')

        meta_vars = namedtuple('meta_vars', 'meta_comp meta_schematic meta_pin meta_param meta_resource meta_use_res')
        xml_vars = namedtuple('xml_vars', 'deviceset device gates symbols packages')

        # create a dictionary of packages and symbols
        packages = {}
        symbols = {}
        for pkg in lib.xpath('packages/package'):
            packages[pkg.get('name')] = pkg
        for sym in lib.xpath('symbols/symbol'):
            symbols[sym.get('name')] = sym
        for dset in lib.xpath('devicesets/deviceset'):
            dset_folder = comp_folder.CreateFolder(mcf)
            dset_folder.Name = sanitize_name(dset.get('name'))
            gates = {}
            for gate in dset.xpath('gates/gate'):
                gates[gate.get('name')] = gate
            for device in dset.xpath('devices/device'):
                create_component_package(args.out, libname,
                                         xml_vars(dset, device, gates, symbols, packages),
                                         meta_vars(meta_comp, meta_schematic, meta_pin, meta_param, meta_resource, meta_use_res),
                                         dset_folder)

        gproject.commit_transaction()
        xmefile = mgafile.replace('.mga','.xme')
        dumper = win32com.client.DispatchEx("Mga.MgaDumper")
        dumper.DumpProject(project, xmefile)

        project.Close(True)
        print 'Created Components Mga File: {0}'.format(xmefile)

    return 0

if __name__ == "__main__":
    sys.exit(main())

