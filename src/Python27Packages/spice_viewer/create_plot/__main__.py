import re
from SpiceVisualizer import spicedatareader
import json
import matplotlib.pyplot


netNameRe = re.compile("v\\((\\d+)\\)")
netData = {}


with open('siginfo.json') as jsonData:
    sigInfo = json.load(jsonData)

sr = spicedatareader.SpiceDataReader('schema.raw')
plotVarMap = {}
plots = []
for i, p in enumerate(sr.get_plots()):
    print '  Plot', i, 'with the attributes'
    print '    Title: ', p.title
    v = p.scale_vector.data
    for j, d in enumerate(p.data_vectors):
        print '    Data vector', j, 'has the following properties:'
        print '      Name: ', d.name, ' Type: ', d.type

        match = netNameRe.match(d.name)
        if match is None:
            continue
        net = match.group(1)
        # pvName = d.name + ' - ' + d.type
        # plotvars.append(pvName)
        # plotVarMap[d.name] = j
        # v = d.data
        # print '      Vector-Length: ', len(v), ' Vector-Type: ', v.dtype

        netData[net] = {
            'min': min(d.data),
            'max': max(d.data)
        }
        stride = max(1, len(p.scale_vector.data) // 10000)  # FIXME aliasing issues
        plot = matplotlib.pyplot.plot(p.scale_vector.data[::stride], d.data[::stride])
        matplotlib.pyplot.savefig('net{}.png'.format(net), dpi=125)
        matplotlib.pyplot.cla()
        matplotlib.pyplot.clf()
        matplotlib.pyplot.close()

        with open('net{}.json'.format(net), 'wb') as jsonOut:
            class StreamArray(list):

                """Generator for datapoints for efficiency."""

                def __init__(self):
                    self.gen = ([p.scale_vector.data[i], d.data[i]] for i in xrange(0, len(d.data), stride))

                def __iter__(self):
                    return self.gen.__iter__()

                def __len__(self):
                    return len(d.data) / stride

            json.dump(StreamArray(), jsonOut)

    with open('netdata.json', 'wb') as netDataOut:
        json.dump(netData, netDataOut)
