__author__ = 'Sandeep'

import os
import json
import argparse
import re
import numpy as np

from SpiceVisualizer.spicedatareader import SpiceDataReader

NET_RE = re.compile(r'[iv]\((?P<net>\d+)\)')
OBJ_RE = re.compile(r'[iv]\((?P<obj>[\w\.]+)\)')

def process_siginfo(siginfo, signet_map, instpath_map, parent, ppath):
    if "signals" in siginfo:
        ndname = siginfo["name"]
        dpos = ndname.find('$')
        sndname = ndname[:dpos] if dpos > 0 else ndname
        cpath = '{}.{}'.format(ppath, sndname) if ppath is not None else ndname
        for signal in siginfo["signals"]:
            process_siginfo(signal, signet_map, instpath_map, siginfo["name"], cpath)
        instpath_map[ndname.lower()] = cpath
        #print 'Inst:{} Path:{}'.format(ndname.lower(), cpath)
    elif "spicePort" in siginfo:
        signame = '{}.{}'.format(parent, siginfo["spicePort"]).lower()
        signet_map[signame] = siginfo["net"]
        #print '{}:{}'.format(signame, siginfo["net"])


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description="Take a raw spice output file and power signals (voltages and currents)")
    parser.add_argument('input_file', help='Raw Spice File to process')
    parser.add_argument('-i', '--info',
                        help='path to siginfo file containing signal hierarchy')
    parser.add_argument('-d', '--downsample',
                        default=10,
                        help='downsample factor to store in output file')
    parser.add_argument('-s', '--sensorkeyword',
                        default='vpowersense',
                        help='the keyword to identify power sensor in spice output')
    parser.add_argument('-m', '--metricSource', default='EndoDC5V',
                        help='the source component for which metrics should be computed')
    args = parser.parse_args()

    PNET_RE = re.compile(r'i\(v.x(?P<inst>[a-z0-9$_]+).' + args.sensorkeyword + r'_(?P<na>\d+)_(?P<nb>\d+)\)')
    #print PNET_RE
    PNET_RE2 = re.compile(r'i\(v.x(?P<inst>[a-z0-9$_]+).x([a-z0-9$_]+).' + args.sensorkeyword + r'_(?P<na>\d+)_(?P<nb>\d+)\)')
    #print PNET_RE2
    ds = int(args.downsample)

    siginfo = None
    info_path = args.info
    if not info_path:
        info_path = os.path.join(
            os.path.dirname(args.input_file), 'siginfo.json')
    if os.path.exists(info_path):
        with open(info_path) as fp:
            siginfo = json.load(fp)

    signet_map = dict()
    instpath_map = dict()
    process_siginfo(siginfo, signet_map, instpath_map, siginfo["name"], None)

    data = {"plots": []}
    reader = SpiceDataReader(args.input_file)

    timeval = []
    datavecs = dict()
    for plot in reader.get_plots():
        s = plot.scale_vector
        timeval = s.data
        for j, d in enumerate(plot.data_vectors):
            datavecs[d.name] = d

    print 'Attempt to write out tables for lumped parameter analysis\n'
    print 'begin key vector\n'
    print datavecs
    print 'match to PNET_RE:\n'
    print PNET_RE
    print 'OR match to PNET_RE2:\n'
    print 'end key vector'
    # for all sensors dump the power consumption in a modelica combitable compatible ascii format
    for key in datavecs.iterkeys():
        #print 'KEY==',key
        m = PNET_RE.match(key)
        if m is None:
            m = PNET_RE2.match(key)
            if m is None:
                continue
        #print m.group('na')
        #print m.group('nb')
        inst = m.group('inst')
        pa = m.group('na')
        pb = m.group('nb')

        sa = '{}.{}'.format(inst, pa)
        sb = '{}.{}'.format(inst, pb)

        di = datavecs[key].data  ## current

        na = signet_map[sa] if signet_map.has_key(sa) else None
        vna = 'v({})'.format(na)
        dva = datavecs[vna].data if datavecs.has_key(vna) else None

        nb = signet_map[sb] if signet_map.has_key(sb) else None
        vnb = 'v({})'.format(nb)
        dvb = datavecs[vnb].data if datavecs.has_key(vnb) else None

        diabs = abs(di)
        print 'node a: {}'.format(vna)
        print 'node b: {}'.format(vnb)
        if dva is None:
            print 'Missing voltage node a: {}'.format(pa)
            continue;
            pw = abs(dvb) * diabs
        elif dvb is None:
            pw = abs(dva) * diabs
        else:
            pw = abs(dva - dvb) * diabs

        print 'CurrentSensor: {}, ports: {} --> {}'.format(inst, pa, pb)
        print 'VoltageAcross: {} --> {}'.format(vna, vnb)

        instpath = instpath_map[inst] if instpath_map.has_key(inst) else None

        ### TBD temporary workaround to get common file names
        instpath1 = instpath[instpath.find('.')+1:]
        instpath2 = instpath1[instpath1.find('.')+1:]
        print instpath2

        f = open('{}.txt'.format(instpath2), 'wt')
        print 'Writing: {}, Array Size: {}, Downsample: {}'.format(f.name, pw.size, ds)
        f.write('#1\n')
        f.write('double tab({},2)\n'.format(pw.size/ds))
        for i in range(pw.size/ds):
            avgpw = 0
            if i==0:
                avgpw = pw[0]
            else:
                for j in range(ds):
                    avgpw = avgpw + pw[(i-1)*ds + j]
            avgpw = avgpw/ds
            f.write('{} {}\n'.format(timeval[i*ds],avgpw))
        f.close()

    # open the testbench metric file
    # find node port
    mslower = args.metricSource.lower()
    mskey = mslower + '.1'
    ms = signet_map[mskey] if signet_map.has_key(mskey) else None
    ims = 'i(v{})'.format(mslower)
    vms = 'v({})'.format(ms)
    dims = datavecs[ims].data if datavecs.has_key(ims) else None
    dvms = datavecs[vms].data if datavecs.has_key(vms) else None
    maxCurrent = None
    maxPower = None
    if dims is None or dvms is None:
        print 'Unable to find voltage/current data for metric source: {} {}'.format(ims, vms)
    else:
        dimsabs = abs(dims)
        pwms = abs(dvms) * dimsabs
        try:
            maxPower = max(pwms)
            maxCurrent = max(dimsabs)
        except:
            print 'Exception: zero length power array'

    # write in metrics file
    manifestJson = None
    path_manifest = 'testbench_manifest.json'
    if os.path.exists(path_manifest):
        with open(path_manifest,'rt') as mf:
            manifestJson = json.load(mf)
    else:
        print 'Missing {}, unable to update metrics'.format(path_manifest)

    if manifestJson is not None:
        hasMetrics = []
        for metric in manifestJson['Metrics']:
            #print 'Metric: {}'.format(metric['Name'])
            if metric['Name'] == 'MaxCurrent':
                metric['Value'] = maxCurrent
                hasMetrics.append('MaxCurrent')
            if metric['Name'] == 'MaxPower':
                metric['Value'] = maxPower
                hasMetrics.append('MaxPower')

        if len(hasMetrics) == 0:
            print 'No MaxCurrent or MaxPower metric to update in {}'.format(path_manifest)
        else:
            str_metrics = ', '.join(hasMetrics)
            print 'Updating {} Metrics in {}'.format(str_metrics, path_manifest)
            with open(path_manifest,'wt') as mf:
                json.dump(manifestJson, mf, indent=4)

    print 'Done'
