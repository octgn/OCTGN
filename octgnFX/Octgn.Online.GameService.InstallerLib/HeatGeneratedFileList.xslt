<?xml version="1.0" ?>
<xsl:stylesheet version="2.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:wix="http://schemas.microsoft.com/wix/2006/wi">

  <!-- Copy all attributes and elements to the output. -->
  <xsl:template match="@*|*">
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <xsl:apply-templates select="*" />
    </xsl:copy>
  </xsl:template>

  <xsl:output method="xml" indent="yes" />

  <xsl:key name="exe-search" match="wix:Component[substring(wix:File/@Source, string-length(wix:File/@Source) - string-length('Octgn.Online.GameService.exe') +1)='Octgn.Online.GameService.exe']" use="@Id" />
  <xsl:template match="wix:Component[key('exe-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('exe-search', @Id)]" />

  <xsl:key name="exe-config-search" match="wix:Component[substring(wix:File/@Source, string-length(wix:File/@Source) - string-length('Octgn.Online.GameService.exe.config') +1)='Octgn.Online.GameService.exe.config']" use="@Id" />
  <xsl:template match="wix:Component[key('exe-config-search', @Id)]" />
  <xsl:template match="wix:ComponentRef[key('exe-config-search', @Id)]" />
</xsl:stylesheet>
