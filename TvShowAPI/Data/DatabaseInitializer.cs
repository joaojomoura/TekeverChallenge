using Dapper;

namespace TvShowAPI.Data;

public class DatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DatabaseInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync() {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(
            @"CREATE TABLE IF NOT EXISTS TvShows(
                Id INTEGER PRIMARY KEY,
                Title TEXT NOT NULL,
                ReleaseDate TEXT NOT NULL,
                Genre TEXT,
                Showtype TEXT,
                Actors TEXT,
                Favourite INTEGER                
)"
            );
    }
}