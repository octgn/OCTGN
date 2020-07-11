CREATE TABLE octgn_packages (
	id text NOT NULL,
	version text NOT NULL,
	name text NOT NULL,
	description text NOT NULL,
	website text NOT NULL,
	icon text NOT NULL,
	path text NOT NULL,
	octgnVersion text NOT NULL,
	combinedDependencies text NULL,
	PRIMARY KEY (id, version)
);

CREATE TABLE octgn_plugins (
	id text NOT NULL,
	packageId text NOT NULL,
	packageVersion text NOT NULL,
	name text NOT NULL,
	description text NOT NULL,
	type text NOT NULL,
	format text NOT NULL,
	icon text NULL,
	path text NOT NULL,
	PRIMARY KEY (id, packageId, packageVersion)
);

INSERT INTO
	octgn_packages(id, version, name, description, website, icon, path, octgnVersion)
	values('octgn.sdk', '3.4.0.0', 'Octgn SDK', 'Built in Package', 'https://www.octgn.net', 'icon.ico', 'integrated', '3.4.0.0');

INSERT INTO
	octgn_plugins(id, packageId, packageVersion, name, description, type, format, path)
	values('octgn.plugin.format.xml', 'octgn.sdk', '3.4.0.0', 'Octgn Xml Plugin Format', 'Octgn Xml Plugin Format', 'octgn.plugin.format', 'octgn.plugin.format.integrated', 'integrated');
INSERT INTO
	octgn_plugins(id, packageId, packageVersion, name, description, type, format, path)
	values('octgn.plugin.format.yaml', 'octgn.sdk', '3.4.0.0', 'Octgn Yaml Plugin Format', 'Octgn Yaml Plugin Format', 'octgn.plugin.format', 'octgn.plugin.format.integrated', 'integrated');
INSERT INTO
	octgn_plugins(id, packageId, packageVersion, name, description, type, format, path)
	values('octgn.plugin.format.dotnet', 'octgn.sdk', '3.4.0.0', 'Octgn DotNet Plugin Format', 'Octgn DotNet Plugin Format', 'octgn.plugin.format', 'octgn.plugin.format.integrated', 'integrated');
INSERT INTO
	octgn_plugins(id, packageId, packageVersion, name, description, type, format, path)
	values('octgn.plugin.menu', 'octgn.sdk', '3.4.0.0', 'Octgn Menu Plugin', 'Octgn Menu Plugin', 'octgn.plugin', 'octgn.plugin.format.integrated', 'integrated');

PRAGMA user_version = 1;