begin transaction;

CREATE TABLE [dbinfo] (
  [version] INTEGER NOT NULL
);

INSERT INTO dbinfo([version]) VALUES(3);

CREATE TABLE [games] (
  [real_id] INTEGER PRIMARY KEY AUTOINCREMENT, 
  [id] TEXT UNIQUE NOT NULL, 
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
  [real_id] INTEGER PRIMARY KEY AUTOINCREMENT, 
  [id] TEXT UNIQUE NOT NULL, 
  [name] TEXT NOT NULL,
  [game_real_id] INTEGER REFERENCES [games]([real_id]) ON DELETE CASCADE ON UPDATE CASCADE, 
  [game_version] TEXT NOT NULL, 
  [version] TEXT NOT NULL, 
  [package] TEXT);

CREATE TABLE [cards] (
  [real_id] INTEGER PRIMARY KEY AUTOINCREMENT, 
  [id] TEXT UNIQUE NOT NULL, 
  [game_id] TEXT NOT NULL,
  [set_real_id] INTEGER REFERENCES [sets]([real_id]) ON DELETE CASCADE ON UPDATE CASCADE, 
  [name] TEXT NOT NULL, 
  [image] TEXT NOT NULL);

CREATE TABLE [custom_properties] (
  [real_id] INTEGER PRIMARY KEY AUTOINCREMENT, 
  [id] TEXT UNIQUE NOT NULL, 
  [card_real_id] INTEGER REFERENCES [cards]([real_id]) ON DELETE CASCADE ON UPDATE CASCADE,
  [game_id] TEXT NOT NULL, 
  [name] TEXT NOT NULL, 
  [type] INTEGER NOT NULL, 
  [vint] INTEGER, 
  [vstr] TEXT);
  
CREATE TABLE [markers] (
  [real_id] INTEGER PRIMARY KEY AUTOINCREMENT, 
  [id] TEXT UNIQUE NOT NULL, 
  [game_id] TEXT NOT NULL, 
  [set_real_id] INTEGER REFERENCES [sets]([real_id]) ON DELETE CASCADE ON UPDATE CASCADE, 
  [name] TEXT NOT NULL, 
  [icon] TEXT NOT NULL);

CREATE TABLE [packs] (
  [real_id] INTEGER PRIMARY KEY AUTOINCREMENT, 
  [id] TEXT UNIQUE NOT NULL, 
  [set_real_id] INTEGER REFERENCES [sets]([real_id]) ON DELETE CASCADE ON UPDATE CASCADE, 
  [name] TEXT NOT NULL, 
  [xml] TEXT NOT NULL);

commit transaction;