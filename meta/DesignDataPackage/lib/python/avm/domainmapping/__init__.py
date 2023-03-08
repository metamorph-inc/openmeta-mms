# .\_domainmapping.py
# -*- coding: utf-8 -*-
# PyXB bindings for NM:b784081b45ee884a2d16db92a13a692f11ff6550
# Generated 2023-02-15 11:25:44.117000 by PyXB version 1.2.3
# Namespace domainmapping [xmlns:domainmapping]

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

# NOTE: All namespace declarations are reserved within the binding
Namespace = pyxb.namespace.NamespaceForURI(u'domainmapping', create_if_missing=True)
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


# Complex type {domainmapping}CAD2EDATransform with content type EMPTY
class CAD2EDATransform_ (_ImportedBinding__avm.DomainMapping_):
    """Complex type {domainmapping}CAD2EDATransform with content type EMPTY"""
    _TypeDefinition = None
    _ContentTypeTag = pyxb.binding.basis.complexTypeDefinition._CT_EMPTY
    _Abstract = False
    _ExpandedName = pyxb.namespace.ExpandedName(Namespace, u'CAD2EDATransform')
    _XSDLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 7, 2)
    _ElementMap = _ImportedBinding__avm.DomainMapping_._ElementMap.copy()
    _AttributeMap = _ImportedBinding__avm.DomainMapping_._AttributeMap.copy()
    # Base type is _ImportedBinding__avm.DomainMapping_
    
    # Attribute RotationX uses Python identifier RotationX
    __RotationX = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'RotationX'), 'RotationX', '__domainmapping_CAD2EDATransform__RotationX', pyxb.binding.datatypes.decimal, required=True)
    __RotationX._DeclarationLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 10, 8)
    __RotationX._UseLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 10, 8)
    
    RotationX = property(__RotationX.value, __RotationX.set, None, None)

    
    # Attribute RotationY uses Python identifier RotationY
    __RotationY = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'RotationY'), 'RotationY', '__domainmapping_CAD2EDATransform__RotationY', pyxb.binding.datatypes.decimal, required=True)
    __RotationY._DeclarationLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 11, 8)
    __RotationY._UseLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 11, 8)
    
    RotationY = property(__RotationY.value, __RotationY.set, None, None)

    
    # Attribute RotationZ uses Python identifier RotationZ
    __RotationZ = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'RotationZ'), 'RotationZ', '__domainmapping_CAD2EDATransform__RotationZ', pyxb.binding.datatypes.decimal, required=True)
    __RotationZ._DeclarationLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 12, 8)
    __RotationZ._UseLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 12, 8)
    
    RotationZ = property(__RotationZ.value, __RotationZ.set, None, None)

    
    # Attribute TranslationX uses Python identifier TranslationX
    __TranslationX = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'TranslationX'), 'TranslationX', '__domainmapping_CAD2EDATransform__TranslationX', pyxb.binding.datatypes.decimal, required=True)
    __TranslationX._DeclarationLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 13, 8)
    __TranslationX._UseLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 13, 8)
    
    TranslationX = property(__TranslationX.value, __TranslationX.set, None, None)

    
    # Attribute TranslationY uses Python identifier TranslationY
    __TranslationY = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'TranslationY'), 'TranslationY', '__domainmapping_CAD2EDATransform__TranslationY', pyxb.binding.datatypes.decimal, required=True)
    __TranslationY._DeclarationLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 14, 8)
    __TranslationY._UseLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 14, 8)
    
    TranslationY = property(__TranslationY.value, __TranslationY.set, None, None)

    
    # Attribute TranslationZ uses Python identifier TranslationZ
    __TranslationZ = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'TranslationZ'), 'TranslationZ', '__domainmapping_CAD2EDATransform__TranslationZ', pyxb.binding.datatypes.decimal, required=True)
    __TranslationZ._DeclarationLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 15, 8)
    __TranslationZ._UseLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 15, 8)
    
    TranslationZ = property(__TranslationZ.value, __TranslationZ.set, None, None)

    
    # Attribute ScaleX uses Python identifier ScaleX
    __ScaleX = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ScaleX'), 'ScaleX', '__domainmapping_CAD2EDATransform__ScaleX', pyxb.binding.datatypes.decimal, required=True)
    __ScaleX._DeclarationLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 16, 8)
    __ScaleX._UseLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 16, 8)
    
    ScaleX = property(__ScaleX.value, __ScaleX.set, None, None)

    
    # Attribute ScaleY uses Python identifier ScaleY
    __ScaleY = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ScaleY'), 'ScaleY', '__domainmapping_CAD2EDATransform__ScaleY', pyxb.binding.datatypes.decimal, required=True)
    __ScaleY._DeclarationLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 17, 8)
    __ScaleY._UseLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 17, 8)
    
    ScaleY = property(__ScaleY.value, __ScaleY.set, None, None)

    
    # Attribute ScaleZ uses Python identifier ScaleZ
    __ScaleZ = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'ScaleZ'), 'ScaleZ', '__domainmapping_CAD2EDATransform__ScaleZ', pyxb.binding.datatypes.decimal, required=True)
    __ScaleZ._DeclarationLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 18, 8)
    __ScaleZ._UseLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 18, 8)
    
    ScaleZ = property(__ScaleZ.value, __ScaleZ.set, None, None)

    
    # Attribute EDAModel uses Python identifier EDAModel
    __EDAModel = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'EDAModel'), 'EDAModel', '__domainmapping_CAD2EDATransform__EDAModel', pyxb.binding.datatypes.anyURI, required=True)
    __EDAModel._DeclarationLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 19, 8)
    __EDAModel._UseLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 19, 8)
    
    EDAModel = property(__EDAModel.value, __EDAModel.set, None, None)

    
    # Attribute CADModel uses Python identifier CADModel
    __CADModel = pyxb.binding.content.AttributeUse(pyxb.namespace.ExpandedName(None, u'CADModel'), 'CADModel', '__domainmapping_CAD2EDATransform__CADModel', pyxb.binding.datatypes.anyURI, required=True)
    __CADModel._DeclarationLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 20, 8)
    __CADModel._UseLocation = pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 20, 8)
    
    CADModel = property(__CADModel.value, __CADModel.set, None, None)

    _ElementMap.update({
        
    })
    _AttributeMap.update({
        __RotationX.name() : __RotationX,
        __RotationY.name() : __RotationY,
        __RotationZ.name() : __RotationZ,
        __TranslationX.name() : __TranslationX,
        __TranslationY.name() : __TranslationY,
        __TranslationZ.name() : __TranslationZ,
        __ScaleX.name() : __ScaleX,
        __ScaleY.name() : __ScaleY,
        __ScaleZ.name() : __ScaleZ,
        __EDAModel.name() : __EDAModel,
        __CADModel.name() : __CADModel
    })
Namespace.addCategoryObject('typeBinding', u'CAD2EDATransform', CAD2EDATransform_)


CAD2EDATransform = pyxb.binding.basis.element(pyxb.namespace.ExpandedName(Namespace, u'CAD2EDATransform'), CAD2EDATransform_, location=pyxb.utils.utility.Location(u'avm.domainmapping.xsd', 6, 2))
Namespace.addCategoryObject('elementBinding', CAD2EDATransform.name().localName(), CAD2EDATransform)
