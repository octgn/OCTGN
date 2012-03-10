begin transaction;
SELECT [version] as ver FROM [dbinfo];
WHEN ver = 1 THEN
    UPDATE [dbinfo] SET [version]=2;
    ver = 2;
    ALTER TABLE [cards] ADD COLUMN [alternate] TEXT NOT NULL;
END
WHEN ver = 2 THEN
    UPDATE [dbinfo] SET [version]=3;
    ver = 3;
    ALTER TABLE [cards] ADD COLUMN [dependent] BOOLEAN;
    ALTER TABLE [cards] ADD COLUMN [mutable] BOOLEAN;
END
commit transaction;