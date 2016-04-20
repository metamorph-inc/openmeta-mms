from matlab_wrapper import MatlabWrapper
# from openmdao.api import FileRef
import numpy
import json


c = MatlabWrapper('matlab_wrapper\\test\\stat2.m')


def default(obj):
    # if isinstance(obj, FileRef):
    #     return repr(obj)
    if isinstance(obj, numpy.ndarray):
        return repr(obj)
    raise TypeError(repr(obj) + " is not JSON serializable")

print(json.dumps({'params': c._init_params_dict, 'unknowns': c._init_unknowns_dict}, default=default))

unknowns = {}
c.solve_nonlinear({'a': 2.5, 'b': 3.5, 'c': 4.0}, unknowns, {})
print(repr(unknowns))

# import stat2
# x = numpy.array([12.7, 45.4, 98.9, 26.6, 53.1])
# values = _marray(_dtype(x), _size(x), x)
# (ave, stdev) = stat2(values)

# assert(ave._a[0][0] == 47.339999999999996)
# assert(stdev._a[0][0] == 29.412419145660223)

# ave = 47.3400
# stdev =   29.4124
