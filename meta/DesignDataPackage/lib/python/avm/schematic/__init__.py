# .\_schematic.py
# -*- coding: utf-8 -*-
# PyXB bindings for NM:2b86b09e6504617c4541a8a2f53a65ea784d5722
# Generated 2023-01-23 16:19:49.072000 by PyXB version 1.2.3
# Namespace schematic [xmlns:schematic]

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
Namespace = pyxb.namespace.NamespaceForURI(u'schematic', create_if_missing=True)
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


# Complex type {schematic}SchematicModel with content type ELEMENT_ONLY
class SchematicModel_ (_ImportedBinding__avm.DomainModel_):
    """Complex type {schematic}SchematicModel with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'SchematicModel')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.xsd', 6, 2)
    _ElementMap = _ImportedBinding__avm.DomainModel_._ElementMap.copy()
    _AttributeMap = _ImportedBinding__avm.DomainModel_._AttributeMap.copy()
    # Base type is _ImportedBinding__avm.DomainModel_
    
    # Element Pin uses Python identifier Pin
    __Pin = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Pin'), 'Pin', '__schematic_SchematicModel__Pin', True, pyxb.utils.utility.Location(u'avm.schematic.xsd', 10, 10), )

    
    Pin = property(__Pin.value, __Pin.set, None, None)

    
    # Attribute UsesResource inherited from {avm}DomainModel
    
    # Attribute Author inherited from {avm}DomainModel
    
    # Attribute Notes inherited from {avm}DomainModel
    
    # Attribute XPosition inherited from {avm}DomainModel
    
    # Attribute YPosition inherited from {avm}DomainModel
    
    # Attribute Name inherited from {avm}DomainModel
    
    # Attribute ID inherited from {avm}DomainModel
    _ElementMap.update({
        __Pin.name() : __Pin
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'SchematicModel', SchematicModel_)


# Complex type {schematic}Pin with content type EMPTY
class Pin_ (_ImportedBinding__avm.DomainModelPort_):
    """Complex type {schematic}Pin with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Pin')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.xsd', 15, 2)
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
    
    # Attribute EDAGate uses Python identifier EDAGate
    __EDAGate = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'EDAGate'), 'EDAGate', '__schematic_Pin__EDAGate', pyxb.binding.datatypes.string)
    __EDAGate._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.xsd', 18, 8)
    __EDAGate._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.xsd', 18, 8)
    
    EDAGate = property(__EDAGate.value, __EDAGate.set, None, None)

    
    # Attribute EDASymbolLocationX uses Python identifier EDASymbolLocationX
    __EDASymbolLocationX = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'EDASymbolLocationX'), 'EDASymbolLocationX', '__schematic_Pin__EDASymbolLocationX', pyxb.binding.datatypes.string)
    __EDASymbolLocationX._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.xsd', 19, 8)
    __EDASymbolLocationX._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.xsd', 19, 8)
    
    EDASymbolLocationX = property(__EDASymbolLocationX.value, __EDASymbolLocationX.set, None, None)

    
    # Attribute EDASymbolLocationY uses Python identifier EDASymbolLocationY
    __EDASymbolLocationY = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'EDASymbolLocationY'), 'EDASymbolLocationY', '__schematic_Pin__EDASymbolLocationY', pyxb.binding.datatypes.string)
    __EDASymbolLocationY._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.xsd', 20, 8)
    __EDASymbolLocationY._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.xsd', 20, 8)
    
    EDASymbolLocationY = property(__EDASymbolLocationY.value, __EDASymbolLocationY.set, None, None)

    
    # Attribute EDASymbolRotation uses Python identifier EDASymbolRotation
    __EDASymbolRotation = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'EDASymbolRotation'), 'EDASymbolRotation', '__schematic_Pin__EDASymbolRotation', pyxb.binding.datatypes.string)
    __EDASymbolRotation._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.xsd', 21, 8)
    __EDASymbolRotation._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.xsd', 21, 8)
    
    EDASymbolRotation = property(__EDASymbolRotation.value, __EDASymbolRotation.set, None, None)

    
    # Attribute SPICEPortNumber uses Python identifier SPICEPortNumber
    __SPICEPortNumber = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'SPICEPortNumber'), 'SPICEPortNumber', '__schematic_Pin__SPICEPortNumber', pyxb.binding.datatypes.unsignedInt)
    __SPICEPortNumber._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.xsd', 22, 8)
    __SPICEPortNumber._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.xsd', 22, 8)
    
    SPICEPortNumber = property(__SPICEPortNumber.value, __SPICEPortNumber.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __EDAGate.name() : __EDAGate,
        __EDASymbolLocationX.name() : __EDASymbolLocationX,
        __EDASymbolLocationY.name() : __EDASymbolLocationY,
        __EDASymbolRotation.name() : __EDASymbolRotation,
        __SPICEPortNumber.name() : __SPICEPortNumber
    })
Namespace.addCategoryObject('typeBinding', u'Pin', Pin_)


SchematicModel = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'SchematicModel'), SchematicModel_, location=pyxb.utils.utility.Location(u'avm.schematic.xsd', 4, 2))
Namespace.addCategoryObject('elementBinding', SchematicModel.name().localName(), SchematicModel)

Pin = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Pin'), Pin_, location=pyxb.utils.utility.Location(u'avm.schematic.xsd', 5, 2))
Namespace.addCategoryObject('elementBinding', Pin.name().localName(), Pin)



SchematicModel_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Pin'), Pin_, scope=SchematicModel_, location=pyxb.utils.utility.Location(u'avm.schematic.xsd', 10, 10)))

def _BuildAutomaton ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton
    del _BuildAutomaton
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.schematic.xsd', 10, 10))
    counters.add(cc_0)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(SchematicModel_._UseForTag(pyxb.namespace.ExpandedName(None, u'Pin')), pyxb.utils.utility.Location(u'avm.schematic.xsd', 10, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    st_0._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
SchematicModel_._Automaton = _BuildAutomaton()

