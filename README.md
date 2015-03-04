# FileParser Library

A relatively simple file parser library that breaks up files into batches and removes comments from the processed file contents. The itch I'm scratching is to load SQL script files for SQL Server where the files are correctly broken into batches based on the `GO` SQL keyword, and the comments from the script files are also removed (prior to sending the command to SQL Server).

As an added bonus there is parsing of config files too, which basically strips comment lines out and makes each line its own batch.

A batch, as far as the parser is concerned, is simply a string in an array.

## Usage

```C#
using FileParser;

// ...

var parser = new Parser(FileType.SqlScript);
parser.Parse(fileStream); // Previously defined file stream

foreach(var batch in parser.Batches)
{
    // Do something exciting with batch
}
```

Have a look at the unit tests to see more usage examples.

## Example Files

A SQL script file:

```SQL
/***
 * Create the config table
 **/
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'Config')
    CREATE TABLE [Config] (
        [key] varchar(20) PRIMARY KEY,
        [value] varchar(200)
    );
GO

-- Select from the table, if it already existed something interesting
-- will turn up
SELECT * FROM [Config];
```

and a config file:

```
# Destination path
destination=C:\users

# User credentials
username=joe
password=P@ssword
```



