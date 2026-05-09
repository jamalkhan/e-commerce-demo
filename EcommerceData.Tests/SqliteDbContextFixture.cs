using EcommerceData.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EcommerceData.Tests;

internal sealed class SqliteDbContextFixture : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteDbContextFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<EcommerceDbContext>()
            .UseSqlite(_connection)
            .Options;

        DbContext = new EcommerceDbContext(options);
        DbContext.Database.EnsureCreated();
    }

    public EcommerceDbContext DbContext { get; }

    public void Dispose()
    {
        DbContext.Dispose();
        _connection.Dispose();
    }
}
