using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton(new StoreDb(Path.Combine(builder.Environment.ContentRootPath, "bookstore.db")));

var app = builder.Build();
var db = app.Services.GetRequiredService<StoreDb>();
await db.InitializeAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/books", async (StoreDb store, string? q, string? category) =>
    await store.GetBooksAsync(q, category));

app.MapGet("/api/books/{id:int}", async (StoreDb store, int id) =>
    await store.GetBookAsync(id) is { } book ? Results.Ok(book) : Results.NotFound());

app.MapPost("/api/books", async (StoreDb store, BookInput input) =>
{
    if (string.IsNullOrWhiteSpace(input.Title) || input.Price <= 0)
    {
        return Results.BadRequest(new { message = "Title and valid price are required." });
    }

    var book = await store.CreateBookAsync(input);
    return Results.Created($"/api/books/{book.Id}", book);
});

app.MapPut("/api/books/{id:int}", async (StoreDb store, int id, BookInput input) =>
    await store.UpdateBookAsync(id, input) ? Results.NoContent() : Results.NotFound());

app.MapDelete("/api/books/{id:int}", async (StoreDb store, int id) =>
    await store.DeleteBookAsync(id) ? Results.NoContent() : Results.NotFound());

app.MapGet("/api/orders", async (StoreDb store) => await store.GetOrdersAsync());

app.MapPost("/api/orders", async (StoreDb store, OrderInput input) =>
{
    if (input.Items.Count == 0)
    {
        return Results.BadRequest(new { message = "Cart is empty." });
    }

    var order = await store.CreateOrderAsync(input);
    return Results.Created($"/api/orders/{order.Id}", order);
});

app.MapFallbackToFile("index.html");
app.Run();

record Book(int Id, string Title, string Author, string Category, decimal Price, int Stock);
record BookInput(string Title, string Author, string Category, decimal Price, int Stock);
record OrderLineInput(int BookId, int Quantity);
record OrderInput(string CustomerName, List<OrderLineInput> Items);
record OrderSummary(int Id, string CustomerName, decimal Total, string Status, DateTime CreatedAt);
record OrderCreated(int Id, decimal Total, string Status);

sealed class StoreDb(string databasePath)
{
    private readonly string _connectionString = new SqliteConnectionStringBuilder
    {
        DataSource = databasePath
    }.ToString();

    public async Task InitializeAsync()
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await ExecuteAsync(connection, """
            CREATE TABLE IF NOT EXISTS Books (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Author TEXT NOT NULL,
                Category TEXT NOT NULL,
                Price REAL NOT NULL,
                Stock INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Orders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerName TEXT NOT NULL,
                Total REAL NOT NULL,
                Status TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS OrderItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderId INTEGER NOT NULL,
                BookId INTEGER NOT NULL,
                Quantity INTEGER NOT NULL,
                UnitPrice REAL NOT NULL,
                FOREIGN KEY(OrderId) REFERENCES Orders(Id),
                FOREIGN KEY(BookId) REFERENCES Books(Id)
            );
            """);

        var count = Convert.ToInt32(await ScalarAsync(connection, "SELECT COUNT(*) FROM Books"));
        if (count > 0)
        {
            return;
        }

        var seed = new[]
        {
            new BookInput("Clean Code", "Robert C. Martin", "Software", 35, 12),
            new BookInput("Atomic Habits", "James Clear", "Self Help", 22, 18),
            new BookInput("The Alchemist", "Paulo Coelho", "Fiction", 14, 20),
            new BookInput("Deep Work", "Cal Newport", "Productivity", 24, 10),
            new BookInput("Psychology of Money", "Morgan Housel", "Finance", 19, 15),
            new BookInput("Ikigai", "Hector Garcia", "Lifestyle", 16, 11)
        };

        foreach (var book in seed)
        {
            await CreateBookAsync(book);
        }
    }

    public async Task<List<Book>> GetBooksAsync(string? query, string? category)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        var sql = "SELECT Id, Title, Author, Category, Price, Stock FROM Books WHERE 1=1";
        await using var command = connection.CreateCommand();

        if (!string.IsNullOrWhiteSpace(query))
        {
            sql += " AND (Title LIKE $query OR Author LIKE $query)";
            command.Parameters.AddWithValue("$query", $"%{query.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(category) && category != "All")
        {
            sql += " AND Category = $category";
            command.Parameters.AddWithValue("$category", category);
        }

        command.CommandText = $"{sql} ORDER BY Id DESC";
        return await ReadBooksAsync(command);
    }

    public async Task<Book?> GetBookAsync(int id)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Author, Category, Price, Stock FROM Books WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);
        return (await ReadBooksAsync(command)).FirstOrDefault();
    }

    public async Task<Book> CreateBookAsync(BookInput input)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Books (Title, Author, Category, Price, Stock)
            VALUES ($title, $author, $category, $price, $stock);
            SELECT last_insert_rowid();
            """;
        AddBookParameters(command, input);
        var id = Convert.ToInt32(await command.ExecuteScalarAsync());
        return (await GetBookAsync(id))!;
    }

    public async Task<bool> UpdateBookAsync(int id, BookInput input)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Books
            SET Title = $title, Author = $author, Category = $category, Price = $price, Stock = $stock
            WHERE Id = $id
            """;
        command.Parameters.AddWithValue("$id", id);
        AddBookParameters(command, input);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Books WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);
        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<List<OrderSummary>> GetOrdersAsync()
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, CustomerName, Total, Status, CreatedAt FROM Orders ORDER BY Id DESC";
        var orders = new List<OrderSummary>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            orders.Add(new OrderSummary(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDecimal(2),
                reader.GetString(3),
                DateTime.Parse(reader.GetString(4))));
        }

        return orders;
    }

    public async Task<OrderCreated> CreateOrderAsync(OrderInput input)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        decimal total = 0;
        var lines = new List<(Book Book, int Quantity)>();
        foreach (var line in input.Items.Where(item => item.Quantity > 0))
        {
            await using var bookCommand = connection.CreateCommand();
            bookCommand.Transaction = (SqliteTransaction)transaction;
            bookCommand.CommandText = "SELECT Id, Title, Author, Category, Price, Stock FROM Books WHERE Id = $id";
            bookCommand.Parameters.AddWithValue("$id", line.BookId);
            var book = (await ReadBooksAsync(bookCommand)).FirstOrDefault()
                ?? throw new InvalidOperationException("Book not found.");

            if (book.Stock < line.Quantity)
            {
                throw new InvalidOperationException($"{book.Title} has only {book.Stock} in stock.");
            }

            total += book.Price * line.Quantity;
            lines.Add((book, line.Quantity));
        }

        if (lines.Count == 0)
        {
            throw new InvalidOperationException("Cart is empty.");
        }

        await using var orderCommand = connection.CreateCommand();
        orderCommand.Transaction = (SqliteTransaction)transaction;
        orderCommand.CommandText = """
            INSERT INTO Orders (CustomerName, Total, Status, CreatedAt)
            VALUES ($name, $total, 'Placed', $createdAt);
            SELECT last_insert_rowid();
            """;
        orderCommand.Parameters.AddWithValue("$name", string.IsNullOrWhiteSpace(input.CustomerName) ? "Guest" : input.CustomerName.Trim());
        orderCommand.Parameters.AddWithValue("$total", total);
        orderCommand.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("O"));
        var orderId = Convert.ToInt32(await orderCommand.ExecuteScalarAsync());

        foreach (var (book, quantity) in lines)
        {
            await using var itemCommand = connection.CreateCommand();
            itemCommand.Transaction = (SqliteTransaction)transaction;
            itemCommand.CommandText = """
                INSERT INTO OrderItems (OrderId, BookId, Quantity, UnitPrice)
                VALUES ($orderId, $bookId, $quantity, $unitPrice);
                UPDATE Books SET Stock = Stock - $quantity WHERE Id = $bookId;
                """;
            itemCommand.Parameters.AddWithValue("$orderId", orderId);
            itemCommand.Parameters.AddWithValue("$bookId", book.Id);
            itemCommand.Parameters.AddWithValue("$quantity", quantity);
            itemCommand.Parameters.AddWithValue("$unitPrice", book.Price);
            await itemCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
        return new OrderCreated(orderId, total, "Placed");
    }

    private SqliteConnection CreateConnection() => new(_connectionString);

    private static async Task ExecuteAsync(SqliteConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<object?> ScalarAsync(SqliteConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return await command.ExecuteScalarAsync();
    }

    private static void AddBookParameters(SqliteCommand command, BookInput input)
    {
        command.Parameters.AddWithValue("$title", input.Title.Trim());
        command.Parameters.AddWithValue("$author", string.IsNullOrWhiteSpace(input.Author) ? "Unknown" : input.Author.Trim());
        command.Parameters.AddWithValue("$category", string.IsNullOrWhiteSpace(input.Category) ? "General" : input.Category.Trim());
        command.Parameters.AddWithValue("$price", input.Price);
        command.Parameters.AddWithValue("$stock", Math.Max(0, input.Stock));
    }

    private static async Task<List<Book>> ReadBooksAsync(SqliteCommand command)
    {
        var books = new List<Book>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            books.Add(new Book(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetDecimal(4),
                reader.GetInt32(5)));
        }

        return books;
    }
}
