# .\_systemc.py
# -*- coding: utf-8 -*-
# PyXB bindings for NM:acabe3c8394de3f41da11a8fb34cb58c8e1b3a5a
# Generated 2023-01-23 16:19:49.077000 by PyXB version 1.2.3
# Namespace systemc [xmlns:systemc]

import pyxb
import pyxb.binding
import pyxb.binding.saxer
import io
import pyxb.utils.utility
import pyxb.utils.domutils
import sys

# Unique identifier for bindings created at the same time
_GenerationUID = pyxb.utils.utility.UniqueIdentifier('urn:uuid:0c832d0f-9b6c-11ed-be80-415645000030')

# Version of PyXB used to generate the bindings
_PyXBVersion = '1.2.3'
# Generated bindings are not compatible across PyXB versions
if pyxb.__version__ != _PyXBVersion:
    raise pyxb.PyXBVersionError(_PyXBVersion)

# Import bindings for namespaces imported into schema
import avm as _ImportedBinding__avm
import pyxb.binding.datatypes

# NOTE: All namespace declarations are reserved within the binding
Namespace = pyxb.namespace.NamespaceForURI(u'systemc', create_if_missing=True)
Namespace.configureCategories(['typeBinding', 'elementBinding'])

def CreateFromDocument (xml_text, default_namespace=None, location_base=None):
    """Parse the given XML and use the document element to create a
    Python instance.

    @param xml_text An XML document.  This should be data (Python 2
    str or Python 3 bytes), or a text (Python 2 unicode or Python 3
    str) in the L{pyxb._InputEncoding} encoding.

    @keyword default_namespace The L{pyxb.Namespace} instance to use as the
    default namespace where there is no default namespace in scope.
    If unspecified or C{None}, the namespace of the module containing
    this function will be used.

    @keyword location_base: An object to be recorded as the base of all
    L{pyxb.utils.utility.Location} instances associated with events and
    objects handled by the parser.  You might pass the URI from which
    the document was obtained.
    """

    if pyxb.XMLStyle_saxer != pyxb._XMLStyle:
        dom = pyxb.utils.domutils.StringToDOM(xml_text)
        return CreateFromDOM(dom.documentElement)
    if default_namespace is None:
        default_namespace = Namespace.fallbackNamespace()
    saxer = pyxb.binding.saxer.make_parser(fallback_namespace=default_namespace, location_base=location_base)
    handler = saxer.getContentHandler()
    xmld = xml_text
    if isinstance(xmld, unicode):
        xmld = xmld.encode(pyxb._InputEncoding)
    saxer.parse(io.BytesIO(xmld))
    instance = handler.rootObject()
    return instance

def CreateFromDOM (node, default_namespace=None):
    """Create a Python instance from the given DOM node.
    The node tag must correspond to an element declaration in this module.

    @deprecated: Forcing use of DOM interface is unnecessary; use L{CreateFromDocument}."""
    if default_namespace is None:
        default_namespace = Namespace.fallbackNamespace()
    return pyxb.binding.basis.element.AnyCreateFromDOM(node, default_namespace)


# Atomic simple type: {systemc}SystemCDataTypeEnum
class SystemCDataTypeEnum (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'SystemCDataTypeEnum')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 39, 2)
    _Documentation = None
SystemCDataTypeEnum._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=SystemCDataTypeEnum, enum_prefix=None)
SystemCDataTypeEnum.bool = SystemCDataTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'bool', tag=u'bool')
SystemCDataTypeEnum.sc_int = SystemCDataTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'sc_int', tag=u'sc_int')
SystemCDataTypeEnum.sc_uint = SystemCDataTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'sc_uint', tag=u'sc_uint')
SystemCDataTypeEnum.sc_logic = SystemCDataTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'sc_logic', tag=u'sc_logic')
SystemCDataTypeEnum.sc_bit = SystemCDataTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'sc_bit', tag=u'sc_bit')
SystemCDataTypeEnum._InitializeFacetMap(SystemCDataTypeEnum._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'SystemCDataTypeEnum', SystemCDataTypeEnum)

# Atomic simple type: {systemc}DirectionalityEnum
class DirectionalityEnum (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'DirectionalityEnum')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 48, 2)
    _Documentation = None
DirectionalityEnum._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=DirectionalityEnum, enum_prefix=None)
DirectionalityEnum.in_ = DirectionalityEnum._CF_enumeration.addEnumeration(unicode_value=u'in', tag=u'in_')
DirectionalityEnum.out = DirectionalityEnum._CF_enumeration.addEnumeration(unicode_value=u'out', tag=u'out')
DirectionalityEnum.inout = DirectionalityEnum._CF_enumeration.addEnumeration(unicode_value=u'inout', tag=u'inout')
DirectionalityEnum.not_applicable = DirectionalityEnum._CF_enumeration.addEnumeration(unicode_value=u'not_applicable', tag=u'not_applicable')
DirectionalityEnum._InitializeFacetMap(DirectionalityEnum._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'DirectionalityEnum', DirectionalityEnum)

# Atomic simple type: {systemc}FunctionEnum
class FunctionEnum (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'FunctionEnum')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 56, 2)
    _Documentation = None
FunctionEnum._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=FunctionEnum, enum_prefix=None)
FunctionEnum.normal = FunctionEnum._CF_enumeration.addEnumeration(unicode_value=u'normal', tag=u'normal')
FunctionEnum.clock = FunctionEnum._CF_enumeration.addEnumeration(unicode_value=u'clock', tag=u'clock')
FunctionEnum.reset_async = FunctionEnum._CF_enumeration.addEnumeration(unicode_value=u'reset_async', tag=u'reset_async')
FunctionEnum.reset_sync = FunctionEnum._CF_enumeration.addEnumeration(unicode_value=u'reset_sync', tag=u'reset_sync')
FunctionEnum._InitializeFacetMap(FunctionEnum._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'FunctionEnum', FunctionEnum)

# Complex type {systemc}SystemCModel with content type ELEMENT_ONLY
class SystemCModel_ (_ImportedBinding__avm.DomainModel_):
    """Complex type {systemc}SystemCModel with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'SystemCModel')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 7, 2)
    _ElementMap = _ImportedBinding__avm.DomainModel_._ElementMap.copy()
    _AttributeMap = _ImportedBinding__avm.DomainModel_._AttributeMap.copy()
    # Base type is _ImportedBinding__avm.DomainModel_
    
    # Element SystemCPort uses Python identifier SystemCPort
    __SystemCPort = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'SystemCPort'), 'SystemCPort', '__systemc_SystemCModel__SystemCPort', True, pyxb.utils.utility.Location(u'avm.systemc.xsd', 11, 10), )

    
    SystemCPort = property(__SystemCPort.value, __SystemCPort.set, None, None)

    
    # Element Parameter uses Python identifier Parameter
    __Parameter = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Parameter'), 'Parameter', '__systemc_SystemCModel__Parameter', True, pyxb.utils.utility.Location(u'avm.systemc.xsd', 12, 10), )

    
    Parameter = property(__Parameter.value, __Parameter.set, None, None)

    
    # Attribute UsesResource inherited from {avm}DomainModel
    
    # Attribute Author inherited from {avm}DomainModel
    
    # Attribute Notes inherited from {avm}DomainModel
    
    # Attribute XPosition inherited from {avm}DomainModel
    
    # Attribute YPosition inherited from {avm}DomainModel
    
    # Attribute Name inherited from {avm}DomainModel
    
    # Attribute ID inherited from {avm}DomainModel
    
    # Attribute ModuleName uses Python identifier ModuleName
    __ModuleName = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ModuleName'), 'ModuleName', '__systemc_SystemCModel__ModuleName', pyxb.binding.datatypes.string, required=True)
    __ModuleName._DeclarationLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 14, 8)
    __ModuleName._UseLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 14, 8)
    
    ModuleName = property(__ModuleName.value, __ModuleName.set, None, None)

    _ElementMap.update({
        __SystemCPort.name() : __SystemCPort,
        __Parameter.name() : __Parameter
    })
    _AttributeMap.update({
        __ModuleName.name() : __ModuleName
    })
Namespace.addCategoryObject('typeBinding', u'SystemCModel', SystemCModel_)


# Complex type {systemc}Parameter with content type ELEMENT_ONLY
class Parameter_ (_ImportedBinding__avm.DomainModelParameter_):
    """Complex type {systemc}Parameter with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Parameter')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 18, 2)
    _ElementMap = _ImportedBinding__avm.DomainModelParameter_._ElementMap.copy()
    _AttributeMap = _ImportedBinding__avm.DomainModelParameter_._AttributeMap.copy()
    # Base type is _ImportedBinding__avm.DomainModelParameter_
    
    # Element Value uses Python identifier Value
    __Value = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Value'), 'Value', '__systemc_Parameter__Value', False, pyxb.utils.utility.Location(u'avm.systemc.xsd', 22, 10), )

    
    Value = property(__Value.value, __Value.set, None, None)

    
    # Attribute YPosition inherited from {avm}DomainModelParameter
    
    # Attribute Notes inherited from {avm}DomainModelParameter
    
    # Attribute XPosition inherited from {avm}DomainModelParameter
    
    # Attribute ParamName uses Python identifier ParamName
    __ParamName = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ParamName'), 'ParamName', '__systemc_Parameter__ParamName', pyxb.binding.datatypes.string)
    __ParamName._DeclarationLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 24, 8)
    __ParamName._UseLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 24, 8)
    
    ParamName = property(__ParamName.value, __ParamName.set, None, None)

    
    # Attribute ParamPosition uses Python identifier ParamPosition
    __ParamPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ParamPosition'), 'ParamPosition', '__systemc_Parameter__ParamPosition', pyxb.binding.datatypes.int)
    __ParamPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 25, 8)
    __ParamPosition._UseLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 25, 8)
    
    ParamPosition = property(__ParamPosition.value, __ParamPosition.set, None, None)

    _ElementMap.update({
        __Value.name() : __Value
    })
    _AttributeMap.update({
        __ParamName.name() : __ParamName,
        __ParamPosition.name() : __ParamPosition
    })
Namespace.addCategoryObject('typeBinding', u'Parameter', Parameter_)


# Complex type {systemc}SystemCPort with content type EMPTY
class SystemCPort_ (_ImportedBinding__avm.DomainModelPort_):
    """Complex type {systemc}SystemCPort with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'SystemCPort')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 29, 2)
    _ElementMap = _ImportedBinding__avm.DomainModelPort_._ElementMap.copy()
    _AttributeMap = _ImportedBinding__avm.DomainModelPort_._AttributeMap.copy()
    # Base type is _ImportedBinding__avm.DomainModelPort_
    
    # Attribute Notes inherited from {avm}Port
    
    # Attribute XPosition inherited from {avm}Port
    
    # Attribute Definition inherited from {avm}Port
    
    # Attribute YPosition inherited from {avm}Port
    
    # Attribute Name inherited from {avm}Port
    
    # Attribute ID inherited from {avm}PortMapTarget
    
    # Attribute PortMap inherited from {avm}PortMapTarget
    
    # Attribute DataType uses Python identifier DataType
    __DataType = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'DataType'), 'DataType', '__systemc_SystemCPort__DataType', SystemCDataTypeEnum)
    __DataType._DeclarationLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 32, 8)
    __DataType._UseLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 32, 8)
    
    DataType = property(__DataType.value, __DataType.set, None, None)

    
    # Attribute DataTypeDimension uses Python identifier DataTypeDimension
    __DataTypeDimension = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'DataTypeDimension'), 'DataTypeDimension', '__systemc_SystemCPort__DataTypeDimension', pyxb.binding.datatypes.int)
    __DataTypeDimension._DeclarationLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 33, 8)
    __DataTypeDimension._UseLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 33, 8)
    
    DataTypeDimension = property(__DataTypeDimension.value, __DataTypeDimension.set, None, None)

    
    # Attribute Directionality uses Python identifier Directionality
    __Directionality = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Directionality'), 'Directionality', '__systemc_SystemCPort__Directionality', DirectionalityEnum)
    __Directionality._DeclarationLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 34, 8)
    __Directionality._UseLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 34, 8)
    
    Directionality = property(__Directionality.value, __Directionality.set, None, None)

    
    # Attribute Function uses Python identifier Function
    __Function = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Function'), 'Function', '__systemc_SystemCPort__Function', FunctionEnum)
    __Function._DeclarationLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 35, 8)
    __Function._UseLocation = pyxb.utils.utility.Location(u'avm.systemc.xsd', 35, 8)
    
    Function = property(__Function.value, __Function.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __DataType.name() : __DataType,
        __DataTypeDimension.name() : __DataTypeDimension,
        __Directionality.name() : __Directionality,
        __Function.name() : __Function
    })
Namespace.addCategoryObject('typeBinding', u'SystemCPort', SystemCPort_)


SystemCModel = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'SystemCModel'), SystemCModel_, location=pyxb.utils.utility.Location(u'avm.systemc.xsd', 4, 2))
Namespace.addCategoryObject('elementBinding', SystemCModel.name().localName(), SystemCModel)

Parameter = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Parameter'), Parameter_, location=pyxb.utils.utility.Location(u'avm.systemc.xsd', 5, 2))
Namespace.addCategoryObject('elementBinding', Parameter.name().localName(), Parameter)

SystemCPort = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'SystemCPort'), SystemCPort_, location=pyxb.utils.utility.Location(u'avm.systemc.xsd', 6, 2))
Namespace.addCategoryObject('elementBinding', SystemCPort.name().localName(), SystemCPort)



SystemCModel_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'SystemCPort'), SystemCPort_, scope=SystemCModel_, location=pyxb.utils.utility.Location(u'avm.systemc.xsd', 11, 10)))

SystemCModel_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Parameter'), Parameter_, scope=SystemCModel_, location=pyxb.utils.utility.Location(u'avm.systemc.xsd', 12, 10)))

def _BuildAutomaton ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton
    del _BuildAutomaton
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.systemc.xsd', 11, 10))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.systemc.xsd', 12, 10))
    counters.add(cc_1)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(SystemCModel_._UseForTag(pyxb.namespace.ExpandedName(None, u'SystemCPort')), pyxb.utils.utility.Location(u'avm.systemc.xsd', 11, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(SystemCModel_._UseForTag(pyxb.namespace.ExpandedName(None, u'Parameter')), pyxb.utils.utility.Location(u'avm.systemc.xsd', 12, 10))
    st_1 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_1)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_0, False) ]))
    st_0._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_1, True) ]))
    st_1._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
SystemCModel_._Automaton = _BuildAutomaton()




Parameter_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Value'), _ImportedBinding__avm.Value_, scope=Parameter_, location=pyxb.utils.utility.Location(u'avm.systemc.xsd', 22, 10)))

def _BuildAutomaton_ ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_
    del _BuildAutomaton_
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.systemc.xsd', 22, 10))
    counters.add(cc_0)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(Parameter_._UseForTag(pyxb.namespace.ExpandedName(None, u'Value')), pyxb.utils.utility.Location(u'avm.systemc.xsd', 22, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    st_0._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
Parameter_._Automaton = _BuildAutomaton_()

