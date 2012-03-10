begin transaction;
SELECT [version] as ver FROM [dbinfo];
CASE ver WHEN ver = 1 THEN
    UPDATE [dbinfo] SET [version]=2;
    ALTER TABLE [cards] ADD COLUMN [alternate] TEXT NOT NULL;
END
commit transaction;