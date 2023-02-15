# .\_avm.py
# -*- coding: utf-8 -*-
# PyXB bindings for NM:8c3bce54577a879cd94d42789711c9f5d444aa71
# Generated 2023-02-15 11:25:44.103000 by PyXB version 1.2.3
# Namespace avm [xmlns:avm]

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
import iFAB as _ImportedBinding__iFAB

# NOTE: All namespace declarations are reserved within the binding
Namespace = pyxb.namespace.NamespaceForURI(u'avm', create_if_missing=True)
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


# Atomic simple type: {avm}CalculationTypeEnum
class CalculationTypeEnum (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'CalculationTypeEnum')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 227, 2)
    _Documentation = None
CalculationTypeEnum._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=CalculationTypeEnum, enum_prefix=None)
CalculationTypeEnum.Declarative = CalculationTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'Declarative', tag=u'Declarative')
CalculationTypeEnum.Python = CalculationTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'Python', tag=u'Python')
CalculationTypeEnum._InitializeFacetMap(CalculationTypeEnum._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'CalculationTypeEnum', CalculationTypeEnum)

# Atomic simple type: {avm}DataTypeEnum
class DataTypeEnum (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'DataTypeEnum')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 233, 2)
    _Documentation = None
DataTypeEnum._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=DataTypeEnum, enum_prefix=None)
DataTypeEnum.String = DataTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'String', tag=u'String')
DataTypeEnum.Boolean = DataTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'Boolean', tag=u'Boolean')
DataTypeEnum.Integer = DataTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'Integer', tag=u'Integer')
DataTypeEnum.Real = DataTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'Real', tag=u'Real')
DataTypeEnum._InitializeFacetMap(DataTypeEnum._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'DataTypeEnum', DataTypeEnum)

# Atomic simple type: {avm}DimensionTypeEnum
class DimensionTypeEnum (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'DimensionTypeEnum')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 241, 2)
    _Documentation = None
DimensionTypeEnum._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=DimensionTypeEnum, enum_prefix=None)
DimensionTypeEnum.Matrix = DimensionTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'Matrix', tag=u'Matrix')
DimensionTypeEnum.Vector = DimensionTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'Vector', tag=u'Vector')
DimensionTypeEnum.Scalar = DimensionTypeEnum._CF_enumeration.addEnumeration(unicode_value=u'Scalar', tag=u'Scalar')
DimensionTypeEnum._InitializeFacetMap(DimensionTypeEnum._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'DimensionTypeEnum', DimensionTypeEnum)

# List simple type: [anonymous]
# superclasses pyxb.binding.datatypes.anySimpleType
class STD_ANON (pyxb.binding.basis.STD_list):

    """Simple type that is a list of pyxb.binding.datatypes.anyURI."""

    _ExpandedName = None
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 322, 6)
    _Documentation = None

    _ItemType = pyxb.binding.datatypes.anyURI
STD_ANON._InitializeFacetMap()

# List simple type: [anonymous]
# superclasses pyxb.binding.datatypes.anySimpleType
class STD_ANON_ (pyxb.binding.basis.STD_list):

    """Simple type that is a list of pyxb.binding.datatypes.anyURI."""

    _ExpandedName = None
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 412, 6)
    _Documentation = None

    _ItemType = pyxb.binding.datatypes.anyURI
STD_ANON_._InitializeFacetMap()

# List simple type: [anonymous]
# superclasses pyxb.binding.datatypes.anySimpleType
class STD_ANON_2 (pyxb.binding.basis.STD_list):

    """Simple type that is a list of pyxb.binding.datatypes.anyURI."""

    _ExpandedName = None
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 426, 6)
    _Documentation = None

    _ItemType = pyxb.binding.datatypes.anyURI
STD_ANON_2._InitializeFacetMap()

# List simple type: [anonymous]
# superclasses pyxb.binding.datatypes.anySimpleType
class STD_ANON_3 (pyxb.binding.basis.STD_list):

    """Simple type that is a list of pyxb.binding.datatypes.anyURI."""

    _ExpandedName = None
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 432, 6)
    _Documentation = None

    _ItemType = pyxb.binding.datatypes.anyURI
STD_ANON_3._InitializeFacetMap()

# List simple type: [anonymous]
# superclasses pyxb.binding.datatypes.anySimpleType
class STD_ANON_4 (pyxb.binding.basis.STD_list):

    """Simple type that is a list of pyxb.binding.datatypes.anyURI."""

    _ExpandedName = None
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 451, 10)
    _Documentation = None

    _ItemType = pyxb.binding.datatypes.anyURI
STD_ANON_4._InitializeFacetMap()

# Atomic simple type: {avm}SimpleFormulaOperation
class SimpleFormulaOperation (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'SimpleFormulaOperation')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 458, 2)
    _Documentation = None
SimpleFormulaOperation._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=SimpleFormulaOperation, enum_prefix=None)
SimpleFormulaOperation.Addition = SimpleFormulaOperation._CF_enumeration.addEnumeration(unicode_value=u'Addition', tag=u'Addition')
SimpleFormulaOperation.Multiplication = SimpleFormulaOperation._CF_enumeration.addEnumeration(unicode_value=u'Multiplication', tag=u'Multiplication')
SimpleFormulaOperation.ArithmeticMean = SimpleFormulaOperation._CF_enumeration.addEnumeration(unicode_value=u'ArithmeticMean', tag=u'ArithmeticMean')
SimpleFormulaOperation.GeometricMean = SimpleFormulaOperation._CF_enumeration.addEnumeration(unicode_value=u'GeometricMean', tag=u'GeometricMean')
SimpleFormulaOperation.Maximum = SimpleFormulaOperation._CF_enumeration.addEnumeration(unicode_value=u'Maximum', tag=u'Maximum')
SimpleFormulaOperation.Minimum = SimpleFormulaOperation._CF_enumeration.addEnumeration(unicode_value=u'Minimum', tag=u'Minimum')
SimpleFormulaOperation._InitializeFacetMap(SimpleFormulaOperation._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'SimpleFormulaOperation', SimpleFormulaOperation)

# Atomic simple type: {avm}DoDDistributionStatementEnum
class DoDDistributionStatementEnum (pyxb.binding.datatypes.string, pyxb.binding.basis.enumeration_mixin):

    """An atomic simple type."""

    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'DoDDistributionStatementEnum')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 492, 2)
    _Documentation = None
DoDDistributionStatementEnum._CF_enumeration = pyxb.binding.facets.CF_enumeration(value_datatype=DoDDistributionStatementEnum, enum_prefix=None)
DoDDistributionStatementEnum.StatementA = DoDDistributionStatementEnum._CF_enumeration.addEnumeration(unicode_value=u'StatementA', tag=u'StatementA')
DoDDistributionStatementEnum.StatementB = DoDDistributionStatementEnum._CF_enumeration.addEnumeration(unicode_value=u'StatementB', tag=u'StatementB')
DoDDistributionStatementEnum.StatementC = DoDDistributionStatementEnum._CF_enumeration.addEnumeration(unicode_value=u'StatementC', tag=u'StatementC')
DoDDistributionStatementEnum.StatementD = DoDDistributionStatementEnum._CF_enumeration.addEnumeration(unicode_value=u'StatementD', tag=u'StatementD')
DoDDistributionStatementEnum.StatementE = DoDDistributionStatementEnum._CF_enumeration.addEnumeration(unicode_value=u'StatementE', tag=u'StatementE')
DoDDistributionStatementEnum._InitializeFacetMap(DoDDistributionStatementEnum._CF_enumeration)
Namespace.addCategoryObject('typeBinding', u'DoDDistributionStatementEnum', DoDDistributionStatementEnum)

# List simple type: [anonymous]
# superclasses pyxb.binding.datatypes.anySimpleType
class STD_ANON_5 (pyxb.binding.basis.STD_list):

    """Simple type that is a list of pyxb.binding.datatypes.anyURI."""

    _ExpandedName = None
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 584, 10)
    _Documentation = None

    _ItemType = pyxb.binding.datatypes.anyURI
STD_ANON_5._InitializeFacetMap()

# Complex type {avm}Component with content type ELEMENT_ONLY
class Component_ (pyxb.binding.basis.complexTypeDefinition):
    """Test documentation for Component type. Yep."""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Component')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 70, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Element DomainModel uses Python identifier DomainModel
    __DomainModel = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'DomainModel'), 'DomainModel', '__avm_Component__DomainModel', True, pyxb.utils.utility.Location(u'avm.xsd', 75, 6), )

    
    DomainModel = property(__DomainModel.value, __DomainModel.set, None, None)

    
    # Element Property uses Python identifier Property
    __Property = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Property'), 'Property', '__avm_Component__Property', True, pyxb.utils.utility.Location(u'avm.xsd', 76, 6), )

    
    Property = property(__Property.value, __Property.set, None, None)

    
    # Element ResourceDependency uses Python identifier ResourceDependency
    __ResourceDependency = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'ResourceDependency'), 'ResourceDependency', '__avm_Component__ResourceDependency', True, pyxb.utils.utility.Location(u'avm.xsd', 77, 6), )

    
    ResourceDependency = property(__ResourceDependency.value, __ResourceDependency.set, None, None)

    
    # Element Connector uses Python identifier Connector
    __Connector = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Connector'), 'Connector', '__avm_Component__Connector', True, pyxb.utils.utility.Location(u'avm.xsd', 78, 6), )

    
    Connector = property(__Connector.value, __Connector.set, None, None)

    
    # Element DistributionRestriction uses Python identifier DistributionRestriction
    __DistributionRestriction = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'DistributionRestriction'), 'DistributionRestriction', '__avm_Component__DistributionRestriction', True, pyxb.utils.utility.Location(u'avm.xsd', 79, 6), )

    
    DistributionRestriction = property(__DistributionRestriction.value, __DistributionRestriction.set, None, None)

    
    # Element Port uses Python identifier Port
    __Port = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Port'), 'Port', '__avm_Component__Port', True, pyxb.utils.utility.Location(u'avm.xsd', 80, 6), )

    
    Port = property(__Port.value, __Port.set, None, None)

    
    # Element Classifications uses Python identifier Classifications
    __Classifications = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Classifications'), 'Classifications', '__avm_Component__Classifications', True, pyxb.utils.utility.Location(u'avm.xsd', 81, 6), )

    
    Classifications = property(__Classifications.value, __Classifications.set, None, None)

    
    # Element AnalysisConstruct uses Python identifier AnalysisConstruct
    __AnalysisConstruct = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'AnalysisConstruct'), 'AnalysisConstruct', '__avm_Component__AnalysisConstruct', True, pyxb.utils.utility.Location(u'avm.xsd', 82, 6), )

    
    AnalysisConstruct = property(__AnalysisConstruct.value, __AnalysisConstruct.set, None, None)

    
    # Element Supercedes uses Python identifier Supercedes
    __Supercedes = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Supercedes'), 'Supercedes', '__avm_Component__Supercedes', True, pyxb.utils.utility.Location(u'avm.xsd', 83, 6), )

    
    Supercedes = property(__Supercedes.value, __Supercedes.set, None, None)

    
    # Element Formula uses Python identifier Formula
    __Formula = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Formula'), 'Formula', '__avm_Component__Formula', True, pyxb.utils.utility.Location(u'avm.xsd', 84, 6), )

    
    Formula = property(__Formula.value, __Formula.set, None, None)

    
    # Element DomainMapping uses Python identifier DomainMapping
    __DomainMapping = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'DomainMapping'), 'DomainMapping', '__avm_Component__DomainMapping', True, pyxb.utils.utility.Location(u'avm.xsd', 85, 6), )

    
    DomainMapping = property(__DomainMapping.value, __DomainMapping.set, None, None)

    
    # Attribute Name uses Python identifier Name
    __Name = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Name'), 'Name', '__avm_Component__Name', pyxb.binding.datatypes.string)
    __Name._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 87, 4)
    __Name._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 87, 4)
    
    Name = property(__Name.value, __Name.set, None, None)

    
    # Attribute Version uses Python identifier Version
    __Version = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Version'), 'Version', '__avm_Component__Version', pyxb.binding.datatypes.string)
    __Version._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 88, 4)
    __Version._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 88, 4)
    
    Version = property(__Version.value, __Version.set, None, None)

    
    # Attribute SchemaVersion uses Python identifier SchemaVersion
    __SchemaVersion = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'SchemaVersion'), 'SchemaVersion', '__avm_Component__SchemaVersion', pyxb.binding.datatypes.string)
    __SchemaVersion._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 89, 4)
    __SchemaVersion._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 89, 4)
    
    SchemaVersion = property(__SchemaVersion.value, __SchemaVersion.set, None, None)

    
    # Attribute ID uses Python identifier ID
    __ID = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ID'), 'ID', '__avm_Component__ID', pyxb.binding.datatypes.string)
    __ID._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 90, 4)
    __ID._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 90, 4)
    
    ID = property(__ID.value, __ID.set, None, None)

    _ElementMap.update({
        __DomainModel.name() : __DomainModel,
        __Property.name() : __Property,
        __ResourceDependency.name() : __ResourceDependency,
        __Connector.name() : __Connector,
        __DistributionRestriction.name() : __DistributionRestriction,
        __Port.name() : __Port,
        __Classifications.name() : __Classifications,
        __AnalysisConstruct.name() : __AnalysisConstruct,
        __Supercedes.name() : __Supercedes,
        __Formula.name() : __Formula,
        __DomainMapping.name() : __DomainMapping
    })
    _AttributeMap.update({
        __Name.name() : __Name,
        __Version.name() : __Version,
        __SchemaVersion.name() : __SchemaVersion,
        __ID.name() : __ID
    })
Namespace.addCategoryObject('typeBinding', u'Component', Component_)


# Complex type {avm}DomainModel with content type EMPTY
class DomainModel_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}DomainModel with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'DomainModel')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 92, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Attribute UsesResource uses Python identifier UsesResource
    __UsesResource = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'UsesResource'), 'UsesResource', '__avm_DomainModel__UsesResource', pyxb.binding.datatypes.IDREFS)
    __UsesResource._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 93, 4)
    __UsesResource._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 93, 4)
    
    UsesResource = property(__UsesResource.value, __UsesResource.set, None, None)

    
    # Attribute Author uses Python identifier Author
    __Author = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Author'), 'Author', '__avm_DomainModel__Author', pyxb.binding.datatypes.string)
    __Author._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 94, 4)
    __Author._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 94, 4)
    
    Author = property(__Author.value, __Author.set, None, None)

    
    # Attribute Notes uses Python identifier Notes
    __Notes = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Notes'), 'Notes', '__avm_DomainModel__Notes', pyxb.binding.datatypes.string)
    __Notes._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 95, 4)
    __Notes._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 95, 4)
    
    Notes = property(__Notes.value, __Notes.set, None, None)

    
    # Attribute XPosition uses Python identifier XPosition
    __XPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XPosition'), 'XPosition', '__avm_DomainModel__XPosition', pyxb.binding.datatypes.unsignedInt)
    __XPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 96, 4)
    __XPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 96, 4)
    
    XPosition = property(__XPosition.value, __XPosition.set, None, None)

    
    # Attribute YPosition uses Python identifier YPosition
    __YPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YPosition'), 'YPosition', '__avm_DomainModel__YPosition', pyxb.binding.datatypes.unsignedInt)
    __YPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 97, 4)
    __YPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 97, 4)
    
    YPosition = property(__YPosition.value, __YPosition.set, None, None)

    
    # Attribute Name uses Python identifier Name
    __Name = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Name'), 'Name', '__avm_DomainModel__Name', pyxb.binding.datatypes.string)
    __Name._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 98, 4)
    __Name._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 98, 4)
    
    Name = property(__Name.value, __Name.set, None, None)

    
    # Attribute ID uses Python identifier ID
    __ID = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ID'), 'ID', '__avm_DomainModel__ID', pyxb.binding.datatypes.ID)
    __ID._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 99, 4)
    __ID._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 99, 4)
    
    ID = property(__ID.value, __ID.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __UsesResource.name() : __UsesResource,
        __Author.name() : __Author,
        __Notes.name() : __Notes,
        __XPosition.name() : __XPosition,
        __YPosition.name() : __YPosition,
        __Name.name() : __Name,
        __ID.name() : __ID
    })
Namespace.addCategoryObject('typeBinding', u'DomainModel', DomainModel_)


# Complex type {avm}Property with content type EMPTY
class Property_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}Property with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Property')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 101, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Attribute Name uses Python identifier Name
    __Name = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Name'), 'Name', '__avm_Property__Name', pyxb.binding.datatypes.string)
    __Name._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 102, 4)
    __Name._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 102, 4)
    
    Name = property(__Name.value, __Name.set, None, None)

    
    # Attribute OnDataSheet uses Python identifier OnDataSheet
    __OnDataSheet = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'OnDataSheet'), 'OnDataSheet', '__avm_Property__OnDataSheet', pyxb.binding.datatypes.boolean)
    __OnDataSheet._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 103, 4)
    __OnDataSheet._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 103, 4)
    
    OnDataSheet = property(__OnDataSheet.value, __OnDataSheet.set, None, None)

    
    # Attribute Notes uses Python identifier Notes
    __Notes = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Notes'), 'Notes', '__avm_Property__Notes', pyxb.binding.datatypes.string)
    __Notes._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 104, 4)
    __Notes._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 104, 4)
    
    Notes = property(__Notes.value, __Notes.set, None, None)

    
    # Attribute Definition uses Python identifier Definition
    __Definition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Definition'), 'Definition', '__avm_Property__Definition', pyxb.binding.datatypes.anyURI)
    __Definition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 105, 4)
    __Definition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 105, 4)
    
    Definition = property(__Definition.value, __Definition.set, None, None)

    
    # Attribute ID uses Python identifier ID
    __ID = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ID'), 'ID', '__avm_Property__ID', pyxb.binding.datatypes.ID)
    __ID._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 106, 4)
    __ID._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 106, 4)
    
    ID = property(__ID.value, __ID.set, None, None)

    
    # Attribute XPosition uses Python identifier XPosition
    __XPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XPosition'), 'XPosition', '__avm_Property__XPosition', pyxb.binding.datatypes.unsignedInt)
    __XPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 107, 4)
    __XPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 107, 4)
    
    XPosition = property(__XPosition.value, __XPosition.set, None, None)

    
    # Attribute YPosition uses Python identifier YPosition
    __YPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YPosition'), 'YPosition', '__avm_Property__YPosition', pyxb.binding.datatypes.unsignedInt)
    __YPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 108, 4)
    __YPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 108, 4)
    
    YPosition = property(__YPosition.value, __YPosition.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __Name.name() : __Name,
        __OnDataSheet.name() : __OnDataSheet,
        __Notes.name() : __Notes,
        __Definition.name() : __Definition,
        __ID.name() : __ID,
        __XPosition.name() : __XPosition,
        __YPosition.name() : __YPosition
    })
Namespace.addCategoryObject('typeBinding', u'Property', Property_)


# Complex type {avm}Resource with content type EMPTY
class Resource_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}Resource with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Resource')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 151, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Attribute Name uses Python identifier Name
    __Name = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Name'), 'Name', '__avm_Resource__Name', pyxb.binding.datatypes.string)
    __Name._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 152, 4)
    __Name._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 152, 4)
    
    Name = property(__Name.value, __Name.set, None, None)

    
    # Attribute Path uses Python identifier Path
    __Path = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Path'), 'Path', '__avm_Resource__Path', pyxb.binding.datatypes.anyURI)
    __Path._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 153, 4)
    __Path._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 153, 4)
    
    Path = property(__Path.value, __Path.set, None, None)

    
    # Attribute Hash uses Python identifier Hash
    __Hash = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Hash'), 'Hash', '__avm_Resource__Hash', pyxb.binding.datatypes.string)
    __Hash._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 154, 4)
    __Hash._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 154, 4)
    
    Hash = property(__Hash.value, __Hash.set, None, None)

    
    # Attribute ID uses Python identifier ID
    __ID = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ID'), 'ID', '__avm_Resource__ID', pyxb.binding.datatypes.ID)
    __ID._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 155, 4)
    __ID._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 155, 4)
    
    ID = property(__ID.value, __ID.set, None, None)

    
    # Attribute Notes uses Python identifier Notes
    __Notes = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Notes'), 'Notes', '__avm_Resource__Notes', pyxb.binding.datatypes.string)
    __Notes._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 156, 4)
    __Notes._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 156, 4)
    
    Notes = property(__Notes.value, __Notes.set, None, None)

    
    # Attribute XPosition uses Python identifier XPosition
    __XPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XPosition'), 'XPosition', '__avm_Resource__XPosition', pyxb.binding.datatypes.unsignedInt)
    __XPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 157, 4)
    __XPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 157, 4)
    
    XPosition = property(__XPosition.value, __XPosition.set, None, None)

    
    # Attribute YPosition uses Python identifier YPosition
    __YPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YPosition'), 'YPosition', '__avm_Resource__YPosition', pyxb.binding.datatypes.unsignedInt)
    __YPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 158, 4)
    __YPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 158, 4)
    
    YPosition = property(__YPosition.value, __YPosition.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __Name.name() : __Name,
        __Path.name() : __Path,
        __Hash.name() : __Hash,
        __ID.name() : __ID,
        __Notes.name() : __Notes,
        __XPosition.name() : __XPosition,
        __YPosition.name() : __YPosition
    })
Namespace.addCategoryObject('typeBinding', u'Resource', Resource_)


# Complex type {avm}DomainModelParameter with content type EMPTY
class DomainModelParameter_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}DomainModelParameter with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'DomainModelParameter')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 194, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Attribute YPosition uses Python identifier YPosition
    __YPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YPosition'), 'YPosition', '__avm_DomainModelParameter__YPosition', pyxb.binding.datatypes.unsignedInt)
    __YPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 195, 4)
    __YPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 195, 4)
    
    YPosition = property(__YPosition.value, __YPosition.set, None, None)

    
    # Attribute Notes uses Python identifier Notes
    __Notes = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Notes'), 'Notes', '__avm_DomainModelParameter__Notes', pyxb.binding.datatypes.string)
    __Notes._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 196, 4)
    __Notes._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 196, 4)
    
    Notes = property(__Notes.value, __Notes.set, None, None)

    
    # Attribute XPosition uses Python identifier XPosition
    __XPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XPosition'), 'XPosition', '__avm_DomainModelParameter__XPosition', pyxb.binding.datatypes.unsignedInt)
    __XPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 197, 4)
    __XPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 197, 4)
    
    XPosition = property(__XPosition.value, __XPosition.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __YPosition.name() : __YPosition,
        __Notes.name() : __Notes,
        __XPosition.name() : __XPosition
    })
Namespace.addCategoryObject('typeBinding', u'DomainModelParameter', DomainModelParameter_)


# Complex type {avm}ValueExpressionType with content type EMPTY
class ValueExpressionType_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}ValueExpressionType with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ValueExpressionType')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 211, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'ValueExpressionType', ValueExpressionType_)


# Complex type {avm}DistributionRestriction with content type EMPTY
class DistributionRestriction_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}DistributionRestriction with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'DistributionRestriction')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 248, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Attribute Notes uses Python identifier Notes
    __Notes = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Notes'), 'Notes', '__avm_DistributionRestriction__Notes', pyxb.binding.datatypes.string)
    __Notes._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 249, 4)
    __Notes._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 249, 4)
    
    Notes = property(__Notes.value, __Notes.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __Notes.name() : __Notes
    })
Namespace.addCategoryObject('typeBinding', u'DistributionRestriction', DistributionRestriction_)


# Complex type {avm}DomainModelMetric with content type ELEMENT_ONLY
class DomainModelMetric_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}DomainModelMetric with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'DomainModelMetric')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 270, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Element Value uses Python identifier Value
    __Value = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Value'), 'Value', '__avm_DomainModelMetric__Value', False, pyxb.utils.utility.Location(u'avm.xsd', 272, 6), )

    
    Value = property(__Value.value, __Value.set, None, None)

    
    # Attribute ID uses Python identifier ID
    __ID = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ID'), 'ID', '__avm_DomainModelMetric__ID', pyxb.binding.datatypes.ID, required=True)
    __ID._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 274, 4)
    __ID._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 274, 4)
    
    ID = property(__ID.value, __ID.set, None, None)

    
    # Attribute Notes uses Python identifier Notes
    __Notes = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Notes'), 'Notes', '__avm_DomainModelMetric__Notes', pyxb.binding.datatypes.string)
    __Notes._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 275, 4)
    __Notes._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 275, 4)
    
    Notes = property(__Notes.value, __Notes.set, None, None)

    
    # Attribute XPosition uses Python identifier XPosition
    __XPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XPosition'), 'XPosition', '__avm_DomainModelMetric__XPosition', pyxb.binding.datatypes.unsignedInt)
    __XPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 276, 4)
    __XPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 276, 4)
    
    XPosition = property(__XPosition.value, __XPosition.set, None, None)

    
    # Attribute YPosition uses Python identifier YPosition
    __YPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YPosition'), 'YPosition', '__avm_DomainModelMetric__YPosition', pyxb.binding.datatypes.unsignedInt)
    __YPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 277, 4)
    __YPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 277, 4)
    
    YPosition = property(__YPosition.value, __YPosition.set, None, None)

    _ElementMap.update({
        __Value.name() : __Value
    })
    _AttributeMap.update({
        __ID.name() : __ID,
        __Notes.name() : __Notes,
        __XPosition.name() : __XPosition,
        __YPosition.name() : __YPosition
    })
Namespace.addCategoryObject('typeBinding', u'DomainModelMetric', DomainModelMetric_)


# Complex type {avm}AnalysisConstruct with content type EMPTY
class AnalysisConstruct_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}AnalysisConstruct with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'AnalysisConstruct')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 318, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'AnalysisConstruct', AnalysisConstruct_)


# Complex type {avm}Design with content type ELEMENT_ONLY
class Design_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}Design with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Design')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 327, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Element RootContainer uses Python identifier RootContainer
    __RootContainer = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'RootContainer'), 'RootContainer', '__avm_Design__RootContainer', False, pyxb.utils.utility.Location(u'avm.xsd', 329, 6), )

    
    RootContainer = property(__RootContainer.value, __RootContainer.set, None, None)

    
    # Element DomainFeature uses Python identifier DomainFeature
    __DomainFeature = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'DomainFeature'), 'DomainFeature', '__avm_Design__DomainFeature', True, pyxb.utils.utility.Location(u'avm.xsd', 330, 6), )

    
    DomainFeature = property(__DomainFeature.value, __DomainFeature.set, None, None)

    
    # Element ResourceDependency uses Python identifier ResourceDependency
    __ResourceDependency = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'ResourceDependency'), 'ResourceDependency', '__avm_Design__ResourceDependency', True, pyxb.utils.utility.Location(u'avm.xsd', 331, 6), )

    
    ResourceDependency = property(__ResourceDependency.value, __ResourceDependency.set, None, None)

    
    # Attribute SchemaVersion uses Python identifier SchemaVersion
    __SchemaVersion = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'SchemaVersion'), 'SchemaVersion', '__avm_Design__SchemaVersion', pyxb.binding.datatypes.string)
    __SchemaVersion._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 333, 4)
    __SchemaVersion._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 333, 4)
    
    SchemaVersion = property(__SchemaVersion.value, __SchemaVersion.set, None, None)

    
    # Attribute DesignID uses Python identifier DesignID
    __DesignID = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'DesignID'), 'DesignID', '__avm_Design__DesignID', pyxb.binding.datatypes.string)
    __DesignID._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 334, 4)
    __DesignID._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 334, 4)
    
    DesignID = property(__DesignID.value, __DesignID.set, None, None)

    
    # Attribute Name uses Python identifier Name
    __Name = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Name'), 'Name', '__avm_Design__Name', pyxb.binding.datatypes.string)
    __Name._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 335, 4)
    __Name._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 335, 4)
    
    Name = property(__Name.value, __Name.set, None, None)

    
    # Attribute DesignSpaceSrcID uses Python identifier DesignSpaceSrcID
    __DesignSpaceSrcID = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'DesignSpaceSrcID'), 'DesignSpaceSrcID', '__avm_Design__DesignSpaceSrcID', pyxb.binding.datatypes.string)
    __DesignSpaceSrcID._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 336, 4)
    __DesignSpaceSrcID._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 336, 4)
    
    DesignSpaceSrcID = property(__DesignSpaceSrcID.value, __DesignSpaceSrcID.set, None, None)

    _ElementMap.update({
        __RootContainer.name() : __RootContainer,
        __DomainFeature.name() : __DomainFeature,
        __ResourceDependency.name() : __ResourceDependency
    })
    _AttributeMap.update({
        __SchemaVersion.name() : __SchemaVersion,
        __DesignID.name() : __DesignID,
        __Name.name() : __Name,
        __DesignSpaceSrcID.name() : __DesignSpaceSrcID
    })
Namespace.addCategoryObject('typeBinding', u'Design', Design_)


# Complex type {avm}Container with content type ELEMENT_ONLY
class Container_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}Container with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Container')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 338, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Element Container uses Python identifier Container
    __Container = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Container'), 'Container', '__avm_Container__Container', True, pyxb.utils.utility.Location(u'avm.xsd', 340, 6), )

    
    Container = property(__Container.value, __Container.set, None, None)

    
    # Element Property uses Python identifier Property
    __Property = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Property'), 'Property', '__avm_Container__Property', True, pyxb.utils.utility.Location(u'avm.xsd', 341, 6), )

    
    Property = property(__Property.value, __Property.set, None, None)

    
    # Element ComponentInstance uses Python identifier ComponentInstance
    __ComponentInstance = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'ComponentInstance'), 'ComponentInstance', '__avm_Container__ComponentInstance', True, pyxb.utils.utility.Location(u'avm.xsd', 342, 6), )

    
    ComponentInstance = property(__ComponentInstance.value, __ComponentInstance.set, None, None)

    
    # Element Port uses Python identifier Port
    __Port = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Port'), 'Port', '__avm_Container__Port', True, pyxb.utils.utility.Location(u'avm.xsd', 343, 6), )

    
    Port = property(__Port.value, __Port.set, None, None)

    
    # Element Connector uses Python identifier Connector
    __Connector = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Connector'), 'Connector', '__avm_Container__Connector', True, pyxb.utils.utility.Location(u'avm.xsd', 344, 6), )

    
    Connector = property(__Connector.value, __Connector.set, None, None)

    
    # Element JoinData uses Python identifier JoinData
    __JoinData = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'JoinData'), 'JoinData', '__avm_Container__JoinData', True, pyxb.utils.utility.Location(u'avm.xsd', 345, 6), )

    
    JoinData = property(__JoinData.value, __JoinData.set, None, None)

    
    # Element Formula uses Python identifier Formula
    __Formula = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Formula'), 'Formula', '__avm_Container__Formula', True, pyxb.utils.utility.Location(u'avm.xsd', 346, 6), )

    
    Formula = property(__Formula.value, __Formula.set, None, None)

    
    # Element ContainerFeature uses Python identifier ContainerFeature
    __ContainerFeature = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'ContainerFeature'), 'ContainerFeature', '__avm_Container__ContainerFeature', True, pyxb.utils.utility.Location(u'avm.xsd', 347, 6), )

    
    ContainerFeature = property(__ContainerFeature.value, __ContainerFeature.set, None, None)

    
    # Element ResourceDependency uses Python identifier ResourceDependency
    __ResourceDependency = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'ResourceDependency'), 'ResourceDependency', '__avm_Container__ResourceDependency', True, pyxb.utils.utility.Location(u'avm.xsd', 348, 6), )

    
    ResourceDependency = property(__ResourceDependency.value, __ResourceDependency.set, None, None)

    
    # Element DomainModel uses Python identifier DomainModel
    __DomainModel = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'DomainModel'), 'DomainModel', '__avm_Container__DomainModel', True, pyxb.utils.utility.Location(u'avm.xsd', 349, 6), )

    
    DomainModel = property(__DomainModel.value, __DomainModel.set, None, None)

    
    # Element Resource uses Python identifier Resource
    __Resource = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Resource'), 'Resource', '__avm_Container__Resource', True, pyxb.utils.utility.Location(u'avm.xsd', 350, 6), )

    
    Resource = property(__Resource.value, __Resource.set, None, None)

    
    # Element Classifications uses Python identifier Classifications
    __Classifications = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Classifications'), 'Classifications', '__avm_Container__Classifications', True, pyxb.utils.utility.Location(u'avm.xsd', 351, 6), )

    
    Classifications = property(__Classifications.value, __Classifications.set, None, None)

    
    # Attribute XPosition uses Python identifier XPosition
    __XPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XPosition'), 'XPosition', '__avm_Container__XPosition', pyxb.binding.datatypes.unsignedInt)
    __XPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 353, 4)
    __XPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 353, 4)
    
    XPosition = property(__XPosition.value, __XPosition.set, None, None)

    
    # Attribute Name uses Python identifier Name
    __Name = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Name'), 'Name', '__avm_Container__Name', pyxb.binding.datatypes.string)
    __Name._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 354, 4)
    __Name._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 354, 4)
    
    Name = property(__Name.value, __Name.set, None, None)

    
    # Attribute YPosition uses Python identifier YPosition
    __YPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YPosition'), 'YPosition', '__avm_Container__YPosition', pyxb.binding.datatypes.unsignedInt)
    __YPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 355, 4)
    __YPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 355, 4)
    
    YPosition = property(__YPosition.value, __YPosition.set, None, None)

    
    # Attribute ID uses Python identifier ID
    __ID = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ID'), 'ID', '__avm_Container__ID', pyxb.binding.datatypes.ID)
    __ID._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 356, 4)
    __ID._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 356, 4)
    
    ID = property(__ID.value, __ID.set, None, None)

    
    # Attribute Description uses Python identifier Description
    __Description = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Description'), 'Description', '__avm_Container__Description', pyxb.binding.datatypes.string)
    __Description._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 357, 4)
    __Description._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 357, 4)
    
    Description = property(__Description.value, __Description.set, None, None)

    _ElementMap.update({
        __Container.name() : __Container,
        __Property.name() : __Property,
        __ComponentInstance.name() : __ComponentInstance,
        __Port.name() : __Port,
        __Connector.name() : __Connector,
        __JoinData.name() : __JoinData,
        __Formula.name() : __Formula,
        __ContainerFeature.name() : __ContainerFeature,
        __ResourceDependency.name() : __ResourceDependency,
        __DomainModel.name() : __DomainModel,
        __Resource.name() : __Resource,
        __Classifications.name() : __Classifications
    })
    _AttributeMap.update({
        __XPosition.name() : __XPosition,
        __Name.name() : __Name,
        __YPosition.name() : __YPosition,
        __ID.name() : __ID,
        __Description.name() : __Description
    })
Namespace.addCategoryObject('typeBinding', u'Container', Container_)


# Complex type {avm}ComponentInstance with content type ELEMENT_ONLY
class ComponentInstance_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}ComponentInstance with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ComponentInstance')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 378, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Element PortInstance uses Python identifier PortInstance
    __PortInstance = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'PortInstance'), 'PortInstance', '__avm_ComponentInstance__PortInstance', True, pyxb.utils.utility.Location(u'avm.xsd', 380, 6), )

    
    PortInstance = property(__PortInstance.value, __PortInstance.set, None, None)

    
    # Element PrimitivePropertyInstance uses Python identifier PrimitivePropertyInstance
    __PrimitivePropertyInstance = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'PrimitivePropertyInstance'), 'PrimitivePropertyInstance', '__avm_ComponentInstance__PrimitivePropertyInstance', True, pyxb.utils.utility.Location(u'avm.xsd', 381, 6), )

    
    PrimitivePropertyInstance = property(__PrimitivePropertyInstance.value, __PrimitivePropertyInstance.set, None, None)

    
    # Element ConnectorInstance uses Python identifier ConnectorInstance
    __ConnectorInstance = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'ConnectorInstance'), 'ConnectorInstance', '__avm_ComponentInstance__ConnectorInstance', True, pyxb.utils.utility.Location(u'avm.xsd', 382, 6), )

    
    ConnectorInstance = property(__ConnectorInstance.value, __ConnectorInstance.set, None, None)

    
    # Attribute ComponentID uses Python identifier ComponentID
    __ComponentID = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ComponentID'), 'ComponentID', '__avm_ComponentInstance__ComponentID', pyxb.binding.datatypes.string)
    __ComponentID._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 384, 4)
    __ComponentID._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 384, 4)
    
    ComponentID = property(__ComponentID.value, __ComponentID.set, None, None)

    
    # Attribute ID uses Python identifier ID
    __ID = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ID'), 'ID', '__avm_ComponentInstance__ID', pyxb.binding.datatypes.ID)
    __ID._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 385, 4)
    __ID._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 385, 4)
    
    ID = property(__ID.value, __ID.set, None, None)

    
    # Attribute Name uses Python identifier Name
    __Name = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Name'), 'Name', '__avm_ComponentInstance__Name', pyxb.binding.datatypes.string)
    __Name._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 386, 4)
    __Name._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 386, 4)
    
    Name = property(__Name.value, __Name.set, None, None)

    
    # Attribute DesignSpaceSrcComponentID uses Python identifier DesignSpaceSrcComponentID
    __DesignSpaceSrcComponentID = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'DesignSpaceSrcComponentID'), 'DesignSpaceSrcComponentID', '__avm_ComponentInstance__DesignSpaceSrcComponentID', pyxb.binding.datatypes.string)
    __DesignSpaceSrcComponentID._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 387, 4)
    __DesignSpaceSrcComponentID._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 387, 4)
    
    DesignSpaceSrcComponentID = property(__DesignSpaceSrcComponentID.value, __DesignSpaceSrcComponentID.set, None, None)

    
    # Attribute XPosition uses Python identifier XPosition
    __XPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XPosition'), 'XPosition', '__avm_ComponentInstance__XPosition', pyxb.binding.datatypes.unsignedInt)
    __XPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 388, 4)
    __XPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 388, 4)
    
    XPosition = property(__XPosition.value, __XPosition.set, None, None)

    
    # Attribute YPosition uses Python identifier YPosition
    __YPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YPosition'), 'YPosition', '__avm_ComponentInstance__YPosition', pyxb.binding.datatypes.unsignedInt)
    __YPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 389, 4)
    __YPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 389, 4)
    
    YPosition = property(__YPosition.value, __YPosition.set, None, None)

    _ElementMap.update({
        __PortInstance.name() : __PortInstance,
        __PrimitivePropertyInstance.name() : __PrimitivePropertyInstance,
        __ConnectorInstance.name() : __ConnectorInstance
    })
    _AttributeMap.update({
        __ComponentID.name() : __ComponentID,
        __ID.name() : __ID,
        __Name.name() : __Name,
        __DesignSpaceSrcComponentID.name() : __DesignSpaceSrcComponentID,
        __XPosition.name() : __XPosition,
        __YPosition.name() : __YPosition
    })
Namespace.addCategoryObject('typeBinding', u'ComponentInstance', ComponentInstance_)


# Complex type {avm}ComponentPrimitivePropertyInstance with content type ELEMENT_ONLY
class ComponentPrimitivePropertyInstance_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}ComponentPrimitivePropertyInstance with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ComponentPrimitivePropertyInstance')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 398, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Element Value uses Python identifier Value
    __Value = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Value'), 'Value', '__avm_ComponentPrimitivePropertyInstance__Value', False, pyxb.utils.utility.Location(u'avm.xsd', 400, 6), )

    
    Value = property(__Value.value, __Value.set, None, None)

    
    # Attribute IDinComponentModel uses Python identifier IDinComponentModel
    __IDinComponentModel = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'IDinComponentModel'), 'IDinComponentModel', '__avm_ComponentPrimitivePropertyInstance__IDinComponentModel', pyxb.binding.datatypes.string, required=True)
    __IDinComponentModel._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 402, 4)
    __IDinComponentModel._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 402, 4)
    
    IDinComponentModel = property(__IDinComponentModel.value, __IDinComponentModel.set, None, None)

    _ElementMap.update({
        __Value.name() : __Value
    })
    _AttributeMap.update({
        __IDinComponentModel.name() : __IDinComponentModel
    })
Namespace.addCategoryObject('typeBinding', u'ComponentPrimitivePropertyInstance', ComponentPrimitivePropertyInstance_)


# Complex type {avm}ValueNode with content type EMPTY
class ValueNode_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}ValueNode with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ValueNode')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 468, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Attribute ID uses Python identifier ID
    __ID = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ID'), 'ID', '__avm_ValueNode__ID', pyxb.binding.datatypes.ID)
    __ID._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 469, 4)
    __ID._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 469, 4)
    
    ID = property(__ID.value, __ID.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __ID.name() : __ID
    })
Namespace.addCategoryObject('typeBinding', u'ValueNode', ValueNode_)


# Complex type {avm}Operand with content type EMPTY
class Operand_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}Operand with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Operand')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 481, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Attribute Symbol uses Python identifier Symbol
    __Symbol = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Symbol'), 'Symbol', '__avm_Operand__Symbol', pyxb.binding.datatypes.string, required=True)
    __Symbol._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 482, 4)
    __Symbol._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 482, 4)
    
    Symbol = property(__Symbol.value, __Symbol.set, None, None)

    
    # Attribute ValueSource uses Python identifier ValueSource
    __ValueSource = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ValueSource'), 'ValueSource', '__avm_Operand__ValueSource', pyxb.binding.datatypes.anyURI, required=True)
    __ValueSource._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 483, 4)
    __ValueSource._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 483, 4)
    
    ValueSource = property(__ValueSource.value, __ValueSource.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __Symbol.name() : __Symbol,
        __ValueSource.name() : __ValueSource
    })
Namespace.addCategoryObject('typeBinding', u'Operand', Operand_)


# Complex type {avm}ConnectorFeature with content type EMPTY
class ConnectorFeature_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}ConnectorFeature with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ConnectorFeature')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 501, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'ConnectorFeature', ConnectorFeature_)


# Complex type {avm}ContainerFeature with content type EMPTY
class ContainerFeature_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}ContainerFeature with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ContainerFeature')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 502, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'ContainerFeature', ContainerFeature_)


# Complex type {avm}DomainMapping with content type EMPTY
class DomainMapping_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}DomainMapping with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'DomainMapping')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 503, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'DomainMapping', DomainMapping_)


# Complex type {avm}TestBench with content type ELEMENT_ONLY
class TestBench_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}TestBench with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'TestBench')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 504, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Element TopLevelSystemUnderTest uses Python identifier TopLevelSystemUnderTest
    __TopLevelSystemUnderTest = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'TopLevelSystemUnderTest'), 'TopLevelSystemUnderTest', '__avm_TestBench__TopLevelSystemUnderTest', False, pyxb.utils.utility.Location(u'avm.xsd', 506, 6), )

    
    TopLevelSystemUnderTest = property(__TopLevelSystemUnderTest.value, __TopLevelSystemUnderTest.set, None, None)

    
    # Element Parameter uses Python identifier Parameter
    __Parameter = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Parameter'), 'Parameter', '__avm_TestBench__Parameter', True, pyxb.utils.utility.Location(u'avm.xsd', 507, 6), )

    
    Parameter = property(__Parameter.value, __Parameter.set, None, None)

    
    # Element Metric uses Python identifier Metric
    __Metric = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Metric'), 'Metric', '__avm_TestBench__Metric', True, pyxb.utils.utility.Location(u'avm.xsd', 508, 6), )

    
    Metric = property(__Metric.value, __Metric.set, None, None)

    
    # Element TestInjectionPoint uses Python identifier TestInjectionPoint
    __TestInjectionPoint = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'TestInjectionPoint'), 'TestInjectionPoint', '__avm_TestBench__TestInjectionPoint', True, pyxb.utils.utility.Location(u'avm.xsd', 509, 6), )

    
    TestInjectionPoint = property(__TestInjectionPoint.value, __TestInjectionPoint.set, None, None)

    
    # Element TestComponent uses Python identifier TestComponent
    __TestComponent = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'TestComponent'), 'TestComponent', '__avm_TestBench__TestComponent', True, pyxb.utils.utility.Location(u'avm.xsd', 510, 6), )

    
    TestComponent = property(__TestComponent.value, __TestComponent.set, None, None)

    
    # Element Workflow uses Python identifier Workflow
    __Workflow = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Workflow'), 'Workflow', '__avm_TestBench__Workflow', False, pyxb.utils.utility.Location(u'avm.xsd', 511, 6), )

    
    Workflow = property(__Workflow.value, __Workflow.set, None, None)

    
    # Element Settings uses Python identifier Settings
    __Settings = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Settings'), 'Settings', '__avm_TestBench__Settings', True, pyxb.utils.utility.Location(u'avm.xsd', 512, 6), )

    
    Settings = property(__Settings.value, __Settings.set, None, None)

    
    # Element TestStructure uses Python identifier TestStructure
    __TestStructure = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'TestStructure'), 'TestStructure', '__avm_TestBench__TestStructure', True, pyxb.utils.utility.Location(u'avm.xsd', 513, 6), )

    
    TestStructure = property(__TestStructure.value, __TestStructure.set, None, None)

    
    # Attribute Name uses Python identifier Name
    __Name = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Name'), 'Name', '__avm_TestBench__Name', pyxb.binding.datatypes.string, required=True)
    __Name._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 515, 4)
    __Name._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 515, 4)
    
    Name = property(__Name.value, __Name.set, None, None)

    _ElementMap.update({
        __TopLevelSystemUnderTest.name() : __TopLevelSystemUnderTest,
        __Parameter.name() : __Parameter,
        __Metric.name() : __Metric,
        __TestInjectionPoint.name() : __TestInjectionPoint,
        __TestComponent.name() : __TestComponent,
        __Workflow.name() : __Workflow,
        __Settings.name() : __Settings,
        __TestStructure.name() : __TestStructure
    })
    _AttributeMap.update({
        __Name.name() : __Name
    })
Namespace.addCategoryObject('typeBinding', u'TestBench', TestBench_)


# Complex type {avm}ContainerInstanceBase with content type EMPTY
class ContainerInstanceBase_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}ContainerInstanceBase with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ContainerInstanceBase')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 534, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Attribute IDinSourceModel uses Python identifier IDinSourceModel
    __IDinSourceModel = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'IDinSourceModel'), 'IDinSourceModel', '__avm_ContainerInstanceBase__IDinSourceModel', pyxb.binding.datatypes.string, required=True)
    __IDinSourceModel._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 535, 4)
    __IDinSourceModel._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 535, 4)
    
    IDinSourceModel = property(__IDinSourceModel.value, __IDinSourceModel.set, None, None)

    
    # Attribute XPosition uses Python identifier XPosition
    __XPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XPosition'), 'XPosition', '__avm_ContainerInstanceBase__XPosition', pyxb.binding.datatypes.unsignedInt)
    __XPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 536, 4)
    __XPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 536, 4)
    
    XPosition = property(__XPosition.value, __XPosition.set, None, None)

    
    # Attribute YPosition uses Python identifier YPosition
    __YPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YPosition'), 'YPosition', '__avm_ContainerInstanceBase__YPosition', pyxb.binding.datatypes.unsignedInt)
    __YPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 537, 4)
    __YPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 537, 4)
    
    YPosition = property(__YPosition.value, __YPosition.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __IDinSourceModel.name() : __IDinSourceModel,
        __XPosition.name() : __XPosition,
        __YPosition.name() : __YPosition
    })
Namespace.addCategoryObject('typeBinding', u'ContainerInstanceBase', ContainerInstanceBase_)


# Complex type {avm}TestBenchValueBase with content type ELEMENT_ONLY
class TestBenchValueBase_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}TestBenchValueBase with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'TestBenchValueBase')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 539, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Element Value uses Python identifier Value
    __Value = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Value'), 'Value', '__avm_TestBenchValueBase__Value', False, pyxb.utils.utility.Location(u'avm.xsd', 541, 6), )

    
    Value = property(__Value.value, __Value.set, None, None)

    
    # Attribute ID uses Python identifier ID
    __ID = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ID'), 'ID', '__avm_TestBenchValueBase__ID', pyxb.binding.datatypes.string, required=True)
    __ID._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 543, 4)
    __ID._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 543, 4)
    
    ID = property(__ID.value, __ID.set, None, None)

    
    # Attribute Name uses Python identifier Name
    __Name = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Name'), 'Name', '__avm_TestBenchValueBase__Name', pyxb.binding.datatypes.string, required=True)
    __Name._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 544, 4)
    __Name._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 544, 4)
    
    Name = property(__Name.value, __Name.set, None, None)

    
    # Attribute Notes uses Python identifier Notes
    __Notes = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Notes'), 'Notes', '__avm_TestBenchValueBase__Notes', pyxb.binding.datatypes.string)
    __Notes._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 545, 4)
    __Notes._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 545, 4)
    
    Notes = property(__Notes.value, __Notes.set, None, None)

    
    # Attribute XPosition uses Python identifier XPosition
    __XPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XPosition'), 'XPosition', '__avm_TestBenchValueBase__XPosition', pyxb.binding.datatypes.unsignedInt)
    __XPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 546, 4)
    __XPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 546, 4)
    
    XPosition = property(__XPosition.value, __XPosition.set, None, None)

    
    # Attribute YPosition uses Python identifier YPosition
    __YPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YPosition'), 'YPosition', '__avm_TestBenchValueBase__YPosition', pyxb.binding.datatypes.unsignedInt)
    __YPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 547, 4)
    __YPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 547, 4)
    
    YPosition = property(__YPosition.value, __YPosition.set, None, None)

    _ElementMap.update({
        __Value.name() : __Value
    })
    _AttributeMap.update({
        __ID.name() : __ID,
        __Name.name() : __Name,
        __Notes.name() : __Notes,
        __XPosition.name() : __XPosition,
        __YPosition.name() : __YPosition
    })
Namespace.addCategoryObject('typeBinding', u'TestBenchValueBase', TestBenchValueBase_)


# Complex type {avm}Workflow with content type ELEMENT_ONLY
class Workflow_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}Workflow with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Workflow')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 554, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Element Task uses Python identifier Task
    __Task = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Task'), 'Task', '__avm_Workflow__Task', True, pyxb.utils.utility.Location(u'avm.xsd', 556, 6), )

    
    Task = property(__Task.value, __Task.set, None, None)

    
    # Attribute Name uses Python identifier Name
    __Name = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Name'), 'Name', '__avm_Workflow__Name', pyxb.binding.datatypes.string)
    __Name._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 558, 4)
    __Name._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 558, 4)
    
    Name = property(__Name.value, __Name.set, None, None)

    _ElementMap.update({
        __Task.name() : __Task
    })
    _AttributeMap.update({
        __Name.name() : __Name
    })
Namespace.addCategoryObject('typeBinding', u'Workflow', Workflow_)


# Complex type {avm}WorkflowTaskBase with content type EMPTY
class WorkflowTaskBase_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}WorkflowTaskBase with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'WorkflowTaskBase')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 560, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Attribute Name uses Python identifier Name
    __Name = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Name'), 'Name', '__avm_WorkflowTaskBase__Name', pyxb.binding.datatypes.string)
    __Name._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 561, 4)
    __Name._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 561, 4)
    
    Name = property(__Name.value, __Name.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __Name.name() : __Name
    })
Namespace.addCategoryObject('typeBinding', u'WorkflowTaskBase', WorkflowTaskBase_)


# Complex type {avm}Settings with content type EMPTY
class Settings_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}Settings with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Settings')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 579, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'Settings', Settings_)


# Complex type {avm}DesignDomainFeature with content type EMPTY
class DesignDomainFeature_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}DesignDomainFeature with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'DesignDomainFeature')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 591, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'DesignDomainFeature', DesignDomainFeature_)


# Complex type {avm}Value with content type ELEMENT_ONLY
class Value_ (ValueNode_):
    """Complex type {avm}Value with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Value')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 110, 2)
    _ElementMap = ValueNode_._ElementMap.copy()
    _AttributeMap = ValueNode_._AttributeMap.copy()
    # Base type is ValueNode_
    
    # Element ValueExpression uses Python identifier ValueExpression
    __ValueExpression = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'ValueExpression'), 'ValueExpression', '__avm_Value__ValueExpression', False, pyxb.utils.utility.Location(u'avm.xsd', 114, 10), )

    
    ValueExpression = property(__ValueExpression.value, __ValueExpression.set, None, None)

    
    # Element DataSource uses Python identifier DataSource
    __DataSource = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'DataSource'), 'DataSource', '__avm_Value__DataSource', True, pyxb.utils.utility.Location(u'avm.xsd', 115, 10), )

    
    DataSource = property(__DataSource.value, __DataSource.set, None, None)

    
    # Attribute DimensionType uses Python identifier DimensionType
    __DimensionType = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'DimensionType'), 'DimensionType', '__avm_Value__DimensionType', DimensionTypeEnum)
    __DimensionType._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 117, 8)
    __DimensionType._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 117, 8)
    
    DimensionType = property(__DimensionType.value, __DimensionType.set, None, None)

    
    # Attribute DataType uses Python identifier DataType
    __DataType = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'DataType'), 'DataType', '__avm_Value__DataType', DataTypeEnum)
    __DataType._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 118, 8)
    __DataType._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 118, 8)
    
    DataType = property(__DataType.value, __DataType.set, None, None)

    
    # Attribute Dimensions uses Python identifier Dimensions
    __Dimensions = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Dimensions'), 'Dimensions', '__avm_Value__Dimensions', pyxb.binding.datatypes.string)
    __Dimensions._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 119, 8)
    __Dimensions._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 119, 8)
    
    Dimensions = property(__Dimensions.value, __Dimensions.set, None, None)

    
    # Attribute Unit uses Python identifier Unit
    __Unit = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Unit'), 'Unit', '__avm_Value__Unit', pyxb.binding.datatypes.string)
    __Unit._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 120, 8)
    __Unit._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 120, 8)
    
    Unit = property(__Unit.value, __Unit.set, None, None)

    
    # Attribute ID inherited from {avm}ValueNode
    _ElementMap.update({
        __ValueExpression.name() : __ValueExpression,
        __DataSource.name() : __DataSource
    })
    _AttributeMap.update({
        __DimensionType.name() : __DimensionType,
        __DataType.name() : __DataType,
        __Dimensions.name() : __Dimensions,
        __Unit.name() : __Unit
    })
Namespace.addCategoryObject('typeBinding', u'Value', Value_)


# Complex type {avm}FixedValue with content type ELEMENT_ONLY
class FixedValue_ (ValueExpressionType_):
    """Complex type {avm}FixedValue with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'FixedValue')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 124, 2)
    _ElementMap = ValueExpressionType_._ElementMap.copy()
    _AttributeMap = ValueExpressionType_._AttributeMap.copy()
    # Base type is ValueExpressionType_
    
    # Element Value uses Python identifier Value
    __Value = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Value'), 'Value', '__avm_FixedValue__Value', False, pyxb.utils.utility.Location(u'avm.xsd', 128, 10), )

    
    Value = property(__Value.value, __Value.set, None, None)

    
    # Attribute Uncertainty uses Python identifier Uncertainty
    __Uncertainty = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Uncertainty'), 'Uncertainty', '__avm_FixedValue__Uncertainty', pyxb.binding.datatypes.float)
    __Uncertainty._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 130, 8)
    __Uncertainty._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 130, 8)
    
    Uncertainty = property(__Uncertainty.value, __Uncertainty.set, None, None)

    _ElementMap.update({
        __Value.name() : __Value
    })
    _AttributeMap.update({
        __Uncertainty.name() : __Uncertainty
    })
Namespace.addCategoryObject('typeBinding', u'FixedValue', FixedValue_)


# Complex type {avm}CalculatedValue with content type ELEMENT_ONLY
class CalculatedValue_ (ValueExpressionType_):
    """Complex type {avm}CalculatedValue with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'CalculatedValue')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 134, 2)
    _ElementMap = ValueExpressionType_._ElementMap.copy()
    _AttributeMap = ValueExpressionType_._AttributeMap.copy()
    # Base type is ValueExpressionType_
    
    # Element Expression uses Python identifier Expression
    __Expression = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Expression'), 'Expression', '__avm_CalculatedValue__Expression', False, pyxb.utils.utility.Location(u'avm.xsd', 138, 10), )

    
    Expression = property(__Expression.value, __Expression.set, None, None)

    
    # Attribute Type uses Python identifier Type
    __Type = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Type'), 'Type', '__avm_CalculatedValue__Type', CalculationTypeEnum, required=True)
    __Type._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 140, 8)
    __Type._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 140, 8)
    
    Type = property(__Type.value, __Type.set, None, None)

    _ElementMap.update({
        __Expression.name() : __Expression
    })
    _AttributeMap.update({
        __Type.name() : __Type
    })
Namespace.addCategoryObject('typeBinding', u'CalculatedValue', CalculatedValue_)


# Complex type {avm}DerivedValue with content type EMPTY
class DerivedValue_ (ValueExpressionType_):
    """Complex type {avm}DerivedValue with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'DerivedValue')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 144, 2)
    _ElementMap = ValueExpressionType_._ElementMap.copy()
    _AttributeMap = ValueExpressionType_._AttributeMap.copy()
    # Base type is ValueExpressionType_
    
    # Attribute ValueSource uses Python identifier ValueSource
    __ValueSource = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ValueSource'), 'ValueSource', '__avm_DerivedValue__ValueSource', pyxb.binding.datatypes.IDREF, required=True)
    __ValueSource._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 147, 8)
    __ValueSource._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 147, 8)
    
    ValueSource = property(__ValueSource.value, __ValueSource.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __ValueSource.name() : __ValueSource
    })
Namespace.addCategoryObject('typeBinding', u'DerivedValue', DerivedValue_)


# Complex type {avm}ParametricValue with content type ELEMENT_ONLY
class ParametricValue_ (ValueExpressionType_):
    """Complex type {avm}ParametricValue with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ParametricValue')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 199, 2)
    _ElementMap = ValueExpressionType_._ElementMap.copy()
    _AttributeMap = ValueExpressionType_._AttributeMap.copy()
    # Base type is ValueExpressionType_
    
    # Element Default uses Python identifier Default
    __Default = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Default'), 'Default', '__avm_ParametricValue__Default', False, pyxb.utils.utility.Location(u'avm.xsd', 203, 10), )

    
    Default = property(__Default.value, __Default.set, None, None)

    
    # Element Maximum uses Python identifier Maximum
    __Maximum = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Maximum'), 'Maximum', '__avm_ParametricValue__Maximum', False, pyxb.utils.utility.Location(u'avm.xsd', 204, 10), )

    
    Maximum = property(__Maximum.value, __Maximum.set, None, None)

    
    # Element Minimum uses Python identifier Minimum
    __Minimum = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Minimum'), 'Minimum', '__avm_ParametricValue__Minimum', False, pyxb.utils.utility.Location(u'avm.xsd', 205, 10), )

    
    Minimum = property(__Minimum.value, __Minimum.set, None, None)

    
    # Element AssignedValue uses Python identifier AssignedValue
    __AssignedValue = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'AssignedValue'), 'AssignedValue', '__avm_ParametricValue__AssignedValue', False, pyxb.utils.utility.Location(u'avm.xsd', 206, 10), )

    
    AssignedValue = property(__AssignedValue.value, __AssignedValue.set, None, None)

    _ElementMap.update({
        __Default.name() : __Default,
        __Maximum.name() : __Maximum,
        __Minimum.name() : __Minimum,
        __AssignedValue.name() : __AssignedValue
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'ParametricValue', ParametricValue_)


# Complex type {avm}ProbabilisticValue with content type EMPTY
class ProbabilisticValue_ (ValueExpressionType_):
    """Complex type {avm}ProbabilisticValue with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ProbabilisticValue')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 212, 2)
    _ElementMap = ValueExpressionType_._ElementMap.copy()
    _AttributeMap = ValueExpressionType_._AttributeMap.copy()
    # Base type is ValueExpressionType_
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'ProbabilisticValue', ProbabilisticValue_)


# Complex type {avm}SecurityClassification with content type EMPTY
class SecurityClassification_ (DistributionRestriction_):
    """Complex type {avm}SecurityClassification with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'SecurityClassification')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 251, 2)
    _ElementMap = DistributionRestriction_._ElementMap.copy()
    _AttributeMap = DistributionRestriction_._AttributeMap.copy()
    # Base type is DistributionRestriction_
    
    # Attribute Notes inherited from {avm}DistributionRestriction
    
    # Attribute Level uses Python identifier Level
    __Level = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Level'), 'Level', '__avm_SecurityClassification__Level', pyxb.binding.datatypes.string)
    __Level._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 254, 8)
    __Level._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 254, 8)
    
    Level = property(__Level.value, __Level.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __Level.name() : __Level
    })
Namespace.addCategoryObject('typeBinding', u'SecurityClassification', SecurityClassification_)


# Complex type {avm}Proprietary with content type EMPTY
class Proprietary_ (DistributionRestriction_):
    """Complex type {avm}Proprietary with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Proprietary')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 258, 2)
    _ElementMap = DistributionRestriction_._ElementMap.copy()
    _AttributeMap = DistributionRestriction_._AttributeMap.copy()
    # Base type is DistributionRestriction_
    
    # Attribute Notes inherited from {avm}DistributionRestriction
    
    # Attribute Organization uses Python identifier Organization
    __Organization = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Organization'), 'Organization', '__avm_Proprietary__Organization', pyxb.binding.datatypes.string, required=True)
    __Organization._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 261, 8)
    __Organization._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 261, 8)
    
    Organization = property(__Organization.value, __Organization.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __Organization.name() : __Organization
    })
Namespace.addCategoryObject('typeBinding', u'Proprietary', Proprietary_)


# Complex type {avm}ITAR with content type EMPTY
class ITAR_ (DistributionRestriction_):
    """Complex type {avm}ITAR with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ITAR')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 265, 2)
    _ElementMap = DistributionRestriction_._ElementMap.copy()
    _AttributeMap = DistributionRestriction_._AttributeMap.copy()
    # Base type is DistributionRestriction_
    
    # Attribute Notes inherited from {avm}DistributionRestriction
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'ITAR', ITAR_)


# Complex type {avm}PrimitiveProperty with content type ELEMENT_ONLY
class PrimitiveProperty_ (Property_):
    """Complex type {avm}PrimitiveProperty with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'PrimitiveProperty')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 284, 2)
    _ElementMap = Property_._ElementMap.copy()
    _AttributeMap = Property_._AttributeMap.copy()
    # Base type is Property_
    
    # Element Value uses Python identifier Value
    __Value = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Value'), 'Value', '__avm_PrimitiveProperty__Value', False, pyxb.utils.utility.Location(u'avm.xsd', 288, 10), )

    
    Value = property(__Value.value, __Value.set, None, None)

    
    # Attribute Name inherited from {avm}Property
    
    # Attribute OnDataSheet inherited from {avm}Property
    
    # Attribute Notes inherited from {avm}Property
    
    # Attribute Definition inherited from {avm}Property
    
    # Attribute ID inherited from {avm}Property
    
    # Attribute XPosition inherited from {avm}Property
    
    # Attribute YPosition inherited from {avm}Property
    _ElementMap.update({
        __Value.name() : __Value
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'PrimitiveProperty', PrimitiveProperty_)


# Complex type {avm}CompoundProperty with content type ELEMENT_ONLY
class CompoundProperty_ (Property_):
    """Complex type {avm}CompoundProperty with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'CompoundProperty')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 293, 2)
    _ElementMap = Property_._ElementMap.copy()
    _AttributeMap = Property_._AttributeMap.copy()
    # Base type is Property_
    
    # Element CompoundProperty uses Python identifier CompoundProperty
    __CompoundProperty = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'CompoundProperty'), 'CompoundProperty', '__avm_CompoundProperty__CompoundProperty', True, pyxb.utils.utility.Location(u'avm.xsd', 297, 10), )

    
    CompoundProperty = property(__CompoundProperty.value, __CompoundProperty.set, None, None)

    
    # Element PrimitiveProperty uses Python identifier PrimitiveProperty
    __PrimitiveProperty = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'PrimitiveProperty'), 'PrimitiveProperty', '__avm_CompoundProperty__PrimitiveProperty', True, pyxb.utils.utility.Location(u'avm.xsd', 298, 10), )

    
    PrimitiveProperty = property(__PrimitiveProperty.value, __PrimitiveProperty.set, None, None)

    
    # Attribute Name inherited from {avm}Property
    
    # Attribute OnDataSheet inherited from {avm}Property
    
    # Attribute Notes inherited from {avm}Property
    
    # Attribute Definition inherited from {avm}Property
    
    # Attribute ID inherited from {avm}Property
    
    # Attribute XPosition inherited from {avm}Property
    
    # Attribute YPosition inherited from {avm}Property
    _ElementMap.update({
        __CompoundProperty.name() : __CompoundProperty,
        __PrimitiveProperty.name() : __PrimitiveProperty
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'CompoundProperty', CompoundProperty_)


# Complex type {avm}ParametricEnumeratedValue with content type ELEMENT_ONLY
class ParametricEnumeratedValue_ (ValueExpressionType_):
    """Complex type {avm}ParametricEnumeratedValue with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ParametricEnumeratedValue')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 303, 2)
    _ElementMap = ValueExpressionType_._ElementMap.copy()
    _AttributeMap = ValueExpressionType_._AttributeMap.copy()
    # Base type is ValueExpressionType_
    
    # Element AssignedValue uses Python identifier AssignedValue
    __AssignedValue = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'AssignedValue'), 'AssignedValue', '__avm_ParametricEnumeratedValue__AssignedValue', False, pyxb.utils.utility.Location(u'avm.xsd', 307, 10), )

    
    AssignedValue = property(__AssignedValue.value, __AssignedValue.set, None, None)

    
    # Element Enum uses Python identifier Enum
    __Enum = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Enum'), 'Enum', '__avm_ParametricEnumeratedValue__Enum', True, pyxb.utils.utility.Location(u'avm.xsd', 308, 10), )

    
    Enum = property(__Enum.value, __Enum.set, None, None)

    _ElementMap.update({
        __AssignedValue.name() : __AssignedValue,
        __Enum.name() : __Enum
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'ParametricEnumeratedValue', ParametricEnumeratedValue_)


# Complex type {avm}DataSource with content type EMPTY
class DataSource_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}DataSource with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'DataSource')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 319, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Attribute Notes uses Python identifier Notes
    __Notes = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Notes'), 'Notes', '__avm_DataSource__Notes', pyxb.binding.datatypes.string)
    __Notes._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 320, 4)
    __Notes._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 320, 4)
    
    Notes = property(__Notes.value, __Notes.set, None, None)

    
    # Attribute FromResource uses Python identifier FromResource
    __FromResource = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'FromResource'), 'FromResource', '__avm_DataSource__FromResource', STD_ANON)
    __FromResource._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 321, 4)
    __FromResource._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 321, 4)
    
    FromResource = property(__FromResource.value, __FromResource.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __Notes.name() : __Notes,
        __FromResource.name() : __FromResource
    })
Namespace.addCategoryObject('typeBinding', u'DataSource', DataSource_)


# Complex type {avm}Compound with content type ELEMENT_ONLY
class Compound_ (Container_):
    """Complex type {avm}Compound with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Compound')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 359, 2)
    _ElementMap = Container_._ElementMap.copy()
    _AttributeMap = Container_._AttributeMap.copy()
    # Base type is Container_
    
    # Element Container (Container) inherited from {avm}Container
    
    # Element Property (Property) inherited from {avm}Container
    
    # Element ComponentInstance (ComponentInstance) inherited from {avm}Container
    
    # Element Port (Port) inherited from {avm}Container
    
    # Element Connector (Connector) inherited from {avm}Container
    
    # Element JoinData (JoinData) inherited from {avm}Container
    
    # Element Formula (Formula) inherited from {avm}Container
    
    # Element ContainerFeature (ContainerFeature) inherited from {avm}Container
    
    # Element ResourceDependency (ResourceDependency) inherited from {avm}Container
    
    # Element DomainModel (DomainModel) inherited from {avm}Container
    
    # Element Resource (Resource) inherited from {avm}Container
    
    # Element Classifications (Classifications) inherited from {avm}Container
    
    # Attribute XPosition inherited from {avm}Container
    
    # Attribute Name inherited from {avm}Container
    
    # Attribute YPosition inherited from {avm}Container
    
    # Attribute ID inherited from {avm}Container
    
    # Attribute Description inherited from {avm}Container
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'Compound', Compound_)


# Complex type {avm}DesignSpaceContainer with content type ELEMENT_ONLY
class DesignSpaceContainer_ (Container_):
    """Complex type {avm}DesignSpaceContainer with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'DesignSpaceContainer')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 404, 2)
    _ElementMap = Container_._ElementMap.copy()
    _AttributeMap = Container_._AttributeMap.copy()
    # Base type is Container_
    
    # Element Container (Container) inherited from {avm}Container
    
    # Element Property (Property) inherited from {avm}Container
    
    # Element ComponentInstance (ComponentInstance) inherited from {avm}Container
    
    # Element Port (Port) inherited from {avm}Container
    
    # Element Connector (Connector) inherited from {avm}Container
    
    # Element JoinData (JoinData) inherited from {avm}Container
    
    # Element Formula (Formula) inherited from {avm}Container
    
    # Element ContainerFeature (ContainerFeature) inherited from {avm}Container
    
    # Element ResourceDependency (ResourceDependency) inherited from {avm}Container
    
    # Element DomainModel (DomainModel) inherited from {avm}Container
    
    # Element Resource (Resource) inherited from {avm}Container
    
    # Element Classifications (Classifications) inherited from {avm}Container
    
    # Attribute XPosition inherited from {avm}Container
    
    # Attribute Name inherited from {avm}Container
    
    # Attribute YPosition inherited from {avm}Container
    
    # Attribute ID inherited from {avm}Container
    
    # Attribute Description inherited from {avm}Container
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'DesignSpaceContainer', DesignSpaceContainer_)


# Complex type {avm}PortMapTarget with content type EMPTY
class PortMapTarget_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}PortMapTarget with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'PortMapTarget')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 409, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Attribute ID uses Python identifier ID
    __ID = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ID'), 'ID', '__avm_PortMapTarget__ID', pyxb.binding.datatypes.ID)
    __ID._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 410, 4)
    __ID._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 410, 4)
    
    ID = property(__ID.value, __ID.set, None, None)

    
    # Attribute PortMap uses Python identifier PortMap
    __PortMap = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'PortMap'), 'PortMap', '__avm_PortMapTarget__PortMap', STD_ANON_)
    __PortMap._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 411, 4)
    __PortMap._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 411, 4)
    
    PortMap = property(__PortMap.value, __PortMap.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __ID.name() : __ID,
        __PortMap.name() : __PortMap
    })
Namespace.addCategoryObject('typeBinding', u'PortMapTarget', PortMapTarget_)


# Complex type {avm}ConnectorCompositionTarget with content type EMPTY
class ConnectorCompositionTarget_ (pyxb.binding.basis.complexTypeDefinition):
    """Complex type {avm}ConnectorCompositionTarget with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ConnectorCompositionTarget')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 424, 2)
    _ElementMap = {}
    _AttributeMap = {}
    # Base type is pyxb.binding.datatypes.anyType
    
    # Attribute ConnectorComposition uses Python identifier ConnectorComposition
    __ConnectorComposition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ConnectorComposition'), 'ConnectorComposition', '__avm_ConnectorCompositionTarget__ConnectorComposition', STD_ANON_2)
    __ConnectorComposition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 425, 4)
    __ConnectorComposition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 425, 4)
    
    ConnectorComposition = property(__ConnectorComposition.value, __ConnectorComposition.set, None, None)

    
    # Attribute ID uses Python identifier ID
    __ID = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ID'), 'ID', '__avm_ConnectorCompositionTarget__ID', pyxb.binding.datatypes.string)
    __ID._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 430, 4)
    __ID._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 430, 4)
    
    ID = property(__ID.value, __ID.set, None, None)

    
    # Attribute ApplyJoinData uses Python identifier ApplyJoinData
    __ApplyJoinData = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ApplyJoinData'), 'ApplyJoinData', '__avm_ConnectorCompositionTarget__ApplyJoinData', STD_ANON_3)
    __ApplyJoinData._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 431, 4)
    __ApplyJoinData._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 431, 4)
    
    ApplyJoinData = property(__ApplyJoinData.value, __ApplyJoinData.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __ConnectorComposition.name() : __ConnectorComposition,
        __ID.name() : __ID,
        __ApplyJoinData.name() : __ApplyJoinData
    })
Namespace.addCategoryObject('typeBinding', u'ConnectorCompositionTarget', ConnectorCompositionTarget_)


# Complex type {avm}Formula with content type EMPTY
class Formula_ (ValueNode_):
    """Complex type {avm}Formula with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Formula')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 437, 2)
    _ElementMap = ValueNode_._ElementMap.copy()
    _AttributeMap = ValueNode_._AttributeMap.copy()
    # Base type is ValueNode_
    
    # Attribute Name uses Python identifier Name
    __Name = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Name'), 'Name', '__avm_Formula__Name', pyxb.binding.datatypes.string)
    __Name._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 440, 8)
    __Name._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 440, 8)
    
    Name = property(__Name.value, __Name.set, None, None)

    
    # Attribute XPosition uses Python identifier XPosition
    __XPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XPosition'), 'XPosition', '__avm_Formula__XPosition', pyxb.binding.datatypes.unsignedInt)
    __XPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 441, 8)
    __XPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 441, 8)
    
    XPosition = property(__XPosition.value, __XPosition.set, None, None)

    
    # Attribute YPosition uses Python identifier YPosition
    __YPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YPosition'), 'YPosition', '__avm_Formula__YPosition', pyxb.binding.datatypes.unsignedInt)
    __YPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 442, 8)
    __YPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 442, 8)
    
    YPosition = property(__YPosition.value, __YPosition.set, None, None)

    
    # Attribute ID inherited from {avm}ValueNode
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __Name.name() : __Name,
        __XPosition.name() : __XPosition,
        __YPosition.name() : __YPosition
    })
Namespace.addCategoryObject('typeBinding', u'Formula', Formula_)


# Complex type {avm}DoDDistributionStatement with content type EMPTY
class DoDDistributionStatement_ (DistributionRestriction_):
    """Complex type {avm}DoDDistributionStatement with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'DoDDistributionStatement')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 485, 2)
    _ElementMap = DistributionRestriction_._ElementMap.copy()
    _AttributeMap = DistributionRestriction_._AttributeMap.copy()
    # Base type is DistributionRestriction_
    
    # Attribute Notes inherited from {avm}DistributionRestriction
    
    # Attribute Type uses Python identifier Type
    __Type = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Type'), 'Type', '__avm_DoDDistributionStatement__Type', DoDDistributionStatementEnum, required=True)
    __Type._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 488, 8)
    __Type._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 488, 8)
    
    Type = property(__Type.value, __Type.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __Type.name() : __Type
    })
Namespace.addCategoryObject('typeBinding', u'DoDDistributionStatement', DoDDistributionStatement_)


# Complex type {avm}TopLevelSystemUnderTest with content type EMPTY
class TopLevelSystemUnderTest_ (ContainerInstanceBase_):
    """Complex type {avm}TopLevelSystemUnderTest with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'TopLevelSystemUnderTest')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 517, 2)
    _ElementMap = ContainerInstanceBase_._ElementMap.copy()
    _AttributeMap = ContainerInstanceBase_._AttributeMap.copy()
    # Base type is ContainerInstanceBase_
    
    # Attribute DesignID uses Python identifier DesignID
    __DesignID = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'DesignID'), 'DesignID', '__avm_TopLevelSystemUnderTest__DesignID', pyxb.binding.datatypes.string, required=True)
    __DesignID._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 520, 8)
    __DesignID._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 520, 8)
    
    DesignID = property(__DesignID.value, __DesignID.set, None, None)

    
    # Attribute IDinSourceModel inherited from {avm}ContainerInstanceBase
    
    # Attribute XPosition inherited from {avm}ContainerInstanceBase
    
    # Attribute YPosition inherited from {avm}ContainerInstanceBase
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __DesignID.name() : __DesignID
    })
Namespace.addCategoryObject('typeBinding', u'TopLevelSystemUnderTest', TopLevelSystemUnderTest_)


# Complex type {avm}Parameter with content type ELEMENT_ONLY
class Parameter_ (TestBenchValueBase_):
    """Complex type {avm}Parameter with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Parameter')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 524, 2)
    _ElementMap = TestBenchValueBase_._ElementMap.copy()
    _AttributeMap = TestBenchValueBase_._AttributeMap.copy()
    # Base type is TestBenchValueBase_
    
    # Element Value (Value) inherited from {avm}TestBenchValueBase
    
    # Attribute ID inherited from {avm}TestBenchValueBase
    
    # Attribute Name inherited from {avm}TestBenchValueBase
    
    # Attribute Notes inherited from {avm}TestBenchValueBase
    
    # Attribute XPosition inherited from {avm}TestBenchValueBase
    
    # Attribute YPosition inherited from {avm}TestBenchValueBase
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'Parameter', Parameter_)


# Complex type {avm}Metric with content type ELEMENT_ONLY
class Metric_ (TestBenchValueBase_):
    """Complex type {avm}Metric with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Metric')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 529, 2)
    _ElementMap = TestBenchValueBase_._ElementMap.copy()
    _AttributeMap = TestBenchValueBase_._AttributeMap.copy()
    # Base type is TestBenchValueBase_
    
    # Element Value (Value) inherited from {avm}TestBenchValueBase
    
    # Attribute ID inherited from {avm}TestBenchValueBase
    
    # Attribute Name inherited from {avm}TestBenchValueBase
    
    # Attribute Notes inherited from {avm}TestBenchValueBase
    
    # Attribute XPosition inherited from {avm}TestBenchValueBase
    
    # Attribute YPosition inherited from {avm}TestBenchValueBase
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'Metric', Metric_)


# Complex type {avm}TestInjectionPoint with content type EMPTY
class TestInjectionPoint_ (ContainerInstanceBase_):
    """Complex type {avm}TestInjectionPoint with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'TestInjectionPoint')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 549, 2)
    _ElementMap = ContainerInstanceBase_._ElementMap.copy()
    _AttributeMap = ContainerInstanceBase_._AttributeMap.copy()
    # Base type is ContainerInstanceBase_
    
    # Attribute IDinSourceModel inherited from {avm}ContainerInstanceBase
    
    # Attribute XPosition inherited from {avm}ContainerInstanceBase
    
    # Attribute YPosition inherited from {avm}ContainerInstanceBase
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'TestInjectionPoint', TestInjectionPoint_)


# Complex type {avm}InterpreterTask with content type EMPTY
class InterpreterTask_ (WorkflowTaskBase_):
    """Complex type {avm}InterpreterTask with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'InterpreterTask')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 563, 2)
    _ElementMap = WorkflowTaskBase_._ElementMap.copy()
    _AttributeMap = WorkflowTaskBase_._AttributeMap.copy()
    # Base type is WorkflowTaskBase_
    
    # Attribute Name inherited from {avm}WorkflowTaskBase
    
    # Attribute COMName uses Python identifier COMName
    __COMName = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'COMName'), 'COMName', '__avm_InterpreterTask__COMName', pyxb.binding.datatypes.string, required=True)
    __COMName._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 566, 8)
    __COMName._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 566, 8)
    
    COMName = property(__COMName.value, __COMName.set, None, None)

    
    # Attribute Parameters uses Python identifier Parameters
    __Parameters = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Parameters'), 'Parameters', '__avm_InterpreterTask__Parameters', pyxb.binding.datatypes.string)
    __Parameters._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 567, 8)
    __Parameters._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 567, 8)
    
    Parameters = property(__Parameters.value, __Parameters.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __COMName.name() : __COMName,
        __Parameters.name() : __Parameters
    })
Namespace.addCategoryObject('typeBinding', u'InterpreterTask', InterpreterTask_)


# Complex type {avm}ExecutionTask with content type EMPTY
class ExecutionTask_ (WorkflowTaskBase_):
    """Complex type {avm}ExecutionTask with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ExecutionTask')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 571, 2)
    _ElementMap = WorkflowTaskBase_._ElementMap.copy()
    _AttributeMap = WorkflowTaskBase_._AttributeMap.copy()
    # Base type is WorkflowTaskBase_
    
    # Attribute Name inherited from {avm}WorkflowTaskBase
    
    # Attribute Description uses Python identifier Description
    __Description = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Description'), 'Description', '__avm_ExecutionTask__Description', pyxb.binding.datatypes.string)
    __Description._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 574, 8)
    __Description._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 574, 8)
    
    Description = property(__Description.value, __Description.set, None, None)

    
    # Attribute Invocation uses Python identifier Invocation
    __Invocation = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Invocation'), 'Invocation', '__avm_ExecutionTask__Invocation', pyxb.binding.datatypes.string, required=True)
    __Invocation._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 575, 8)
    __Invocation._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 575, 8)
    
    Invocation = property(__Invocation.value, __Invocation.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __Description.name() : __Description,
        __Invocation.name() : __Invocation
    })
Namespace.addCategoryObject('typeBinding', u'ExecutionTask', ExecutionTask_)


# Complex type {avm}ValueFlowMux with content type EMPTY
class ValueFlowMux_ (ValueNode_):
    """Complex type {avm}ValueFlowMux with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ValueFlowMux')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 580, 2)
    _ElementMap = ValueNode_._ElementMap.copy()
    _AttributeMap = ValueNode_._AttributeMap.copy()
    # Base type is ValueNode_
    
    # Attribute ID inherited from {avm}ValueNode
    
    # Attribute Source uses Python identifier Source
    __Source = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Source'), 'Source', '__avm_ValueFlowMux__Source', STD_ANON_5)
    __Source._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 583, 8)
    __Source._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 583, 8)
    
    Source = property(__Source.value, __Source.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __Source.name() : __Source
    })
Namespace.addCategoryObject('typeBinding', u'ValueFlowMux', ValueFlowMux_)


# Complex type {avm}GenericDomainModel with content type ELEMENT_ONLY
class GenericDomainModel_ (DomainModel_):
    """Complex type {avm}GenericDomainModel with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'GenericDomainModel')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 592, 2)
    _ElementMap = DomainModel_._ElementMap.copy()
    _AttributeMap = DomainModel_._AttributeMap.copy()
    # Base type is DomainModel_
    
    # Element GenericDomainModelPort uses Python identifier GenericDomainModelPort
    __GenericDomainModelPort = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'GenericDomainModelPort'), 'GenericDomainModelPort', '__avm_GenericDomainModel__GenericDomainModelPort', True, pyxb.utils.utility.Location(u'avm.xsd', 596, 10), )

    
    GenericDomainModelPort = property(__GenericDomainModelPort.value, __GenericDomainModelPort.set, None, None)

    
    # Element GenericDomainModelParameter uses Python identifier GenericDomainModelParameter
    __GenericDomainModelParameter = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'GenericDomainModelParameter'), 'GenericDomainModelParameter', '__avm_GenericDomainModel__GenericDomainModelParameter', True, pyxb.utils.utility.Location(u'avm.xsd', 597, 10), )

    
    GenericDomainModelParameter = property(__GenericDomainModelParameter.value, __GenericDomainModelParameter.set, None, None)

    
    # Attribute UsesResource inherited from {avm}DomainModel
    
    # Attribute Author inherited from {avm}DomainModel
    
    # Attribute Notes inherited from {avm}DomainModel
    
    # Attribute XPosition inherited from {avm}DomainModel
    
    # Attribute YPosition inherited from {avm}DomainModel
    
    # Attribute Name inherited from {avm}DomainModel
    
    # Attribute ID inherited from {avm}DomainModel
    
    # Attribute Type uses Python identifier Type
    __Type = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Type'), 'Type', '__avm_GenericDomainModel__Type', pyxb.binding.datatypes.string)
    __Type._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 599, 8)
    __Type._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 599, 8)
    
    Type = property(__Type.value, __Type.set, None, None)

    
    # Attribute Domain uses Python identifier Domain
    __Domain = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Domain'), 'Domain', '__avm_GenericDomainModel__Domain', pyxb.binding.datatypes.string)
    __Domain._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 600, 8)
    __Domain._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 600, 8)
    
    Domain = property(__Domain.value, __Domain.set, None, None)

    
    # Attribute GenericAttribute0 uses Python identifier GenericAttribute0
    __GenericAttribute0 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute0'), 'GenericAttribute0', '__avm_GenericDomainModel__GenericAttribute0', pyxb.binding.datatypes.string)
    __GenericAttribute0._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 601, 8)
    __GenericAttribute0._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 601, 8)
    
    GenericAttribute0 = property(__GenericAttribute0.value, __GenericAttribute0.set, None, None)

    
    # Attribute GenericAttribute1 uses Python identifier GenericAttribute1
    __GenericAttribute1 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute1'), 'GenericAttribute1', '__avm_GenericDomainModel__GenericAttribute1', pyxb.binding.datatypes.string)
    __GenericAttribute1._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 602, 8)
    __GenericAttribute1._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 602, 8)
    
    GenericAttribute1 = property(__GenericAttribute1.value, __GenericAttribute1.set, None, None)

    
    # Attribute GenericAttribute2 uses Python identifier GenericAttribute2
    __GenericAttribute2 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute2'), 'GenericAttribute2', '__avm_GenericDomainModel__GenericAttribute2', pyxb.binding.datatypes.string)
    __GenericAttribute2._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 603, 8)
    __GenericAttribute2._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 603, 8)
    
    GenericAttribute2 = property(__GenericAttribute2.value, __GenericAttribute2.set, None, None)

    
    # Attribute GenericAttribute3 uses Python identifier GenericAttribute3
    __GenericAttribute3 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute3'), 'GenericAttribute3', '__avm_GenericDomainModel__GenericAttribute3', pyxb.binding.datatypes.string)
    __GenericAttribute3._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 604, 8)
    __GenericAttribute3._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 604, 8)
    
    GenericAttribute3 = property(__GenericAttribute3.value, __GenericAttribute3.set, None, None)

    
    # Attribute GenericAttribute4 uses Python identifier GenericAttribute4
    __GenericAttribute4 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute4'), 'GenericAttribute4', '__avm_GenericDomainModel__GenericAttribute4', pyxb.binding.datatypes.string)
    __GenericAttribute4._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 605, 8)
    __GenericAttribute4._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 605, 8)
    
    GenericAttribute4 = property(__GenericAttribute4.value, __GenericAttribute4.set, None, None)

    
    # Attribute GenericAttribute5 uses Python identifier GenericAttribute5
    __GenericAttribute5 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute5'), 'GenericAttribute5', '__avm_GenericDomainModel__GenericAttribute5', pyxb.binding.datatypes.string)
    __GenericAttribute5._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 606, 8)
    __GenericAttribute5._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 606, 8)
    
    GenericAttribute5 = property(__GenericAttribute5.value, __GenericAttribute5.set, None, None)

    
    # Attribute GenericAttribute6 uses Python identifier GenericAttribute6
    __GenericAttribute6 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute6'), 'GenericAttribute6', '__avm_GenericDomainModel__GenericAttribute6', pyxb.binding.datatypes.string)
    __GenericAttribute6._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 607, 8)
    __GenericAttribute6._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 607, 8)
    
    GenericAttribute6 = property(__GenericAttribute6.value, __GenericAttribute6.set, None, None)

    
    # Attribute GenericAttribute7 uses Python identifier GenericAttribute7
    __GenericAttribute7 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute7'), 'GenericAttribute7', '__avm_GenericDomainModel__GenericAttribute7', pyxb.binding.datatypes.string)
    __GenericAttribute7._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 608, 8)
    __GenericAttribute7._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 608, 8)
    
    GenericAttribute7 = property(__GenericAttribute7.value, __GenericAttribute7.set, None, None)

    _ElementMap.update({
        __GenericDomainModelPort.name() : __GenericDomainModelPort,
        __GenericDomainModelParameter.name() : __GenericDomainModelParameter
    })
    _AttributeMap.update({
        __Type.name() : __Type,
        __Domain.name() : __Domain,
        __GenericAttribute0.name() : __GenericAttribute0,
        __GenericAttribute1.name() : __GenericAttribute1,
        __GenericAttribute2.name() : __GenericAttribute2,
        __GenericAttribute3.name() : __GenericAttribute3,
        __GenericAttribute4.name() : __GenericAttribute4,
        __GenericAttribute5.name() : __GenericAttribute5,
        __GenericAttribute6.name() : __GenericAttribute6,
        __GenericAttribute7.name() : __GenericAttribute7
    })
Namespace.addCategoryObject('typeBinding', u'GenericDomainModel', GenericDomainModel_)


# Complex type {avm}GenericDomainModelParameter with content type ELEMENT_ONLY
class GenericDomainModelParameter_ (DomainModelParameter_):
    """Complex type {avm}GenericDomainModelParameter with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'GenericDomainModelParameter')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 627, 2)
    _ElementMap = DomainModelParameter_._ElementMap.copy()
    _AttributeMap = DomainModelParameter_._AttributeMap.copy()
    # Base type is DomainModelParameter_
    
    # Element Value uses Python identifier Value
    __Value = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Value'), 'Value', '__avm_GenericDomainModelParameter__Value', False, pyxb.utils.utility.Location(u'avm.xsd', 631, 10), )

    
    Value = property(__Value.value, __Value.set, None, None)

    
    # Attribute YPosition inherited from {avm}DomainModelParameter
    
    # Attribute Notes inherited from {avm}DomainModelParameter
    
    # Attribute XPosition inherited from {avm}DomainModelParameter
    
    # Attribute GenericAttribute0 uses Python identifier GenericAttribute0
    __GenericAttribute0 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute0'), 'GenericAttribute0', '__avm_GenericDomainModelParameter__GenericAttribute0', pyxb.binding.datatypes.string)
    __GenericAttribute0._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 633, 8)
    __GenericAttribute0._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 633, 8)
    
    GenericAttribute0 = property(__GenericAttribute0.value, __GenericAttribute0.set, None, None)

    
    # Attribute GenericAttribute1 uses Python identifier GenericAttribute1
    __GenericAttribute1 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute1'), 'GenericAttribute1', '__avm_GenericDomainModelParameter__GenericAttribute1', pyxb.binding.datatypes.string)
    __GenericAttribute1._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 634, 8)
    __GenericAttribute1._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 634, 8)
    
    GenericAttribute1 = property(__GenericAttribute1.value, __GenericAttribute1.set, None, None)

    
    # Attribute GenericAttribute2 uses Python identifier GenericAttribute2
    __GenericAttribute2 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute2'), 'GenericAttribute2', '__avm_GenericDomainModelParameter__GenericAttribute2', pyxb.binding.datatypes.string)
    __GenericAttribute2._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 635, 8)
    __GenericAttribute2._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 635, 8)
    
    GenericAttribute2 = property(__GenericAttribute2.value, __GenericAttribute2.set, None, None)

    
    # Attribute GenericAttribute3 uses Python identifier GenericAttribute3
    __GenericAttribute3 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute3'), 'GenericAttribute3', '__avm_GenericDomainModelParameter__GenericAttribute3', pyxb.binding.datatypes.string)
    __GenericAttribute3._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 636, 8)
    __GenericAttribute3._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 636, 8)
    
    GenericAttribute3 = property(__GenericAttribute3.value, __GenericAttribute3.set, None, None)

    
    # Attribute GenericAttribute4 uses Python identifier GenericAttribute4
    __GenericAttribute4 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute4'), 'GenericAttribute4', '__avm_GenericDomainModelParameter__GenericAttribute4', pyxb.binding.datatypes.string)
    __GenericAttribute4._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 637, 8)
    __GenericAttribute4._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 637, 8)
    
    GenericAttribute4 = property(__GenericAttribute4.value, __GenericAttribute4.set, None, None)

    
    # Attribute GenericAttribute5 uses Python identifier GenericAttribute5
    __GenericAttribute5 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute5'), 'GenericAttribute5', '__avm_GenericDomainModelParameter__GenericAttribute5', pyxb.binding.datatypes.string)
    __GenericAttribute5._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 638, 8)
    __GenericAttribute5._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 638, 8)
    
    GenericAttribute5 = property(__GenericAttribute5.value, __GenericAttribute5.set, None, None)

    
    # Attribute GenericAttribute6 uses Python identifier GenericAttribute6
    __GenericAttribute6 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute6'), 'GenericAttribute6', '__avm_GenericDomainModelParameter__GenericAttribute6', pyxb.binding.datatypes.string)
    __GenericAttribute6._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 639, 8)
    __GenericAttribute6._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 639, 8)
    
    GenericAttribute6 = property(__GenericAttribute6.value, __GenericAttribute6.set, None, None)

    
    # Attribute GenericAttribute7 uses Python identifier GenericAttribute7
    __GenericAttribute7 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute7'), 'GenericAttribute7', '__avm_GenericDomainModelParameter__GenericAttribute7', pyxb.binding.datatypes.string)
    __GenericAttribute7._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 640, 8)
    __GenericAttribute7._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 640, 8)
    
    GenericAttribute7 = property(__GenericAttribute7.value, __GenericAttribute7.set, None, None)

    
    # Attribute Name uses Python identifier Name
    __Name = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Name'), 'Name', '__avm_GenericDomainModelParameter__Name', pyxb.binding.datatypes.string)
    __Name._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 641, 8)
    __Name._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 641, 8)
    
    Name = property(__Name.value, __Name.set, None, None)

    _ElementMap.update({
        __Value.name() : __Value
    })
    _AttributeMap.update({
        __GenericAttribute0.name() : __GenericAttribute0,
        __GenericAttribute1.name() : __GenericAttribute1,
        __GenericAttribute2.name() : __GenericAttribute2,
        __GenericAttribute3.name() : __GenericAttribute3,
        __GenericAttribute4.name() : __GenericAttribute4,
        __GenericAttribute5.name() : __GenericAttribute5,
        __GenericAttribute6.name() : __GenericAttribute6,
        __GenericAttribute7.name() : __GenericAttribute7,
        __Name.name() : __Name
    })
Namespace.addCategoryObject('typeBinding', u'GenericDomainModelParameter', GenericDomainModelParameter_)


# Complex type {avm}Connector with content type ELEMENT_ONLY
class Connector_ (ConnectorCompositionTarget_):
    """Complex type {avm}Connector with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Connector')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 160, 2)
    _ElementMap = ConnectorCompositionTarget_._ElementMap.copy()
    _AttributeMap = ConnectorCompositionTarget_._AttributeMap.copy()
    # Base type is ConnectorCompositionTarget_
    
    # Element Role uses Python identifier Role
    __Role = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Role'), 'Role', '__avm_Connector__Role', True, pyxb.utils.utility.Location(u'avm.xsd', 164, 10), )

    
    Role = property(__Role.value, __Role.set, None, None)

    
    # Element Property uses Python identifier Property
    __Property = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Property'), 'Property', '__avm_Connector__Property', True, pyxb.utils.utility.Location(u'avm.xsd', 165, 10), )

    
    Property = property(__Property.value, __Property.set, None, None)

    
    # Element DefaultJoin uses Python identifier DefaultJoin
    __DefaultJoin = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'DefaultJoin'), 'DefaultJoin', '__avm_Connector__DefaultJoin', True, pyxb.utils.utility.Location(u'avm.xsd', 166, 10), )

    
    DefaultJoin = property(__DefaultJoin.value, __DefaultJoin.set, None, None)

    
    # Element Connector uses Python identifier Connector
    __Connector = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Connector'), 'Connector', '__avm_Connector__Connector', True, pyxb.utils.utility.Location(u'avm.xsd', 167, 10), )

    
    Connector = property(__Connector.value, __Connector.set, None, None)

    
    # Element ConnectorFeature uses Python identifier ConnectorFeature
    __ConnectorFeature = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'ConnectorFeature'), 'ConnectorFeature', '__avm_Connector__ConnectorFeature', True, pyxb.utils.utility.Location(u'avm.xsd', 168, 10), )

    
    ConnectorFeature = property(__ConnectorFeature.value, __ConnectorFeature.set, None, None)

    
    # Attribute Definition uses Python identifier Definition
    __Definition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Definition'), 'Definition', '__avm_Connector__Definition', pyxb.binding.datatypes.anyURI)
    __Definition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 170, 8)
    __Definition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 170, 8)
    
    Definition = property(__Definition.value, __Definition.set, None, None)

    
    # Attribute Name uses Python identifier Name
    __Name = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Name'), 'Name', '__avm_Connector__Name', pyxb.binding.datatypes.string)
    __Name._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 171, 8)
    __Name._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 171, 8)
    
    Name = property(__Name.value, __Name.set, None, None)

    
    # Attribute Notes uses Python identifier Notes
    __Notes = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Notes'), 'Notes', '__avm_Connector__Notes', pyxb.binding.datatypes.string)
    __Notes._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 172, 8)
    __Notes._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 172, 8)
    
    Notes = property(__Notes.value, __Notes.set, None, None)

    
    # Attribute XPosition uses Python identifier XPosition
    __XPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XPosition'), 'XPosition', '__avm_Connector__XPosition', pyxb.binding.datatypes.unsignedInt)
    __XPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 173, 8)
    __XPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 173, 8)
    
    XPosition = property(__XPosition.value, __XPosition.set, None, None)

    
    # Attribute YPosition uses Python identifier YPosition
    __YPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YPosition'), 'YPosition', '__avm_Connector__YPosition', pyxb.binding.datatypes.unsignedInt)
    __YPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 174, 8)
    __YPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 174, 8)
    
    YPosition = property(__YPosition.value, __YPosition.set, None, None)

    
    # Attribute ConnectorComposition inherited from {avm}ConnectorCompositionTarget
    
    # Attribute ID inherited from {avm}ConnectorCompositionTarget
    
    # Attribute ApplyJoinData inherited from {avm}ConnectorCompositionTarget
    _ElementMap.update({
        __Role.name() : __Role,
        __Property.name() : __Property,
        __DefaultJoin.name() : __DefaultJoin,
        __Connector.name() : __Connector,
        __ConnectorFeature.name() : __ConnectorFeature
    })
    _AttributeMap.update({
        __Definition.name() : __Definition,
        __Name.name() : __Name,
        __Notes.name() : __Notes,
        __XPosition.name() : __XPosition,
        __YPosition.name() : __YPosition
    })
Namespace.addCategoryObject('typeBinding', u'Connector', Connector_)


# Complex type {avm}Port with content type EMPTY
class Port_ (PortMapTarget_):
    """Complex type {avm}Port with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Port')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 178, 2)
    _ElementMap = PortMapTarget_._ElementMap.copy()
    _AttributeMap = PortMapTarget_._AttributeMap.copy()
    # Base type is PortMapTarget_
    
    # Attribute Notes uses Python identifier Notes
    __Notes = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Notes'), 'Notes', '__avm_Port__Notes', pyxb.binding.datatypes.string)
    __Notes._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 181, 8)
    __Notes._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 181, 8)
    
    Notes = property(__Notes.value, __Notes.set, None, None)

    
    # Attribute XPosition uses Python identifier XPosition
    __XPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'XPosition'), 'XPosition', '__avm_Port__XPosition', pyxb.binding.datatypes.unsignedInt)
    __XPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 182, 8)
    __XPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 182, 8)
    
    XPosition = property(__XPosition.value, __XPosition.set, None, None)

    
    # Attribute Definition uses Python identifier Definition
    __Definition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Definition'), 'Definition', '__avm_Port__Definition', pyxb.binding.datatypes.anyURI)
    __Definition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 183, 8)
    __Definition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 183, 8)
    
    Definition = property(__Definition.value, __Definition.set, None, None)

    
    # Attribute YPosition uses Python identifier YPosition
    __YPosition = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'YPosition'), 'YPosition', '__avm_Port__YPosition', pyxb.binding.datatypes.unsignedInt)
    __YPosition._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 184, 8)
    __YPosition._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 184, 8)
    
    YPosition = property(__YPosition.value, __YPosition.set, None, None)

    
    # Attribute Name uses Python identifier Name
    __Name = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Name'), 'Name', '__avm_Port__Name', pyxb.binding.datatypes.string)
    __Name._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 185, 8)
    __Name._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 185, 8)
    
    Name = property(__Name.value, __Name.set, None, None)

    
    # Attribute ID inherited from {avm}PortMapTarget
    
    # Attribute PortMap inherited from {avm}PortMapTarget
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __Notes.name() : __Notes,
        __XPosition.name() : __XPosition,
        __Definition.name() : __Definition,
        __YPosition.name() : __YPosition,
        __Name.name() : __Name
    })
Namespace.addCategoryObject('typeBinding', u'Port', Port_)


# Complex type {avm}NormalDistribution with content type ELEMENT_ONLY
class NormalDistribution_ (ProbabilisticValue_):
    """Complex type {avm}NormalDistribution with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'NormalDistribution')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 217, 2)
    _ElementMap = ProbabilisticValue_._ElementMap.copy()
    _AttributeMap = ProbabilisticValue_._AttributeMap.copy()
    # Base type is ProbabilisticValue_
    
    # Element Mean uses Python identifier Mean
    __Mean = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Mean'), 'Mean', '__avm_NormalDistribution__Mean', False, pyxb.utils.utility.Location(u'avm.xsd', 221, 10), )

    
    Mean = property(__Mean.value, __Mean.set, None, None)

    
    # Element StandardDeviation uses Python identifier StandardDeviation
    __StandardDeviation = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'StandardDeviation'), 'StandardDeviation', '__avm_NormalDistribution__StandardDeviation', False, pyxb.utils.utility.Location(u'avm.xsd', 222, 10), )

    
    StandardDeviation = property(__StandardDeviation.value, __StandardDeviation.set, None, None)

    _ElementMap.update({
        __Mean.name() : __Mean,
        __StandardDeviation.name() : __StandardDeviation
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'NormalDistribution', NormalDistribution_)


# Complex type {avm}UniformDistribution with content type EMPTY
class UniformDistribution_ (ProbabilisticValue_):
    """Complex type {avm}UniformDistribution with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'UniformDistribution')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 279, 2)
    _ElementMap = ProbabilisticValue_._ElementMap.copy()
    _AttributeMap = ProbabilisticValue_._AttributeMap.copy()
    # Base type is ProbabilisticValue_
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'UniformDistribution', UniformDistribution_)


# Complex type {avm}Optional with content type ELEMENT_ONLY
class Optional_ (DesignSpaceContainer_):
    """Complex type {avm}Optional with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Optional')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 364, 2)
    _ElementMap = DesignSpaceContainer_._ElementMap.copy()
    _AttributeMap = DesignSpaceContainer_._AttributeMap.copy()
    # Base type is DesignSpaceContainer_
    
    # Element Container (Container) inherited from {avm}Container
    
    # Element Property (Property) inherited from {avm}Container
    
    # Element ComponentInstance (ComponentInstance) inherited from {avm}Container
    
    # Element Port (Port) inherited from {avm}Container
    
    # Element Connector (Connector) inherited from {avm}Container
    
    # Element JoinData (JoinData) inherited from {avm}Container
    
    # Element Formula (Formula) inherited from {avm}Container
    
    # Element ContainerFeature (ContainerFeature) inherited from {avm}Container
    
    # Element ResourceDependency (ResourceDependency) inherited from {avm}Container
    
    # Element DomainModel (DomainModel) inherited from {avm}Container
    
    # Element Resource (Resource) inherited from {avm}Container
    
    # Element Classifications (Classifications) inherited from {avm}Container
    
    # Attribute XPosition inherited from {avm}Container
    
    # Attribute Name inherited from {avm}Container
    
    # Attribute YPosition inherited from {avm}Container
    
    # Attribute ID inherited from {avm}Container
    
    # Attribute Description inherited from {avm}Container
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'Optional', Optional_)


# Complex type {avm}Alternative with content type ELEMENT_ONLY
class Alternative_ (DesignSpaceContainer_):
    """Complex type {avm}Alternative with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'Alternative')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 369, 2)
    _ElementMap = DesignSpaceContainer_._ElementMap.copy()
    _AttributeMap = DesignSpaceContainer_._AttributeMap.copy()
    # Base type is DesignSpaceContainer_
    
    # Element Container (Container) inherited from {avm}Container
    
    # Element Property (Property) inherited from {avm}Container
    
    # Element ComponentInstance (ComponentInstance) inherited from {avm}Container
    
    # Element Port (Port) inherited from {avm}Container
    
    # Element Connector (Connector) inherited from {avm}Container
    
    # Element JoinData (JoinData) inherited from {avm}Container
    
    # Element Formula (Formula) inherited from {avm}Container
    
    # Element ContainerFeature (ContainerFeature) inherited from {avm}Container
    
    # Element ResourceDependency (ResourceDependency) inherited from {avm}Container
    
    # Element DomainModel (DomainModel) inherited from {avm}Container
    
    # Element Resource (Resource) inherited from {avm}Container
    
    # Element Classifications (Classifications) inherited from {avm}Container
    
    # Element ValueFlowMux uses Python identifier ValueFlowMux
    __ValueFlowMux = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'ValueFlowMux'), 'ValueFlowMux', '__avm_Alternative__ValueFlowMux', True, pyxb.utils.utility.Location(u'avm.xsd', 373, 10), )

    
    ValueFlowMux = property(__ValueFlowMux.value, __ValueFlowMux.set, None, None)

    
    # Attribute XPosition inherited from {avm}Container
    
    # Attribute Name inherited from {avm}Container
    
    # Attribute YPosition inherited from {avm}Container
    
    # Attribute ID inherited from {avm}Container
    
    # Attribute Description inherited from {avm}Container
    _ElementMap.update({
        __ValueFlowMux.name() : __ValueFlowMux
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'Alternative', Alternative_)


# Complex type {avm}ComponentPortInstance with content type EMPTY
class ComponentPortInstance_ (PortMapTarget_):
    """Complex type {avm}ComponentPortInstance with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ComponentPortInstance')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 391, 2)
    _ElementMap = PortMapTarget_._ElementMap.copy()
    _AttributeMap = PortMapTarget_._AttributeMap.copy()
    # Base type is PortMapTarget_
    
    # Attribute IDinComponentModel uses Python identifier IDinComponentModel
    __IDinComponentModel = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'IDinComponentModel'), 'IDinComponentModel', '__avm_ComponentPortInstance__IDinComponentModel', pyxb.binding.datatypes.string, required=True)
    __IDinComponentModel._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 394, 8)
    __IDinComponentModel._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 394, 8)
    
    IDinComponentModel = property(__IDinComponentModel.value, __IDinComponentModel.set, None, None)

    
    # Attribute ID inherited from {avm}PortMapTarget
    
    # Attribute PortMap inherited from {avm}PortMapTarget
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __IDinComponentModel.name() : __IDinComponentModel
    })
Namespace.addCategoryObject('typeBinding', u'ComponentPortInstance', ComponentPortInstance_)


# Complex type {avm}ComponentConnectorInstance with content type EMPTY
class ComponentConnectorInstance_ (ConnectorCompositionTarget_):
    """Complex type {avm}ComponentConnectorInstance with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ComponentConnectorInstance')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 417, 2)
    _ElementMap = ConnectorCompositionTarget_._ElementMap.copy()
    _AttributeMap = ConnectorCompositionTarget_._AttributeMap.copy()
    # Base type is ConnectorCompositionTarget_
    
    # Attribute IDinComponentModel uses Python identifier IDinComponentModel
    __IDinComponentModel = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'IDinComponentModel'), 'IDinComponentModel', '__avm_ComponentConnectorInstance__IDinComponentModel', pyxb.binding.datatypes.string, required=True)
    __IDinComponentModel._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 420, 8)
    __IDinComponentModel._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 420, 8)
    
    IDinComponentModel = property(__IDinComponentModel.value, __IDinComponentModel.set, None, None)

    
    # Attribute ConnectorComposition inherited from {avm}ConnectorCompositionTarget
    
    # Attribute ID inherited from {avm}ConnectorCompositionTarget
    
    # Attribute ApplyJoinData inherited from {avm}ConnectorCompositionTarget
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __IDinComponentModel.name() : __IDinComponentModel
    })
Namespace.addCategoryObject('typeBinding', u'ComponentConnectorInstance', ComponentConnectorInstance_)


# Complex type {avm}SimpleFormula with content type EMPTY
class SimpleFormula_ (Formula_):
    """Complex type {avm}SimpleFormula with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'SimpleFormula')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 446, 2)
    _ElementMap = Formula_._ElementMap.copy()
    _AttributeMap = Formula_._AttributeMap.copy()
    # Base type is Formula_
    
    # Attribute Name inherited from {avm}Formula
    
    # Attribute XPosition inherited from {avm}Formula
    
    # Attribute YPosition inherited from {avm}Formula
    
    # Attribute Operation uses Python identifier Operation
    __Operation = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Operation'), 'Operation', '__avm_SimpleFormula__Operation', SimpleFormulaOperation)
    __Operation._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 449, 8)
    __Operation._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 449, 8)
    
    Operation = property(__Operation.value, __Operation.set, None, None)

    
    # Attribute Operand uses Python identifier Operand
    __Operand = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Operand'), 'Operand', '__avm_SimpleFormula__Operand', STD_ANON_4, required=True)
    __Operand._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 450, 8)
    __Operand._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 450, 8)
    
    Operand = property(__Operand.value, __Operand.set, None, None)

    
    # Attribute ID inherited from {avm}ValueNode
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __Operation.name() : __Operation,
        __Operand.name() : __Operand
    })
Namespace.addCategoryObject('typeBinding', u'SimpleFormula', SimpleFormula_)


# Complex type {avm}ComplexFormula with content type ELEMENT_ONLY
class ComplexFormula_ (Formula_):
    """Complex type {avm}ComplexFormula with content type ELEMENT_ONLY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_ELEMENT_ONLY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'ComplexFormula')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 471, 2)
    _ElementMap = Formula_._ElementMap.copy()
    _AttributeMap = Formula_._AttributeMap.copy()
    # Base type is Formula_
    
    # Element Operand uses Python identifier Operand
    __Operand = pyxb.binding.content.ElementDeclaration(pyxb.namespace.ExpandedName(None, u'Operand'), 'Operand', '__avm_ComplexFormula__Operand', True, pyxb.utils.utility.Location(u'avm.xsd', 475, 10), )

    
    Operand = property(__Operand.value, __Operand.set, None, None)

    
    # Attribute Name inherited from {avm}Formula
    
    # Attribute XPosition inherited from {avm}Formula
    
    # Attribute YPosition inherited from {avm}Formula
    
    # Attribute ID inherited from {avm}ValueNode
    
    # Attribute Expression uses Python identifier Expression
    __Expression = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Expression'), 'Expression', '__avm_ComplexFormula__Expression', pyxb.binding.datatypes.string, required=True)
    __Expression._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 477, 8)
    __Expression._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 477, 8)
    
    Expression = property(__Expression.value, __Expression.set, None, None)

    _ElementMap.update({
        __Operand.name() : __Operand
    })
    _AttributeMap.update({
        __Expression.name() : __Expression
    })
Namespace.addCategoryObject('typeBinding', u'ComplexFormula', ComplexFormula_)


# Complex type {avm}DomainModelPort with content type EMPTY
class DomainModelPort_ (Port_):
    """Complex type {avm}DomainModelPort with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = True
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'DomainModelPort')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 189, 2)
    _ElementMap = Port_._ElementMap.copy()
    _AttributeMap = Port_._AttributeMap.copy()
    # Base type is Port_
    
    # Attribute Notes inherited from {avm}Port
    
    # Attribute XPosition inherited from {avm}Port
    
    # Attribute Definition inherited from {avm}Port
    
    # Attribute YPosition inherited from {avm}Port
    
    # Attribute Name inherited from {avm}Port
    
    # Attribute ID inherited from {avm}PortMapTarget
    
    # Attribute PortMap inherited from {avm}PortMapTarget
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'DomainModelPort', DomainModelPort_)


# Complex type {avm}AbstractPort with content type EMPTY
class AbstractPort_ (Port_):
    """Complex type {avm}AbstractPort with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'AbstractPort')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 313, 2)
    _ElementMap = Port_._ElementMap.copy()
    _AttributeMap = Port_._AttributeMap.copy()
    # Base type is Port_
    
    # Attribute Notes inherited from {avm}Port
    
    # Attribute XPosition inherited from {avm}Port
    
    # Attribute Definition inherited from {avm}Port
    
    # Attribute YPosition inherited from {avm}Port
    
    # Attribute Name inherited from {avm}Port
    
    # Attribute ID inherited from {avm}PortMapTarget
    
    # Attribute PortMap inherited from {avm}PortMapTarget
    _ElementMap.update({
        
    })
    _AttributeMap.update({
        
    })
Namespace.addCategoryObject('typeBinding', u'AbstractPort', AbstractPort_)


# Complex type {avm}GenericDomainModelPort with content type EMPTY
class GenericDomainModelPort_ (DomainModelPort_):
    """Complex type {avm}GenericDomainModelPort with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'GenericDomainModelPort')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.xsd', 612, 2)
    _ElementMap = DomainModelPort_._ElementMap.copy()
    _AttributeMap = DomainModelPort_._AttributeMap.copy()
    # Base type is DomainModelPort_
    
    # Attribute Notes inherited from {avm}Port
    
    # Attribute XPosition inherited from {avm}Port
    
    # Attribute Definition inherited from {avm}Port
    
    # Attribute YPosition inherited from {avm}Port
    
    # Attribute Name inherited from {avm}Port
    
    # Attribute ID inherited from {avm}PortMapTarget
    
    # Attribute PortMap inherited from {avm}PortMapTarget
    
    # Attribute Type uses Python identifier Type
    __Type = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'Type'), 'Type', '__avm_GenericDomainModelPort__Type', pyxb.binding.datatypes.string)
    __Type._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 615, 8)
    __Type._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 615, 8)
    
    Type = property(__Type.value, __Type.set, None, None)

    
    # Attribute GenericAttribute0 uses Python identifier GenericAttribute0
    __GenericAttribute0 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute0'), 'GenericAttribute0', '__avm_GenericDomainModelPort__GenericAttribute0', pyxb.binding.datatypes.string)
    __GenericAttribute0._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 616, 8)
    __GenericAttribute0._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 616, 8)
    
    GenericAttribute0 = property(__GenericAttribute0.value, __GenericAttribute0.set, None, None)

    
    # Attribute GenericAttribute1 uses Python identifier GenericAttribute1
    __GenericAttribute1 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute1'), 'GenericAttribute1', '__avm_GenericDomainModelPort__GenericAttribute1', pyxb.binding.datatypes.string)
    __GenericAttribute1._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 617, 8)
    __GenericAttribute1._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 617, 8)
    
    GenericAttribute1 = property(__GenericAttribute1.value, __GenericAttribute1.set, None, None)

    
    # Attribute GenericAttribute2 uses Python identifier GenericAttribute2
    __GenericAttribute2 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute2'), 'GenericAttribute2', '__avm_GenericDomainModelPort__GenericAttribute2', pyxb.binding.datatypes.string)
    __GenericAttribute2._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 618, 8)
    __GenericAttribute2._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 618, 8)
    
    GenericAttribute2 = property(__GenericAttribute2.value, __GenericAttribute2.set, None, None)

    
    # Attribute GenericAttribute3 uses Python identifier GenericAttribute3
    __GenericAttribute3 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute3'), 'GenericAttribute3', '__avm_GenericDomainModelPort__GenericAttribute3', pyxb.binding.datatypes.string)
    __GenericAttribute3._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 619, 8)
    __GenericAttribute3._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 619, 8)
    
    GenericAttribute3 = property(__GenericAttribute3.value, __GenericAttribute3.set, None, None)

    
    # Attribute GenericAttribute4 uses Python identifier GenericAttribute4
    __GenericAttribute4 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute4'), 'GenericAttribute4', '__avm_GenericDomainModelPort__GenericAttribute4', pyxb.binding.datatypes.string)
    __GenericAttribute4._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 620, 8)
    __GenericAttribute4._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 620, 8)
    
    GenericAttribute4 = property(__GenericAttribute4.value, __GenericAttribute4.set, None, None)

    
    # Attribute GenericAttribute5 uses Python identifier GenericAttribute5
    __GenericAttribute5 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute5'), 'GenericAttribute5', '__avm_GenericDomainModelPort__GenericAttribute5', pyxb.binding.datatypes.string)
    __GenericAttribute5._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 621, 8)
    __GenericAttribute5._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 621, 8)
    
    GenericAttribute5 = property(__GenericAttribute5.value, __GenericAttribute5.set, None, None)

    
    # Attribute GenericAttribute6 uses Python identifier GenericAttribute6
    __GenericAttribute6 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute6'), 'GenericAttribute6', '__avm_GenericDomainModelPort__GenericAttribute6', pyxb.binding.datatypes.string)
    __GenericAttribute6._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 622, 8)
    __GenericAttribute6._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 622, 8)
    
    GenericAttribute6 = property(__GenericAttribute6.value, __GenericAttribute6.set, None, None)

    
    # Attribute GenericAttribute7 uses Python identifier GenericAttribute7
    __GenericAttribute7 = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'GenericAttribute7'), 'GenericAttribute7', '__avm_GenericDomainModelPort__GenericAttribute7', pyxb.binding.datatypes.string)
    __GenericAttribute7._DeclarationLocation = pyxb.utils.utility.Location(u'avm.xsd', 623, 8)
    __GenericAttribute7._UseLocation = pyxb.utils.utility.Location(u'avm.xsd', 623, 8)
    
    GenericAttribute7 = property(__GenericAttribute7.value, __GenericAttribute7.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __Type.name() : __Type,
        __GenericAttribute0.name() : __GenericAttribute0,
        __GenericAttribute1.name() : __GenericAttribute1,
        __GenericAttribute2.name() : __GenericAttribute2,
        __GenericAttribute3.name() : __GenericAttribute3,
        __GenericAttribute4.name() : __GenericAttribute4,
        __GenericAttribute5.name() : __GenericAttribute5,
        __GenericAttribute6.name() : __GenericAttribute6,
        __GenericAttribute7.name() : __GenericAttribute7
    })
Namespace.addCategoryObject('typeBinding', u'GenericDomainModelPort', GenericDomainModelPort_)


Component = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Component'), Component_, location=pyxb.utils.utility.Location(u'avm.xsd', 4, 2))
Namespace.addCategoryObject('elementBinding', Component.name().localName(), Component)

DomainModel = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'DomainModel'), DomainModel_, location=pyxb.utils.utility.Location(u'avm.xsd', 5, 2))
Namespace.addCategoryObject('elementBinding', DomainModel.name().localName(), DomainModel)

Property = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Property'), Property_, location=pyxb.utils.utility.Location(u'avm.xsd', 6, 2))
Namespace.addCategoryObject('elementBinding', Property.name().localName(), Property)

Resource = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Resource'), Resource_, location=pyxb.utils.utility.Location(u'avm.xsd', 11, 2))
Namespace.addCategoryObject('elementBinding', Resource.name().localName(), Resource)

DomainModelParameter = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'DomainModelParameter'), DomainModelParameter_, location=pyxb.utils.utility.Location(u'avm.xsd', 15, 2))
Namespace.addCategoryObject('elementBinding', DomainModelParameter.name().localName(), DomainModelParameter)

ValueExpressionType = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ValueExpressionType'), ValueExpressionType_, location=pyxb.utils.utility.Location(u'avm.xsd', 17, 2))
Namespace.addCategoryObject('elementBinding', ValueExpressionType.name().localName(), ValueExpressionType)

DistributionRestriction = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'DistributionRestriction'), DistributionRestriction_, location=pyxb.utils.utility.Location(u'avm.xsd', 20, 2))
Namespace.addCategoryObject('elementBinding', DistributionRestriction.name().localName(), DistributionRestriction)

DomainModelMetric = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'DomainModelMetric'), DomainModelMetric_, location=pyxb.utils.utility.Location(u'avm.xsd', 24, 2))
Namespace.addCategoryObject('elementBinding', DomainModelMetric.name().localName(), DomainModelMetric)

AnalysisConstruct = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'AnalysisConstruct'), AnalysisConstruct_, location=pyxb.utils.utility.Location(u'avm.xsd', 30, 2))
Namespace.addCategoryObject('elementBinding', AnalysisConstruct.name().localName(), AnalysisConstruct)

Design = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Design'), Design_, location=pyxb.utils.utility.Location(u'avm.xsd', 32, 2))
Namespace.addCategoryObject('elementBinding', Design.name().localName(), Design)

Container = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Container'), Container_, location=pyxb.utils.utility.Location(u'avm.xsd', 33, 2))
Namespace.addCategoryObject('elementBinding', Container.name().localName(), Container)

ComponentInstance = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ComponentInstance'), ComponentInstance_, location=pyxb.utils.utility.Location(u'avm.xsd', 37, 2))
Namespace.addCategoryObject('elementBinding', ComponentInstance.name().localName(), ComponentInstance)

ComponentPrimitivePropertyInstance = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ComponentPrimitivePropertyInstance'), ComponentPrimitivePropertyInstance_, location=pyxb.utils.utility.Location(u'avm.xsd', 39, 2))
Namespace.addCategoryObject('elementBinding', ComponentPrimitivePropertyInstance.name().localName(), ComponentPrimitivePropertyInstance)

ValueNode = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ValueNode'), ValueNode_, location=pyxb.utils.utility.Location(u'avm.xsd', 46, 2))
Namespace.addCategoryObject('elementBinding', ValueNode.name().localName(), ValueNode)

Operand = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Operand'), Operand_, location=pyxb.utils.utility.Location(u'avm.xsd', 48, 2))
Namespace.addCategoryObject('elementBinding', Operand.name().localName(), Operand)

ConnectorFeature = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ConnectorFeature'), ConnectorFeature_, location=pyxb.utils.utility.Location(u'avm.xsd', 50, 2))
Namespace.addCategoryObject('elementBinding', ConnectorFeature.name().localName(), ConnectorFeature)

ContainerFeature = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ContainerFeature'), ContainerFeature_, location=pyxb.utils.utility.Location(u'avm.xsd', 51, 2))
Namespace.addCategoryObject('elementBinding', ContainerFeature.name().localName(), ContainerFeature)

DomainMapping = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'DomainMapping'), DomainMapping_, location=pyxb.utils.utility.Location(u'avm.xsd', 52, 2))
Namespace.addCategoryObject('elementBinding', DomainMapping.name().localName(), DomainMapping)

TestBench = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'TestBench'), TestBench_, location=pyxb.utils.utility.Location(u'avm.xsd', 53, 2))
Namespace.addCategoryObject('elementBinding', TestBench.name().localName(), TestBench)

ContainerInstanceBase = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ContainerInstanceBase'), ContainerInstanceBase_, location=pyxb.utils.utility.Location(u'avm.xsd', 57, 2))
Namespace.addCategoryObject('elementBinding', ContainerInstanceBase.name().localName(), ContainerInstanceBase)

TestBenchValueBase = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'TestBenchValueBase'), TestBenchValueBase_, location=pyxb.utils.utility.Location(u'avm.xsd', 58, 2))
Namespace.addCategoryObject('elementBinding', TestBenchValueBase.name().localName(), TestBenchValueBase)

Workflow = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Workflow'), Workflow_, location=pyxb.utils.utility.Location(u'avm.xsd', 60, 2))
Namespace.addCategoryObject('elementBinding', Workflow.name().localName(), Workflow)

WorkflowTaskBase = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'WorkflowTaskBase'), WorkflowTaskBase_, location=pyxb.utils.utility.Location(u'avm.xsd', 61, 2))
Namespace.addCategoryObject('elementBinding', WorkflowTaskBase.name().localName(), WorkflowTaskBase)

Settings = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Settings'), Settings_, location=pyxb.utils.utility.Location(u'avm.xsd', 64, 2))
Namespace.addCategoryObject('elementBinding', Settings.name().localName(), Settings)

DesignDomainFeature = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'DesignDomainFeature'), DesignDomainFeature_, location=pyxb.utils.utility.Location(u'avm.xsd', 66, 2))
Namespace.addCategoryObject('elementBinding', DesignDomainFeature.name().localName(), DesignDomainFeature)

Value = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Value'), Value_, location=pyxb.utils.utility.Location(u'avm.xsd', 7, 2))
Namespace.addCategoryObject('elementBinding', Value.name().localName(), Value)

FixedValue = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'FixedValue'), FixedValue_, location=pyxb.utils.utility.Location(u'avm.xsd', 8, 2))
Namespace.addCategoryObject('elementBinding', FixedValue.name().localName(), FixedValue)

CalculatedValue = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'CalculatedValue'), CalculatedValue_, location=pyxb.utils.utility.Location(u'avm.xsd', 9, 2))
Namespace.addCategoryObject('elementBinding', CalculatedValue.name().localName(), CalculatedValue)

DerivedValue = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'DerivedValue'), DerivedValue_, location=pyxb.utils.utility.Location(u'avm.xsd', 10, 2))
Namespace.addCategoryObject('elementBinding', DerivedValue.name().localName(), DerivedValue)

ParametricValue = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ParametricValue'), ParametricValue_, location=pyxb.utils.utility.Location(u'avm.xsd', 16, 2))
Namespace.addCategoryObject('elementBinding', ParametricValue.name().localName(), ParametricValue)

ProbabilisticValue = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ProbabilisticValue'), ProbabilisticValue_, location=pyxb.utils.utility.Location(u'avm.xsd', 18, 2))
Namespace.addCategoryObject('elementBinding', ProbabilisticValue.name().localName(), ProbabilisticValue)

SecurityClassification = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'SecurityClassification'), SecurityClassification_, location=pyxb.utils.utility.Location(u'avm.xsd', 21, 2))
Namespace.addCategoryObject('elementBinding', SecurityClassification.name().localName(), SecurityClassification)

Proprietary = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Proprietary'), Proprietary_, location=pyxb.utils.utility.Location(u'avm.xsd', 22, 2))
Namespace.addCategoryObject('elementBinding', Proprietary.name().localName(), Proprietary)

ITAR = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ITAR'), ITAR_, location=pyxb.utils.utility.Location(u'avm.xsd', 23, 2))
Namespace.addCategoryObject('elementBinding', ITAR.name().localName(), ITAR)

PrimitiveProperty = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'PrimitiveProperty'), PrimitiveProperty_, location=pyxb.utils.utility.Location(u'avm.xsd', 26, 2))
Namespace.addCategoryObject('elementBinding', PrimitiveProperty.name().localName(), PrimitiveProperty)

CompoundProperty = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'CompoundProperty'), CompoundProperty_, location=pyxb.utils.utility.Location(u'avm.xsd', 27, 2))
Namespace.addCategoryObject('elementBinding', CompoundProperty.name().localName(), CompoundProperty)

ParametricEnumeratedValue = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ParametricEnumeratedValue'), ParametricEnumeratedValue_, location=pyxb.utils.utility.Location(u'avm.xsd', 28, 2))
Namespace.addCategoryObject('elementBinding', ParametricEnumeratedValue.name().localName(), ParametricEnumeratedValue)

DataSource = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'DataSource'), DataSource_, location=pyxb.utils.utility.Location(u'avm.xsd', 31, 2))
Namespace.addCategoryObject('elementBinding', DataSource.name().localName(), DataSource)

Compound = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Compound'), Compound_, location=pyxb.utils.utility.Location(u'avm.xsd', 34, 2))
Namespace.addCategoryObject('elementBinding', Compound.name().localName(), Compound)

DesignSpaceContainer = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'DesignSpaceContainer'), DesignSpaceContainer_, location=pyxb.utils.utility.Location(u'avm.xsd', 40, 2))
Namespace.addCategoryObject('elementBinding', DesignSpaceContainer.name().localName(), DesignSpaceContainer)

PortMapTarget = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'PortMapTarget'), PortMapTarget_, location=pyxb.utils.utility.Location(u'avm.xsd', 41, 2))
Namespace.addCategoryObject('elementBinding', PortMapTarget.name().localName(), PortMapTarget)

ConnectorCompositionTarget = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ConnectorCompositionTarget'), ConnectorCompositionTarget_, location=pyxb.utils.utility.Location(u'avm.xsd', 43, 2))
Namespace.addCategoryObject('elementBinding', ConnectorCompositionTarget.name().localName(), ConnectorCompositionTarget)

Formula = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Formula'), Formula_, location=pyxb.utils.utility.Location(u'avm.xsd', 44, 2))
Namespace.addCategoryObject('elementBinding', Formula.name().localName(), Formula)

DoDDistributionStatement = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'DoDDistributionStatement'), DoDDistributionStatement_, location=pyxb.utils.utility.Location(u'avm.xsd', 49, 2))
Namespace.addCategoryObject('elementBinding', DoDDistributionStatement.name().localName(), DoDDistributionStatement)

TopLevelSystemUnderTest = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'TopLevelSystemUnderTest'), TopLevelSystemUnderTest_, location=pyxb.utils.utility.Location(u'avm.xsd', 54, 2))
Namespace.addCategoryObject('elementBinding', TopLevelSystemUnderTest.name().localName(), TopLevelSystemUnderTest)

Parameter = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Parameter'), Parameter_, location=pyxb.utils.utility.Location(u'avm.xsd', 55, 2))
Namespace.addCategoryObject('elementBinding', Parameter.name().localName(), Parameter)

Metric = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Metric'), Metric_, location=pyxb.utils.utility.Location(u'avm.xsd', 56, 2))
Namespace.addCategoryObject('elementBinding', Metric.name().localName(), Metric)

TestInjectionPoint = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'TestInjectionPoint'), TestInjectionPoint_, location=pyxb.utils.utility.Location(u'avm.xsd', 59, 2))
Namespace.addCategoryObject('elementBinding', TestInjectionPoint.name().localName(), TestInjectionPoint)

InterpreterTask = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'InterpreterTask'), InterpreterTask_, location=pyxb.utils.utility.Location(u'avm.xsd', 62, 2))
Namespace.addCategoryObject('elementBinding', InterpreterTask.name().localName(), InterpreterTask)

ExecutionTask = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ExecutionTask'), ExecutionTask_, location=pyxb.utils.utility.Location(u'avm.xsd', 63, 2))
Namespace.addCategoryObject('elementBinding', ExecutionTask.name().localName(), ExecutionTask)

ValueFlowMux = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ValueFlowMux'), ValueFlowMux_, location=pyxb.utils.utility.Location(u'avm.xsd', 65, 2))
Namespace.addCategoryObject('elementBinding', ValueFlowMux.name().localName(), ValueFlowMux)

GenericDomainModel = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'GenericDomainModel'), GenericDomainModel_, location=pyxb.utils.utility.Location(u'avm.xsd', 67, 2))
Namespace.addCategoryObject('elementBinding', GenericDomainModel.name().localName(), GenericDomainModel)

GenericDomainModelParameter = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'GenericDomainModelParameter'), GenericDomainModelParameter_, location=pyxb.utils.utility.Location(u'avm.xsd', 69, 2))
Namespace.addCategoryObject('elementBinding', GenericDomainModelParameter.name().localName(), GenericDomainModelParameter)

Connector = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Connector'), Connector_, location=pyxb.utils.utility.Location(u'avm.xsd', 12, 2))
Namespace.addCategoryObject('elementBinding', Connector.name().localName(), Connector)

Port = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Port'), Port_, location=pyxb.utils.utility.Location(u'avm.xsd', 13, 2))
Namespace.addCategoryObject('elementBinding', Port.name().localName(), Port)

NormalDistribution = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'NormalDistribution'), NormalDistribution_, location=pyxb.utils.utility.Location(u'avm.xsd', 19, 2))
Namespace.addCategoryObject('elementBinding', NormalDistribution.name().localName(), NormalDistribution)

UniformDistribution = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'UniformDistribution'), UniformDistribution_, location=pyxb.utils.utility.Location(u'avm.xsd', 25, 2))
Namespace.addCategoryObject('elementBinding', UniformDistribution.name().localName(), UniformDistribution)

Optional = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Optional'), Optional_, location=pyxb.utils.utility.Location(u'avm.xsd', 35, 2))
Namespace.addCategoryObject('elementBinding', Optional.name().localName(), Optional)

Alternative = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'Alternative'), Alternative_, location=pyxb.utils.utility.Location(u'avm.xsd', 36, 2))
Namespace.addCategoryObject('elementBinding', Alternative.name().localName(), Alternative)

ComponentPortInstance = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ComponentPortInstance'), ComponentPortInstance_, location=pyxb.utils.utility.Location(u'avm.xsd', 38, 2))
Namespace.addCategoryObject('elementBinding', ComponentPortInstance.name().localName(), ComponentPortInstance)

ComponentConnectorInstance = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ComponentConnectorInstance'), ComponentConnectorInstance_, location=pyxb.utils.utility.Location(u'avm.xsd', 42, 2))
Namespace.addCategoryObject('elementBinding', ComponentConnectorInstance.name().localName(), ComponentConnectorInstance)

SimpleFormula = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'SimpleFormula'), SimpleFormula_, location=pyxb.utils.utility.Location(u'avm.xsd', 45, 2))
Namespace.addCategoryObject('elementBinding', SimpleFormula.name().localName(), SimpleFormula)

ComplexFormula = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'ComplexFormula'), ComplexFormula_, location=pyxb.utils.utility.Location(u'avm.xsd', 47, 2))
Namespace.addCategoryObject('elementBinding', ComplexFormula.name().localName(), ComplexFormula)

DomainModelPort = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'DomainModelPort'), DomainModelPort_, location=pyxb.utils.utility.Location(u'avm.xsd', 14, 2))
Namespace.addCategoryObject('elementBinding', DomainModelPort.name().localName(), DomainModelPort)

AbstractPort = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'AbstractPort'), AbstractPort_, location=pyxb.utils.utility.Location(u'avm.xsd', 29, 2))
Namespace.addCategoryObject('elementBinding', AbstractPort.name().localName(), AbstractPort)

GenericDomainModelPort = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'GenericDomainModelPort'), GenericDomainModelPort_, location=pyxb.utils.utility.Location(u'avm.xsd', 68, 2))
Namespace.addCategoryObject('elementBinding', GenericDomainModelPort.name().localName(), GenericDomainModelPort)



Component_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'DomainModel'), DomainModel_, scope=Component_, location=pyxb.utils.utility.Location(u'avm.xsd', 75, 6)))

Component_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Property'), Property_, scope=Component_, location=pyxb.utils.utility.Location(u'avm.xsd', 76, 6)))

Component_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'ResourceDependency'), Resource_, scope=Component_, location=pyxb.utils.utility.Location(u'avm.xsd', 77, 6)))

Component_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Connector'), Connector_, scope=Component_, location=pyxb.utils.utility.Location(u'avm.xsd', 78, 6)))

Component_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'DistributionRestriction'), DistributionRestriction_, scope=Component_, location=pyxb.utils.utility.Location(u'avm.xsd', 79, 6)))

Component_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Port'), Port_, scope=Component_, location=pyxb.utils.utility.Location(u'avm.xsd', 80, 6)))

Component_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Classifications'), pyxb.binding.datatypes.anyURI, nillable=pyxb.binding.datatypes.boolean(1), scope=Component_, location=pyxb.utils.utility.Location(u'avm.xsd', 81, 6)))

Component_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'AnalysisConstruct'), AnalysisConstruct_, scope=Component_, location=pyxb.utils.utility.Location(u'avm.xsd', 82, 6)))

Component_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Supercedes'), pyxb.binding.datatypes.string, nillable=pyxb.binding.datatypes.boolean(1), scope=Component_, location=pyxb.utils.utility.Location(u'avm.xsd', 83, 6)))

Component_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Formula'), Formula_, scope=Component_, location=pyxb.utils.utility.Location(u'avm.xsd', 84, 6)))

Component_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'DomainMapping'), DomainMapping_, scope=Component_, location=pyxb.utils.utility.Location(u'avm.xsd', 85, 6)))

def _BuildAutomaton ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton
    del _BuildAutomaton
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 75, 6))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 76, 6))
    counters.add(cc_1)
    cc_2 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 77, 6))
    counters.add(cc_2)
    cc_3 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 78, 6))
    counters.add(cc_3)
    cc_4 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 79, 6))
    counters.add(cc_4)
    cc_5 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 80, 6))
    counters.add(cc_5)
    cc_6 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 81, 6))
    counters.add(cc_6)
    cc_7 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 82, 6))
    counters.add(cc_7)
    cc_8 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 83, 6))
    counters.add(cc_8)
    cc_9 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 84, 6))
    counters.add(cc_9)
    cc_10 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 85, 6))
    counters.add(cc_10)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(Component_._UseForTag(pyxb.namespace.ExpandedName(None, u'DomainModel')), pyxb.utils.utility.Location(u'avm.xsd', 75, 6))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(Component_._UseForTag(pyxb.namespace.ExpandedName(None, u'Property')), pyxb.utils.utility.Location(u'avm.xsd', 76, 6))
    st_1 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_1)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_2, False))
    symbol = pyxb.binding.content.ElementUse(Component_._UseForTag(pyxb.namespace.ExpandedName(None, u'ResourceDependency')), pyxb.utils.utility.Location(u'avm.xsd', 77, 6))
    st_2 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_2)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_3, False))
    symbol = pyxb.binding.content.ElementUse(Component_._UseForTag(pyxb.namespace.ExpandedName(None, u'Connector')), pyxb.utils.utility.Location(u'avm.xsd', 78, 6))
    st_3 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_3)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_4, False))
    symbol = pyxb.binding.content.ElementUse(Component_._UseForTag(pyxb.namespace.ExpandedName(None, u'DistributionRestriction')), pyxb.utils.utility.Location(u'avm.xsd', 79, 6))
    st_4 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_4)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_5, False))
    symbol = pyxb.binding.content.ElementUse(Component_._UseForTag(pyxb.namespace.ExpandedName(None, u'Port')), pyxb.utils.utility.Location(u'avm.xsd', 80, 6))
    st_5 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_5)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_6, False))
    symbol = pyxb.binding.content.ElementUse(Component_._UseForTag(pyxb.namespace.ExpandedName(None, u'Classifications')), pyxb.utils.utility.Location(u'avm.xsd', 81, 6))
    st_6 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_6)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_7, False))
    symbol = pyxb.binding.content.ElementUse(Component_._UseForTag(pyxb.namespace.ExpandedName(None, u'AnalysisConstruct')), pyxb.utils.utility.Location(u'avm.xsd', 82, 6))
    st_7 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_7)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_8, False))
    symbol = pyxb.binding.content.ElementUse(Component_._UseForTag(pyxb.namespace.ExpandedName(None, u'Supercedes')), pyxb.utils.utility.Location(u'avm.xsd', 83, 6))
    st_8 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_8)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_9, False))
    symbol = pyxb.binding.content.ElementUse(Component_._UseForTag(pyxb.namespace.ExpandedName(None, u'Formula')), pyxb.utils.utility.Location(u'avm.xsd', 84, 6))
    st_9 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_9)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_10, False))
    symbol = pyxb.binding.content.ElementUse(Component_._UseForTag(pyxb.namespace.ExpandedName(None, u'DomainMapping')), pyxb.utils.utility.Location(u'avm.xsd', 85, 6))
    st_10 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_10)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_0, False) ]))
    st_0._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_1, True) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_1, False) ]))
    st_1._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_2, True) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_2, False) ]))
    st_2._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_3, True) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_3, False) ]))
    st_3._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_4, True) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_4, False) ]))
    st_4._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_5, True) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_5, False) ]))
    st_5._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_6, True) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_6, False) ]))
    st_6._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_7, True) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_7, False) ]))
    st_7._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_8, True) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_8, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_8, False) ]))
    st_8._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_9, True) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_9, False) ]))
    st_9._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_10, True) ]))
    st_10._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
Component_._Automaton = _BuildAutomaton()




DomainModelMetric_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Value'), Value_, scope=DomainModelMetric_, location=pyxb.utils.utility.Location(u'avm.xsd', 272, 6)))

def _BuildAutomaton_ ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_
    del _BuildAutomaton_
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.xsd', 272, 6))
    counters.add(cc_0)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(DomainModelMetric_._UseForTag(pyxb.namespace.ExpandedName(None, u'Value')), pyxb.utils.utility.Location(u'avm.xsd', 272, 6))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    st_0._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
DomainModelMetric_._Automaton = _BuildAutomaton_()




Design_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'RootContainer'), Container_, scope=Design_, location=pyxb.utils.utility.Location(u'avm.xsd', 329, 6)))

Design_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'DomainFeature'), DesignDomainFeature_, scope=Design_, location=pyxb.utils.utility.Location(u'avm.xsd', 330, 6)))

Design_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'ResourceDependency'), Resource_, scope=Design_, location=pyxb.utils.utility.Location(u'avm.xsd', 331, 6)))

def _BuildAutomaton_2 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_2
    del _BuildAutomaton_2
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.xsd', 329, 6))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 330, 6))
    counters.add(cc_1)
    cc_2 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 331, 6))
    counters.add(cc_2)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(Design_._UseForTag(pyxb.namespace.ExpandedName(None, u'RootContainer')), pyxb.utils.utility.Location(u'avm.xsd', 329, 6))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(Design_._UseForTag(pyxb.namespace.ExpandedName(None, u'DomainFeature')), pyxb.utils.utility.Location(u'avm.xsd', 330, 6))
    st_1 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_1)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_2, False))
    symbol = pyxb.binding.content.ElementUse(Design_._UseForTag(pyxb.namespace.ExpandedName(None, u'ResourceDependency')), pyxb.utils.utility.Location(u'avm.xsd', 331, 6))
    st_2 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_2)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_0, False) ]))
    st_0._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_1, True) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_1, False) ]))
    st_1._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_2, True) ]))
    st_2._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
Design_._Automaton = _BuildAutomaton_2()




Container_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Container'), Container_, scope=Container_, location=pyxb.utils.utility.Location(u'avm.xsd', 340, 6)))

Container_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Property'), Property_, scope=Container_, location=pyxb.utils.utility.Location(u'avm.xsd', 341, 6)))

Container_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'ComponentInstance'), ComponentInstance_, scope=Container_, location=pyxb.utils.utility.Location(u'avm.xsd', 342, 6)))

Container_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Port'), Port_, scope=Container_, location=pyxb.utils.utility.Location(u'avm.xsd', 343, 6)))

Container_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Connector'), Connector_, scope=Container_, location=pyxb.utils.utility.Location(u'avm.xsd', 344, 6)))

Container_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'JoinData'), _ImportedBinding__iFAB.assemblyDetail, scope=Container_, location=pyxb.utils.utility.Location(u'avm.xsd', 345, 6)))

Container_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Formula'), Formula_, scope=Container_, location=pyxb.utils.utility.Location(u'avm.xsd', 346, 6)))

Container_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'ContainerFeature'), ContainerFeature_, scope=Container_, location=pyxb.utils.utility.Location(u'avm.xsd', 347, 6)))

Container_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'ResourceDependency'), Resource_, scope=Container_, location=pyxb.utils.utility.Location(u'avm.xsd', 348, 6)))

Container_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'DomainModel'), DomainModel_, scope=Container_, location=pyxb.utils.utility.Location(u'avm.xsd', 349, 6)))

Container_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Resource'), Resource_, scope=Container_, location=pyxb.utils.utility.Location(u'avm.xsd', 350, 6)))

Container_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Classifications'), pyxb.binding.datatypes.anyURI, nillable=pyxb.binding.datatypes.boolean(1), scope=Container_, location=pyxb.utils.utility.Location(u'avm.xsd', 351, 6)))

def _BuildAutomaton_3 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_3
    del _BuildAutomaton_3
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 340, 6))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 341, 6))
    counters.add(cc_1)
    cc_2 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 342, 6))
    counters.add(cc_2)
    cc_3 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 343, 6))
    counters.add(cc_3)
    cc_4 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 344, 6))
    counters.add(cc_4)
    cc_5 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 345, 6))
    counters.add(cc_5)
    cc_6 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 346, 6))
    counters.add(cc_6)
    cc_7 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 347, 6))
    counters.add(cc_7)
    cc_8 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 348, 6))
    counters.add(cc_8)
    cc_9 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 349, 6))
    counters.add(cc_9)
    cc_10 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 350, 6))
    counters.add(cc_10)
    cc_11 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 351, 6))
    counters.add(cc_11)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(Container_._UseForTag(pyxb.namespace.ExpandedName(None, u'Container')), pyxb.utils.utility.Location(u'avm.xsd', 340, 6))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(Container_._UseForTag(pyxb.namespace.ExpandedName(None, u'Property')), pyxb.utils.utility.Location(u'avm.xsd', 341, 6))
    st_1 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_1)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_2, False))
    symbol = pyxb.binding.content.ElementUse(Container_._UseForTag(pyxb.namespace.ExpandedName(None, u'ComponentInstance')), pyxb.utils.utility.Location(u'avm.xsd', 342, 6))
    st_2 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_2)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_3, False))
    symbol = pyxb.binding.content.ElementUse(Container_._UseForTag(pyxb.namespace.ExpandedName(None, u'Port')), pyxb.utils.utility.Location(u'avm.xsd', 343, 6))
    st_3 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_3)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_4, False))
    symbol = pyxb.binding.content.ElementUse(Container_._UseForTag(pyxb.namespace.ExpandedName(None, u'Connector')), pyxb.utils.utility.Location(u'avm.xsd', 344, 6))
    st_4 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_4)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_5, False))
    symbol = pyxb.binding.content.ElementUse(Container_._UseForTag(pyxb.namespace.ExpandedName(None, u'JoinData')), pyxb.utils.utility.Location(u'avm.xsd', 345, 6))
    st_5 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_5)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_6, False))
    symbol = pyxb.binding.content.ElementUse(Container_._UseForTag(pyxb.namespace.ExpandedName(None, u'Formula')), pyxb.utils.utility.Location(u'avm.xsd', 346, 6))
    st_6 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_6)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_7, False))
    symbol = pyxb.binding.content.ElementUse(Container_._UseForTag(pyxb.namespace.ExpandedName(None, u'ContainerFeature')), pyxb.utils.utility.Location(u'avm.xsd', 347, 6))
    st_7 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_7)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_8, False))
    symbol = pyxb.binding.content.ElementUse(Container_._UseForTag(pyxb.namespace.ExpandedName(None, u'ResourceDependency')), pyxb.utils.utility.Location(u'avm.xsd', 348, 6))
    st_8 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_8)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_9, False))
    symbol = pyxb.binding.content.ElementUse(Container_._UseForTag(pyxb.namespace.ExpandedName(None, u'DomainModel')), pyxb.utils.utility.Location(u'avm.xsd', 349, 6))
    st_9 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_9)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_10, False))
    symbol = pyxb.binding.content.ElementUse(Container_._UseForTag(pyxb.namespace.ExpandedName(None, u'Resource')), pyxb.utils.utility.Location(u'avm.xsd', 350, 6))
    st_10 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_10)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_11, False))
    symbol = pyxb.binding.content.ElementUse(Container_._UseForTag(pyxb.namespace.ExpandedName(None, u'Classifications')), pyxb.utils.utility.Location(u'avm.xsd', 351, 6))
    st_11 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_11)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_0, False) ]))
    st_0._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_1, True) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_1, False) ]))
    st_1._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_2, True) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_2, False) ]))
    st_2._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_3, True) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_3, False) ]))
    st_3._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_4, True) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_4, False) ]))
    st_4._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_5, True) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_5, False) ]))
    st_5._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_6, True) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_6, False) ]))
    st_6._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_7, True) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_7, False) ]))
    st_7._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_8, True) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_8, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_8, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_8, False) ]))
    st_8._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_9, True) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_9, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_9, False) ]))
    st_9._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_10, True) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_10, False) ]))
    st_10._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_11, True) ]))
    st_11._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
Container_._Automaton = _BuildAutomaton_3()




ComponentInstance_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'PortInstance'), ComponentPortInstance_, scope=ComponentInstance_, location=pyxb.utils.utility.Location(u'avm.xsd', 380, 6)))

ComponentInstance_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'PrimitivePropertyInstance'), ComponentPrimitivePropertyInstance_, scope=ComponentInstance_, location=pyxb.utils.utility.Location(u'avm.xsd', 381, 6)))

ComponentInstance_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'ConnectorInstance'), ComponentConnectorInstance_, scope=ComponentInstance_, location=pyxb.utils.utility.Location(u'avm.xsd', 382, 6)))

def _BuildAutomaton_4 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_4
    del _BuildAutomaton_4
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 380, 6))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 381, 6))
    counters.add(cc_1)
    cc_2 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 382, 6))
    counters.add(cc_2)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(ComponentInstance_._UseForTag(pyxb.namespace.ExpandedName(None, u'PortInstance')), pyxb.utils.utility.Location(u'avm.xsd', 380, 6))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(ComponentInstance_._UseForTag(pyxb.namespace.ExpandedName(None, u'PrimitivePropertyInstance')), pyxb.utils.utility.Location(u'avm.xsd', 381, 6))
    st_1 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_1)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_2, False))
    symbol = pyxb.binding.content.ElementUse(ComponentInstance_._UseForTag(pyxb.namespace.ExpandedName(None, u'ConnectorInstance')), pyxb.utils.utility.Location(u'avm.xsd', 382, 6))
    st_2 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_2)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_0, False) ]))
    st_0._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_1, True) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_1, False) ]))
    st_1._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_2, True) ]))
    st_2._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
ComponentInstance_._Automaton = _BuildAutomaton_4()




ComponentPrimitivePropertyInstance_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Value'), Value_, scope=ComponentPrimitivePropertyInstance_, location=pyxb.utils.utility.Location(u'avm.xsd', 400, 6)))

def _BuildAutomaton_5 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_5
    del _BuildAutomaton_5
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.xsd', 400, 6))
    counters.add(cc_0)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(ComponentPrimitivePropertyInstance_._UseForTag(pyxb.namespace.ExpandedName(None, u'Value')), pyxb.utils.utility.Location(u'avm.xsd', 400, 6))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    st_0._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
ComponentPrimitivePropertyInstance_._Automaton = _BuildAutomaton_5()




TestBench_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'TopLevelSystemUnderTest'), TopLevelSystemUnderTest_, scope=TestBench_, location=pyxb.utils.utility.Location(u'avm.xsd', 506, 6)))

TestBench_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Parameter'), Parameter_, scope=TestBench_, location=pyxb.utils.utility.Location(u'avm.xsd', 507, 6)))

TestBench_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Metric'), Metric_, scope=TestBench_, location=pyxb.utils.utility.Location(u'avm.xsd', 508, 6)))

TestBench_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'TestInjectionPoint'), TestInjectionPoint_, scope=TestBench_, location=pyxb.utils.utility.Location(u'avm.xsd', 509, 6)))

TestBench_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'TestComponent'), ComponentInstance_, scope=TestBench_, location=pyxb.utils.utility.Location(u'avm.xsd', 510, 6)))

TestBench_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Workflow'), Workflow_, scope=TestBench_, location=pyxb.utils.utility.Location(u'avm.xsd', 511, 6)))

TestBench_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Settings'), Settings_, scope=TestBench_, location=pyxb.utils.utility.Location(u'avm.xsd', 512, 6)))

TestBench_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'TestStructure'), DomainModel_, scope=TestBench_, location=pyxb.utils.utility.Location(u'avm.xsd', 513, 6)))

def _BuildAutomaton_6 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_6
    del _BuildAutomaton_6
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.xsd', 506, 6))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 507, 6))
    counters.add(cc_1)
    cc_2 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 508, 6))
    counters.add(cc_2)
    cc_3 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 509, 6))
    counters.add(cc_3)
    cc_4 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 510, 6))
    counters.add(cc_4)
    cc_5 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.xsd', 511, 6))
    counters.add(cc_5)
    cc_6 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 512, 6))
    counters.add(cc_6)
    cc_7 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 513, 6))
    counters.add(cc_7)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(TestBench_._UseForTag(pyxb.namespace.ExpandedName(None, u'TopLevelSystemUnderTest')), pyxb.utils.utility.Location(u'avm.xsd', 506, 6))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(TestBench_._UseForTag(pyxb.namespace.ExpandedName(None, u'Parameter')), pyxb.utils.utility.Location(u'avm.xsd', 507, 6))
    st_1 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_1)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_2, False))
    symbol = pyxb.binding.content.ElementUse(TestBench_._UseForTag(pyxb.namespace.ExpandedName(None, u'Metric')), pyxb.utils.utility.Location(u'avm.xsd', 508, 6))
    st_2 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_2)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_3, False))
    symbol = pyxb.binding.content.ElementUse(TestBench_._UseForTag(pyxb.namespace.ExpandedName(None, u'TestInjectionPoint')), pyxb.utils.utility.Location(u'avm.xsd', 509, 6))
    st_3 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_3)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_4, False))
    symbol = pyxb.binding.content.ElementUse(TestBench_._UseForTag(pyxb.namespace.ExpandedName(None, u'TestComponent')), pyxb.utils.utility.Location(u'avm.xsd', 510, 6))
    st_4 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_4)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_5, False))
    symbol = pyxb.binding.content.ElementUse(TestBench_._UseForTag(pyxb.namespace.ExpandedName(None, u'Workflow')), pyxb.utils.utility.Location(u'avm.xsd', 511, 6))
    st_5 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_5)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_6, False))
    symbol = pyxb.binding.content.ElementUse(TestBench_._UseForTag(pyxb.namespace.ExpandedName(None, u'Settings')), pyxb.utils.utility.Location(u'avm.xsd', 512, 6))
    st_6 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_6)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_7, False))
    symbol = pyxb.binding.content.ElementUse(TestBench_._UseForTag(pyxb.namespace.ExpandedName(None, u'TestStructure')), pyxb.utils.utility.Location(u'avm.xsd', 513, 6))
    st_7 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_7)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_0, False) ]))
    st_0._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_1, True) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_1, False) ]))
    st_1._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_2, True) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_2, False) ]))
    st_2._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_3, True) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_3, False) ]))
    st_3._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_4, True) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_4, False) ]))
    st_4._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_5, True) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_5, False) ]))
    st_5._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_6, True) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_6, False) ]))
    st_6._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_7, True) ]))
    st_7._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
TestBench_._Automaton = _BuildAutomaton_6()




TestBenchValueBase_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Value'), Value_, scope=TestBenchValueBase_, location=pyxb.utils.utility.Location(u'avm.xsd', 541, 6)))

def _BuildAutomaton_7 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_7
    del _BuildAutomaton_7
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.xsd', 541, 6))
    counters.add(cc_0)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(TestBenchValueBase_._UseForTag(pyxb.namespace.ExpandedName(None, u'Value')), pyxb.utils.utility.Location(u'avm.xsd', 541, 6))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    st_0._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
TestBenchValueBase_._Automaton = _BuildAutomaton_7()




Workflow_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Task'), WorkflowTaskBase_, scope=Workflow_, location=pyxb.utils.utility.Location(u'avm.xsd', 556, 6)))

def _BuildAutomaton_8 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_8
    del _BuildAutomaton_8
    import pyxb.utils.fac as fac

    counters = set()
    states = []
    final_update = set()
    symbol = pyxb.binding.content.ElementUse(Workflow_._UseForTag(pyxb.namespace.ExpandedName(None, u'Task')), pyxb.utils.utility.Location(u'avm.xsd', 556, 6))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    transitions = []
    transitions.append(fac.Transition(st_0, [
         ]))
    st_0._set_transitionSet(transitions)
    return fac.Automaton(states, counters, False, containing_state=None)
Workflow_._Automaton = _BuildAutomaton_8()




Value_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'ValueExpression'), ValueExpressionType_, scope=Value_, location=pyxb.utils.utility.Location(u'avm.xsd', 114, 10)))

Value_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'DataSource'), DataSource_, scope=Value_, location=pyxb.utils.utility.Location(u'avm.xsd', 115, 10)))

def _BuildAutomaton_9 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_9
    del _BuildAutomaton_9
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.xsd', 114, 10))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 115, 10))
    counters.add(cc_1)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(Value_._UseForTag(pyxb.namespace.ExpandedName(None, u'ValueExpression')), pyxb.utils.utility.Location(u'avm.xsd', 114, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(Value_._UseForTag(pyxb.namespace.ExpandedName(None, u'DataSource')), pyxb.utils.utility.Location(u'avm.xsd', 115, 10))
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
Value_._Automaton = _BuildAutomaton_9()




FixedValue_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Value'), pyxb.binding.datatypes.string, scope=FixedValue_, location=pyxb.utils.utility.Location(u'avm.xsd', 128, 10)))

def _BuildAutomaton_10 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_10
    del _BuildAutomaton_10
    import pyxb.utils.fac as fac

    counters = set()
    states = []
    final_update = set()
    symbol = pyxb.binding.content.ElementUse(FixedValue_._UseForTag(pyxb.namespace.ExpandedName(None, u'Value')), pyxb.utils.utility.Location(u'avm.xsd', 128, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    transitions = []
    st_0._set_transitionSet(transitions)
    return fac.Automaton(states, counters, False, containing_state=None)
FixedValue_._Automaton = _BuildAutomaton_10()




CalculatedValue_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Expression'), pyxb.binding.datatypes.string, scope=CalculatedValue_, location=pyxb.utils.utility.Location(u'avm.xsd', 138, 10)))

def _BuildAutomaton_11 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_11
    del _BuildAutomaton_11
    import pyxb.utils.fac as fac

    counters = set()
    states = []
    final_update = set()
    symbol = pyxb.binding.content.ElementUse(CalculatedValue_._UseForTag(pyxb.namespace.ExpandedName(None, u'Expression')), pyxb.utils.utility.Location(u'avm.xsd', 138, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    transitions = []
    st_0._set_transitionSet(transitions)
    return fac.Automaton(states, counters, False, containing_state=None)
CalculatedValue_._Automaton = _BuildAutomaton_11()




ParametricValue_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Default'), ValueExpressionType_, scope=ParametricValue_, location=pyxb.utils.utility.Location(u'avm.xsd', 203, 10)))

ParametricValue_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Maximum'), ValueExpressionType_, scope=ParametricValue_, location=pyxb.utils.utility.Location(u'avm.xsd', 204, 10)))

ParametricValue_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Minimum'), ValueExpressionType_, scope=ParametricValue_, location=pyxb.utils.utility.Location(u'avm.xsd', 205, 10)))

ParametricValue_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'AssignedValue'), ValueExpressionType_, scope=ParametricValue_, location=pyxb.utils.utility.Location(u'avm.xsd', 206, 10)))

def _BuildAutomaton_12 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_12
    del _BuildAutomaton_12
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.xsd', 204, 10))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.xsd', 205, 10))
    counters.add(cc_1)
    cc_2 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.xsd', 206, 10))
    counters.add(cc_2)
    states = []
    final_update = set()
    symbol = pyxb.binding.content.ElementUse(ParametricValue_._UseForTag(pyxb.namespace.ExpandedName(None, u'Default')), pyxb.utils.utility.Location(u'avm.xsd', 203, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(ParametricValue_._UseForTag(pyxb.namespace.ExpandedName(None, u'Maximum')), pyxb.utils.utility.Location(u'avm.xsd', 204, 10))
    st_1 = fac.State(symbol, is_initial=False, final_update=final_update, is_unordered_catenation=False)
    states.append(st_1)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(ParametricValue_._UseForTag(pyxb.namespace.ExpandedName(None, u'Minimum')), pyxb.utils.utility.Location(u'avm.xsd', 205, 10))
    st_2 = fac.State(symbol, is_initial=False, final_update=final_update, is_unordered_catenation=False)
    states.append(st_2)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_2, False))
    symbol = pyxb.binding.content.ElementUse(ParametricValue_._UseForTag(pyxb.namespace.ExpandedName(None, u'AssignedValue')), pyxb.utils.utility.Location(u'avm.xsd', 206, 10))
    st_3 = fac.State(symbol, is_initial=False, final_update=final_update, is_unordered_catenation=False)
    states.append(st_3)
    transitions = []
    transitions.append(fac.Transition(st_1, [
         ]))
    transitions.append(fac.Transition(st_2, [
         ]))
    transitions.append(fac.Transition(st_3, [
         ]))
    st_0._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_0, True) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_0, False) ]))
    st_1._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_1, True) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_1, False) ]))
    st_2._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_2, True) ]))
    st_3._set_transitionSet(transitions)
    return fac.Automaton(states, counters, False, containing_state=None)
ParametricValue_._Automaton = _BuildAutomaton_12()




PrimitiveProperty_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Value'), Value_, scope=PrimitiveProperty_, location=pyxb.utils.utility.Location(u'avm.xsd', 288, 10)))

def _BuildAutomaton_13 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_13
    del _BuildAutomaton_13
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.xsd', 288, 10))
    counters.add(cc_0)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(PrimitiveProperty_._UseForTag(pyxb.namespace.ExpandedName(None, u'Value')), pyxb.utils.utility.Location(u'avm.xsd', 288, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    st_0._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
PrimitiveProperty_._Automaton = _BuildAutomaton_13()




CompoundProperty_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'CompoundProperty'), CompoundProperty_, scope=CompoundProperty_, location=pyxb.utils.utility.Location(u'avm.xsd', 297, 10)))

CompoundProperty_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'PrimitiveProperty'), PrimitiveProperty_, scope=CompoundProperty_, location=pyxb.utils.utility.Location(u'avm.xsd', 298, 10)))

def _BuildAutomaton_14 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_14
    del _BuildAutomaton_14
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 297, 10))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 298, 10))
    counters.add(cc_1)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(CompoundProperty_._UseForTag(pyxb.namespace.ExpandedName(None, u'CompoundProperty')), pyxb.utils.utility.Location(u'avm.xsd', 297, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(CompoundProperty_._UseForTag(pyxb.namespace.ExpandedName(None, u'PrimitiveProperty')), pyxb.utils.utility.Location(u'avm.xsd', 298, 10))
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
CompoundProperty_._Automaton = _BuildAutomaton_14()




ParametricEnumeratedValue_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'AssignedValue'), FixedValue_, scope=ParametricEnumeratedValue_, location=pyxb.utils.utility.Location(u'avm.xsd', 307, 10)))

ParametricEnumeratedValue_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Enum'), ValueExpressionType_, scope=ParametricEnumeratedValue_, location=pyxb.utils.utility.Location(u'avm.xsd', 308, 10)))

def _BuildAutomaton_15 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_15
    del _BuildAutomaton_15
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.xsd', 307, 10))
    counters.add(cc_0)
    states = []
    final_update = None
    symbol = pyxb.binding.content.ElementUse(ParametricEnumeratedValue_._UseForTag(pyxb.namespace.ExpandedName(None, u'AssignedValue')), pyxb.utils.utility.Location(u'avm.xsd', 307, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    symbol = pyxb.binding.content.ElementUse(ParametricEnumeratedValue_._UseForTag(pyxb.namespace.ExpandedName(None, u'Enum')), pyxb.utils.utility.Location(u'avm.xsd', 308, 10))
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
         ]))
    st_1._set_transitionSet(transitions)
    return fac.Automaton(states, counters, False, containing_state=None)
ParametricEnumeratedValue_._Automaton = _BuildAutomaton_15()




def _BuildAutomaton_16 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_16
    del _BuildAutomaton_16
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 340, 6))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 341, 6))
    counters.add(cc_1)
    cc_2 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 342, 6))
    counters.add(cc_2)
    cc_3 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 343, 6))
    counters.add(cc_3)
    cc_4 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 344, 6))
    counters.add(cc_4)
    cc_5 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 345, 6))
    counters.add(cc_5)
    cc_6 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 346, 6))
    counters.add(cc_6)
    cc_7 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 347, 6))
    counters.add(cc_7)
    cc_8 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 348, 6))
    counters.add(cc_8)
    cc_9 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 349, 6))
    counters.add(cc_9)
    cc_10 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 350, 6))
    counters.add(cc_10)
    cc_11 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 351, 6))
    counters.add(cc_11)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(Compound_._UseForTag(pyxb.namespace.ExpandedName(None, u'Container')), pyxb.utils.utility.Location(u'avm.xsd', 340, 6))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(Compound_._UseForTag(pyxb.namespace.ExpandedName(None, u'Property')), pyxb.utils.utility.Location(u'avm.xsd', 341, 6))
    st_1 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_1)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_2, False))
    symbol = pyxb.binding.content.ElementUse(Compound_._UseForTag(pyxb.namespace.ExpandedName(None, u'ComponentInstance')), pyxb.utils.utility.Location(u'avm.xsd', 342, 6))
    st_2 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_2)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_3, False))
    symbol = pyxb.binding.content.ElementUse(Compound_._UseForTag(pyxb.namespace.ExpandedName(None, u'Port')), pyxb.utils.utility.Location(u'avm.xsd', 343, 6))
    st_3 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_3)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_4, False))
    symbol = pyxb.binding.content.ElementUse(Compound_._UseForTag(pyxb.namespace.ExpandedName(None, u'Connector')), pyxb.utils.utility.Location(u'avm.xsd', 344, 6))
    st_4 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_4)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_5, False))
    symbol = pyxb.binding.content.ElementUse(Compound_._UseForTag(pyxb.namespace.ExpandedName(None, u'JoinData')), pyxb.utils.utility.Location(u'avm.xsd', 345, 6))
    st_5 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_5)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_6, False))
    symbol = pyxb.binding.content.ElementUse(Compound_._UseForTag(pyxb.namespace.ExpandedName(None, u'Formula')), pyxb.utils.utility.Location(u'avm.xsd', 346, 6))
    st_6 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_6)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_7, False))
    symbol = pyxb.binding.content.ElementUse(Compound_._UseForTag(pyxb.namespace.ExpandedName(None, u'ContainerFeature')), pyxb.utils.utility.Location(u'avm.xsd', 347, 6))
    st_7 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_7)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_8, False))
    symbol = pyxb.binding.content.ElementUse(Compound_._UseForTag(pyxb.namespace.ExpandedName(None, u'ResourceDependency')), pyxb.utils.utility.Location(u'avm.xsd', 348, 6))
    st_8 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_8)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_9, False))
    symbol = pyxb.binding.content.ElementUse(Compound_._UseForTag(pyxb.namespace.ExpandedName(None, u'DomainModel')), pyxb.utils.utility.Location(u'avm.xsd', 349, 6))
    st_9 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_9)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_10, False))
    symbol = pyxb.binding.content.ElementUse(Compound_._UseForTag(pyxb.namespace.ExpandedName(None, u'Resource')), pyxb.utils.utility.Location(u'avm.xsd', 350, 6))
    st_10 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_10)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_11, False))
    symbol = pyxb.binding.content.ElementUse(Compound_._UseForTag(pyxb.namespace.ExpandedName(None, u'Classifications')), pyxb.utils.utility.Location(u'avm.xsd', 351, 6))
    st_11 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_11)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_0, False) ]))
    st_0._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_1, True) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_1, False) ]))
    st_1._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_2, True) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_2, False) ]))
    st_2._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_3, True) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_3, False) ]))
    st_3._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_4, True) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_4, False) ]))
    st_4._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_5, True) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_5, False) ]))
    st_5._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_6, True) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_6, False) ]))
    st_6._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_7, True) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_7, False) ]))
    st_7._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_8, True) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_8, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_8, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_8, False) ]))
    st_8._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_9, True) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_9, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_9, False) ]))
    st_9._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_10, True) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_10, False) ]))
    st_10._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_11, True) ]))
    st_11._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
Compound_._Automaton = _BuildAutomaton_16()




def _BuildAutomaton_17 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_17
    del _BuildAutomaton_17
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 340, 6))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 341, 6))
    counters.add(cc_1)
    cc_2 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 342, 6))
    counters.add(cc_2)
    cc_3 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 343, 6))
    counters.add(cc_3)
    cc_4 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 344, 6))
    counters.add(cc_4)
    cc_5 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 345, 6))
    counters.add(cc_5)
    cc_6 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 346, 6))
    counters.add(cc_6)
    cc_7 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 347, 6))
    counters.add(cc_7)
    cc_8 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 348, 6))
    counters.add(cc_8)
    cc_9 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 349, 6))
    counters.add(cc_9)
    cc_10 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 350, 6))
    counters.add(cc_10)
    cc_11 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 351, 6))
    counters.add(cc_11)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(DesignSpaceContainer_._UseForTag(pyxb.namespace.ExpandedName(None, u'Container')), pyxb.utils.utility.Location(u'avm.xsd', 340, 6))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(DesignSpaceContainer_._UseForTag(pyxb.namespace.ExpandedName(None, u'Property')), pyxb.utils.utility.Location(u'avm.xsd', 341, 6))
    st_1 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_1)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_2, False))
    symbol = pyxb.binding.content.ElementUse(DesignSpaceContainer_._UseForTag(pyxb.namespace.ExpandedName(None, u'ComponentInstance')), pyxb.utils.utility.Location(u'avm.xsd', 342, 6))
    st_2 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_2)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_3, False))
    symbol = pyxb.binding.content.ElementUse(DesignSpaceContainer_._UseForTag(pyxb.namespace.ExpandedName(None, u'Port')), pyxb.utils.utility.Location(u'avm.xsd', 343, 6))
    st_3 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_3)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_4, False))
    symbol = pyxb.binding.content.ElementUse(DesignSpaceContainer_._UseForTag(pyxb.namespace.ExpandedName(None, u'Connector')), pyxb.utils.utility.Location(u'avm.xsd', 344, 6))
    st_4 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_4)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_5, False))
    symbol = pyxb.binding.content.ElementUse(DesignSpaceContainer_._UseForTag(pyxb.namespace.ExpandedName(None, u'JoinData')), pyxb.utils.utility.Location(u'avm.xsd', 345, 6))
    st_5 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_5)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_6, False))
    symbol = pyxb.binding.content.ElementUse(DesignSpaceContainer_._UseForTag(pyxb.namespace.ExpandedName(None, u'Formula')), pyxb.utils.utility.Location(u'avm.xsd', 346, 6))
    st_6 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_6)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_7, False))
    symbol = pyxb.binding.content.ElementUse(DesignSpaceContainer_._UseForTag(pyxb.namespace.ExpandedName(None, u'ContainerFeature')), pyxb.utils.utility.Location(u'avm.xsd', 347, 6))
    st_7 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_7)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_8, False))
    symbol = pyxb.binding.content.ElementUse(DesignSpaceContainer_._UseForTag(pyxb.namespace.ExpandedName(None, u'ResourceDependency')), pyxb.utils.utility.Location(u'avm.xsd', 348, 6))
    st_8 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_8)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_9, False))
    symbol = pyxb.binding.content.ElementUse(DesignSpaceContainer_._UseForTag(pyxb.namespace.ExpandedName(None, u'DomainModel')), pyxb.utils.utility.Location(u'avm.xsd', 349, 6))
    st_9 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_9)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_10, False))
    symbol = pyxb.binding.content.ElementUse(DesignSpaceContainer_._UseForTag(pyxb.namespace.ExpandedName(None, u'Resource')), pyxb.utils.utility.Location(u'avm.xsd', 350, 6))
    st_10 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_10)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_11, False))
    symbol = pyxb.binding.content.ElementUse(DesignSpaceContainer_._UseForTag(pyxb.namespace.ExpandedName(None, u'Classifications')), pyxb.utils.utility.Location(u'avm.xsd', 351, 6))
    st_11 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_11)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_0, False) ]))
    st_0._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_1, True) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_1, False) ]))
    st_1._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_2, True) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_2, False) ]))
    st_2._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_3, True) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_3, False) ]))
    st_3._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_4, True) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_4, False) ]))
    st_4._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_5, True) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_5, False) ]))
    st_5._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_6, True) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_6, False) ]))
    st_6._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_7, True) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_7, False) ]))
    st_7._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_8, True) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_8, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_8, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_8, False) ]))
    st_8._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_9, True) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_9, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_9, False) ]))
    st_9._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_10, True) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_10, False) ]))
    st_10._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_11, True) ]))
    st_11._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
DesignSpaceContainer_._Automaton = _BuildAutomaton_17()




def _BuildAutomaton_18 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_18
    del _BuildAutomaton_18
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.xsd', 541, 6))
    counters.add(cc_0)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(Parameter_._UseForTag(pyxb.namespace.ExpandedName(None, u'Value')), pyxb.utils.utility.Location(u'avm.xsd', 541, 6))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    st_0._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
Parameter_._Automaton = _BuildAutomaton_18()




def _BuildAutomaton_19 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_19
    del _BuildAutomaton_19
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.xsd', 541, 6))
    counters.add(cc_0)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(Metric_._UseForTag(pyxb.namespace.ExpandedName(None, u'Value')), pyxb.utils.utility.Location(u'avm.xsd', 541, 6))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    st_0._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
Metric_._Automaton = _BuildAutomaton_19()




GenericDomainModel_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'GenericDomainModelPort'), GenericDomainModelPort_, scope=GenericDomainModel_, location=pyxb.utils.utility.Location(u'avm.xsd', 596, 10)))

GenericDomainModel_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'GenericDomainModelParameter'), GenericDomainModelParameter_, scope=GenericDomainModel_, location=pyxb.utils.utility.Location(u'avm.xsd', 597, 10)))

def _BuildAutomaton_20 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_20
    del _BuildAutomaton_20
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 596, 10))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 597, 10))
    counters.add(cc_1)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(GenericDomainModel_._UseForTag(pyxb.namespace.ExpandedName(None, u'GenericDomainModelPort')), pyxb.utils.utility.Location(u'avm.xsd', 596, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(GenericDomainModel_._UseForTag(pyxb.namespace.ExpandedName(None, u'GenericDomainModelParameter')), pyxb.utils.utility.Location(u'avm.xsd', 597, 10))
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
GenericDomainModel_._Automaton = _BuildAutomaton_20()




GenericDomainModelParameter_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Value'), Value_, scope=GenericDomainModelParameter_, location=pyxb.utils.utility.Location(u'avm.xsd', 631, 10)))

def _BuildAutomaton_21 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_21
    del _BuildAutomaton_21
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=1, metadata=pyxb.utils.utility.Location(u'avm.xsd', 631, 10))
    counters.add(cc_0)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(GenericDomainModelParameter_._UseForTag(pyxb.namespace.ExpandedName(None, u'Value')), pyxb.utils.utility.Location(u'avm.xsd', 631, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    st_0._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
GenericDomainModelParameter_._Automaton = _BuildAutomaton_21()




Connector_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Role'), Port_, scope=Connector_, location=pyxb.utils.utility.Location(u'avm.xsd', 164, 10)))

Connector_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Property'), Property_, scope=Connector_, location=pyxb.utils.utility.Location(u'avm.xsd', 165, 10)))

Connector_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'DefaultJoin'), _ImportedBinding__iFAB.assemblyDetail, scope=Connector_, location=pyxb.utils.utility.Location(u'avm.xsd', 166, 10)))

Connector_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Connector'), Connector_, scope=Connector_, location=pyxb.utils.utility.Location(u'avm.xsd', 167, 10)))

Connector_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'ConnectorFeature'), ConnectorFeature_, scope=Connector_, location=pyxb.utils.utility.Location(u'avm.xsd', 168, 10)))

def _BuildAutomaton_22 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_22
    del _BuildAutomaton_22
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 164, 10))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 165, 10))
    counters.add(cc_1)
    cc_2 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 166, 10))
    counters.add(cc_2)
    cc_3 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 167, 10))
    counters.add(cc_3)
    cc_4 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 168, 10))
    counters.add(cc_4)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(Connector_._UseForTag(pyxb.namespace.ExpandedName(None, u'Role')), pyxb.utils.utility.Location(u'avm.xsd', 164, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(Connector_._UseForTag(pyxb.namespace.ExpandedName(None, u'Property')), pyxb.utils.utility.Location(u'avm.xsd', 165, 10))
    st_1 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_1)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_2, False))
    symbol = pyxb.binding.content.ElementUse(Connector_._UseForTag(pyxb.namespace.ExpandedName(None, u'DefaultJoin')), pyxb.utils.utility.Location(u'avm.xsd', 166, 10))
    st_2 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_2)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_3, False))
    symbol = pyxb.binding.content.ElementUse(Connector_._UseForTag(pyxb.namespace.ExpandedName(None, u'Connector')), pyxb.utils.utility.Location(u'avm.xsd', 167, 10))
    st_3 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_3)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_4, False))
    symbol = pyxb.binding.content.ElementUse(Connector_._UseForTag(pyxb.namespace.ExpandedName(None, u'ConnectorFeature')), pyxb.utils.utility.Location(u'avm.xsd', 168, 10))
    st_4 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_4)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_0, False) ]))
    st_0._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_1, True) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_1, False) ]))
    st_1._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_2, True) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_2, False) ]))
    st_2._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_3, True) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_3, False) ]))
    st_3._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_4, True) ]))
    st_4._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
Connector_._Automaton = _BuildAutomaton_22()




NormalDistribution_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Mean'), ValueExpressionType_, scope=NormalDistribution_, location=pyxb.utils.utility.Location(u'avm.xsd', 221, 10)))

NormalDistribution_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'StandardDeviation'), ValueExpressionType_, scope=NormalDistribution_, location=pyxb.utils.utility.Location(u'avm.xsd', 222, 10)))

def _BuildAutomaton_23 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_23
    del _BuildAutomaton_23
    import pyxb.utils.fac as fac

    counters = set()
    states = []
    final_update = None
    symbol = pyxb.binding.content.ElementUse(NormalDistribution_._UseForTag(pyxb.namespace.ExpandedName(None, u'Mean')), pyxb.utils.utility.Location(u'avm.xsd', 221, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    symbol = pyxb.binding.content.ElementUse(NormalDistribution_._UseForTag(pyxb.namespace.ExpandedName(None, u'StandardDeviation')), pyxb.utils.utility.Location(u'avm.xsd', 222, 10))
    st_1 = fac.State(symbol, is_initial=False, final_update=final_update, is_unordered_catenation=False)
    states.append(st_1)
    transitions = []
    transitions.append(fac.Transition(st_1, [
         ]))
    st_0._set_transitionSet(transitions)
    transitions = []
    st_1._set_transitionSet(transitions)
    return fac.Automaton(states, counters, False, containing_state=None)
NormalDistribution_._Automaton = _BuildAutomaton_23()




def _BuildAutomaton_24 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_24
    del _BuildAutomaton_24
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 340, 6))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 341, 6))
    counters.add(cc_1)
    cc_2 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 342, 6))
    counters.add(cc_2)
    cc_3 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 343, 6))
    counters.add(cc_3)
    cc_4 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 344, 6))
    counters.add(cc_4)
    cc_5 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 345, 6))
    counters.add(cc_5)
    cc_6 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 346, 6))
    counters.add(cc_6)
    cc_7 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 347, 6))
    counters.add(cc_7)
    cc_8 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 348, 6))
    counters.add(cc_8)
    cc_9 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 349, 6))
    counters.add(cc_9)
    cc_10 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 350, 6))
    counters.add(cc_10)
    cc_11 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 351, 6))
    counters.add(cc_11)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(Optional_._UseForTag(pyxb.namespace.ExpandedName(None, u'Container')), pyxb.utils.utility.Location(u'avm.xsd', 340, 6))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(Optional_._UseForTag(pyxb.namespace.ExpandedName(None, u'Property')), pyxb.utils.utility.Location(u'avm.xsd', 341, 6))
    st_1 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_1)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_2, False))
    symbol = pyxb.binding.content.ElementUse(Optional_._UseForTag(pyxb.namespace.ExpandedName(None, u'ComponentInstance')), pyxb.utils.utility.Location(u'avm.xsd', 342, 6))
    st_2 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_2)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_3, False))
    symbol = pyxb.binding.content.ElementUse(Optional_._UseForTag(pyxb.namespace.ExpandedName(None, u'Port')), pyxb.utils.utility.Location(u'avm.xsd', 343, 6))
    st_3 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_3)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_4, False))
    symbol = pyxb.binding.content.ElementUse(Optional_._UseForTag(pyxb.namespace.ExpandedName(None, u'Connector')), pyxb.utils.utility.Location(u'avm.xsd', 344, 6))
    st_4 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_4)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_5, False))
    symbol = pyxb.binding.content.ElementUse(Optional_._UseForTag(pyxb.namespace.ExpandedName(None, u'JoinData')), pyxb.utils.utility.Location(u'avm.xsd', 345, 6))
    st_5 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_5)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_6, False))
    symbol = pyxb.binding.content.ElementUse(Optional_._UseForTag(pyxb.namespace.ExpandedName(None, u'Formula')), pyxb.utils.utility.Location(u'avm.xsd', 346, 6))
    st_6 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_6)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_7, False))
    symbol = pyxb.binding.content.ElementUse(Optional_._UseForTag(pyxb.namespace.ExpandedName(None, u'ContainerFeature')), pyxb.utils.utility.Location(u'avm.xsd', 347, 6))
    st_7 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_7)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_8, False))
    symbol = pyxb.binding.content.ElementUse(Optional_._UseForTag(pyxb.namespace.ExpandedName(None, u'ResourceDependency')), pyxb.utils.utility.Location(u'avm.xsd', 348, 6))
    st_8 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_8)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_9, False))
    symbol = pyxb.binding.content.ElementUse(Optional_._UseForTag(pyxb.namespace.ExpandedName(None, u'DomainModel')), pyxb.utils.utility.Location(u'avm.xsd', 349, 6))
    st_9 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_9)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_10, False))
    symbol = pyxb.binding.content.ElementUse(Optional_._UseForTag(pyxb.namespace.ExpandedName(None, u'Resource')), pyxb.utils.utility.Location(u'avm.xsd', 350, 6))
    st_10 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_10)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_11, False))
    symbol = pyxb.binding.content.ElementUse(Optional_._UseForTag(pyxb.namespace.ExpandedName(None, u'Classifications')), pyxb.utils.utility.Location(u'avm.xsd', 351, 6))
    st_11 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_11)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_0, False) ]))
    st_0._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_1, True) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_1, False) ]))
    st_1._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_2, True) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_2, False) ]))
    st_2._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_3, True) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_3, False) ]))
    st_3._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_4, True) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_4, False) ]))
    st_4._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_5, True) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_5, False) ]))
    st_5._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_6, True) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_6, False) ]))
    st_6._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_7, True) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_7, False) ]))
    st_7._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_8, True) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_8, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_8, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_8, False) ]))
    st_8._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_9, True) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_9, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_9, False) ]))
    st_9._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_10, True) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_10, False) ]))
    st_10._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_11, True) ]))
    st_11._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
Optional_._Automaton = _BuildAutomaton_24()




Alternative_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'ValueFlowMux'), ValueFlowMux_, scope=Alternative_, location=pyxb.utils.utility.Location(u'avm.xsd', 373, 10)))

def _BuildAutomaton_25 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_25
    del _BuildAutomaton_25
    import pyxb.utils.fac as fac

    counters = set()
    cc_0 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 340, 6))
    counters.add(cc_0)
    cc_1 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 341, 6))
    counters.add(cc_1)
    cc_2 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 342, 6))
    counters.add(cc_2)
    cc_3 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 343, 6))
    counters.add(cc_3)
    cc_4 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 344, 6))
    counters.add(cc_4)
    cc_5 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 345, 6))
    counters.add(cc_5)
    cc_6 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 346, 6))
    counters.add(cc_6)
    cc_7 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 347, 6))
    counters.add(cc_7)
    cc_8 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 348, 6))
    counters.add(cc_8)
    cc_9 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 349, 6))
    counters.add(cc_9)
    cc_10 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 350, 6))
    counters.add(cc_10)
    cc_11 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 351, 6))
    counters.add(cc_11)
    cc_12 = fac.CounterCondition(min=0L, max=None, metadata=pyxb.utils.utility.Location(u'avm.xsd', 373, 10))
    counters.add(cc_12)
    states = []
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_0, False))
    symbol = pyxb.binding.content.ElementUse(Alternative_._UseForTag(pyxb.namespace.ExpandedName(None, u'Container')), pyxb.utils.utility.Location(u'avm.xsd', 340, 6))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_1, False))
    symbol = pyxb.binding.content.ElementUse(Alternative_._UseForTag(pyxb.namespace.ExpandedName(None, u'Property')), pyxb.utils.utility.Location(u'avm.xsd', 341, 6))
    st_1 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_1)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_2, False))
    symbol = pyxb.binding.content.ElementUse(Alternative_._UseForTag(pyxb.namespace.ExpandedName(None, u'ComponentInstance')), pyxb.utils.utility.Location(u'avm.xsd', 342, 6))
    st_2 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_2)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_3, False))
    symbol = pyxb.binding.content.ElementUse(Alternative_._UseForTag(pyxb.namespace.ExpandedName(None, u'Port')), pyxb.utils.utility.Location(u'avm.xsd', 343, 6))
    st_3 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_3)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_4, False))
    symbol = pyxb.binding.content.ElementUse(Alternative_._UseForTag(pyxb.namespace.ExpandedName(None, u'Connector')), pyxb.utils.utility.Location(u'avm.xsd', 344, 6))
    st_4 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_4)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_5, False))
    symbol = pyxb.binding.content.ElementUse(Alternative_._UseForTag(pyxb.namespace.ExpandedName(None, u'JoinData')), pyxb.utils.utility.Location(u'avm.xsd', 345, 6))
    st_5 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_5)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_6, False))
    symbol = pyxb.binding.content.ElementUse(Alternative_._UseForTag(pyxb.namespace.ExpandedName(None, u'Formula')), pyxb.utils.utility.Location(u'avm.xsd', 346, 6))
    st_6 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_6)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_7, False))
    symbol = pyxb.binding.content.ElementUse(Alternative_._UseForTag(pyxb.namespace.ExpandedName(None, u'ContainerFeature')), pyxb.utils.utility.Location(u'avm.xsd', 347, 6))
    st_7 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_7)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_8, False))
    symbol = pyxb.binding.content.ElementUse(Alternative_._UseForTag(pyxb.namespace.ExpandedName(None, u'ResourceDependency')), pyxb.utils.utility.Location(u'avm.xsd', 348, 6))
    st_8 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_8)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_9, False))
    symbol = pyxb.binding.content.ElementUse(Alternative_._UseForTag(pyxb.namespace.ExpandedName(None, u'DomainModel')), pyxb.utils.utility.Location(u'avm.xsd', 349, 6))
    st_9 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_9)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_10, False))
    symbol = pyxb.binding.content.ElementUse(Alternative_._UseForTag(pyxb.namespace.ExpandedName(None, u'Resource')), pyxb.utils.utility.Location(u'avm.xsd', 350, 6))
    st_10 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_10)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_11, False))
    symbol = pyxb.binding.content.ElementUse(Alternative_._UseForTag(pyxb.namespace.ExpandedName(None, u'Classifications')), pyxb.utils.utility.Location(u'avm.xsd', 351, 6))
    st_11 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_11)
    final_update = set()
    final_update.add(fac.UpdateInstruction(cc_12, False))
    symbol = pyxb.binding.content.ElementUse(Alternative_._UseForTag(pyxb.namespace.ExpandedName(None, u'ValueFlowMux')), pyxb.utils.utility.Location(u'avm.xsd', 373, 10))
    st_12 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_12)
    transitions = []
    transitions.append(fac.Transition(st_0, [
        fac.UpdateInstruction(cc_0, True) ]))
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_0, False) ]))
    transitions.append(fac.Transition(st_12, [
        fac.UpdateInstruction(cc_0, False) ]))
    st_0._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_1, [
        fac.UpdateInstruction(cc_1, True) ]))
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_1, False) ]))
    transitions.append(fac.Transition(st_12, [
        fac.UpdateInstruction(cc_1, False) ]))
    st_1._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_2, [
        fac.UpdateInstruction(cc_2, True) ]))
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_2, False) ]))
    transitions.append(fac.Transition(st_12, [
        fac.UpdateInstruction(cc_2, False) ]))
    st_2._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_3, [
        fac.UpdateInstruction(cc_3, True) ]))
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_3, False) ]))
    transitions.append(fac.Transition(st_12, [
        fac.UpdateInstruction(cc_3, False) ]))
    st_3._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_4, [
        fac.UpdateInstruction(cc_4, True) ]))
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_4, False) ]))
    transitions.append(fac.Transition(st_12, [
        fac.UpdateInstruction(cc_4, False) ]))
    st_4._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_5, [
        fac.UpdateInstruction(cc_5, True) ]))
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_5, False) ]))
    transitions.append(fac.Transition(st_12, [
        fac.UpdateInstruction(cc_5, False) ]))
    st_5._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_6, [
        fac.UpdateInstruction(cc_6, True) ]))
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_6, False) ]))
    transitions.append(fac.Transition(st_12, [
        fac.UpdateInstruction(cc_6, False) ]))
    st_6._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_7, [
        fac.UpdateInstruction(cc_7, True) ]))
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_7, False) ]))
    transitions.append(fac.Transition(st_12, [
        fac.UpdateInstruction(cc_7, False) ]))
    st_7._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_8, [
        fac.UpdateInstruction(cc_8, True) ]))
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_8, False) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_8, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_8, False) ]))
    transitions.append(fac.Transition(st_12, [
        fac.UpdateInstruction(cc_8, False) ]))
    st_8._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_9, [
        fac.UpdateInstruction(cc_9, True) ]))
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_9, False) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_9, False) ]))
    transitions.append(fac.Transition(st_12, [
        fac.UpdateInstruction(cc_9, False) ]))
    st_9._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_10, [
        fac.UpdateInstruction(cc_10, True) ]))
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_10, False) ]))
    transitions.append(fac.Transition(st_12, [
        fac.UpdateInstruction(cc_10, False) ]))
    st_10._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_11, [
        fac.UpdateInstruction(cc_11, True) ]))
    transitions.append(fac.Transition(st_12, [
        fac.UpdateInstruction(cc_11, False) ]))
    st_11._set_transitionSet(transitions)
    transitions = []
    transitions.append(fac.Transition(st_12, [
        fac.UpdateInstruction(cc_12, True) ]))
    st_12._set_transitionSet(transitions)
    return fac.Automaton(states, counters, True, containing_state=None)
Alternative_._Automaton = _BuildAutomaton_25()




ComplexFormula_._AddElement(pyxb.binding.basis.element(pyxb.namespace.ExpandedName(None, u'Operand'), Operand_, scope=ComplexFormula_, location=pyxb.utils.utility.Location(u'avm.xsd', 475, 10)))

def _BuildAutomaton_26 ():
    # Remove this helper function from the namespace after it is invoked
    global _BuildAutomaton_26
    del _BuildAutomaton_26
    import pyxb.utils.fac as fac

    counters = set()
    states = []
    final_update = set()
    symbol = pyxb.binding.content.ElementUse(ComplexFormula_._UseForTag(pyxb.namespace.ExpandedName(None, u'Operand')), pyxb.utils.utility.Location(u'avm.xsd', 475, 10))
    st_0 = fac.State(symbol, is_initial=True, final_update=final_update, is_unordered_catenation=False)
    states.append(st_0)
    transitions = []
    transitions.append(fac.Transition(st_0, [
         ]))
    st_0._set_transitionSet(transitions)
    return fac.Automaton(states, counters, False, containing_state=None)
ComplexFormula_._Automaton = _BuildAutomaton_26()

