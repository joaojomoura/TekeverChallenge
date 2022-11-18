namespace TvShowAPI.Models;

public class TvShow
{
    public int Id { get; set; } = default!;

    public string Title { get; set; } = default!;

    public DateTime ReleaseDate { get; set; } = default!;

    public string Showtype { get; set; }

    public string Genre { get; set; }

    public string Actors { get; set; }

    public int Favorite { get; set; } = 0;
}