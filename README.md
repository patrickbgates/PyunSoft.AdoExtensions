# PyunSoft.AdoExtensions

PyunSoft.AdoExtensions is a Nuget package library that adds a small set of data-retrieval extension methods to `System.Data.Common.DbCommand`.

## Installation

Use the Nuget package manager to install PyunSoft.AdoExtensions.

```
dotnet add package PyunSoft.AdoExtensions
```

## Usage

```c#
using PyunSoft.AdoExtensions;

using var command = connection.CreateCommand();
command.CommandText = "SELECT Id, Name, Amount FROM dbo.Foo";
await connection.OpenAsync();

// DataTable example
using var dataTable = await command.ExecuteDataTableAsync();

// Dictionary example
var dictionary = await command.ExecuteDictionaryAsync<int, string>("Id", "Name");

// List example
var list = await command.ExecuteListAsync<int>("Id");

// ExecuteScalar example
command.CommandText = "SELECT Amount FROM dbo.Foo WHERE Id = @Id";
command.AddParameterWithValue("@Id", 1);
var amount = await command.ExecuteScalarAsync<decimal>();

// ExecuteScalarNullable example
command.CommandText = "SELECT SomeNullableIntColumn FROM dbo.Foo WHERE Id = @Id";
command.AddParameterWithValue("@Id", 1);
var value = await command.ExecuteScalarNullableAsync<int>();
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](https://choosealicense.com/licenses/mit/)