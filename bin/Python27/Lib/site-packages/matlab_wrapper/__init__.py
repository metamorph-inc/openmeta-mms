from __future__ import absolute_import
from __future__ import print_function
from openmdao.api import Component
import sys
import os
import os.path
import inspect
import json
import six
import importlib

import StringIO

import smop
import smop.parse
import smop.backend
import smop.node


def import_mfile(mFile):
    buf = open(mFile).read().replace("\r\n", "\n")
    func_list = smop.parse.parse(buf if buf[-1] == '\n' else buf + '\n', mFile)
    return func_list


class MatlabWrapper(Component):
    def __init__(self, mFile, varFile=None):
        super(MatlabWrapper, self).__init__()
        # self.var_dict = None
        # self.jsonFile = varFile
        # self.create_json_dict()

        if False:
            for key, value in self.var_dict.items():
                if key == "params":
                    for z in value:
                        # print(repr(z))
                        self.add_param(**z)
                elif key == "unknowns":
                    for z in value:
                        # print z["name"]
                        self.add_output(**z)

        if not os.path.exists(mFile):
            open(mFile)

        self.mFile = os.path.abspath(mFile)

        mfname = os.path.splitext(mFile)[0]
        self.basename = os.path.basename(mfname)

        self.func_list = func_list = import_mfile(mFile)
        fn = [f for f in func_list if type(f) == smop.node.function and f.head.ident.name == self.basename][0]
        self._input_names = [e.name for e in fn.head.args]
        self._output_names = [e.name for e in fn.head.ret]

        for input in self._input_names:
            self.add_param(input, val=0.0)
        for output in self._output_names:
            self.add_output(output, val=0.0)

        try:
            import matlab.engine
            self.eng = matlab.engine.start_matlab()
            self.eng.addpath(os.path.dirname(os.path.abspath(mFile)), nargout=0)
            # self.eng.addpath(r'C:\AVM-M\Seeker\AVM_Seeker_Modelling - Version_2.1', nargout=0)
        except ImportError as e:
            import warnings
            warnings.warn("Failed to import matlab.engine: %s" % e, RuntimeWarning)
            self.eng = None
            imports = ['from smop.runtime import *',
                       'from smop.core import *']

            # FIXME won't work for files with dependencies
            source = '\n\n'.join(imports + [smop.backend.backend(func_obj) for func_obj in self.func_list])
            with open(self.basename + '.py', 'wb') as out:
                out.write(source)
            code = compile(source, self.basename + '.py', 'exec')
            mod = {}
            # eval(code, six.moves.builtins, mod)
            eval(code, mod, mod)
            self.func = mod[self.basename]

    def __del__(self):
        # TODO
        # if self.eng
        # self.eng.quit()
        pass

    def _coerce_val(self, variable):
        if variable['type'] == 'Bool':
            variable['val'] = variable['val'] == 'True'
        elif variable['type'] == 'Str':
            variable['val'] = six.text_type(variable['val'])
        else:
            variable['val'] = getattr(six.moves.builtins, variable['type'].lower())(variable['val'])

    def create_json_dict(self):
        with open(self.jsonFile) as jsonReader:
            self.var_dict = json.load(jsonReader)
            for vartype in ('params', 'unknowns'):
                for var in self.var_dict.get(vartype, []):
                    self._coerce_val(var)

    def solve_nonlinear(self, params, unknowns, resids):
        args = [params[name] for name in self._input_names]

        def set_unknowns(outputs):
            for i, name in enumerate(self._output_names):
                unknowns[name] = outputs[i]

        if self.eng is not None:
            out = StringIO.StringIO()
            err = StringIO.StringIO()

            outputs = getattr(self.eng, self.basename)(*args, nargout=len(self._output_names), stdout=out, stderr=err)

            set_unknowns(outputs)
        else:
            outputs = self.func(*args)

            set_unknowns(outputs)


if __name__ == '__main__':
    # print(repr(sys.argv[1:]))
    c = MatlabWrapper(*sys.argv[1:])
    print((json.dumps({'params': c._init_params_dict, 'unknowns': c._init_unknowns_dict})))
