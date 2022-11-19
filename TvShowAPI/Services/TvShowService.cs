using Dapper;
using TvShowAPI.Data;
using TvShowAPI.Models;

namespace TvShowAPI.Services; 

public class TvShowService : ITvShowService {

    private readonly IDbConnectionFactory _connectionFactory;

    public TvShowService(IDbConnectionFactory connectionFactory) {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> CreateAsync(TvShow tvshow) {
        var existingTvShow = await GetByIdAsync(tvshow.Id);
        if (existingTvShow is not null)
            return false;

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var result = await connection.ExecuteAsync(
            @"INSERT INTO TvShows (Id, Title, ReleaseDate, Genre, Showtype, Actors, Favourite) 
            VALUES (@Id, @Title, @ReleaseDate, @Genre, @Showtype, @Actors, @Favourite)",
            tvshow);
        return result > 0;
    }

    public async Task<TvShow?> GetByIdAsync(int id) {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return connection.QuerySingleOrDefault<TvShow>(
            "SELECT * FROM TvShows WHERE Id = @Id LIMIT 1",new {Id = id});
    }

    public async Task<IEnumerable<TvShow>> GetAllAsync() {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<TvShow>("SELECT * FROM TvShows");
    }

    public async Task<IEnumerable<TvShow>> GetByGenreAsync(string genre) {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<TvShow>("SELECT * FROM TvShows WHERE Genre = ''||@Genre||''",
            new {Genre = genre});
    }

    public async Task<IEnumerable<TvShow>> GetByShowTypeAsync(string showType) {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<TvShow>("SELECT * FROM TvShows WHERE Showtype = ''||@Showtype||''",
            new {Showtype = showType});
    }

    public async Task<IEnumerable<TvShow>> GetByFavouriteAsync() {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var t= await connection.QueryAsync<TvShow>("select * from TvShows where Favourite = 1 ");
        return t;
    }

    public async Task<IEnumerable<TvShow>> SearchByTitleAsync(string title) {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<TvShow>("SELECT * FROM TvShows WHERE Title LIKE '%'||@Title||'%'",
            new {Title = title});
    }

    public async Task<IEnumerable<string>> SearchByTitle_GetActors(string title) {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<string>("SELECT Actors FROM TvShows WHERE Title LIKE '%'||@Title||'%' LIMIT 1",
            new {Title = title});
    }

    public async Task<bool> UpdateAsync(TvShow tvshow) {

        var existingTvshow = await GetByIdAsync(tvshow.Id);
        if (existingTvshow is null)
            return false;
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var result = await connection.ExecuteAsync(
            @"UPDATE TvShows SET Title = @Title, ReleaseDate = @ReleaseDate, Genre = @Genre, 
                   Showtype = @Showtype, Actors = @Actors, Favourite = @Favourite WHERE Id = @Id", 
            tvshow);
        return result > 0;
    }

    public async Task<bool> DeleteAsync(int id) {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var result = await connection.ExecuteAsync(
            @"DELETE FROM TvShows WHERE Id = @Id", new{Id = id});
        return result > 0;
    }
}