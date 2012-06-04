= Autoupdating xml

If you want to use autoupdating xmls, you need to provide OCTGN with link to valid xml.
Xml file need to have this construction:
```
<?xml version="1.0" encoding="UTF-8"?>
<set>
  <name>name of the set</name>
  <game>uuid of the game</game>
  <uuid>uuid of this specific set</uuid>
  <version>version of set</version>
  <date>date of last regeneration</date>
  <link>link to download set</link>
  <user>user to basic auth to download set (of left empty)</user>
  <password>password to basic auth to download set (of left empty)</password>
</set>
```