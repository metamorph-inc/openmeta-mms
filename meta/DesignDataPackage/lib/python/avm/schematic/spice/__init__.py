# .\_spice.py
# -*- coding: utf-8 -*-
# PyXB bindings for NM:7c05204dcfaf173d8b1e2cdd5fc900e3eadc4351
# Generated 2023-01-23 16:19:49.078000 by PyXB version 1.2.3
# Namespace spice [xmlns:spice]

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
import avm.schematic as _ImportedBinding__schematic
import pyxb.binding.datatypes

# NOTE: All namespace declarations are reserved within the binding
Namespace = pyxb.namespace.NamespaceForURI(u'spice', create_if_missing=True)
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


# Complex type {spice}Parameter with content type ELEMENT_ONLY
class Parameter_ (_ImportedBinding__avm.DomainModelParameter_):
    """Complex type {spice}Parameter with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Parameter')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.spice.xsd', 17, 2)
    _ElementMap = _ImportedBinding__avm.DomainModelParameter_._ElementMap.copy()
    _AttributeMap = _ImportedBinding__avm.DomainModelParameter_._AttributeMap.copy()
    # Base type is _ImportedBinding__avm.DomainModelParameter_
    
    # Element Value uses Python identifier Value
    __Value = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Value'), 'Value', '__spice_Parameter__Value', False, pyxb.utils.utility.Location(u'avm.schematic.spice.xsd', 21, 10), )

    
    Value = property(__Value.value, __Value.set, None, None)

    
    # Attribute YPosition inherited from {avm}DomainModelParameter
    
    # Attribute Notes inherited from {avm}DomainModelParameter
    
    # Attribute XPosition inherited from {avm}DomainModelParameter
    
    # Attribute Locator uses Python identifier Locator
    __Locator = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Locator'), 'Locator', '__spice_Parameter__Locator', pyxb.binding.datatypes.string, required=True)
    __Locator._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.spice.xsd', 23, 8)
    __Locator._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.spice.xsd', 23, 8)
    
    Locator = property(__Locator.value, __Locator.set, None, None)

    _ElementMap.update({
        __Value.name() : __Value
    })
    _AttributeMap.update({
        __Locator.name() : __Locator
    })
Namespace.addCategoryObject('typeBinding', u'Parameter', Parameter_)


# Complex type {spice}SPICEModel with content type ELEMENT_ONLY
class SPICEModel_ (_ImportedBinding__schematic.SchematicModel_):
    """Complex type {spice}SPICEModel with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'SPICEModel')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.spice.xsd', 7, 2)
    _ElementMap = _ImportedBinding__schematic.SchematicModel_._ElementMap.copy()
    _AttributeMap = _ImportedBinding__schematic.SchematicModel_._AttributeMap.copy()
    # Base type is _ImportedBinding__schematic.SchematicModel_
    
    # Element Pin (Pin) inherited from {schematic}SchematicModel
    
    # Element Parameter uses Python identifier Parameter
    __Parameter = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Parameter'), 'Parameter', '__spice_SPICEModel__Parameter', True, pyxb.utils.utility.Location(u'avm.schematic.spice.xsd', 11, 10), )

    
    Parameter = property(__Parameter.value, __Parameter.set, None, None)

    
    # Attribute UsesResource inherited from {avm}DomainModel
    
    # Attribute Author inherited from {avm}DomainModel
    
    # Attribute Notes inherited from {avm}DomainModel
    
    # Attribute XPosition inherited from {avm}DomainModel
    
    # Attribute YPosition inherited from {avm}DomainModel
    
    # Attribute Name inherited from {avm}DomainModel
    
    # Attribute ID inherited from {avm}DomainModel
    
    # Attribute Class uses Python identifier Class
    __Class = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Class'), 'Class', '__spice_SPICEModel__Class', pyxb.binding.datatypes.string, required=True)
    __Class._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.spice.xsd', 13, 8)
    __Class._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.spice.xsd', 13, 8)
    
    Class = property(__Class.value, __Class.set, None, None)

    _ElementMap.update({
        __Parameter.name() : __Parameter
    })
    _AttributeMap.update({
        __Class.name() : __Class
    })
Namespace.addCategoryObject('typeBinding', u'SPICEModel', SPICEModel_)


Parameter = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Parameter'), Parameter_, location=pyxb.utils.utility.Location(u'avm.schematic.spice.xsd', 6, 2))
Namespace.addCategoryObject('elementBinding', Parameter.name().localName(), Parameter)

SPICEModel = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'SPICEModel'), SPICEModel_, location=pyxb.utils.utility.Location(u'avm.schematic.spice.xsd', 5, 2))
Namespace.addCategoryObject('elementBinding', SPICEModel.name().localName(), SPICEModel)



Parameter_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Value'), _ImportedBinding__avm.Value_, scope=Parameter_, location=pyxb.utils.utility.Location(u'avm.schematic.spice.xsd', 21, 10)))

def _BuildAutomaton ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton
    del _BuildAutomaton
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.schematic.spice.xsd', 21, 10))
    counters.add(cc_0)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(Parameter_._UseForTag(pyxb.namespace.ExpandedName(None, u'Value')), pyxb.utils.utility.Location(u'avm.schematic.spice.xsd', 21, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    st_0._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
Parameter_._Automaton = _BuildAutomaton()




SPICEModel_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Parameter'), Parameter_, scope=SPICEModel_, location=pyxb.utils.utility.Location(u'avm.schematic.spice.xsd', 11, 10)))

def _BuildAutomaton_ ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_
    del _BuildAutomaton_
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.schematic.xsd', 10, 10))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.schematic.spice.xsd', 11, 10))
    counters.add(cc_1)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(SPICEModel_._UseForTag(pyxb.namespace.ExpandedName(None, u'Pin')), pyxb.utils.utility.Location(u'avm.schematic.xsd', 10, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(SPICEModel_._UseForTag(pyxb.namespace.ExpandedName(None, u'Parameter')), pyxb.utils.utility.Location(u'avm.schematic.spice.xsd', 11, 10))
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
SPICEModel_._Automaton = _BuildAutomaton_()

