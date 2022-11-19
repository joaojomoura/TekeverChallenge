using TvShowAPI.Models;

namespace TvShowAPI.Services; 

public interface ITvShowService {

    public Task<bool> CreateAsync(TvShow tvshow);

    public Task<TvShow?> GetByIdAsync(int id);

    public Task<IEnumerable<TvShow>> GetAllAsync();

    public Task<IEnumerable<TvShow>> GetByGenreAsync(string genre);

    public Task<IEnumerable<TvShow>> GetByShowTypeAsync(string showType);

    public Task<IEnumerable<TvShow>> GetByFavouriteAsync();

    public Task<IEnumerable<TvShow>> SearchByTitleAsync(string title);

    public Task<IEnumerable<string>> SearchByTitle_GetActors(string title);

    public Task<bool> UpdateAsync(TvShow tvshow);

    public Task<bool> DeleteAsync(int id);
}