begin transaction;

pragma auto_vacuum=1;
pragma default_cache_size=2000;
pragma encoding='UTF-8';
pragma page_size=1024;

CREATE TABLE [dbinfo] (
  [version] INTEGER NOT NULL
);

INSERT INTO dbinfo([version]) VALUES(1);

CREATE TABLE [games] (
  [id] TEXT PRIMARY KEY, 
  [name] TEXT NOT NULL,
  [filename] TEXT NOT NULL, 
  [version] TEXT NOT NULL, 
  [card_width] INTEGER, 
  [card_height] INTEGER, 
  [card_back] TEXT, 
  [deck_sections] TEXT, 
  [shared_deck_sections] TEXT,
  [file_hash] TEXT);

CREATE TABLE [sets] (
  [id] TEXT PRIMARY KEY, 
  [name] TEXT NOT NULL,
  [game_id] TEXT CONSTRAINT [id_game_id] REFERENCES [games]([id]) ON DELETE CASCADE ON UPDATE CASCADE MATCH SIMPLE NOT DEFERRABLE INITIALLY IMMEDIATE, 
  [game_version] TEXT NOT NULL, 
  [version] TEXT NOT NULL, 
  [package] TEXT);

CREATE TABLE [cards] (
  [id] TEXT PRIMARY KEY, 
  [game_id] TEXT NOT NULL,
  [set_id] TEXT CONSTRAINT [id_set_id] REFERENCES [sets]([id]) ON DELETE CASCADE ON UPDATE CASCADE MATCH SIMPLE NOT DEFERRABLE INITIALLY IMMEDIATE, 
  [name] TEXT NOT NULL, 
  [image] TEXT NOT NULL);

CREATE TABLE [custom_properties] (
  [id] TEXT PRIMARY KEY, 
  [card_id] TEXT CONSTRAINT [id_card_id] REFERENCES [cards]([id]) ON DELETE CASCADE ON UPDATE CASCADE MATCH SIMPLE NOT DEFERRABLE INITIALLY IMMEDIATE,
  [game_id] TEXT NOT NULL, 
  [name] TEXT NOT NULL, 
  [type] INTEGER NOT NULL, 
  [vint] INTEGER, 
  [vstr] TEXT);

CREATE TABLE [markers] (
  [id] TEXT PRIMARY KEY, 
  [game_id] TEXT NOT NULL, 
  [set_id] TEXT CONSTRAINT [id_set_id2] REFERENCES [sets]([id]) ON DELETE CASCADE ON UPDATE CASCADE MATCH SIMPLE NOT DEFERRABLE INITIALLY IMMEDIATE, 
  [name] TEXT NOT NULL, 
  [icon] TEXT NOT NULL);

CREATE TABLE [packs] (
  [id] TEXT PRIMARY KEY, 
  [set_id] TEXT CONSTRAINT [id_set_id3] REFERENCES [sets]([id]) ON DELETE CASCADE ON UPDATE CASCADE MATCH SIMPLE NOT DEFERRABLE INITIALLY IMMEDIATE, 
  [name] TEXT NOT NULL, 
  [xml] TEXT NOT NULL);

commit transaction;