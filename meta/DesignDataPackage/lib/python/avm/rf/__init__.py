# .\_rf.py
# -*- coding: utf-8 -*-
# PyXB bindings for NM:22ee0097135072451429b5ac45630d52779cd49b
# Generated 2016-02-15 11:24:52.073000 by PyXB version 1.2.3
# Namespace rf [xmlns:rf]

import pyxb
import pyxb.binding
import pyxb.binding.saxer
import io
import pyxb.utils.utility
import pyxb.utils.domutils
import sys

# Unique identifier for bindings created at the same time
_GenerationUID = pyxb.utils.utility.UniqueIdentifier('urn:uuid:057f9670-d409-11e5-9520-7429af7917c0')

# Version of PyXB used to generate the bindings
_PyXBVersion = '1.2.3'
# Generated bindings are not compatible across PyXB versions
if pyxb.__version__ != _PyXBVersion:
    raise pyxb.PyXBVersionError(_PyXBVersion)

# Import bindings for namespaces imported into schema
import pyxb.binding.datatypes
import avm as _ImportedBinding__avm

# NOTE: All namespace declarations are reserved within the binding
Namespace = pyxb.namespace.NamespaceForURI(u'rf', create_if_missing=True)
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


# Atomic simple type: {rf}RotationEnum
class RotationEnum (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'RotationEnum')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.rf.xsd', 26, 2)
    _Documentation = None
RotationEnum._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=RotationEnum, enum_prefix=None)
RotationEnum.r0 = RotationEnum._CF_enumeration.addEnumeration(unicode_value=u'r0', tag=u'r0')
RotationEnum.n90 = RotationEnum._CF_enumeration.addEnumeration(unicode_value=u'90', tag=u'n90')
RotationEnum.r180 = RotationEnum._CF_enumeration.addEnumeration(unicode_value=u'r180', tag=u'r180')
RotationEnum.r270 = RotationEnum._CF_enumeration.addEnumeration(unicode_value=u'r270', tag=u'r270')
RotationEnum._InitializeFacetMap(RotationEnum._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'RotationEnum', RotationEnum)

# Atomic simple type: {rf}PortDirectionality
class PortDirectionality (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'PortDirectionality')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.rf.xsd', 34, 2)
    _Documentation = None
PortDirectionality._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=PortDirectionality, enum_prefix=None)
PortDirectionality.in_ = PortDirectionality._CF_enumeration.addEnumeration(unicode_value=u'in', tag=u'in_')
PortDirectionality.out = PortDirectionality._CF_enumeration.addEnumeration(unicode_value=u'out', tag=u'out')
PortDirectionality._InitializeFacetMap(PortDirectionality._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'PortDirectionality', PortDirectionality)

# Complex type {rf}RFModel with content type ELEMENT_ONLY
class RFModel_ (_ImportedBinding__avm.DomainModel_):
    """Complex type {rf}RFModel with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'RFModel')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.rf.xsd', 6, 2)
    _ElementMap = _ImportedBinding__avm.DomainModel_._ElementMap.copy()
    _AttributeMap = _ImportedBinding__avm.DomainModel_._AttributeMap.copy()
    # Base type is _ImportedBinding__avm.DomainModel_
    
    # Element RFPort uses Python identifier RFPort
    __RFPort = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'RFPort'), 'RFPort', '__rf_RFModel__RFPort', True, pyxb.utils.utility.Location(u'avm.rf.xsd', 10, 10), )

    
    RFPort = property(__RFPort.value, __RFPort.set, None, None)

    
    # Attribute UsesResource inherited from {avm}DomainModel
    
    # Attribute Author inherited from {avm}DomainModel
    
    # Attribute Notes inherited from {avm}DomainModel
    
    # Attribute XPosition inherited from {avm}DomainModel
    
    # Attribute YPosition inherited from {avm}DomainModel
    
    # Attribute Name inherited from {avm}DomainModel
    
    # Attribute ID inherited from {avm}DomainModel
    
    # Attribute Rotation uses Python identifier Rotation
    __Rotation = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Rotation'), 'Rotation', '__rf_RFModel__Rotation', RotationEnum)
    __Rotation._DeclarationLocation = pyxb.utils.utility.Location(u'avm.rf.xsd', 12, 8)
    __Rotation._UseLocation = pyxb.utils.utility.Location(u'avm.rf.xsd', 12, 8)
    
    Rotation = property(__Rotation.value, __Rotation.set, None, None)

    
    # Attribute X uses Python identifier X
    __X = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'X'), 'X', '__rf_RFModel__X', pyxb.binding.datatypes.float)
    __X._DeclarationLocation = pyxb.utils.utility.Location(u'avm.rf.xsd', 13, 8)
    __X._UseLocation = pyxb.utils.utility.Location(u'avm.rf.xsd', 13, 8)
    
    X = property(__X.value, __X.set, None, None)

    
    # Attribute Y uses Python identifier Y
    __Y = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Y'), 'Y', '__rf_RFModel__Y', pyxb.binding.datatypes.float)
    __Y._DeclarationLocation = pyxb.utils.utility.Location(u'avm.rf.xsd', 14, 8)
    __Y._UseLocation = pyxb.utils.utility.Location(u'avm.rf.xsd', 14, 8)
    
    Y = property(__Y.value, __Y.set, None, None)

    _ElementMap.update({
        __RFPort.name() : __RFPort
    })
    _AttributeMap.update({
        __Rotation.name() : __Rotation,
        __X.name() : __X,
        __Y.name() : __Y
    })
Namespace.addCategoryObject('typeBinding', u'RFModel', RFModel_)


# Complex type {rf}RFPort with content type EMPTY
class RFPort_ (_ImportedBinding__avm.DomainModelPort_):
    """Complex type {rf}RFPort with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'RFPort')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.rf.xsd', 18, 2)
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
    
    # Attribute Directionality uses Python identifier Directionality
    __Directionality = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Directionality'), 'Directionality', '__rf_RFPort__Directionality', PortDirectionality)
    __Directionality._DeclarationLocation = pyxb.utils.utility.Location(u'avm.rf.xsd', 21, 8)
    __Directionality._UseLocation = pyxb.utils.utility.Location(u'avm.rf.xsd', 21, 8)
    
    Directionality = property(__Directionality.value, __Directionality.set, None, None)

    
    # Attribute NominalImpedance uses Python identifier NominalImpedance
    __NominalImpedance = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'NominalImpedance'), 'NominalImpedance', '__rf_RFPort__NominalImpedance', pyxb.binding.datatypes.float)
    __NominalImpedance._DeclarationLocation = pyxb.utils.utility.Location(u'avm.rf.xsd', 22, 8)
    __NominalImpedance._UseLocation = pyxb.utils.utility.Location(u'avm.rf.xsd', 22, 8)
    
    NominalImpedance = property(__NominalImpedance.value, __NominalImpedance.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __Directionality.name() : __Directionality,
        __NominalImpedance.name() : __NominalImpedance
    })
Namespace.addCategoryObject('typeBinding', u'RFPort', RFPort_)


RFModel = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'RFModel'), RFModel_, location=pyxb.utils.utility.Location(u'avm.rf.xsd', 4, 2))
Namespace.addCategoryObject('elementBinding', RFModel.name().localName(), RFModel)

RFPort = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'RFPort'), RFPort_, location=pyxb.utils.utility.Location(u'avm.rf.xsd', 5, 2))
Namespace.addCategoryObject('elementBinding', RFPort.name().localName(), RFPort)



RFModel_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'RFPort'), RFPort_, scope=RFModel_, location=pyxb.utils.utility.Location(u'avm.rf.xsd', 10, 10)))

def _BuildAutomaton ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton
    del _BuildAutomaton
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.rf.xsd', 10, 10))
    counters.add(cc_0)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(RFModel_._UseForTag(pyxb.namespace.ExpandedName(None, u'RFPort')), pyxb.utils.utility.Location(u'avm.rf.xsd', 10, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    st_0._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
RFModel_._Automaton = _BuildAutomaton()

