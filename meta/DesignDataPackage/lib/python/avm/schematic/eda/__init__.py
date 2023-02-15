# .\_eda.py
# -*- coding: utf-8 -*-
# PyXB bindings for NM:9f5b2e4c02a063822535af58fedb94550ecc79cc
# Generated 2023-02-15 11:25:44.113000 by PyXB version 1.2.3
# Namespace eda [xmlns:eda]

import pyxb
import pyxb.binding
import pyxb.binding.saxer
import io
import pyxb.utils.utility
import pyxb.utils.domutils
import sys

# Unique identifier for bindings created at the same time
_GenerationUID = pyxb.utils.utility.UniqueIdentifier('urn:uuid:c6a1c7b0-ad55-11ed-a747-50e085b81351')

# Version of PyXB used to generate the bindings
_PyXBVersion = '1.2.3'
# Generated bindings are not compatible across PyXB versions
if pyxb.__version__ != _PyXBVersion:
    raise pyxb.PyXBVersionError(_PyXBVersion)

# Import bindings for namespaces imported into schema
import pyxb.binding.datatypes
import avm as _ImportedBinding__avm
import avm.schematic as _ImportedBinding__schematic

# NOTE: All namespace declarations are reserved within the binding
Namespace = pyxb.namespace.NamespaceForURI(u'eda', create_if_missing=True)
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


# List simple type: [anonymous]
# superclasses pyxb.binding.datatypes.anySimpleType
class STD_ANON (pyxb.binding.basis.STD_list):

    """Simple type that is a list of pyxb.binding.datatypes.anyURI."""

    _ExpandedName = None
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 55, 10)
    _Documentation = None

    _ItemType = pyxb.binding.datatypes.anyURI
STD_ANON._InitializeFacetMap()

# List simple type: [anonymous]
# superclasses pyxb.binding.datatypes.anySimpleType
class STD_ANON_ (pyxb.binding.basis.STD_list):

    """Simple type that is a list of pyxb.binding.datatypes.anyURI."""

    _ExpandedName = None
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 60, 10)
    _Documentation = None

    _ItemType = pyxb.binding.datatypes.anyURI
STD_ANON_._InitializeFacetMap()

# List simple type: [anonymous]
# superclasses pyxb.binding.datatypes.anySimpleType
class STD_ANON_2 (pyxb.binding.basis.STD_list):

    """Simple type that is a list of pyxb.binding.datatypes.anyURI."""

    _ExpandedName = None
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 76, 10)
    _Documentation = None

    _ItemType = pyxb.binding.datatypes.anyURI
STD_ANON_2._InitializeFacetMap()

# List simple type: [anonymous]
# superclasses pyxb.binding.datatypes.anySimpleType
class STD_ANON_3 (pyxb.binding.basis.STD_list):

    """Simple type that is a list of pyxb.binding.datatypes.anyURI."""

    _ExpandedName = None
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 81, 10)
    _Documentation = None

    _ItemType = pyxb.binding.datatypes.anyURI
STD_ANON_3._InitializeFacetMap()

# List simple type: [anonymous]
# superclasses pyxb.binding.datatypes.anySimpleType
class STD_ANON_4 (pyxb.binding.basis.STD_list):

    """Simple type that is a list of pyxb.binding.datatypes.anyURI."""

    _ExpandedName = None
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 95, 10)
    _Documentation = None

    _ItemType = pyxb.binding.datatypes.anyURI
STD_ANON_4._InitializeFacetMap()

# List simple type: [anonymous]
# superclasses pyxb.binding.datatypes.anySimpleType
class STD_ANON_5 (pyxb.binding.basis.STD_list):

    """Simple type that is a list of pyxb.binding.datatypes.anyURI."""

    _ExpandedName = None
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 101, 10)
    _Documentation = None

    _ItemType = pyxb.binding.datatypes.anyURI
STD_ANON_5._InitializeFacetMap()

# Atomic simple type: {eda}RotationEnum
class RotationEnum (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'RotationEnum')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 110, 2)
    _Documentation = None
RotationEnum._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=RotationEnum, enum_prefix=None)
RotationEnum.r0 = RotationEnum._CF_enumeration.addEnumeration(unicode_value=u'r0', tag=u'r0')
RotationEnum.r90 = RotationEnum._CF_enumeration.addEnumeration(unicode_value=u'r90', tag=u'r90')
RotationEnum.r180 = RotationEnum._CF_enumeration.addEnumeration(unicode_value=u'r180', tag=u'r180')
RotationEnum.r270 = RotationEnum._CF_enumeration.addEnumeration(unicode_value=u'r270', tag=u'r270')
RotationEnum._InitializeFacetMap(RotationEnum._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'RotationEnum', RotationEnum)

# Atomic simple type: {eda}LayerEnum
class LayerEnum (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'LayerEnum')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 118, 2)
    _Documentation = None
LayerEnum._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=LayerEnum, enum_prefix=None)
LayerEnum.Top = LayerEnum._CF_enumeration.addEnumeration(unicode_value=u'Top', tag=u'Top')
LayerEnum.Bottom = LayerEnum._CF_enumeration.addEnumeration(unicode_value=u'Bottom', tag=u'Bottom')
LayerEnum._InitializeFacetMap(LayerEnum._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'LayerEnum', LayerEnum)

# Atomic simple type: {eda}LayerRangeEnum
class LayerRangeEnum (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'LayerRangeEnum')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 124, 2)
    _Documentation = None
LayerRangeEnum._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=LayerRangeEnum, enum_prefix=None)
LayerRangeEnum.Either = LayerRangeEnum._CF_enumeration.addEnumeration(unicode_value=u'Either', tag=u'Either')
LayerRangeEnum.Top = LayerRangeEnum._CF_enumeration.addEnumeration(unicode_value=u'Top', tag=u'Top')
LayerRangeEnum.Bottom = LayerRangeEnum._CF_enumeration.addEnumeration(unicode_value=u'Bottom', tag=u'Bottom')
LayerRangeEnum._InitializeFacetMap(LayerRangeEnum._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'LayerRangeEnum', LayerRangeEnum)

# Atomic simple type: {eda}RelativeLayerEnum
class RelativeLayerEnum (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'RelativeLayerEnum')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 131, 2)
    _Documentation = None
RelativeLayerEnum._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=RelativeLayerEnum, enum_prefix=None)
RelativeLayerEnum.Same = RelativeLayerEnum._CF_enumeration.addEnumeration(unicode_value=u'Same', tag=u'Same')
RelativeLayerEnum.Opposite = RelativeLayerEnum._CF_enumeration.addEnumeration(unicode_value=u'Opposite', tag=u'Opposite')
RelativeLayerEnum._InitializeFacetMap(RelativeLayerEnum._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'RelativeLayerEnum', RelativeLayerEnum)

# Atomic simple type: {eda}RangeConstraintTypeEnum
class RangeConstraintTypeEnum (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'RangeConstraintTypeEnum')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 137, 2)
    _Documentation = None
RangeConstraintTypeEnum._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=RangeConstraintTypeEnum, enum_prefix=None)
RangeConstraintTypeEnum.Inclusion = RangeConstraintTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'Inclusion', tag=u'Inclusion')
RangeConstraintTypeEnum.Exclusion = RangeConstraintTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'Exclusion', tag=u'Exclusion')
RangeConstraintTypeEnum._InitializeFacetMap(RangeConstraintTypeEnum._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'RangeConstraintTypeEnum', RangeConstraintTypeEnum)

# List simple type: [anonymous]
# superclasses pyxb.binding.datatypes.anySimpleType
class STD_ANON_6 (pyxb.binding.basis.STD_list):

    """Simple type that is a list of pyxb.binding.datatypes.anyURI."""

    _ExpandedName = None
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 152, 10)
    _Documentation = None

    _ItemType = pyxb.binding.datatypes.anyURI
STD_ANON_6._InitializeFacetMap()

# List simple type: [anonymous]
# superclasses pyxb.binding.datatypes.anySimpleType
class STD_ANON_7 (pyxb.binding.basis.STD_list):

    """Simple type that is a list of pyxb.binding.datatypes.anyURI."""

    _ExpandedName = None
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 158, 10)
    _Documentation = None

    _ItemType = pyxb.binding.datatypes.anyURI
STD_ANON_7._InitializeFacetMap()

# List simple type: [anonymous]
# superclasses pyxb.binding.datatypes.anySimpleType
class STD_ANON_8 (pyxb.binding.basis.STD_list):

    """Simple type that is a list of pyxb.binding.datatypes.anyURI."""

    _ExpandedName = None
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 169, 10)
    _Documentation = None

    _ItemType = pyxb.binding.datatypes.anyURI
STD_ANON_8._InitializeFacetMap()

# List simple type: [anonymous]
# superclasses pyxb.binding.datatypes.anySimpleType
class STD_ANON_9 (pyxb.binding.basis.STD_list):

    """Simple type that is a list of pyxb.binding.datatypes.anyURI."""

    _ExpandedName = None
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 174, 10)
    _Documentation = None

    _ItemType = pyxb.binding.datatypes.anyURI
STD_ANON_9._InitializeFacetMap()

# Atomic simple type: {eda}GlobalConstraintTypeEnum
class GlobalConstraintTypeEnum (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'GlobalConstraintTypeEnum')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 182, 2)
    _Documentation = None
GlobalConstraintTypeEnum._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=GlobalConstraintTypeEnum, enum_prefix=None)
GlobalConstraintTypeEnum.BoardEdgeSpacing = GlobalConstraintTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'BoardEdgeSpacing', tag=u'BoardEdgeSpacing')
GlobalConstraintTypeEnum._InitializeFacetMap(GlobalConstraintTypeEnum._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'GlobalConstraintTypeEnum', GlobalConstraintTypeEnum)

# Atomic simple type: {eda}RelativeRotationEnum
class RelativeRotationEnum (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'RelativeRotationEnum')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 187, 2)
    _Documentation = None
RelativeRotationEnum._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=RelativeRotationEnum, enum_prefix=None)
RelativeRotationEnum.r0 = RelativeRotationEnum._CF_enumeration.addEnumeration(unicode_value=u'r0', tag=u'r0')
RelativeRotationEnum.r90 = RelativeRotationEnum._CF_enumeration.addEnumeration(unicode_value=u'r90', tag=u'r90')
RelativeRotationEnum.r180 = RelativeRotationEnum._CF_enumeration.addEnumeration(unicode_value=u'r180', tag=u'r180')
RelativeRotationEnum.r270 = RelativeRotationEnum._CF_enumeration.addEnumeration(unicode_value=u'r270', tag=u'r270')
RelativeRotationEnum.NoRestriction = RelativeRotationEnum._CF_enumeration.addEnumeration(unicode_value=u'NoRestriction', tag=u'NoRestriction')
RelativeRotationEnum._InitializeFacetMap(RelativeRotationEnum._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'RelativeRotationEnum', RelativeRotationEnum)

# Complex type {eda}Parameter with content type ELEMENT_ONLY
class Parameter_ (_ImportedBinding__avm.DomainModelParameter_):
    """Complex type {eda}Parameter with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Parameter')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 28, 2)
    _ElementMap = _ImportedBinding__avm.DomainModelParameter_._ElementMap.copy()
    _AttributeMap = _ImportedBinding__avm.DomainModelParameter_._AttributeMap.copy()
    # Base type is _ImportedBinding__avm.DomainModelParameter_
    
    # Element Value uses Python identifier Value
    __Value = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Value'), 'Value', '__eda_Parameter__Value', False, pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 32, 10), )

    
    Value = property(__Value.value, __Value.set, None, None)

    
    # Attribute YPosition inherited from {avm}DomainModelParameter
    
    # Attribute Notes inherited from {avm}DomainModelParameter
    
    # Attribute XPosition inherited from {avm}DomainModelParameter
    
    # Attribute Locator uses Python identifier Locator
    __Locator = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Locator'), 'Locator', '__eda_Parameter__Locator', pyxb.binding.datatypes.string, required=True)
    __Locator._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 34, 8)
    __Locator._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 34, 8)
    
    Locator = property(__Locator.value, __Locator.set, None, None)

    _ElementMap.update({
        __Value.name() : __Value
    })
    _AttributeMap.update({
        __Locator.name() : __Locator
    })
Namespace.addCategoryObject('typeBinding', u'Parameter', Parameter_)


# Complex type {eda}PcbLayoutConstraint with content type EMPTY
class PcbLayoutConstraint_ (_ImportedBinding__avm.ContainerFeature_):
    """Complex type {eda}PcbLayoutConstraint with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'PcbLayoutConstraint')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 38, 2)
    _ElementMap = _ImportedBinding__avm.ContainerFeature_._ElementMap.copy()
    _AttributeMap = _ImportedBinding__avm.ContainerFeature_._AttributeMap.copy()
    # Base type is _ImportedBinding__avm.ContainerFeature_
    
    # Attribute XPosition uses Python identifier XPosition
    __XPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XPosition'), 'XPosition', '__eda_PcbLayoutConstraint__XPosition', pyxb.binding.datatypes.unsignedInt)
    __XPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 41, 8)
    __XPosition._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 41, 8)
    
    XPosition = property(__XPosition.value, __XPosition.set, None, None)

    
    # Attribute YPosition uses Python identifier YPosition
    __YPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YPosition'), 'YPosition', '__eda_PcbLayoutConstraint__YPosition', pyxb.binding.datatypes.unsignedInt)
    __YPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 42, 8)
    __YPosition._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 42, 8)
    
    YPosition = property(__YPosition.value, __YPosition.set, None, None)

    
    # Attribute Notes uses Python identifier Notes
    __Notes = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Notes'), 'Notes', '__eda_PcbLayoutConstraint__Notes', pyxb.binding.datatypes.string)
    __Notes._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 43, 8)
    __Notes._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 43, 8)
    
    Notes = property(__Notes.value, __Notes.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __XPosition.name() : __XPosition,
        __YPosition.name() : __YPosition,
        __Notes.name() : __Notes
    })
Namespace.addCategoryObject('typeBinding', u'PcbLayoutConstraint', PcbLayoutConstraint_)


# Complex type {eda}CircuitLayout with content type EMPTY
class CircuitLayout_ (_ImportedBinding__avm.DomainModel_):
    """Complex type {eda}CircuitLayout with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'CircuitLayout')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 196, 2)
    _ElementMap = _ImportedBinding__avm.DomainModel_._ElementMap.copy()
    _AttributeMap = _ImportedBinding__avm.DomainModel_._AttributeMap.copy()
    # Base type is _ImportedBinding__avm.DomainModel_
    
    # Attribute UsesResource inherited from {avm}DomainModel
    
    # Attribute Author inherited from {avm}DomainModel
    
    # Attribute Notes inherited from {avm}DomainModel
    
    # Attribute XPosition inherited from {avm}DomainModel
    
    # Attribute YPosition inherited from {avm}DomainModel
    
    # Attribute Name inherited from {avm}DomainModel
    
    # Attribute ID inherited from {avm}DomainModel
    
    # Attribute BoundingBoxes uses Python identifier BoundingBoxes
    __BoundingBoxes = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'BoundingBoxes'), 'BoundingBoxes', '__eda_CircuitLayout__BoundingBoxes', pyxb.binding.datatypes.string)
    __BoundingBoxes._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 199, 8)
    __BoundingBoxes._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 199, 8)
    
    BoundingBoxes = property(__BoundingBoxes.value, __BoundingBoxes.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __BoundingBoxes.name() : __BoundingBoxes
    })
Namespace.addCategoryObject('typeBinding', u'CircuitLayout', CircuitLayout_)


# Complex type {eda}EDAModel with content type ELEMENT_ONLY
class EDAModel_ (_ImportedBinding__schematic.SchematicModel_):
    """Complex type {eda}EDAModel with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'EDAModel')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 14, 2)
    _ElementMap = _ImportedBinding__schematic.SchematicModel_._ElementMap.copy()
    _AttributeMap = _ImportedBinding__schematic.SchematicModel_._AttributeMap.copy()
    # Base type is _ImportedBinding__schematic.SchematicModel_
    
    # Element Parameter uses Python identifier Parameter
    __Parameter = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Parameter'), 'Parameter', '__eda_EDAModel__Parameter', True, pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 18, 10), )

    
    Parameter = property(__Parameter.value, __Parameter.set, None, None)

    
    # Element Pin (Pin) inherited from {schematic}SchematicModel
    
    # Attribute UsesResource inherited from {avm}DomainModel
    
    # Attribute Author inherited from {avm}DomainModel
    
    # Attribute Notes inherited from {avm}DomainModel
    
    # Attribute XPosition inherited from {avm}DomainModel
    
    # Attribute YPosition inherited from {avm}DomainModel
    
    # Attribute Name inherited from {avm}DomainModel
    
    # Attribute ID inherited from {avm}DomainModel
    
    # Attribute Library uses Python identifier Library
    __Library = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Library'), 'Library', '__eda_EDAModel__Library', pyxb.binding.datatypes.string)
    __Library._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 20, 8)
    __Library._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 20, 8)
    
    Library = property(__Library.value, __Library.set, None, None)

    
    # Attribute DeviceSet uses Python identifier DeviceSet
    __DeviceSet = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'DeviceSet'), 'DeviceSet', '__eda_EDAModel__DeviceSet', pyxb.binding.datatypes.string)
    __DeviceSet._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 21, 8)
    __DeviceSet._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 21, 8)
    
    DeviceSet = property(__DeviceSet.value, __DeviceSet.set, None, None)

    
    # Attribute Device uses Python identifier Device
    __Device = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Device'), 'Device', '__eda_EDAModel__Device', pyxb.binding.datatypes.string)
    __Device._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 22, 8)
    __Device._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 22, 8)
    
    Device = property(__Device.value, __Device.set, None, None)

    
    # Attribute Package uses Python identifier Package
    __Package = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Package'), 'Package', '__eda_EDAModel__Package', pyxb.binding.datatypes.string)
    __Package._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 23, 8)
    __Package._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 23, 8)
    
    Package = property(__Package.value, __Package.set, None, None)

    
    # Attribute HasMultiLayerFootprint uses Python identifier HasMultiLayerFootprint
    __HasMultiLayerFootprint = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'HasMultiLayerFootprint'), 'HasMultiLayerFootprint', '__eda_EDAModel__HasMultiLayerFootprint', pyxb.binding.datatypes.boolean, unicode_default=u'false')
    __HasMultiLayerFootprint._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 24, 8)
    __HasMultiLayerFootprint._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 24, 8)
    
    HasMultiLayerFootprint = property(__HasMultiLayerFootprint.value, __HasMultiLayerFootprint.set, None, None)

    _ElementMap.update({
        __Parameter.name() : __Parameter
    })
    _AttributeMap.update({
        __Library.name() : __Library,
        __DeviceSet.name() : __DeviceSet,
        __Device.name() : __Device,
        __Package.name() : __Package,
        __HasMultiLayerFootprint.name() : __HasMultiLayerFootprint
    })
Namespace.addCategoryObject('typeBinding', u'EDAModel', EDAModel_)


# Complex type {eda}ExactLayoutConstraint with content type EMPTY
class ExactLayoutConstraint_ (PcbLayoutConstraint_):
    """Complex type {eda}ExactLayoutConstraint with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ExactLayoutConstraint')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 47, 2)
    _ElementMap = PcbLayoutConstraint_._ElementMap.copy()
    _AttributeMap = PcbLayoutConstraint_._AttributeMap.copy()
    # Base type is PcbLayoutConstraint_
    
    # Attribute XPosition inherited from {eda}PcbLayoutConstraint
    
    # Attribute YPosition inherited from {eda}PcbLayoutConstraint
    
    # Attribute Notes inherited from {eda}PcbLayoutConstraint
    
    # Attribute X uses Python identifier X
    __X = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'X'), 'X', '__eda_ExactLayoutConstraint__X', pyxb.binding.datatypes.double)
    __X._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 50, 8)
    __X._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 50, 8)
    
    X = property(__X.value, __X.set, None, None)

    
    # Attribute Y uses Python identifier Y
    __Y = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Y'), 'Y', '__eda_ExactLayoutConstraint__Y', pyxb.binding.datatypes.double)
    __Y._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 51, 8)
    __Y._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 51, 8)
    
    Y = property(__Y.value, __Y.set, None, None)

    
    # Attribute Layer uses Python identifier Layer
    __Layer = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Layer'), 'Layer', '__eda_ExactLayoutConstraint__Layer', LayerEnum)
    __Layer._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 52, 8)
    __Layer._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 52, 8)
    
    Layer = property(__Layer.value, __Layer.set, None, None)

    
    # Attribute Rotation uses Python identifier Rotation
    __Rotation = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Rotation'), 'Rotation', '__eda_ExactLayoutConstraint__Rotation', RotationEnum)
    __Rotation._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 53, 8)
    __Rotation._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 53, 8)
    
    Rotation = property(__Rotation.value, __Rotation.set, None, None)

    
    # Attribute ConstraintTarget uses Python identifier ConstraintTarget
    __ConstraintTarget = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ConstraintTarget'), 'ConstraintTarget', '__eda_ExactLayoutConstraint__ConstraintTarget', STD_ANON)
    __ConstraintTarget._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 54, 8)
    __ConstraintTarget._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 54, 8)
    
    ConstraintTarget = property(__ConstraintTarget.value, __ConstraintTarget.set, None, None)

    
    # Attribute ContainerConstraintTarget uses Python identifier ContainerConstraintTarget
    __ContainerConstraintTarget = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ContainerConstraintTarget'), 'ContainerConstraintTarget', '__eda_ExactLayoutConstraint__ContainerConstraintTarget', STD_ANON_)
    __ContainerConstraintTarget._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 59, 8)
    __ContainerConstraintTarget._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 59, 8)
    
    ContainerConstraintTarget = property(__ContainerConstraintTarget.value, __ContainerConstraintTarget.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __X.name() : __X,
        __Y.name() : __Y,
        __Layer.name() : __Layer,
        __Rotation.name() : __Rotation,
        __ConstraintTarget.name() : __ConstraintTarget,
        __ContainerConstraintTarget.name() : __ContainerConstraintTarget
    })
Namespace.addCategoryObject('typeBinding', u'ExactLayoutConstraint', ExactLayoutConstraint_)


# Complex type {eda}RangeLayoutConstraint with content type EMPTY
class RangeLayoutConstraint_ (PcbLayoutConstraint_):
    """Complex type {eda}RangeLayoutConstraint with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'RangeLayoutConstraint')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 67, 2)
    _ElementMap = PcbLayoutConstraint_._ElementMap.copy()
    _AttributeMap = PcbLayoutConstraint_._AttributeMap.copy()
    # Base type is PcbLayoutConstraint_
    
    # Attribute XPosition inherited from {eda}PcbLayoutConstraint
    
    # Attribute YPosition inherited from {eda}PcbLayoutConstraint
    
    # Attribute Notes inherited from {eda}PcbLayoutConstraint
    
    # Attribute XRangeMin uses Python identifier XRangeMin
    __XRangeMin = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XRangeMin'), 'XRangeMin', '__eda_RangeLayoutConstraint__XRangeMin', pyxb.binding.datatypes.double)
    __XRangeMin._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 70, 8)
    __XRangeMin._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 70, 8)
    
    XRangeMin = property(__XRangeMin.value, __XRangeMin.set, None, None)

    
    # Attribute XRangeMax uses Python identifier XRangeMax
    __XRangeMax = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XRangeMax'), 'XRangeMax', '__eda_RangeLayoutConstraint__XRangeMax', pyxb.binding.datatypes.double)
    __XRangeMax._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 71, 8)
    __XRangeMax._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 71, 8)
    
    XRangeMax = property(__XRangeMax.value, __XRangeMax.set, None, None)

    
    # Attribute YRangeMin uses Python identifier YRangeMin
    __YRangeMin = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YRangeMin'), 'YRangeMin', '__eda_RangeLayoutConstraint__YRangeMin', pyxb.binding.datatypes.double)
    __YRangeMin._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 72, 8)
    __YRangeMin._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 72, 8)
    
    YRangeMin = property(__YRangeMin.value, __YRangeMin.set, None, None)

    
    # Attribute YRangeMax uses Python identifier YRangeMax
    __YRangeMax = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YRangeMax'), 'YRangeMax', '__eda_RangeLayoutConstraint__YRangeMax', pyxb.binding.datatypes.double)
    __YRangeMax._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 73, 8)
    __YRangeMax._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 73, 8)
    
    YRangeMax = property(__YRangeMax.value, __YRangeMax.set, None, None)

    
    # Attribute LayerRange uses Python identifier LayerRange
    __LayerRange = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'LayerRange'), 'LayerRange', '__eda_RangeLayoutConstraint__LayerRange', LayerRangeEnum, unicode_default=u'Either')
    __LayerRange._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 74, 8)
    __LayerRange._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 74, 8)
    
    LayerRange = property(__LayerRange.value, __LayerRange.set, None, None)

    
    # Attribute ConstraintTarget uses Python identifier ConstraintTarget
    __ConstraintTarget = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ConstraintTarget'), 'ConstraintTarget', '__eda_RangeLayoutConstraint__ConstraintTarget', STD_ANON_2)
    __ConstraintTarget._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 75, 8)
    __ConstraintTarget._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 75, 8)
    
    ConstraintTarget = property(__ConstraintTarget.value, __ConstraintTarget.set, None, None)

    
    # Attribute ContainerConstraintTarget uses Python identifier ContainerConstraintTarget
    __ContainerConstraintTarget = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ContainerConstraintTarget'), 'ContainerConstraintTarget', '__eda_RangeLayoutConstraint__ContainerConstraintTarget', STD_ANON_3)
    __ContainerConstraintTarget._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 80, 8)
    __ContainerConstraintTarget._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 80, 8)
    
    ContainerConstraintTarget = property(__ContainerConstraintTarget.value, __ContainerConstraintTarget.set, None, None)

    
    # Attribute Type uses Python identifier Type
    __Type = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Type'), 'Type', '__eda_RangeLayoutConstraint__Type', RangeConstraintTypeEnum)
    __Type._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 85, 8)
    __Type._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 85, 8)
    
    Type = property(__Type.value, __Type.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __XRangeMin.name() : __XRangeMin,
        __XRangeMax.name() : __XRangeMax,
        __YRangeMin.name() : __YRangeMin,
        __YRangeMax.name() : __YRangeMax,
        __LayerRange.name() : __LayerRange,
        __ConstraintTarget.name() : __ConstraintTarget,
        __ContainerConstraintTarget.name() : __ContainerConstraintTarget,
        __Type.name() : __Type
    })
Namespace.addCategoryObject('typeBinding', u'RangeLayoutConstraint', RangeLayoutConstraint_)


# Complex type {eda}RelativeLayoutConstraint with content type EMPTY
class RelativeLayoutConstraint_ (PcbLayoutConstraint_):
    """Complex type {eda}RelativeLayoutConstraint with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'RelativeLayoutConstraint')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 89, 2)
    _ElementMap = PcbLayoutConstraint_._ElementMap.copy()
    _AttributeMap = PcbLayoutConstraint_._AttributeMap.copy()
    # Base type is PcbLayoutConstraint_
    
    # Attribute XPosition inherited from {eda}PcbLayoutConstraint
    
    # Attribute YPosition inherited from {eda}PcbLayoutConstraint
    
    # Attribute Notes inherited from {eda}PcbLayoutConstraint
    
    # Attribute XOffset uses Python identifier XOffset
    __XOffset = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XOffset'), 'XOffset', '__eda_RelativeLayoutConstraint__XOffset', pyxb.binding.datatypes.double)
    __XOffset._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 92, 8)
    __XOffset._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 92, 8)
    
    XOffset = property(__XOffset.value, __XOffset.set, None, None)

    
    # Attribute YOffset uses Python identifier YOffset
    __YOffset = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YOffset'), 'YOffset', '__eda_RelativeLayoutConstraint__YOffset', pyxb.binding.datatypes.double)
    __YOffset._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 93, 8)
    __YOffset._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 93, 8)
    
    YOffset = property(__YOffset.value, __YOffset.set, None, None)

    
    # Attribute ConstraintTarget uses Python identifier ConstraintTarget
    __ConstraintTarget = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ConstraintTarget'), 'ConstraintTarget', '__eda_RelativeLayoutConstraint__ConstraintTarget', STD_ANON_4)
    __ConstraintTarget._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 94, 8)
    __ConstraintTarget._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 94, 8)
    
    ConstraintTarget = property(__ConstraintTarget.value, __ConstraintTarget.set, None, None)

    
    # Attribute Origin uses Python identifier Origin
    __Origin = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Origin'), 'Origin', '__eda_RelativeLayoutConstraint__Origin', pyxb.binding.datatypes.anyURI, required=True)
    __Origin._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 99, 8)
    __Origin._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 99, 8)
    
    Origin = property(__Origin.value, __Origin.set, None, None)

    
    # Attribute ContainerConstraintTarget uses Python identifier ContainerConstraintTarget
    __ContainerConstraintTarget = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ContainerConstraintTarget'), 'ContainerConstraintTarget', '__eda_RelativeLayoutConstraint__ContainerConstraintTarget', STD_ANON_5)
    __ContainerConstraintTarget._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 100, 8)
    __ContainerConstraintTarget._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 100, 8)
    
    ContainerConstraintTarget = property(__ContainerConstraintTarget.value, __ContainerConstraintTarget.set, None, None)

    
    # Attribute RelativeLayer uses Python identifier RelativeLayer
    __RelativeLayer = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'RelativeLayer'), 'RelativeLayer', '__eda_RelativeLayoutConstraint__RelativeLayer', RelativeLayerEnum)
    __RelativeLayer._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 105, 8)
    __RelativeLayer._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 105, 8)
    
    RelativeLayer = property(__RelativeLayer.value, __RelativeLayer.set, None, None)

    
    # Attribute RelativeRotation uses Python identifier RelativeRotation
    __RelativeRotation = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'RelativeRotation'), 'RelativeRotation', '__eda_RelativeLayoutConstraint__RelativeRotation', RelativeRotationEnum)
    __RelativeRotation._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 106, 8)
    __RelativeRotation._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 106, 8)
    
    RelativeRotation = property(__RelativeRotation.value, __RelativeRotation.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __XOffset.name() : __XOffset,
        __YOffset.name() : __YOffset,
        __ConstraintTarget.name() : __ConstraintTarget,
        __Origin.name() : __Origin,
        __ContainerConstraintTarget.name() : __ContainerConstraintTarget,
        __RelativeLayer.name() : __RelativeLayer,
        __RelativeRotation.name() : __RelativeRotation
    })
Namespace.addCategoryObject('typeBinding', u'RelativeLayoutConstraint', RelativeLayoutConstraint_)


# Complex type {eda}RelativeRangeLayoutConstraint with content type EMPTY
class RelativeRangeLayoutConstraint_ (PcbLayoutConstraint_):
    """Complex type {eda}RelativeRangeLayoutConstraint with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'RelativeRangeLayoutConstraint')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 143, 2)
    _ElementMap = PcbLayoutConstraint_._ElementMap.copy()
    _AttributeMap = PcbLayoutConstraint_._AttributeMap.copy()
    # Base type is PcbLayoutConstraint_
    
    # Attribute XPosition inherited from {eda}PcbLayoutConstraint
    
    # Attribute YPosition inherited from {eda}PcbLayoutConstraint
    
    # Attribute Notes inherited from {eda}PcbLayoutConstraint
    
    # Attribute XRelativeRangeMin uses Python identifier XRelativeRangeMin
    __XRelativeRangeMin = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XRelativeRangeMin'), 'XRelativeRangeMin', '__eda_RelativeRangeLayoutConstraint__XRelativeRangeMin', pyxb.binding.datatypes.double)
    __XRelativeRangeMin._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 146, 8)
    __XRelativeRangeMin._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 146, 8)
    
    XRelativeRangeMin = property(__XRelativeRangeMin.value, __XRelativeRangeMin.set, None, None)

    
    # Attribute XRelativeRangeMax uses Python identifier XRelativeRangeMax
    __XRelativeRangeMax = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XRelativeRangeMax'), 'XRelativeRangeMax', '__eda_RelativeRangeLayoutConstraint__XRelativeRangeMax', pyxb.binding.datatypes.double)
    __XRelativeRangeMax._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 147, 8)
    __XRelativeRangeMax._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 147, 8)
    
    XRelativeRangeMax = property(__XRelativeRangeMax.value, __XRelativeRangeMax.set, None, None)

    
    # Attribute YRelativeRangeMin uses Python identifier YRelativeRangeMin
    __YRelativeRangeMin = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YRelativeRangeMin'), 'YRelativeRangeMin', '__eda_RelativeRangeLayoutConstraint__YRelativeRangeMin', pyxb.binding.datatypes.double)
    __YRelativeRangeMin._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 148, 8)
    __YRelativeRangeMin._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 148, 8)
    
    YRelativeRangeMin = property(__YRelativeRangeMin.value, __YRelativeRangeMin.set, None, None)

    
    # Attribute YRelativeRangeMax uses Python identifier YRelativeRangeMax
    __YRelativeRangeMax = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YRelativeRangeMax'), 'YRelativeRangeMax', '__eda_RelativeRangeLayoutConstraint__YRelativeRangeMax', pyxb.binding.datatypes.double)
    __YRelativeRangeMax._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 149, 8)
    __YRelativeRangeMax._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 149, 8)
    
    YRelativeRangeMax = property(__YRelativeRangeMax.value, __YRelativeRangeMax.set, None, None)

    
    # Attribute RelativeLayer uses Python identifier RelativeLayer
    __RelativeLayer = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'RelativeLayer'), 'RelativeLayer', '__eda_RelativeRangeLayoutConstraint__RelativeLayer', RelativeLayerEnum)
    __RelativeLayer._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 150, 8)
    __RelativeLayer._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 150, 8)
    
    RelativeLayer = property(__RelativeLayer.value, __RelativeLayer.set, None, None)

    
    # Attribute ContainerConstraintTarget uses Python identifier ContainerConstraintTarget
    __ContainerConstraintTarget = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ContainerConstraintTarget'), 'ContainerConstraintTarget', '__eda_RelativeRangeLayoutConstraint__ContainerConstraintTarget', STD_ANON_6)
    __ContainerConstraintTarget._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 151, 8)
    __ContainerConstraintTarget._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 151, 8)
    
    ContainerConstraintTarget = property(__ContainerConstraintTarget.value, __ContainerConstraintTarget.set, None, None)

    
    # Attribute Origin uses Python identifier Origin
    __Origin = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Origin'), 'Origin', '__eda_RelativeRangeLayoutConstraint__Origin', pyxb.binding.datatypes.anyURI, required=True)
    __Origin._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 156, 8)
    __Origin._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 156, 8)
    
    Origin = property(__Origin.value, __Origin.set, None, None)

    
    # Attribute ConstraintTarget uses Python identifier ConstraintTarget
    __ConstraintTarget = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ConstraintTarget'), 'ConstraintTarget', '__eda_RelativeRangeLayoutConstraint__ConstraintTarget', STD_ANON_7)
    __ConstraintTarget._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 157, 8)
    __ConstraintTarget._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 157, 8)
    
    ConstraintTarget = property(__ConstraintTarget.value, __ConstraintTarget.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __XRelativeRangeMin.name() : __XRelativeRangeMin,
        __XRelativeRangeMax.name() : __XRelativeRangeMax,
        __YRelativeRangeMin.name() : __YRelativeRangeMin,
        __YRelativeRangeMax.name() : __YRelativeRangeMax,
        __RelativeLayer.name() : __RelativeLayer,
        __ContainerConstraintTarget.name() : __ContainerConstraintTarget,
        __Origin.name() : __Origin,
        __ConstraintTarget.name() : __ConstraintTarget
    })
Namespace.addCategoryObject('typeBinding', u'RelativeRangeLayoutConstraint', RelativeRangeLayoutConstraint_)


# Complex type {eda}GlobalLayoutConstraintException with content type EMPTY
class GlobalLayoutConstraintException_ (PcbLayoutConstraint_):
    """Complex type {eda}GlobalLayoutConstraintException with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'GlobalLayoutConstraintException')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 165, 2)
    _ElementMap = PcbLayoutConstraint_._ElementMap.copy()
    _AttributeMap = PcbLayoutConstraint_._AttributeMap.copy()
    # Base type is PcbLayoutConstraint_
    
    # Attribute XPosition inherited from {eda}PcbLayoutConstraint
    
    # Attribute YPosition inherited from {eda}PcbLayoutConstraint
    
    # Attribute Notes inherited from {eda}PcbLayoutConstraint
    
    # Attribute ConstraintTarget uses Python identifier ConstraintTarget
    __ConstraintTarget = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ConstraintTarget'), 'ConstraintTarget', '__eda_GlobalLayoutConstraintException__ConstraintTarget', STD_ANON_8)
    __ConstraintTarget._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 168, 8)
    __ConstraintTarget._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 168, 8)
    
    ConstraintTarget = property(__ConstraintTarget.value, __ConstraintTarget.set, None, None)

    
    # Attribute ContainerConstraintTarget uses Python identifier ContainerConstraintTarget
    __ContainerConstraintTarget = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ContainerConstraintTarget'), 'ContainerConstraintTarget', '__eda_GlobalLayoutConstraintException__ContainerConstraintTarget', STD_ANON_9)
    __ContainerConstraintTarget._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 173, 8)
    __ContainerConstraintTarget._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 173, 8)
    
    ContainerConstraintTarget = property(__ContainerConstraintTarget.value, __ContainerConstraintTarget.set, None, None)

    
    # Attribute Constraint uses Python identifier Constraint
    __Constraint = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Constraint'), 'Constraint', '__eda_GlobalLayoutConstraintException__Constraint', GlobalConstraintTypeEnum, required=True)
    __Constraint._DeclarationLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 178, 8)
    __Constraint._UseLocation = pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 178, 8)
    
    Constraint = property(__Constraint.value, __Constraint.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __ConstraintTarget.name() : __ConstraintTarget,
        __ContainerConstraintTarget.name() : __ContainerConstraintTarget,
        __Constraint.name() : __Constraint
    })
Namespace.addCategoryObject('typeBinding', u'GlobalLayoutConstraintException', GlobalLayoutConstraintException_)


Parameter = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Parameter'), Parameter_, location=pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 6, 2))
Namespace.addCategoryObject('elementBinding', Parameter.name().localName(), Parameter)

PcbLayoutConstraint = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'PcbLayoutConstraint'), PcbLayoutConstraint_, location=pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 7, 2))
Namespace.addCategoryObject('elementBinding', PcbLayoutConstraint.name().localName(), PcbLayoutConstraint)

CircuitLayout = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'CircuitLayout'), CircuitLayout_, location=pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 13, 2))
Namespace.addCategoryObject('elementBinding', CircuitLayout.name().localName(), CircuitLayout)

EDAModel = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'EDAModel'), EDAModel_, location=pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 5, 2))
Namespace.addCategoryObject('elementBinding', EDAModel.name().localName(), EDAModel)

ExactLayoutConstraint = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ExactLayoutConstraint'), ExactLayoutConstraint_, location=pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 8, 2))
Namespace.addCategoryObject('elementBinding', ExactLayoutConstraint.name().localName(), ExactLayoutConstraint)

RangeLayoutConstraint = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'RangeLayoutConstraint'), RangeLayoutConstraint_, location=pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 9, 2))
Namespace.addCategoryObject('elementBinding', RangeLayoutConstraint.name().localName(), RangeLayoutConstraint)

RelativeLayoutConstraint = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'RelativeLayoutConstraint'), RelativeLayoutConstraint_, location=pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 10, 2))
Namespace.addCategoryObject('elementBinding', RelativeLayoutConstraint.name().localName(), RelativeLayoutConstraint)

RelativeRangeLayoutConstraint = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'RelativeRangeLayoutConstraint'), RelativeRangeLayoutConstraint_, location=pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 11, 2))
Namespace.addCategoryObject('elementBinding', RelativeRangeLayoutConstraint.name().localName(), RelativeRangeLayoutConstraint)

GlobalLayoutConstraintException = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'GlobalLayoutConstraintException'), GlobalLayoutConstraintException_, location=pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 12, 2))
Namespace.addCategoryObject('elementBinding', GlobalLayoutConstraintException.name().localName(), GlobalLayoutConstraintException)



Parameter_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Value'), _ImportedBinding__avm.Value_, scope=Parameter_, location=pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 32, 10)))

def _BuildAutomaton ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton
    del _BuildAutomaton
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 32, 10))
    counters.add(cc_0)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(Parameter_._UseForTag(pyxb.namespace.ExpandedName(None, u'Value')), pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 32, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    st_0._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
Parameter_._Automaton = _BuildAutomaton()




EDAModel_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Parameter'), Parameter_, scope=EDAModel_, location=pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 18, 10)))

def _BuildAutomaton_ ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_
    del _BuildAutomaton_
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.schematic.xsd', 10, 10))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 18, 10))
    counters.add(cc_1)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(EDAModel_._UseForTag(pyxb.namespace.ExpandedName(None, u'Pin')), pyxb.utils.utility.Location(u'avm.schematic.xsd', 10, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(EDAModel_._UseForTag(pyxb.namespace.ExpandedName(None, u'Parameter')), pyxb.utils.utility.Location(u'avm.schematic.eda.xsd', 18, 10))
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
EDAModel_._Automaton = _BuildAutomaton_()

