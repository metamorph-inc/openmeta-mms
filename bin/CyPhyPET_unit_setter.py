# coding: utf-8
import operator
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
    return
    log(s)


def start_pdb():
    """Start pdb, the Python debugger, in a console window."""
    import ctypes
    ctypes.windll.kernel32.AllocConsole()
    import sys
    sys.stdout = open('CONOUT$', 'wt')
    sys.stdin = open('CONIN$', 'rt')
    import pdb
    pdb.set_trace()


# This is the entry point
def invoke(focusObject, rootObject, componentParameters, udmProject, **kwargs):
    if focusObject is not None and focusObject.type.name == "ParameterStudy":
        focusObject = focusObject.parent
    if focusObject is None or focusObject.type.name != "ParametricExploration":
        raise CyPhyPython.ErrorMessageException("Run on ParametricExploration")

    mga_project = focusObject.convert_udm2gme().Project

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

    for index, gme_id in [(key, value) for key, value in componentParameters.iteritems() if key.startswith('unit_id_')]:
        unit_fco = mga_project.GetFCOByID(gme_id)

        def set_units(units):
            componentParameters[index + "_ret"] = units

        import re
        symbol = str(unit_fco.GetStrAttrByNameDisp('Symbol'))
        debug_log(symbol)
        symbol = re.sub('-(?!\\d)', '*', symbol).replace('^', '**')
        symbol = re.sub(' +', '*', symbol)
        debug_log(symbol)
        try:
            gme_unit = get_unit_for_gme(unit_fco)
        except TypeError as e:
            if 'cannot multiply units with non-zero offset' in e.message:
                # FIXME: investigate why .../degC always fails
                set_units('')
            raise
        try:
            sym_unit = _find_unit(symbol)
            if unit_eq(sym_unit, gme_unit):
                debug_log('match ' + unit_fco.Name + ' ' + symbol)
                set_units(symbol)
            else:
                debug_log('mismatch ' + unit_fco.Name + '  ' + repr(gme_unit) + '   ' + repr(sym_unit))
                raise ValueError('mismatch; using GME version')
        except:
            # log('exception ' + derived_unit.Name + '  ' + symbol)
            # import traceback
            # log(traceback.format_exc())
            debug_log(in_base_units(1, gme_unit))
            set_units(in_base_units(1, gme_unit))
