import sys
import six
import pickle
import re
import os
import os.path
import platform
import warnings
import subprocess
from contextlib import contextmanager

from base64 import b64encode, b64decode
from functools import total_ordering

if sys.version_info[0:2] >= (3, 8):
    from os import add_dll_directory
else:
    # can use PATH with these versions
    @contextmanager
    def add_dll_directory(directory):
        os.environ['PATH'] = directory + os.pathsep + os.environ['PATH']
        yield None


DEBUG = False


class AnalysisError(Exception):
    """The client will convert this to openmdao.api.AnalysisError.
    The proxy server does not have openmdao"""
    pass


class MatlabInProcessProxyReplacement(object):
    def __init__(self, engine):
        self.engine = engine

    def addpath(self, path, nargout=0):
        self.engine.addpath(path, nargout=0)

    def __getattr__(self, name):
        import numpy

        def _invoke(args, nargout=1, bare=False, input_names=None, output_names=None, stdout=None, stderr=None):
            import openmdao.api
            import matlab
            import matlab.engine

            def transcode(val):
                if numpy and isinstance(val, numpy.ndarray):
                    return val.tolist()
                if numpy and isinstance(val, (numpy.float16, numpy.float32, numpy.float64)):
                    return float(val)
                return val
            args = [transcode(val) for val in args]
            def convert_to_list(array, size=None):
                if not isinstance(array, matlab.double):
                    return array
                if size is None:
                    size = array.size
                if len(size) == 1:
                    return list(array)
                return list((convert_to_list(l, size[1:]) for l in array))

            string_stdout = six.StringIO()
            string_stderr = six.StringIO()

            try:
                if bare:
                    nargout = 0
                    for i, input_name in enumerate(input_names):
                        arg = args[i]

                        if isinstance(args[i], list):
                            if len(arg) and isinstance(arg[0], six.string_types):
                                pass
                            else:
                                arg = matlab.double(arg)
                        self.engine.workspace[str(input_name)] = arg

                    getattr(self.engine, name)(nargout=nargout, stdout=string_stdout, stderr=string_stderr)
                    outputs = []

                    for output_name in output_names:
                        output = self.engine.workspace[str(output_name)]
                        output = convert_to_list(output)
                        outputs.append(output)
                    # open('debug.txt', 'a').write(repr(outputs) + '\n')
                else:
                    outputs = getattr(self.engine, name)(*args, nargout=nargout, stdout=string_stdout, stderr=string_stderr)
                    if type(outputs) == tuple:
                        outputs = tuple(map(convert_to_list, outputs))
                    else:
                        outputs = convert_to_list(outputs)
            except Exception as e:
                stdout.write(string_stdout.getvalue())
                stderr.write(string_stderr.getvalue())
                if type(e).__module__ in ('matlab', 'matlab.engine'):
                    e = openmdao.api.AnalysisError(getattr(e, 'message', getattr(e, 'args', ['unknown MATLAB exception'])[0]))
                    raise e
                raise


            stdout.write(string_stdout.getvalue())
            stderr.write(string_stderr.getvalue())

            # print(repr(outputs))
            return outputs

        _invoke.__name__ = name
        return _invoke

    def quit(self):
        self.engine.quit()


class EngineProxyServer(object):
    def __init__(self, engine):
        self.engine = engine

    def addpath(self, path):
        self.engine.addpath(path, nargout=0)

    def invoke(self, name, args, nargout, bare, input_names, output_names):
        import matlab
        import matlab.engine
        out = six.StringIO()
        err = six.StringIO()

        def convert_arg_to_matlab_datatype(arg):
            if isinstance(arg, list):
                if len(arg) and isinstance(arg[0], six.string_types):
                    return arg
                else:
                    return matlab.double(arg)
            elif isinstance(arg, dict):
                for k, v in six.iteritems(arg):
                    arg[k] = convert_arg_to_matlab_datatype(v)
                return arg
            else:
                return arg

        def convert_to_list(array, size=None):
            if array is None:
                return None
            if isinstance(array, list):
                return [convert_dictionary_contents_to_list(v) for v in array]
            if not isinstance(array, matlab.double):
               return array
            if size is None:
                size = array.size
            if len(size) == 1:
                return list(array)
            return list((convert_to_list(l, size[1:]) for l in array))

        def convert_dictionary_contents_to_list(d):
            if not isinstance(d, dict):
                return convert_to_list(d)
            else: # dict
                for k,v in six.iteritems(d):
                    d[k] = convert_dictionary_contents_to_list(v)
                return d

        if bare:
            nargout = 0

            for i, input_name in enumerate(input_names):
                arg = convert_arg_to_matlab_datatype(args[i])

                try:
                    self.engine.workspace[input_name] = arg
                except ValueError as e:
                    if e.args[0] == 'invalid field for MATLAB struct':
                        for name in arg:
                            if not re.match('^[a-zA-Z][a-zA-Z0-9_]{0,62}$', name):
                                raise ValueError('invalid field for MATLAB struct "{0}". '.format(name) +
                                    'MATLAB fields must start with a letter and contain only letters, numbers, and underscores. ' +
                                    'MATLAB fields must contain 63 characters or fewer.')
                    raise

            try:
                getattr(self.engine, name)(nargout=nargout, stdout=out, stderr=err)
            except Exception as e:
                e.matlab_proxy_stdout = out.getvalue()
                e.matlab_proxy_stderr = err.getvalue()
                raise

            outputs = []

            # debug_file = open('DEBUG: EngineProxyServer_invoke.txt', 'a')
            # debug_file.write("MATLAB outputs - raw:"+'\n')
            # for output_name in output_names:
            #     output = self.engine.workspace[str(output_name)]
            #     debug_file.write("  " + repr(output) + '\n')
            # debug_file.write("Done."+'\n')

            for output_name in output_names:
                output = self.engine.workspace[str(output_name)]
                output = convert_dictionary_contents_to_list(output)
                outputs.append(output)

            # debug_file.write("MATLAB outputs - processed:"+'\n')
            # debug_file.write(repr(outputs) + '\n')
            # debug_file.write("Done."+'\n')
            # debug_file.close()

        else:
            try:
                outputs = getattr(self.engine, name)(*args, nargout=nargout, stdout=out, stderr=err)
            except Exception as e:
                e.matlab_proxy_stdout = out.getvalue()
                e.matlab_proxy_stderr = err.getvalue()
                raise

            if type(outputs) == tuple:
                outputs = tuple(map(convert_to_list, outputs))
            else:
                outputs = convert_to_list(outputs)

        return {"output": pickle.dumps(outputs), "stdout": out.getvalue(), "stderr": err.getvalue()}

    def quit(self):
        self.engine.quit()


class EngineProxyClient(object):
    def __init__(self, proxy):
        self.proxy = proxy
        numpy = None
        try:
            import numpy
        except ImportError:
            pass
        self.numpy = numpy

    def addpath(self, path, nargout=0):
        self.proxy.addpath(path)

    def quit(self):
        self.proxy.quit()
        self.proxy = None

    def __del__(self):
        if self.proxy:
            self.quit()

    def __getattr__(self, name):
        numpy = self.numpy

        def transcode(val):
            if numpy and isinstance(val, numpy.ndarray):
                return val.tolist()
            if numpy and isinstance(val, (numpy.float16, numpy.float32, numpy.float64)):
                return float(val)
            return val

        def invoke(args, **kwargs):
            # (*args, nargout=len(self._output_names), stdout=out, stderr=err)
            args = list(map(transcode, args))
            kwargs = {k: transcode(v) for k, v in six.iteritems(kwargs)}

            try:
                ret = self.proxy.invoke(name, args, kwargs.get('nargout'), bare=kwargs['bare'],
                    input_names=kwargs['input_names'], output_names=kwargs['output_names'])
                for output in ('stdout', 'stderr'):
                    stdout = kwargs.get(output)
                    if stdout:
                        stdout.write(ret[output])
            except Exception as e:
                stdout = kwargs.get("stdout")
                if stdout:
                    stdout.write(getattr(e, "matlab_proxy_stdout", ""))

                stdout = kwargs.get("stderr")
                if stdout:
                    stdout.write(getattr(e, "matlab_proxy_stderr", ""))

                raise

            return pickle.loads(ret["output"])

        return invoke


def get_matlab_engine():
    if sys.platform == 'win32':
        matlab = get_preferred_matlab()
        if not matlab:
            warnings.warn("MATLAB not found in registry. Using Python implementation.", RuntimeWarning)
            return None
        if matlab[0] == platform.architecture()[0]:
            MATLABROOT = matlab[2]
            engine = import_matlab_python_engine(MATLABROOT)
            with matlab_in_env_path(MATLABROOT, platform.architecture()[0]):
                return MatlabInProcessProxyReplacement(engine.start_matlab())
        else:
            if six.PY2:
                proxy_python_version = ''.join((str(v) for v in sys.version_info[0:2]))
            else:
                # https://www.mathworks.com/support/requirements/python-compatibility.html
                # R2021a is 9.10
                if MatlabVersion(matlab[1]) <= MatlabVersion('9.10'):
                    proxy_python_version = '37'
                else:
                    proxy_python_version = '39'

            python_exe = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'dist_{}_{}\\matlab_proxy.exe'.format(proxy_python_version, matlab[0]))
            if not os.path.isfile(python_exe):
                raise Exception("'{}' does not exist. Run `setup.py py2exe` with Python {} in the matlab_proxy dir".format(python_exe, matlab[0]))
            return get_engine_proxy(matlab[2], python_exe, matlab[0])
    try:
        import matlab.engine
        return MatlabInProcessProxyReplacement(matlab.engine.start_matlab())
    except ImportError as e:
        warnings.warn("Failed to import matlab.engine: %s" % e, RuntimeWarning)
        return None


@contextmanager
def matlab_in_env_path(MATLABROOT, target_architecture):
    # e.g. C:\Program Files\MATLAB\R2022b\bin\win64\libmx.dll
    win_bit = {'32bit': 'win32',
            '64bit': 'win64'}[target_architecture]
    old_path = os.environ['PATH']
    os.environ['PATH'] = os.path.join(MATLABROOT, 'bin', win_bit) + os.pathsep + os.environ['PATH']
    # numpy modifies PATH to add tbb.dll et al. But its tbb.dll is incompatible with MATLAB's
    # os.environ['PATH'] = os.pathsep.join(p for p in os.environ['PATH'].split(os.pathsep) if 'numpy' not in p)
    try:
        # `import` uses LOAD_LIBRARY_SEARCH_DEFAULT_DIRS in Python >=3.8; https://bugs.python.org/issue36085 ; don't try PATH
        with add_dll_directory(os.path.join(MATLABROOT, 'bin', win_bit)):
            yield None
    finally:
        os.environ['PATH'] = old_path


def get_engine_proxy(MATLABROOT, python_exe, target_architecture):
    import openmdao.api

    with matlab_in_env_path(MATLABROOT, target_architecture):
        # test with python.exe: python_exe = r"C:\Users\kevin\Documents\matlab_wrapper\env310_64\Scripts\python.exe" and add os.path.abspath(__file__),

        kwargs = dict(stdout=subprocess.PIPE, stdin=subprocess.PIPE, stderr=subprocess.STDOUT, bufsize=1)
        if not six.PY2:
            kwargs['encoding'] = 'utf8'
        worker = subprocess.Popen([python_exe, MATLABROOT], **kwargs)

    def read():
        return worker.stdout.readline()

    def write(output):
        worker.stdin.write(output)
        if six.PY2:
            worker.stdin.flush()

    magic = read().rstrip('\n')
    if magic != HANDSHAKE_MAGIC:
        rest, _ = worker.communicate()
        raise Exception(repr(magic) + '\n' + rest)

    def dispatch(method, *args, **kwargs):
        write(method + '\n')
        # print >> sys.stderr, "AAArgs:", args

        write(b64encode(pickle.dumps(args)).decode('ascii') + '\n')
        write(b64encode(pickle.dumps(kwargs)).decode('ascii') + '\n')
        error_str = read().rstrip('\n').encode('ascii')
        # print("XXX:", error_str, "---", len(error_str), file=sys.stderr)
        # import pdb; pdb.set_trace()  # p read()
        e = pickle.loads(b64decode(error_str))
        ret_str = read().rstrip('\n').encode('ascii')
        # print >> sys.stderr, "YYY:", ret_str, "---", len(ret_str)
        ret = pickle.loads(b64decode(ret_str))
        if e:
            if isinstance(e, AnalysisError):
                original_exception = e
                e = openmdao.api.AnalysisError(getattr(e, 'message', getattr(e, 'args', ['unknown MATLAB exception'])[0]))
                e.matlab_proxy_stdout = getattr(original_exception, "matlab_proxy_stdout", "")
                e.matlab_proxy_stderr = getattr(original_exception, "matlab_proxy_stderr", "")
            raise e
        return ret

    class Proxy(object):
        def addpath(self, *args, **kwargs):
            return dispatch('addpath', *args, **kwargs)

        def invoke(self, *args, **kwargs):
            return dispatch('invoke', *args, **kwargs)

        def quit(self, *args, **kwargs):
            return dispatch('quit', *args, **kwargs)

    eng = EngineProxyClient(Proxy())

    return eng


def get_preferred_matlab():
    """Return a 3-tuple (arch, version, MATLABROOT) of the latest MATLAB found in the registry."""
    import six.moves.winreg as winreg

    def get_latest_matlab(reg_wow64):
        try:
            matlab = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r'SOFTWARE\MathWorks\MATLAB', 0, winreg.KEY_READ | reg_wow64)
        except WindowsError as e:
            if e.winerror != 2:
                raise
            return (MatlabVersion(None), None)
        with matlab:
            matlab_versions = []
            try:
                for i in six.moves.range(1000):
                    matlab_versions.append(MatlabVersion(winreg.EnumKey(matlab, i)))
            except WindowsError as e:
                if e.winerror != 259:
                    raise
            matlab_versions.sort()

            matlab_version = matlab_versions[-1]
            with winreg.OpenKey(matlab, matlab_version.version, 0, winreg.KEY_READ | reg_wow64) as matlab_version_key:
                try:
                    value, type_ = winreg.QueryValueEx(matlab_version_key, 'MATLABROOT')
                except WindowsError as e:
                    if e.winerror != 2:
                        raise
                else:
                    if type_ in (winreg.REG_SZ, winreg.REG_EXPAND_SZ):
                        return (matlab_version, value)

    m64 = get_latest_matlab(winreg.KEY_WOW64_64KEY)
    m32 = get_latest_matlab(winreg.KEY_WOW64_32KEY)
    if m64[0].version is None and m32[0].version is None:
        return None
    if m32[0].version is None or m64[0] > m32[0] or (m64[0] == m32[0] and platform.architecture()[0] == '64bit'):
        return ('64bit', m64[0].version, m64[1])
    else:
        return ('32bit', m32[0].version, m32[1])


@total_ordering
class MatlabVersion:
    def __init__(self, version):
        self.version = version

    def __repr__(self):
        return 'MatlabVersion(' + repr(self.version) + ')'

    def __eq__(self, other):
        return self.version == other.version

    def __lt__(self, other):
        a = self.version
        b = other.version

        if a is None and b is None:
            return False
        if a is None:
            return True
        if b is None:
            return False
        # e.g. R2016a: 9.0
        # R2015aSP1: 8.5.1
        a_match = re.match('(\\d+)\\.(\\d+)(?:\\.(\\d+))?', a)
        b_match = re.match('(\\d+)\\.(\\d+)(?:\\.(\\d+))?', b)
        if a_match is None and b_match is None:
            return a < b
        if a_match is None:
            return True
        if b_match is None:
            return False
        return [int(x) for x in a_match.groups(0)] < \
               [int(x) for x in b_match.groups(0)]


def import_matlab_python_engine(MATLABROOT):
    engine = sys.modules.get('matlab.engine')
    if engine:
        return engine
    with matlab_in_env_path(MATLABROOT, platform.architecture()[0]):
        win_bit = {'32bit': 'win32',
            '64bit': 'win64'}[platform.architecture()[0]]

        sys.path.insert(0, r"{}\extern\engines\python\dist\matlab\engine\{}".format(MATLABROOT, win_bit))
        sys.path.insert(1, r"{}\extern\engines\python\dist".format(MATLABROOT))

        try:
            import shlex  # needed by matlab.engine.matlabfuture; import here so py2exe sees it
            import importlib
            os.environ['MWE_INSTALL'] = MATLABROOT
            # importlib.import_module('matlabengineforpython{}'.format('_'.join(map(str, sys.version_info[0:2]))))
            import matlab.engine
            import matlab
            return matlab.engine
        finally:
            del sys.path[:2]

HANDSHAKE_MAGIC = 'matlab_\bproxy'

def main():
    import argparse

    parser = argparse.ArgumentParser()
    parser.add_argument('MATLABROOT')
    args = parser.parse_args()

    if sys.platform == "win32" and six.PY2:
        import msvcrt
        msvcrt.setmode(sys.stdout.fileno(), os.O_BINARY)

    MATLABROOT = args.MATLABROOT

    engine = EngineProxyServer(import_matlab_python_engine(MATLABROOT).start_matlab())

    if DEBUG:
        debug_log = open('matlab_proxy_debug.log', 'wt')
        def debug(line):
            debug_log.write(line + '\n')
            debug_log.flush()
    else:
        def debug(line):
            pass

    def write(output):
        sys.stdout.write(output)
        sys.stdout.flush()
        if DEBUG:
            debug('write {output!r}'.format(output=output))

    def read():
        line = sys.stdin.readline()
        if DEBUG:
            debug('read {line!r}'.format(line=line))
        return line

    write(HANDSHAKE_MAGIC + '\n')

    while True:
        # debug = open('method.txt', 'wb')
        # while True:
        #    debug.write(read())
        #    debug.flush()
        method = read().rstrip('\n')
        args = read().rstrip('\n').encode('ascii')
        kwargs = read().rstrip('\n').encode('ascii')

        args = pickle.loads(b64decode(args))
        kwargs = pickle.loads(b64decode(kwargs))
        # except binascii.Error:

        e = None
        ret = None
        try:
            ret = getattr(engine, method)(*args, **kwargs)
        except Exception as err:
            e = err
            if os.environ.get('MATLAB_PROXY_DEBUG'):
                import traceback
                traceback.print_exc(100, open('exception{}.txt'.format(method), 'w'))
            # n.b. consumer doesn't have these modules, so create an exception of a different type
            if type(e).__module__ in ('matlab', 'matlab.engine'):
                original_exception = e
                e = AnalysisError(getattr(e, 'message', getattr(e, 'args', ['unknown MATLAB exception'])[0]))
                e.matlab_proxy_stdout = getattr(original_exception, "matlab_proxy_stdout", "")
                e.matlab_proxy_stderr = getattr(original_exception, "matlab_proxy_stderr", "")

        error_str = b64encode(pickle.dumps(e)).decode('ascii')
        write(error_str + '\n')
        # open('exception{}.txt'.format(method), 'w').write(pickle.dumps(ret))
        write(b64encode(pickle.dumps(ret)).decode('ascii') + '\n')
        if method == 'quit':
            break

@contextmanager
def _with_coverage():
    # comment these so py2exe doesn't see it
    # import coverage
    # from datetime import datetime

    timestamp = datetime.timestamp(datetime.now())
    date_time = datetime.fromtimestamp(timestamp)

    cov = coverage.Coverage(data_file='coverage-' + date_time.strftime("%Y%m%d_%H%M%S"))
    cov.load()
    cov.start()

    yield None

    cov.stop()
    cov.save()


if __name__ == '__main__':
    # move __main__.AnalysisError to matlab_proxy.AnalysisError so the client can unpickle it
    # (these few lines are easier than ensuring matlab_proxy can be imported in all cases)
    from types import ModuleType
    matlab_proxy = ModuleType('matlab_proxy')
    matlab_proxy.AnalysisError = AnalysisError
    AnalysisError.__module__ = matlab_proxy.__name__
    AnalysisError = matlab_proxy.AnalysisError
    sys.modules['matlab_proxy'] = matlab_proxy

    # with _with_coverage():
    main()
