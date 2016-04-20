import sys
from SpiceVisualizer.spicedatareader import SpiceDataReader

if __name__ == "__main__":
    ## plot out some information about the spice files given by commandline
    for f in sys.argv[1:]:
        print 'The file: "' + f + '" contains the following plots:' 
        for i, p in enumerate(SpiceDataReader(f).get_plots()):
            print '  Plot', i, 'with the attributes'
            print '    Title: ', p.title
            print '    Date: ', p.date
            print '    Plotname: ', p.name
            print '    Plottype: ', p.type

            s = p.scale_vector
            print '    The Scale vector has the following properties:'
            print '      Name: ', s.name
            print '      Type: ', s.type
            v = s.data
            print '      Vector-Length: ', len(v)
            print '      Vector-Type: ', v.dtype

            for j, d in enumerate(p.data_vectors):
                print '    Data vector', j, 'has the following properties:'
                print '      Name: ', d.name
                print '      Type: ', d.type
                v = d.data
                print '      Vector-Length: ', len(v)
                print '      Vector-Type: ', v.dtype
