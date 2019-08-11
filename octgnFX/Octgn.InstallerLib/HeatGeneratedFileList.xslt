<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"
    xmlns="http://schemas.microsoft.com/wix/2006/wi"
    exclude-result-prefixes="xsl wix">

  <xsl:output method="xml" indent="yes" omit-xml-declaration="yes" />

  <xsl:strip-space elements="*" />

  <xsl:key
        name="PdbToRemove"
        match="wix:Component[ substring( wix:File/@Source, string-length( wix:File/@Source ) - 3 ) = '.pdb' ]"
        use="@Id"
    />

  <xsl:key
        name="XmlToRemove"
        match="wix:Component[ substring( wix:File/@Source, string-length( wix:File/@Source ) - 3 ) = '.xml' ]"
        use="@Id"
    />

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="*[ self::wix:Component or self::wix:ComponentRef ][ key( 'PdbToRemove', @Id ) ]" />
  <xsl:template match="*[ self::wix:Component or self::wix:ComponentRef ][ key( 'XmlToRemove', @Id ) ]" />

  <xsl:template match="wix:DirectoryRef">
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <Component Id="RemoveInstalledDirectories" Guid="{AEBB5117-9734-4364-A4ED-1FD2221B0961}" Location="local">
        <xsl:for-each select=".//wix:Directory[@Id]">
          <RemoveFolder Id="{@Id}" Directory="{@Id}" On="uninstall" />
        </xsl:for-each>
        <RegistryValue Root="HKCU" Key="Software\Octgn" Name="Installed" Type="integer" Value="1" KeyPath="yes" />
      </Component>
      <xsl:apply-templates select="node()" />
    </xsl:copy>
  </xsl:template>

  <!--File keypath to no and add registry keypath-->
  <xsl:template match="wix:Component/wix:File[@Id]">
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <xsl:attribute name="KeyPath">
        <xsl:text>no</xsl:text>
      </xsl:attribute>
    </xsl:copy>
    <RegistryValue Root="HKCU" Key="Software\Octgn" Name="Installed" Type="integer" Value="1" KeyPath="yes" />
  </xsl:template>
</xsl:stylesheet>