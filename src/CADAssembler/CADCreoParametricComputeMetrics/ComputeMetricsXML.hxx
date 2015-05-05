// Copyright (C) 2005-2010 Code Synthesis Tools CC
//
// This program was generated by CodeSynthesis XSD, an XML Schema
// to C++ data binding compiler, in the Proprietary License mode.
// You should have received a proprietary license from Code Synthesis
// Tools CC prior to generating this code. See the license text for
// conditions.
//

#ifndef COMPUTE_METRICS_XML_HXX
#define COMPUTE_METRICS_XML_HXX

// Begin prologue.
//
//
// End prologue.

#include <xsd/cxx/config.hxx>

#if (XSD_INT_VERSION != 3030000L)
#error XSD runtime version mismatch
#endif

#include <xsd/cxx/pre.hxx>

#ifndef XSD_USE_CHAR
#define XSD_USE_CHAR
#endif

#ifndef XSD_CXX_TREE_USE_CHAR
#define XSD_CXX_TREE_USE_CHAR
#endif

#include <xsd/cxx/xml/char-utf8.hxx>

#include <xsd/cxx/tree/exceptions.hxx>
#include <xsd/cxx/tree/elements.hxx>
#include <xsd/cxx/tree/types.hxx>

#include <xsd/cxx/xml/error-handler.hxx>

#include <xsd/cxx/xml/dom/auto-ptr.hxx>

#include <xsd/cxx/tree/parsing.hxx>
#include <xsd/cxx/tree/parsing/byte.hxx>
#include <xsd/cxx/tree/parsing/unsigned-byte.hxx>
#include <xsd/cxx/tree/parsing/short.hxx>
#include <xsd/cxx/tree/parsing/unsigned-short.hxx>
#include <xsd/cxx/tree/parsing/int.hxx>
#include <xsd/cxx/tree/parsing/unsigned-int.hxx>
#include <xsd/cxx/tree/parsing/long.hxx>
#include <xsd/cxx/tree/parsing/unsigned-long.hxx>
#include <xsd/cxx/tree/parsing/boolean.hxx>
#include <xsd/cxx/tree/parsing/float.hxx>
#include <xsd/cxx/tree/parsing/double.hxx>
#include <xsd/cxx/tree/parsing/decimal.hxx>

namespace xml_schema
{
  // anyType and anySimpleType.
  //
  typedef ::xsd::cxx::tree::type type;
  typedef ::xsd::cxx::tree::simple_type< type > simple_type;
  typedef ::xsd::cxx::tree::type container;

  // 8-bit
  //
  typedef signed char byte;
  typedef unsigned char unsigned_byte;

  // 16-bit
  //
  typedef short short_;
  typedef unsigned short unsigned_short;

  // 32-bit
  //
  typedef int int_;
  typedef unsigned int unsigned_int;

  // 64-bit
  //
  typedef long long long_;
  typedef unsigned long long unsigned_long;

  // Supposed to be arbitrary-length integral types.
  //
  typedef long long integer;
  typedef long long non_positive_integer;
  typedef unsigned long long non_negative_integer;
  typedef unsigned long long positive_integer;
  typedef long long negative_integer;

  // Boolean.
  //
  typedef bool boolean;

  // Floating-point types.
  //
  typedef float float_;
  typedef double double_;
  typedef double decimal;

  // String types.
  //
  typedef ::xsd::cxx::tree::string< char, simple_type > string;
  typedef ::xsd::cxx::tree::normalized_string< char, string > normalized_string;
  typedef ::xsd::cxx::tree::token< char, normalized_string > token;
  typedef ::xsd::cxx::tree::name< char, token > name;
  typedef ::xsd::cxx::tree::nmtoken< char, token > nmtoken;
  typedef ::xsd::cxx::tree::nmtokens< char, simple_type, nmtoken > nmtokens;
  typedef ::xsd::cxx::tree::ncname< char, name > ncname;
  typedef ::xsd::cxx::tree::language< char, token > language;

  // ID/IDREF.
  //
  typedef ::xsd::cxx::tree::id< char, ncname > id;
  typedef ::xsd::cxx::tree::idref< char, ncname, type > idref;
  typedef ::xsd::cxx::tree::idrefs< char, simple_type, idref > idrefs;

  // URI.
  //
  typedef ::xsd::cxx::tree::uri< char, simple_type > uri;

  // Qualified name.
  //
  typedef ::xsd::cxx::tree::qname< char, simple_type, uri, ncname > qname;

  // Binary.
  //
  typedef ::xsd::cxx::tree::buffer< char > buffer;
  typedef ::xsd::cxx::tree::base64_binary< char, simple_type > base64_binary;
  typedef ::xsd::cxx::tree::hex_binary< char, simple_type > hex_binary;

  // Date/time.
  //
  typedef ::xsd::cxx::tree::time_zone time_zone;
  typedef ::xsd::cxx::tree::date< char, simple_type > date;
  typedef ::xsd::cxx::tree::date_time< char, simple_type > date_time;
  typedef ::xsd::cxx::tree::duration< char, simple_type > duration;
  typedef ::xsd::cxx::tree::gday< char, simple_type > gday;
  typedef ::xsd::cxx::tree::gmonth< char, simple_type > gmonth;
  typedef ::xsd::cxx::tree::gmonth_day< char, simple_type > gmonth_day;
  typedef ::xsd::cxx::tree::gyear< char, simple_type > gyear;
  typedef ::xsd::cxx::tree::gyear_month< char, simple_type > gyear_month;
  typedef ::xsd::cxx::tree::time< char, simple_type > time;

  // Entity.
  //
  typedef ::xsd::cxx::tree::entity< char, ncname > entity;
  typedef ::xsd::cxx::tree::entities< char, simple_type, entity > entities;

  // Flags and properties.
  //
  typedef ::xsd::cxx::tree::flags flags;
  typedef ::xsd::cxx::tree::properties< char > properties;

  // Parsing/serialization diagnostics.
  //
  typedef ::xsd::cxx::tree::severity severity;
  typedef ::xsd::cxx::tree::error< char > error;
  typedef ::xsd::cxx::tree::diagnostics< char > diagnostics;

  // Exceptions.
  //
  typedef ::xsd::cxx::tree::exception< char > exception;
  typedef ::xsd::cxx::tree::bounds< char > bounds;
  typedef ::xsd::cxx::tree::duplicate_id< char > duplicate_id;
  typedef ::xsd::cxx::tree::parsing< char > parsing;
  typedef ::xsd::cxx::tree::expected_element< char > expected_element;
  typedef ::xsd::cxx::tree::unexpected_element< char > unexpected_element;
  typedef ::xsd::cxx::tree::expected_attribute< char > expected_attribute;
  typedef ::xsd::cxx::tree::unexpected_enumerator< char > unexpected_enumerator;
  typedef ::xsd::cxx::tree::expected_text_content< char > expected_text_content;
  typedef ::xsd::cxx::tree::no_prefix_mapping< char > no_prefix_mapping;

  // Error handler callback interface.
  //
  typedef ::xsd::cxx::xml::error_handler< char > error_handler;

  // DOM interaction.
  //
  namespace dom
  {
    // Automatic pointer for DOMDocument.
    //
    using ::xsd::cxx::xml::dom::auto_ptr;

#ifndef XSD_CXX_TREE_TREE_NODE_KEY__XML_SCHEMA
#define XSD_CXX_TREE_TREE_NODE_KEY__XML_SCHEMA
    // DOM user data key for back pointers to tree nodes.
    //
    const XMLCh* const tree_node_key = ::xsd::cxx::tree::user_data_keys::node;
#endif
  }
}

// Forward declarations.
//
class CADIncrementParameterType;
class CADReadParameterType;
class IncrementType;
class ReadType;
class ParametricParametersType;
class UnitsType;
class CADComponentType;
class CADComponentsType;

#include <memory>    // std::auto_ptr
#include <limits>    // std::numeric_limits
#include <algorithm> // std::binary_search

#include <xsd/cxx/xml/char-utf8.hxx>

#include <xsd/cxx/tree/exceptions.hxx>
#include <xsd/cxx/tree/elements.hxx>
#include <xsd/cxx/tree/containers.hxx>
#include <xsd/cxx/tree/list.hxx>

#include <xsd/cxx/xml/dom/parsing-header.hxx>

class CADIncrementParameterType: public ::xml_schema::type
{
  public:
  // Units
  // 
  typedef ::UnitsType Units_type;
  typedef ::xsd::cxx::tree::optional< Units_type > Units_optional;
  typedef ::xsd::cxx::tree::traits< Units_type, char > Units_traits;

  const Units_optional&
  Units () const;

  Units_optional&
  Units ();

  void
  Units (const Units_type& x);

  void
  Units (const Units_optional& x);

  void
  Units (::std::auto_ptr< Units_type > p);

  // Type
  // 
  typedef ::xml_schema::string Type_type;
  typedef ::xsd::cxx::tree::traits< Type_type, char > Type_traits;

  const Type_type&
  Type () const;

  Type_type&
  Type ();

  void
  Type (const Type_type& x);

  void
  Type (::std::auto_ptr< Type_type > p);

  // Name
  // 
  typedef ::xml_schema::string Name_type;
  typedef ::xsd::cxx::tree::traits< Name_type, char > Name_traits;

  const Name_type&
  Name () const;

  Name_type&
  Name ();

  void
  Name (const Name_type& x);

  void
  Name (::std::auto_ptr< Name_type > p);

  // StartValue
  // 
  typedef ::xml_schema::string StartValue_type;
  typedef ::xsd::cxx::tree::traits< StartValue_type, char > StartValue_traits;

  const StartValue_type&
  StartValue () const;

  StartValue_type&
  StartValue ();

  void
  StartValue (const StartValue_type& x);

  void
  StartValue (::std::auto_ptr< StartValue_type > p);

  // EndValue
  // 
  typedef ::xml_schema::string EndValue_type;
  typedef ::xsd::cxx::tree::traits< EndValue_type, char > EndValue_traits;

  const EndValue_type&
  EndValue () const;

  EndValue_type&
  EndValue ();

  void
  EndValue (const EndValue_type& x);

  void
  EndValue (::std::auto_ptr< EndValue_type > p);

  // Increment
  // 
  typedef ::xml_schema::string Increment_type;
  typedef ::xsd::cxx::tree::traits< Increment_type, char > Increment_traits;

  const Increment_type&
  Increment () const;

  Increment_type&
  Increment ();

  void
  Increment (const Increment_type& x);

  void
  Increment (::std::auto_ptr< Increment_type > p);

  // Constructors.
  //
  CADIncrementParameterType (const Type_type&,
                             const Name_type&,
                             const StartValue_type&,
                             const EndValue_type&,
                             const Increment_type&);

  CADIncrementParameterType (const ::xercesc::DOMElement& e,
                             ::xml_schema::flags f = 0,
                             ::xml_schema::container* c = 0);

  CADIncrementParameterType (const CADIncrementParameterType& x,
                             ::xml_schema::flags f = 0,
                             ::xml_schema::container* c = 0);

  virtual CADIncrementParameterType*
  _clone (::xml_schema::flags f = 0,
          ::xml_schema::container* c = 0) const;

  virtual 
  ~CADIncrementParameterType ();

  // Implementation.
  //
  protected:
  void
  parse (::xsd::cxx::xml::dom::parser< char >&,
         ::xml_schema::flags);

  protected:
  Units_optional Units_;
  ::xsd::cxx::tree::one< Type_type > Type_;
  ::xsd::cxx::tree::one< Name_type > Name_;
  ::xsd::cxx::tree::one< StartValue_type > StartValue_;
  ::xsd::cxx::tree::one< EndValue_type > EndValue_;
  ::xsd::cxx::tree::one< Increment_type > Increment_;
};

class CADReadParameterType: public ::xml_schema::type
{
  public:
  // Owner
  // 
  typedef ::xml_schema::string Owner_type;
  typedef ::xsd::cxx::tree::traits< Owner_type, char > Owner_traits;

  const Owner_type&
  Owner () const;

  Owner_type&
  Owner ();

  void
  Owner (const Owner_type& x);

  void
  Owner (::std::auto_ptr< Owner_type > p);

  // Name
  // 
  typedef ::xml_schema::string Name_type;
  typedef ::xsd::cxx::tree::traits< Name_type, char > Name_traits;

  const Name_type&
  Name () const;

  Name_type&
  Name ();

  void
  Name (const Name_type& x);

  void
  Name (::std::auto_ptr< Name_type > p);

  // Constructors.
  //
  CADReadParameterType (const Owner_type&,
                        const Name_type&);

  CADReadParameterType (const ::xercesc::DOMElement& e,
                        ::xml_schema::flags f = 0,
                        ::xml_schema::container* c = 0);

  CADReadParameterType (const CADReadParameterType& x,
                        ::xml_schema::flags f = 0,
                        ::xml_schema::container* c = 0);

  virtual CADReadParameterType*
  _clone (::xml_schema::flags f = 0,
          ::xml_schema::container* c = 0) const;

  virtual 
  ~CADReadParameterType ();

  // Implementation.
  //
  protected:
  void
  parse (::xsd::cxx::xml::dom::parser< char >&,
         ::xml_schema::flags);

  protected:
  ::xsd::cxx::tree::one< Owner_type > Owner_;
  ::xsd::cxx::tree::one< Name_type > Name_;
};

class IncrementType: public ::xml_schema::type
{
  public:
  // CADIncrementParameter
  // 
  typedef ::CADIncrementParameterType CADIncrementParameter_type;
  typedef ::xsd::cxx::tree::sequence< CADIncrementParameter_type > CADIncrementParameter_sequence;
  typedef CADIncrementParameter_sequence::iterator CADIncrementParameter_iterator;
  typedef CADIncrementParameter_sequence::const_iterator CADIncrementParameter_const_iterator;
  typedef ::xsd::cxx::tree::traits< CADIncrementParameter_type, char > CADIncrementParameter_traits;

  const CADIncrementParameter_sequence&
  CADIncrementParameter () const;

  CADIncrementParameter_sequence&
  CADIncrementParameter ();

  void
  CADIncrementParameter (const CADIncrementParameter_sequence& s);

  // Constructors.
  //
  IncrementType ();

  IncrementType (const ::xercesc::DOMElement& e,
                 ::xml_schema::flags f = 0,
                 ::xml_schema::container* c = 0);

  IncrementType (const IncrementType& x,
                 ::xml_schema::flags f = 0,
                 ::xml_schema::container* c = 0);

  virtual IncrementType*
  _clone (::xml_schema::flags f = 0,
          ::xml_schema::container* c = 0) const;

  virtual 
  ~IncrementType ();

  // Implementation.
  //
  protected:
  void
  parse (::xsd::cxx::xml::dom::parser< char >&,
         ::xml_schema::flags);

  protected:
  CADIncrementParameter_sequence CADIncrementParameter_;
};

class ReadType: public ::xml_schema::type
{
  public:
  // CADReadParameter
  // 
  typedef ::CADReadParameterType CADReadParameter_type;
  typedef ::xsd::cxx::tree::sequence< CADReadParameter_type > CADReadParameter_sequence;
  typedef CADReadParameter_sequence::iterator CADReadParameter_iterator;
  typedef CADReadParameter_sequence::const_iterator CADReadParameter_const_iterator;
  typedef ::xsd::cxx::tree::traits< CADReadParameter_type, char > CADReadParameter_traits;

  const CADReadParameter_sequence&
  CADReadParameter () const;

  CADReadParameter_sequence&
  CADReadParameter ();

  void
  CADReadParameter (const CADReadParameter_sequence& s);

  // Constructors.
  //
  ReadType ();

  ReadType (const ::xercesc::DOMElement& e,
            ::xml_schema::flags f = 0,
            ::xml_schema::container* c = 0);

  ReadType (const ReadType& x,
            ::xml_schema::flags f = 0,
            ::xml_schema::container* c = 0);

  virtual ReadType*
  _clone (::xml_schema::flags f = 0,
          ::xml_schema::container* c = 0) const;

  virtual 
  ~ReadType ();

  // Implementation.
  //
  protected:
  void
  parse (::xsd::cxx::xml::dom::parser< char >&,
         ::xml_schema::flags);

  protected:
  CADReadParameter_sequence CADReadParameter_;
};

class ParametricParametersType: public ::xml_schema::type
{
  public:
  // Increment
  // 
  typedef ::IncrementType Increment_type;
  typedef ::xsd::cxx::tree::optional< Increment_type > Increment_optional;
  typedef ::xsd::cxx::tree::traits< Increment_type, char > Increment_traits;

  const Increment_optional&
  Increment () const;

  Increment_optional&
  Increment ();

  void
  Increment (const Increment_type& x);

  void
  Increment (const Increment_optional& x);

  void
  Increment (::std::auto_ptr< Increment_type > p);

  // Read
  // 
  typedef ::ReadType Read_type;
  typedef ::xsd::cxx::tree::optional< Read_type > Read_optional;
  typedef ::xsd::cxx::tree::traits< Read_type, char > Read_traits;

  const Read_optional&
  Read () const;

  Read_optional&
  Read ();

  void
  Read (const Read_type& x);

  void
  Read (const Read_optional& x);

  void
  Read (::std::auto_ptr< Read_type > p);

  // Constructors.
  //
  ParametricParametersType ();

  ParametricParametersType (const ::xercesc::DOMElement& e,
                            ::xml_schema::flags f = 0,
                            ::xml_schema::container* c = 0);

  ParametricParametersType (const ParametricParametersType& x,
                            ::xml_schema::flags f = 0,
                            ::xml_schema::container* c = 0);

  virtual ParametricParametersType*
  _clone (::xml_schema::flags f = 0,
          ::xml_schema::container* c = 0) const;

  virtual 
  ~ParametricParametersType ();

  // Implementation.
  //
  protected:
  void
  parse (::xsd::cxx::xml::dom::parser< char >&,
         ::xml_schema::flags);

  protected:
  Increment_optional Increment_;
  Read_optional Read_;
};

class UnitsType: public ::xml_schema::type
{
  public:
  // Value
  // 
  typedef ::xml_schema::string Value_type;
  typedef ::xsd::cxx::tree::traits< Value_type, char > Value_traits;

  const Value_type&
  Value () const;

  Value_type&
  Value ();

  void
  Value (const Value_type& x);

  void
  Value (::std::auto_ptr< Value_type > p);

  // Constructors.
  //
  UnitsType (const Value_type&);

  UnitsType (const ::xercesc::DOMElement& e,
             ::xml_schema::flags f = 0,
             ::xml_schema::container* c = 0);

  UnitsType (const UnitsType& x,
             ::xml_schema::flags f = 0,
             ::xml_schema::container* c = 0);

  virtual UnitsType*
  _clone (::xml_schema::flags f = 0,
          ::xml_schema::container* c = 0) const;

  virtual 
  ~UnitsType ();

  // Implementation.
  //
  protected:
  void
  parse (::xsd::cxx::xml::dom::parser< char >&,
         ::xml_schema::flags);

  protected:
  ::xsd::cxx::tree::one< Value_type > Value_;
};

class CADComponentType: public ::xml_schema::type
{
  public:
  // ParametricParameters
  // 
  typedef ::ParametricParametersType ParametricParameters_type;
  typedef ::xsd::cxx::tree::optional< ParametricParameters_type > ParametricParameters_optional;
  typedef ::xsd::cxx::tree::traits< ParametricParameters_type, char > ParametricParameters_traits;

  const ParametricParameters_optional&
  ParametricParameters () const;

  ParametricParameters_optional&
  ParametricParameters ();

  void
  ParametricParameters (const ParametricParameters_type& x);

  void
  ParametricParameters (const ParametricParameters_optional& x);

  void
  ParametricParameters (::std::auto_ptr< ParametricParameters_type > p);

  // Name
  // 
  typedef ::xml_schema::string Name_type;
  typedef ::xsd::cxx::tree::traits< Name_type, char > Name_traits;

  const Name_type&
  Name () const;

  Name_type&
  Name ();

  void
  Name (const Name_type& x);

  void
  Name (::std::auto_ptr< Name_type > p);

  // Type
  // 
  typedef ::xml_schema::string Type_type;
  typedef ::xsd::cxx::tree::traits< Type_type, char > Type_traits;

  const Type_type&
  Type () const;

  Type_type&
  Type ();

  void
  Type (const Type_type& x);

  void
  Type (::std::auto_ptr< Type_type > p);

  // MetricsOutputFile
  // 
  typedef ::xml_schema::string MetricsOutputFile_type;
  typedef ::xsd::cxx::tree::traits< MetricsOutputFile_type, char > MetricsOutputFile_traits;

  const MetricsOutputFile_type&
  MetricsOutputFile () const;

  MetricsOutputFile_type&
  MetricsOutputFile ();

  void
  MetricsOutputFile (const MetricsOutputFile_type& x);

  void
  MetricsOutputFile (::std::auto_ptr< MetricsOutputFile_type > p);

  // Constructors.
  //
  CADComponentType (const Name_type&,
                    const Type_type&,
                    const MetricsOutputFile_type&);

  CADComponentType (const ::xercesc::DOMElement& e,
                    ::xml_schema::flags f = 0,
                    ::xml_schema::container* c = 0);

  CADComponentType (const CADComponentType& x,
                    ::xml_schema::flags f = 0,
                    ::xml_schema::container* c = 0);

  virtual CADComponentType*
  _clone (::xml_schema::flags f = 0,
          ::xml_schema::container* c = 0) const;

  virtual 
  ~CADComponentType ();

  // Implementation.
  //
  protected:
  void
  parse (::xsd::cxx::xml::dom::parser< char >&,
         ::xml_schema::flags);

  protected:
  ParametricParameters_optional ParametricParameters_;
  ::xsd::cxx::tree::one< Name_type > Name_;
  ::xsd::cxx::tree::one< Type_type > Type_;
  ::xsd::cxx::tree::one< MetricsOutputFile_type > MetricsOutputFile_;
};

class CADComponentsType: public ::xml_schema::type
{
  public:
  // CADComponent
  // 
  typedef ::CADComponentType CADComponent_type;
  typedef ::xsd::cxx::tree::sequence< CADComponent_type > CADComponent_sequence;
  typedef CADComponent_sequence::iterator CADComponent_iterator;
  typedef CADComponent_sequence::const_iterator CADComponent_const_iterator;
  typedef ::xsd::cxx::tree::traits< CADComponent_type, char > CADComponent_traits;

  const CADComponent_sequence&
  CADComponent () const;

  CADComponent_sequence&
  CADComponent ();

  void
  CADComponent (const CADComponent_sequence& s);

  // Constructors.
  //
  CADComponentsType ();

  CADComponentsType (const ::xercesc::DOMElement& e,
                     ::xml_schema::flags f = 0,
                     ::xml_schema::container* c = 0);

  CADComponentsType (const CADComponentsType& x,
                     ::xml_schema::flags f = 0,
                     ::xml_schema::container* c = 0);

  virtual CADComponentsType*
  _clone (::xml_schema::flags f = 0,
          ::xml_schema::container* c = 0) const;

  virtual 
  ~CADComponentsType ();

  // Implementation.
  //
  protected:
  void
  parse (::xsd::cxx::xml::dom::parser< char >&,
         ::xml_schema::flags);

  protected:
  CADComponent_sequence CADComponent_;
};

#include <iosfwd>

#include <xercesc/sax/InputSource.hpp>
#include <xercesc/dom/DOMDocument.hpp>
#include <xercesc/dom/DOMErrorHandler.hpp>

// Parse a URI or a local file.
//

::std::auto_ptr< ::CADComponentsType >
CADComponents (const ::std::string& uri,
               ::xml_schema::flags f = 0,
               const ::xml_schema::properties& p = ::xml_schema::properties ());

::std::auto_ptr< ::CADComponentsType >
CADComponents (const ::std::string& uri,
               ::xml_schema::error_handler& eh,
               ::xml_schema::flags f = 0,
               const ::xml_schema::properties& p = ::xml_schema::properties ());

::std::auto_ptr< ::CADComponentsType >
CADComponents (const ::std::string& uri,
               ::xercesc::DOMErrorHandler& eh,
               ::xml_schema::flags f = 0,
               const ::xml_schema::properties& p = ::xml_schema::properties ());

// Parse std::istream.
//

::std::auto_ptr< ::CADComponentsType >
CADComponents (::std::istream& is,
               ::xml_schema::flags f = 0,
               const ::xml_schema::properties& p = ::xml_schema::properties ());

::std::auto_ptr< ::CADComponentsType >
CADComponents (::std::istream& is,
               ::xml_schema::error_handler& eh,
               ::xml_schema::flags f = 0,
               const ::xml_schema::properties& p = ::xml_schema::properties ());

::std::auto_ptr< ::CADComponentsType >
CADComponents (::std::istream& is,
               ::xercesc::DOMErrorHandler& eh,
               ::xml_schema::flags f = 0,
               const ::xml_schema::properties& p = ::xml_schema::properties ());

::std::auto_ptr< ::CADComponentsType >
CADComponents (::std::istream& is,
               const ::std::string& id,
               ::xml_schema::flags f = 0,
               const ::xml_schema::properties& p = ::xml_schema::properties ());

::std::auto_ptr< ::CADComponentsType >
CADComponents (::std::istream& is,
               const ::std::string& id,
               ::xml_schema::error_handler& eh,
               ::xml_schema::flags f = 0,
               const ::xml_schema::properties& p = ::xml_schema::properties ());

::std::auto_ptr< ::CADComponentsType >
CADComponents (::std::istream& is,
               const ::std::string& id,
               ::xercesc::DOMErrorHandler& eh,
               ::xml_schema::flags f = 0,
               const ::xml_schema::properties& p = ::xml_schema::properties ());

// Parse xercesc::InputSource.
//

::std::auto_ptr< ::CADComponentsType >
CADComponents (::xercesc::InputSource& is,
               ::xml_schema::flags f = 0,
               const ::xml_schema::properties& p = ::xml_schema::properties ());

::std::auto_ptr< ::CADComponentsType >
CADComponents (::xercesc::InputSource& is,
               ::xml_schema::error_handler& eh,
               ::xml_schema::flags f = 0,
               const ::xml_schema::properties& p = ::xml_schema::properties ());

::std::auto_ptr< ::CADComponentsType >
CADComponents (::xercesc::InputSource& is,
               ::xercesc::DOMErrorHandler& eh,
               ::xml_schema::flags f = 0,
               const ::xml_schema::properties& p = ::xml_schema::properties ());

// Parse xercesc::DOMDocument.
//

::std::auto_ptr< ::CADComponentsType >
CADComponents (const ::xercesc::DOMDocument& d,
               ::xml_schema::flags f = 0,
               const ::xml_schema::properties& p = ::xml_schema::properties ());

::std::auto_ptr< ::CADComponentsType >
CADComponents (::xml_schema::dom::auto_ptr< ::xercesc::DOMDocument >& d,
               ::xml_schema::flags f = 0,
               const ::xml_schema::properties& p = ::xml_schema::properties ());

#include <xsd/cxx/post.hxx>

// Begin epilogue.
//
//
// End epilogue.

#endif // COMPUTE_METRICS_XML_HXX
