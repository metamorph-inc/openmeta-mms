#!/usr/bin/python

#     Copyright (C) 2007,2011 Werner Hoch
#
#    This program is free software; you can redistribute it and/or modify
#    it under the terms of the GNU General Public License as published by
#    the Free Software Foundation; either version 2 of the License, or
#    (at your option) any later version.
#
#    This program is distributed in the hope that it will be useful,
#    but WITHOUT ANY WARRANTY; without even the implied warranty of
#    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#    GNU General Public License for more details.
#
#    You should have received a copy of the GNU General Public License
#    along with this program.  If not, see <http://www.gnu.org/licenses/>.

import numpy
import string
import sys


class SpiceVector(object):
    """
    Contains a single spice vector with it's data and it's attributes.
    The vector is numpy.array, either real or complex.
    The attributes are:
      * name: vector name
      * type: frequency, voltage or current
    """
    
    def __init__(self, vector=numpy.array([]), **kwargs):
        self.data = vector
        self.name = ""
        self.type = ""
        self.set_attributes(**kwargs)
        
    def set_attributes(self, **kwargs):
        """
        Set the attributes of the vector "name" and "type"
        """
        for name, value in kwargs.items():
            if hasattr(self, name):
                if type(getattr(self, name)) == type(value):
                    setattr(self, name, value)
                else:
                    print("Warning: attribute has wrong type: "
                          + type(value) + " ignored!")
            else:
                print "Warning: unknown attribute" + name + " Ignored!"
    

class SpicePlot(object):
    """
    This class holds a single spice plot
    It contains one scale vector and a list of several data vectors.
    The plot may have some attributes like "title", "date", ...
    """

    def __init__(self, scale=None, data=None, **kwargs):
        """
        Initialize a new spice plot.
        Scale may be an spice_vector and data may be a list of spice_vectors.
        The attributes are provided by **kwargs.
        """
        self.title = None
        self.date = None
        self.name = None
        self.type = None
        self.dimensions = []

        ## a single scale vector
        if scale is None:
            self.scale_vector = SpiceVector()
        else:
            self.scale_vector = scale

        ## init the list of spice_vector
        if data is None:
            self.data_vectors = []
        else:
            self.data_vectors = data     

        self.set_attributes(**kwargs)

    def set_attributes(self, **kwargs):
        """
        Set the attributes of a plot. 
        """
        for name, value in kwargs.items():
            if hasattr(self, name):
                if type(getattr(self, name)) == type(value):
                    setattr(self, name, value)
                else:
                    print "Warning: attribute has wrong type: " \
                          + type(value) + " ignored!"
            else:
                print "Warning: unknown attribute \"" + name + "\". Ignored!"


class SpiceDataReader(object):
    """
    This class is reads a spice data file and returns a list of spice_plot
    objects.

    The file syntax is mostly taken from the function raw_write() from
    ngspice-rework-17 file ./src/frontend/rawfile.c
    """

    def __init__(self, filename):
        self.plots = []
        self.set_default_values()
        error = self.readfile(filename)
        if error:
            ## FIXME create an assertion
            print "error in reading the file"

    def set_default_values(self):
        ## Set the default values for some options
        self.current_plot = SpicePlot()
        self.nvars = 0
        self.npoints = 0
        self.numdims = 0
        self.padded = True
        self.real = True
        self.vectors = []

    def readfile(self, filename):
        with open(filename, "rb") as fp:
            while True:
                line = fp.readline()
                if line == "":   # EOF
                    return

                tok = [t.strip() for t in line.split(":", 1)]
                keyword = tok[0].lower()  # keyword is case insensitive

                if keyword == "title":
                    self.current_plot.title = tok[1]
                elif keyword == "date":
                    self.current_plot.date = tok[1]
                elif keyword == "plotname":  # FIXME: incomplete??
                    self.current_plot.name = tok[1]
                elif keyword == "flags":
                    ftok = [t.strip().lower() for t in tok[1].split()]
                    for flag in ftok:
                        if flag == "real":
                            self.real = True
                        elif flag == "complex":
                            self.real = False
                        elif flag == "unpadded":
                            self.padded = False
                        elif flag == "padded":
                            self.padded = True
                        else:
                            print 'Warning: unknown flag: "' + flag + '"'
                elif keyword == "no. variables":
                    self.nvars = int(tok[1])
                elif keyword == "no. points":
                    self.npoints = int(tok[1])
                elif keyword == "dimensions":
                    if self.npoints == 0:
                        print 'Error: misplaced "Dimensions:" lineprint'
                        continue
                    print 'Warning: "Dimensions" not supported yet'
                    # FIXME: How can I create such simulation files?
                    # numdims = string.atoi(tok[1])
                elif keyword == "command":
                    print 'Warning: "command" option not implemented yet'
                    print '\t' + line
                    # FIXME: what is this command good for
                elif keyword == "option":
                    print 'Warning: "command" option not implemented yet'
                    print '\t' + line
                    # FIXME: what is this command good for
                elif keyword == "variables":
                    for i in xrange(self.nvars):
                        line = fp.readline().strip().split()
                        if len(line) >= 3:
                            number = int(line[0])
                            vector = SpiceVector(name=line[1], type=line[2])
                            self.vectors.append(vector)
                            if len(line) > 3:
                                # print "Attributes: ", line[3:]
                                dummy = 1
                                ## min=, max, color, grid, plot, dim
                                ## I think only dim is useful and neccesary
                        else:
                            raise Exception("Input file is malformed: list of variables is too short")

                elif keyword in ["values", "binary"]:
                    # read the data
                    if self.real:
                        if keyword == "values":
                            i = 0
                            a = numpy.zeros(self.npoints * self.nvars,
                                            dtype="float64")
                            while i < self.npoints * self.nvars:
                                t = fp.readline().split("\t")
                                if len(t) < 2:
                                    continue
                                else:
                                    a[i] = float(t[1])
                                i += 1
                            aa = a.reshape(self.npoints, self.nvars)
                            self.vectors[0].data = aa[:, 0]
                            self.current_plot.scale_vector = self.vectors[0]
                            for n in xrange(1, self.nvars):
                                self.vectors[n].data = aa[:, n]
                                self.current_plot.data_vectors.append(
                                    self.vectors[n])
                        else:  # keyword = "binary"
                            def chunks(size, chunk_size):
                                for i in xrange(size // chunk_size):
                                    yield chunk_size
                                remainder = size % chunk_size
                                if remainder:
                                    yield remainder
                            def iter_aas():
                                float64_size = 8
                                file_beginning = fp.tell()
                                # preferably, read in 100MB during each iteration
                                chunk_size = 100 * 2**20 // float64_size // self.npoints
                                # but read at least 4k for each seek
                                chunk_size = max(4096/float64_size, chunk_size)
                                for chunk_num, chunk in enumerate(chunks(self.nvars, chunk_size)):
                                    fp.seek(file_beginning + chunk_num * chunk_size * float64_size)
                                    a = numpy.empty(shape=(self.npoints, chunk), dtype='float64')
                                    for i in xrange(self.npoints):
                                        a[i,:] = numpy.frombuffer(fp.read(chunk*float64_size), dtype='float64')
                                        if i != self.npoints-1:
                                            fp.seek((self.nvars - chunk) * float64_size, 1)
                                    for i in xrange(chunk):
                                        yield a[:,i]

                            it = iter(iter_aas())
                            self.vectors[0].data = it.next()
                            self.current_plot.scale_vector = self.vectors[0]
                            for n, aa in enumerate(it):
                                self.vectors[n+1].data = aa
                                self.current_plot.data_vectors.append(
                                    self.vectors[n+1])
                                #print repr(aa)

                    else:  # complex data
                        if keyword == "values":
                            i = 0
                            a = numpy.zeros(self.npoints * self.nvars * 2,
                                            dtype="float64")
                            while i < self.npoints * self.nvars * 2:
                                t = fp.readline().split("\t")
                                if len(t) < 2:  # empty lines
                                    continue
                                else:
                                    t = t[1].split(",")
                                    a[i] = float(t[0])
                                    i += 1
                                    a[i] = float(t[1])
                                    i += 1
                        else:  # keyword = "binary"
                            a = numpy.frombuffer(
                                fp.read(self.nvars * self.npoints * 8 * 2),
                                dtype="float64")
                        aa = a.reshape(self.npoints, self.nvars * 2)
                        self.vectors[0].data = aa[:, 0]  # only the real part!
                        self.current_plot.scale_vector = self.vectors[0]
                        for n in xrange(1, self.nvars):
                            self.vectors[n].data = numpy.array(
                                aa[:, 2 * n] + 1j * aa[:, 2 * n + 1])
                            self.current_plot.data_vectors.append(
                                self.vectors[n])

                    # create new plot after the data
                    self.plots.append(self.current_plot)
                    self.set_default_values()

                elif string.strip(keyword) == "":  # ignore empty lines
                    continue

                else:
                    print 'Error: strange line in rawfile:\n\t"'  \
                          + line + '"\n\t load aborted'
                    return 0

    def get_plots(self):
        return self.plots


if __name__ == "__main__":
    ## plot out some informations about the spice files given by commandline
    for f in sys.argv[1:]:
        print 'The file: "' + f + '" contains the following plots:' 
        for i,p in enumerate(SpiceDataReader(f).get_plots()):
            print '  Plot', i, 'with the attributes'
            print '    Title: ' , p.title
            print '    Date: ', p.date
            print '    Plotname: ', p.plotname
            print '    Plottype: ' , p.plottype

            s = p.get_scalevector()
            print '    The Scale vector has the following properties:'
            print '      Name: ', s.name
            print '      Type: ', s.type
            v = s.get_data()
            print '      Vector-Length: ', len(v)
            print '      Vector-Type: ', v.dtype
