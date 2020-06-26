-- up
CREATE TABLE octgn_packages (
	id text NOT NULL,
	version text NOT NULL,
	name text NOT NULL,
	description text NOT NULL,
	website text NOT NULL,
	icon text NOT NULL,
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
	icon text NULL,
	path text NOT NULL,
	PRIMARY KEY (id, packageId, packageVersion)
);

-- down
DROP TABLE octgn_packages;
DROP TABLE octgn_plugins;