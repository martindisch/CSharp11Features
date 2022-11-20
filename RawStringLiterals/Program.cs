var id = 25;

var fantasticSqlInjection = $"""
-- I can use "quotes" as much as I like!
SELECT * FROM schema.table
WHERE Id = {id}
""";

Console.WriteLine(fantasticSqlInjection);
