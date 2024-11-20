# QueryForge

QueryForge is a lightweight, JSON-centric query system designed for seamless integration with SQLite in games and other lightweight applications. Its primary goal is to simplify database interactions with intuitive query-building, JSON responses, and support for nested subqueries and relationships. Made specifically for Unity 

## Features

- **Dynamic SELECT Queries**: Build JSON-based SELECT queries with a fluent API. Supports nested subqueries for relationships and automatically maps model properties to database columns.
- **Custom Attribute Handling**: Use attributes like `[IgnoreField]`, `[PrimaryKey]`, and `[ForeignKey]` to control how properties are mapped to database columns.
- **Dynamic Query Execution**: Execute SQL queries and return results as typed objects (`List<T>`).

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/queryforge.git
   ```

2. Add the library to your Unity or .NET project:
   - For Unity: Place the QueryForge folder in your Assets directory.
   - For .NET: Include the library in your project.

3. Ensure SQLite is available:
   - For Unity, add `sqlite3.dll` to your Assets/Plugins folder.

## Getting Started

### Initialize the Context

The `QueryForgeContext` acts as the gateway to your database:

```csharp
using QueryForge;

var context = new QueryForgeContext("mydatabase.db");
```

### Example Models

```csharp
public class Game
{
    [PrimaryKey] public int Id { get; set; }
    public string Settings { get; set; }
}

public class Player
{
    [PrimaryKey] public int Id { get; set; }
    public int GameId { get; set; }
    public int Currency { get; set; }
    [ForeignKey("Id", "PlayerId")]
    public List<InventoryRecord> Inventory { get; set; }
}

public class InventoryRecord
{
    [PrimaryKey] public int ItemId { get; set; }
    public int PlayerId { get; set; }
    public int Quantity { get; set; }
    [ForeignKey("ItemId", "Id")]
    public Item Item { get; set; }
}

public class Item
{
    [PrimaryKey] public int Id { get; set; }
    public string Name { get; set; }
}
```

### Creating Tables

```csharp
var createGameTable = new CreateTableQuery<Game>(context);
context.ExecuteQuery(createGameTable);

var createPlayerTable = new CreateTableQuery<Player>(context);
context.ExecuteQuery(createPlayerTable);

var createInventoryTable = new CreateTableQuery<InventoryRecord>(context);
context.ExecuteQuery(createInventoryTable);

var createItemTable = new CreateTableQuery<Item>(context);
context.ExecuteQuery(createItemTable);
```

### Inserting Data

```csharp
var game = new Game { Id = 1, Settings = "Default Settings" };
var player = new Player { Id = 1, GameId = game.Id, Currency = 1000 };

var insertGameQuery = new InsertQuery<Game>(context, game);
context.ExecuteQuery(insertGameQuery);

var insertPlayerQuery = new InsertQuery<Player>(context, player);
context.ExecuteQuery(insertPlayerQuery);
```

### Selecting Data

```csharp
var players = context.Select<Player>()
    .Where("GameId = 1")
    .ToList();

foreach (var player in players)
{
    Debug.Log($"Player ID: {player.Id}, Currency: {player.Currency}");
}
```

### Updating Data

```csharp
var updatePlayerQuery = new UpdateQuery<Player>(context, player)
    .Where("Id = 1");
context.ExecuteQuery(updatePlayerQuery);
```

### Deleting Data

```csharp
var deletePlayerQuery = new DeleteQuery<Player>(context)
    .Where("Id = 1");
context.ExecuteQuery(deletePlayerQuery);
```

## Roadmap

The following outlines the planned features and improvements for QueryForge:

1. **Enhance Query System:**
   - Implement support for complex query conditions and joins.
   - Add support for transactions to ensure data integrity during batch operations.

2. **Improve Documentation:**
   - Expand the README with more detailed examples and advanced usage scenarios.
   - Ensure all public methods and classes are well-documented with XML comments.

3. **Community Contributions:**
   - Set up guidelines for contributing to the project.
   - Encourage community feedback and feature requests.

4. **Refactor Codebase:**
   - Review and refactor code for better readability and maintainability.
   - Remove any deprecated or unused code.

5. **Feature Enhancements:**
   - Explore additional query features based on community feedback and project needs.
   - Implement caching mechanisms to improve query performance.

6. **Bug Fixes:**
   - Address any known issues or bugs reported by users.
   - Improve error handling and logging for better debugging.

7. **Testing and Quality Assurance:**
   - Increase test coverage to ensure reliability and stability.
   - Implement continuous integration and deployment pipelines.



## Contributing

Contributions are welcome! Submit pull requests or open issues to suggest new features or improvements.

## License

This project is licensed under the MIT License. See the LICENSE file for details.

Let me know if you'd like further adjustments or examples! 🚀
