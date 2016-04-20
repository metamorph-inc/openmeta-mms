<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output encoding="UTF-8" method="html"/>

  
	<!-- match all nodes recursively (identity transform)-->
	<xsl:template match="@*|node()">
	  <xsl:copy>
		<xsl:apply-templates select="@*|node()"/>
	  </xsl:copy>
	</xsl:template>

	<!-- match root node -->
	<xsl:template match="/">
		<!-- Do header -->
		<xsl:text disable-output-escaping="yes">&lt;?xml version="1.0" encoding="UTF-16"?&gt;
&lt;!DOCTYPE project SYSTEM "mga.dtd"&gt;

</xsl:text>

		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>
		
  <!-- relabel SchematicModels as EDAModels -->
	<xsl:template match="model[@kind='SchematicModel']/@kind" >
    <xsl:attribute name="kind">
      <xsl:text>EDAModel</xsl:text>
    </xsl:attribute>
  </xsl:template>
  <xsl:template match="model[@role='SchematicModel']/@role" >
    <xsl:attribute name="role">
      <xsl:text>EDAModel</xsl:text>
    </xsl:attribute>
  </xsl:template>

  <!-- Change Model Parameter kind -->
  <xsl:template match="reference[@kind = 'SchematicModelParameter']/@kind" >
    <xsl:attribute name="kind">
      <xsl:text>EDAModelParameter</xsl:text>
    </xsl:attribute>
  </xsl:template>
  <xsl:template match="reference[@role = 'SchematicModelParameter']/@role" >
    <xsl:attribute name="role">
      <xsl:text>EDAModelParameter</xsl:text>
    </xsl:attribute>
  </xsl:template>

  <!-- Change Parameter mapping connection -->
  <xsl:template match="connection[@kind = 'SchematicModelParameterMap']/@kind" >
    <xsl:attribute name="kind">
      <xsl:text>EDAModelParameterMap</xsl:text>
    </xsl:attribute>
  </xsl:template>
  <xsl:template match="connection[@role = 'SchematicModelParameterMap']/@role" >
    <xsl:attribute name="role">
      <xsl:text>EDAModelParameterMap</xsl:text>
    </xsl:attribute>
  </xsl:template>

  <!-- Change attributes of SchematicModelPort -->
  <xsl:template match="attribute[@kind = 'Gate']/@kind" >
    <xsl:attribute name="kind">
      <xsl:text>EDAGate</xsl:text>
    </xsl:attribute>
  </xsl:template>
  <xsl:template match="attribute[@kind = 'SymbolLocationX']/@kind" >
    <xsl:attribute name="kind">
      <xsl:text>EDASymbolLocationX</xsl:text>
    </xsl:attribute>
  </xsl:template>
  <xsl:template match="attribute[@kind = 'SymbolLocationY']/@kind" >
    <xsl:attribute name="kind">
      <xsl:text>EDASymbolLocationY</xsl:text>
    </xsl:attribute>
  </xsl:template>
  <xsl:template match="attribute[@kind = 'SymbolRotation']/@kind" >
    <xsl:attribute name="kind">
      <xsl:text>EDASymbolRotation</xsl:text>
    </xsl:attribute>
  </xsl:template>

</xsl:stylesheet>