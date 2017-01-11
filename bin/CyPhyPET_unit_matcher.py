"""Converts OpenMDAO unit strings to a matching CyPhy unit FCO."""
# sys.path[0:0] = ['C:\\Users\\kevin\\Documents\\meta-tonka\\bin\\Python27\\lib\\site-packages']
import json
import operator
import re
import itertools

import CyPhyPET_unit_setter
import run_mdao.python_component
import run_mdao.python_component.get_params_and_unknowns

import openmdao.units.units
PhysicalQuantity = openmdao.units.units.PhysicalQuantity
PhysicalUnit = openmdao.units.units.PhysicalUnit
_find_unit = openmdao.units.units._find_unit
_UNIT_LIB = openmdao.units.units._UNIT_LIB


def log(s):
    print s


def log_formatted(s):
    print s
try:
    import CyPhyPython  # will fail if not running under CyPhyPython
    import cgi

    def log_formatted(s):
        CyPhyPython.log(s)

    def log(s):
        CyPhyPython.log(cgi.escape(s))
except ImportError:
    pass


def debug_log(s):
    # return
    log(s)


def link(fco):
    gme.ConsoleMessage('<a href="mga:{}">{}</a>'.format(fco.ID, fco.Name), 1)


def start_pdb():
    """Start pdb, the Python debugger, in a console window."""
    import ctypes
    ctypes.windll.kernel32.AllocConsole()
    import sys
    sys.stdout = open('CONOUT$', 'wt')
    sys.stdin = open('CONIN$', 'rt')
    import pdb
    pdb.set_trace()


# from OpenMDAO
def in_base_units(value, unit):
    new_value = value * unit.factor
    num = ''
    denom = ''
    for unit, power in zip(_UNIT_LIB.base_names, unit.powers):
        if power < 0:
            denom = denom + '/' + unit
            if power < -1:
                denom = denom + '**' + str(-power)
        elif power > 0:
            num = num + '*' + unit
            if power > 1:
                num = num + '**' + str(power)

    if len(num) == 0:
        num = '1'
    else:
        num = num[1:]

    if new_value != 1:
        return repr(new_value) + '*' + num + denom
    else:
        return num + denom


def reduce_none(op, items):
    return reduce(op, (item for item in items if item is not None))


def get_unit_for_gme(fco, exponent=1):
    if fco.MetaBase.Name == 'si_unit':
        sym = fco.GetStrAttrByNameDisp('Symbol')
        if sym == 'U':
            return None
            # n.b. ignore exponent
            return PhysicalUnit({}, 1.0, [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], 0.0)
        return _find_unit(str(sym)) ** exponent
    if fco.GetStrAttrByNameDisp('Symbol') in ('degF', 'degC'):
        return _find_unit(str(fco.GetStrAttrByNameDisp('Symbol')))
    if fco.MetaBase.Name == 'derived_unit':
        for exponent in (ref.GetFloatAttrByNameDisp('exponent') for ref in fco.ChildFCOs):
            if int(exponent) != exponent:
                raise ValueError()
        return reduce_none(operator.mul, (get_unit_for_gme(ref.Referred, int(ref.GetFloatAttrByNameDisp('exponent'))) for ref in fco.ChildFCOs))
    # log('xxx' + fco.MetaBase.Name)
    if fco.MetaBase.Name == 'conversion_based_unit':
        return reduce_none(operator.mul, (get_unit_for_gme(ref.Referred) * ref.GetFloatAttrByNameDisp('conversion_factor') for ref in fco.ChildFCOs)) ** exponent
    raise ValueError(fco.MetaBase.Name)


def isclose(a, b, rel_tol=1e-09, abs_tol=0.0):
    return abs(a-b) <= max(rel_tol * max(abs(a), abs(b)), abs_tol)


def unit_eq(self, other):
    return isclose(self.factor, other.factor) and \
           self.offset == other.offset and \
           self.powers == other.powers


def convert_unit_symbol(symbol):
    symbol = re.sub('-(?!\\d)', '*', symbol)
    symbol = symbol.replace('^', '**')
    symbol = symbol.replace(u'\u00b5', 'u')
    symbol = re.sub(' */ *', '/', symbol)
    symbol = re.sub(' +', '*', symbol)
    symbol = re.sub('\\bin\\b', 'inch', symbol)
    symbol = re.sub('\\byd\\b', 'yard', symbol)
    # TODO acre?
    # TODO metric tonne?
    return symbol.encode('ascii', 'replace')


# This is the entry point
def invoke(focusObject, rootObject, componentParameters, udmProject, **kwargs):
    mga_project = focusObject.convert_udm2gme().Project

    c = run_mdao.python_component.PythonComponent(componentParameters['openmdao_py'])

    all_units = []

    def set_units(symbol):
        if symbol is None or symbol == '':
            return
        all_units.append((_find_unit(symbol), unit_fco.ID))
    for unit_fco in CyPhyPET_unit_setter.get_all_unit_fcos(mga_project):
        if unit_fco.Name in ('Pebi', 'Tebi', 'Zebi', 'Exbi', 'Kibi', 'Yotta', 'Yobi', 'Mebi', 'Gibi'):
            continue
        if unit_fco.GetStrAttrByNameDisp('Symbol') in ('U',):
            continue
        CyPhyPET_unit_setter.set_unit(unit_fco, set_units)

    for param_name, metadata in itertools.chain(c._init_params_dict.iteritems(), c._init_unknowns_dict.iteritems()):
        unit_expr = metadata.get('units')
        if unit_expr is None:
            continue
        # debug_log(repr(unit_expr))
        openmdao_unit = _find_unit(unit_expr)

        for unit, fco_id in all_units:
            if unit == openmdao_unit:
                metadata['gme_unit_id'] = fco_id
                break
        # else: TODO create a new unit

    componentParameters['ret'] = json.dumps({'params': c._init_params_dict, 'unknowns': c._init_unknowns_dict},
        default=run_mdao.python_component.get_params_and_unknowns.json_default)
    # import cgi
    # debug_log(cgi.escape(componentParameters['ret']))
