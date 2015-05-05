#ifndef CADAnalysisMetaData_xsd_H
#define CADAnalysisMetaData_xsd_H
#include <string>
#pragma warning( disable : 4010)

namespace CADAnalysisMetaData_xsd
{
const std::string& getString()
{
	static std::string str;
	if (str.empty())
	{
		str +="<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
		str +="<?udm interface=\"CADAnalysisMetaData\" version=\"1.00\"?>\n";
		str +="<xsd:schema xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"\n";
		str +=" elementFormDefault=\"qualified\" \n";
		str +=">\n";
//		str +="<!-- generated on Mon Dec 02 17:13:33 2013 -->\n";
		str +="\n";
		str +="\n";
		str +="	<xsd:complexType name=\"MetricType\">\n";
		str +="		<xsd:attribute name=\"ConfigurationID\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"MetricType\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"TopAssemblyComponentInstanceID\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"ComponenInstancetID\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"ComponentName\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"ComponentType\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"RequestedValueType\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"MetricID\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"Details\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"_id\" type=\"xsd:ID\"/>\n";
		str +="		<xsd:attribute name=\"_archetype\" type=\"xsd:IDREF\"/>\n";
		str +="		<xsd:attribute name=\"_derived\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_instances\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_desynched_atts\" type=\"xsd:string\"/>\n";
		str +="		<xsd:attribute name=\"_real_archetype\" type=\"xsd:boolean\"/>\n";
		str +="		<xsd:attribute name=\"_subtype\" type=\"xsd:boolean\"/>\n";
		str +="	</xsd:complexType>\n";
		str +="\n";
		str +="	<xsd:complexType name=\"MetricsType\">\n";
		str +="		<xsd:sequence>\n";
		str +="			<xsd:element name=\"Metric\" type=\"MetricType\" minOccurs=\"0\" maxOccurs=\"unbounded\"/>\n";
		str +="			<xsd:element name=\"Metrics\" type=\"MetricsType\" minOccurs=\"0\" maxOccurs=\"unbounded\"/>\n";
		str +="		</xsd:sequence>\n";
		str +="		<xsd:attribute name=\"_id\" type=\"xsd:ID\"/>\n";
		str +="		<xsd:attribute name=\"_archetype\" type=\"xsd:IDREF\"/>\n";
		str +="		<xsd:attribute name=\"_derived\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_instances\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_desynched_atts\" type=\"xsd:string\"/>\n";
		str +="		<xsd:attribute name=\"_real_archetype\" type=\"xsd:boolean\"/>\n";
		str +="		<xsd:attribute name=\"_subtype\" type=\"xsd:boolean\"/>\n";
		str +="		<xsd:attribute name=\"_libname\" type=\"xsd:string\"/>\n";
		str +="	</xsd:complexType>\n";
		str +="\n";
		str +="	<xsd:complexType name=\"LifeCycleType\">\n";
		str +="		<xsd:attribute name=\"Duration\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"NumberOfCycles\" type=\"xsd:long\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"_id\" type=\"xsd:ID\"/>\n";
		str +="		<xsd:attribute name=\"_archetype\" type=\"xsd:IDREF\"/>\n";
		str +="		<xsd:attribute name=\"_derived\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_instances\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_desynched_atts\" type=\"xsd:string\"/>\n";
		str +="		<xsd:attribute name=\"_real_archetype\" type=\"xsd:boolean\"/>\n";
		str +="		<xsd:attribute name=\"_subtype\" type=\"xsd:boolean\"/>\n";
		str +="	</xsd:complexType>\n";
		str +="\n";
		str +="	<xsd:complexType name=\"AnalysisSupportingDataType\">\n";
		str +="		<xsd:sequence>\n";
		str +="			<xsd:element name=\"LifeCycle\" type=\"LifeCycleType\" minOccurs=\"0\"/>\n";
		str +="		</xsd:sequence>\n";
		str +="		<xsd:attribute name=\"AnalysisType\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"_id\" type=\"xsd:ID\"/>\n";
		str +="		<xsd:attribute name=\"_archetype\" type=\"xsd:IDREF\"/>\n";
		str +="		<xsd:attribute name=\"_derived\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_instances\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_desynched_atts\" type=\"xsd:string\"/>\n";
		str +="		<xsd:attribute name=\"_real_archetype\" type=\"xsd:boolean\"/>\n";
		str +="		<xsd:attribute name=\"_subtype\" type=\"xsd:boolean\"/>\n";
		str +="	</xsd:complexType>\n";
		str +="\n";
		str +="	<xsd:complexType name=\"AllowableBearingStressType\">\n";
		str +="		<xsd:attribute name=\"Value\" type=\"xsd:double\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"Units\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"Source\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"_id\" type=\"xsd:ID\"/>\n";
		str +="		<xsd:attribute name=\"_archetype\" type=\"xsd:IDREF\"/>\n";
		str +="		<xsd:attribute name=\"_derived\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_instances\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_desynched_atts\" type=\"xsd:string\"/>\n";
		str +="		<xsd:attribute name=\"_real_archetype\" type=\"xsd:boolean\"/>\n";
		str +="		<xsd:attribute name=\"_subtype\" type=\"xsd:boolean\"/>\n";
		str +="	</xsd:complexType>\n";
		str +="\n";
		str +="	<xsd:complexType name=\"AllowableShearStressType\">\n";
		str +="		<xsd:attribute name=\"Value\" type=\"xsd:double\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"Units\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"Source\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"_id\" type=\"xsd:ID\"/>\n";
		str +="		<xsd:attribute name=\"_archetype\" type=\"xsd:IDREF\"/>\n";
		str +="		<xsd:attribute name=\"_derived\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_instances\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_desynched_atts\" type=\"xsd:string\"/>\n";
		str +="		<xsd:attribute name=\"_real_archetype\" type=\"xsd:boolean\"/>\n";
		str +="		<xsd:attribute name=\"_subtype\" type=\"xsd:boolean\"/>\n";
		str +="	</xsd:complexType>\n";
		str +="\n";
		str +="	<xsd:complexType name=\"AllowableTensileStressType\">\n";
		str +="		<xsd:attribute name=\"Value\" type=\"xsd:double\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"Units\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"Source\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"_id\" type=\"xsd:ID\"/>\n";
		str +="		<xsd:attribute name=\"_archetype\" type=\"xsd:IDREF\"/>\n";
		str +="		<xsd:attribute name=\"_derived\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_instances\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_desynched_atts\" type=\"xsd:string\"/>\n";
		str +="		<xsd:attribute name=\"_real_archetype\" type=\"xsd:boolean\"/>\n";
		str +="		<xsd:attribute name=\"_subtype\" type=\"xsd:boolean\"/>\n";
		str +="	</xsd:complexType>\n";
		str +="\n";
		str +="	<xsd:complexType name=\"MaterialsType\">\n";
		str +="		<xsd:sequence>\n";
		str +="			<xsd:element name=\"Material\" type=\"MaterialType\" minOccurs=\"0\" maxOccurs=\"unbounded\"/>\n";
		str +="		</xsd:sequence>\n";
		str +="		<xsd:attribute name=\"_id\" type=\"xsd:ID\"/>\n";
		str +="		<xsd:attribute name=\"_archetype\" type=\"xsd:IDREF\"/>\n";
		str +="		<xsd:attribute name=\"_derived\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_instances\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_desynched_atts\" type=\"xsd:string\"/>\n";
		str +="		<xsd:attribute name=\"_real_archetype\" type=\"xsd:boolean\"/>\n";
		str +="		<xsd:attribute name=\"_subtype\" type=\"xsd:boolean\"/>\n";
		str +="	</xsd:complexType>\n";
		str +="\n";
		str +="	<xsd:complexType name=\"MaterialType\">\n";
		str +="		<xsd:sequence>\n";
		str +="			<xsd:element name=\"MaterialProperties\" type=\"MaterialPropertiesType\" minOccurs=\"0\" maxOccurs=\"unbounded\"/>\n";
		str +="		</xsd:sequence>\n";
		str +="		<xsd:attribute name=\"MaterialID\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"_id\" type=\"xsd:ID\"/>\n";
		str +="		<xsd:attribute name=\"_archetype\" type=\"xsd:IDREF\"/>\n";
		str +="		<xsd:attribute name=\"_derived\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_instances\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_desynched_atts\" type=\"xsd:string\"/>\n";
		str +="		<xsd:attribute name=\"_real_archetype\" type=\"xsd:boolean\"/>\n";
		str +="		<xsd:attribute name=\"_subtype\" type=\"xsd:boolean\"/>\n";
		str +="	</xsd:complexType>\n";
		str +="\n";
		str +="	<xsd:complexType name=\"MaterialPropertiesType\">\n";
		str +="		<xsd:sequence>\n";
		str +="			<xsd:element name=\"AllowableBearingStress\" type=\"AllowableBearingStressType\" minOccurs=\"0\"/>\n";
		str +="			<xsd:element name=\"AllowableShearStress\" type=\"AllowableShearStressType\" minOccurs=\"0\"/>\n";
		str +="			<xsd:element name=\"AllowableTensileStress\" type=\"AllowableTensileStressType\" minOccurs=\"0\"/>\n";
		str +="		</xsd:sequence>\n";
		str +="		<xsd:attribute name=\"_id\" type=\"xsd:ID\"/>\n";
		str +="		<xsd:attribute name=\"_archetype\" type=\"xsd:IDREF\"/>\n";
		str +="		<xsd:attribute name=\"_derived\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_instances\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_desynched_atts\" type=\"xsd:string\"/>\n";
		str +="		<xsd:attribute name=\"_real_archetype\" type=\"xsd:boolean\"/>\n";
		str +="		<xsd:attribute name=\"_subtype\" type=\"xsd:boolean\"/>\n";
		str +="	</xsd:complexType>\n";
		str +="\n";
		str +="	<xsd:complexType name=\"AssembliesType\">\n";
		str +="		<xsd:sequence>\n";
		str +="			<xsd:element name=\"Assembly\" type=\"AssemblyType\" minOccurs=\"0\" maxOccurs=\"unbounded\"/>\n";
		str +="		</xsd:sequence>\n";
		str +="		<xsd:attribute name=\"_id\" type=\"xsd:ID\"/>\n";
		str +="		<xsd:attribute name=\"_archetype\" type=\"xsd:IDREF\"/>\n";
		str +="		<xsd:attribute name=\"_derived\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_instances\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_desynched_atts\" type=\"xsd:string\"/>\n";
		str +="		<xsd:attribute name=\"_real_archetype\" type=\"xsd:boolean\"/>\n";
		str +="		<xsd:attribute name=\"_subtype\" type=\"xsd:boolean\"/>\n";
		str +="	</xsd:complexType>\n";
		str +="\n";
		str +="	<xsd:complexType name=\"ComponentType\">\n";
		str +="		<xsd:sequence>\n";
		str +="			<xsd:element name=\"Component\" type=\"ComponentType\" minOccurs=\"0\" maxOccurs=\"unbounded\"/>\n";
		str +="		</xsd:sequence>\n";
		str +="		<xsd:attribute name=\"Name\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"Type\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"MaterialID\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"ComponentInstanceID\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"FEAElementType\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"FEAElementID\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"_id\" type=\"xsd:ID\"/>\n";
		str +="		<xsd:attribute name=\"_archetype\" type=\"xsd:IDREF\"/>\n";
		str +="		<xsd:attribute name=\"_derived\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_instances\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_desynched_atts\" type=\"xsd:string\"/>\n";
		str +="		<xsd:attribute name=\"_real_archetype\" type=\"xsd:boolean\"/>\n";
		str +="		<xsd:attribute name=\"_subtype\" type=\"xsd:boolean\"/>\n";
		str +="	</xsd:complexType>\n";
		str +="\n";
		str +="	<xsd:complexType name=\"CADAnalysisMetaDataType\">\n";
		str +="		<xsd:sequence>\n";
		str +="			<xsd:element name=\"AnalysisSupportingData\" type=\"AnalysisSupportingDataType\"/>\n";
		str +="			<xsd:element name=\"Assemblies\" type=\"AssembliesType\"/>\n";
		str +="			<xsd:element name=\"Materials\" type=\"MaterialsType\"/>\n";
		str +="			<xsd:element name=\"CADAnalysisMetaData\" type=\"CADAnalysisMetaDataType\" minOccurs=\"0\" maxOccurs=\"unbounded\"/>\n";
		str +="		</xsd:sequence>\n";
		str +="		<xsd:attribute name=\"_id\" type=\"xsd:ID\"/>\n";
		str +="		<xsd:attribute name=\"_archetype\" type=\"xsd:IDREF\"/>\n";
		str +="		<xsd:attribute name=\"_derived\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_instances\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_desynched_atts\" type=\"xsd:string\"/>\n";
		str +="		<xsd:attribute name=\"_real_archetype\" type=\"xsd:boolean\"/>\n";
		str +="		<xsd:attribute name=\"_subtype\" type=\"xsd:boolean\"/>\n";
		str +="		<xsd:attribute name=\"_libname\" type=\"xsd:string\"/>\n";
		str +="	</xsd:complexType>\n";
		str +="\n";
		str +="	<xsd:complexType name=\"AssemblyType\">\n";
		str +="		<xsd:sequence>\n";
		str +="			<xsd:element name=\"Component\" type=\"ComponentType\" minOccurs=\"0\" maxOccurs=\"unbounded\"/>\n";
		str +="		</xsd:sequence>\n";
		str +="		<xsd:attribute name=\"ConfigurationID\" type=\"xsd:string\" use=\"required\"/>\n";
		str +="		<xsd:attribute name=\"_id\" type=\"xsd:ID\"/>\n";
		str +="		<xsd:attribute name=\"_archetype\" type=\"xsd:IDREF\"/>\n";
		str +="		<xsd:attribute name=\"_derived\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_instances\" type=\"xsd:IDREFS\"/>\n";
		str +="		<xsd:attribute name=\"_desynched_atts\" type=\"xsd:string\"/>\n";
		str +="		<xsd:attribute name=\"_real_archetype\" type=\"xsd:boolean\"/>\n";
		str +="		<xsd:attribute name=\"_subtype\" type=\"xsd:boolean\"/>\n";
		str +="	</xsd:complexType>\n";
		str +="\n";
		str +=" <xsd:element name=\"Metrics\" type=\"MetricsType\"/>\n";
		str +=" <xsd:element name=\"CADAnalysisMetaData\" type=\"CADAnalysisMetaDataType\"/>\n";
		str +="\n";
		str +="</xsd:schema>\n";
		str +="\n";
	}
		return str;
}
} //namespace
#endif
