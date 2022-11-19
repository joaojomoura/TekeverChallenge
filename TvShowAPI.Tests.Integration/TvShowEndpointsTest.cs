using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TvShowAPI.Models;

namespace TvShowAPI.Tests.Integration; 

public class TvShowEndpointsTest : 
    IClassFixture<WebApplicationFactory<IApiMarker>>, IAsyncLifetime {

    private readonly WebApplicationFactory<IApiMarker> _factory;
    private readonly List<int> _createdId = new();


    public TvShowEndpointsTest(WebApplicationFactory<IApiMarker> factory) {
        _factory = factory;
    }

    [Fact]
    public async Task CreateTvShow_CreatesTvShow_WhenDataIsCorrect() {
        //Arrange
        var httpclient = _factory.CreateClient();
        var tvShow = GenerateTvShow();
        //Act
        var result = await httpclient.PostAsJsonAsync("/tvshows", tvShow);
        var createdTvShow = await result.Content.ReadFromJsonAsync<TvShow>();
        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        createdTvShow.Should().BeEquivalentTo(tvShow);
        result.Headers.Location.Should().Be($"http://localhost/tvshows/{tvShow.Id}");
    }
    
    [Fact]
    public async Task CreateTvShow_Fails_WhenIdIsInvalid()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var tvShow = GenerateTvShow();
        tvShow.Id = -1;

        // Act
        var result = await httpClient.PostAsJsonAsync("/tvshows", tvShow);
        _createdId.Add(tvShow.Id);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Id");
        error.ErrorMessage.Should().Be("'Id' must be greater than '0'.");
    }

    [Fact]
    public async Task CreateTvShow_Fails_WhenTvShowExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var show = GenerateTvShow();

        // Act
        await httpClient.PostAsJsonAsync("/tvshows", show);
        _createdId.Add(show.Id);
        var result = await httpClient.PostAsJsonAsync("/tvshows", show);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Id");
        error.ErrorMessage.Should().Be("A tvShow with this Id already created");
    }

    [Fact]
    public async Task GetTvshow_ReturnsTvshow_WhenBookExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var show = GenerateTvShow();
        await httpClient.PostAsJsonAsync("/tvshows", show);
        _createdId.Add(show.Id);

        // Act
        var result = await httpClient.GetAsync($"/tvshows/{show.Id}");
        var existingBook = await result.Content.ReadFromJsonAsync<TvShow>();

        // Assert
        existingBook.Should().BeEquivalentTo(show);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetShow_ReturnsNotFound_WhenShowDoesNotExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var id = GenerateId();

        // Act
        var result = await httpClient.GetAsync($"/tvshows/{id}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllShow_ReturnsAllShows_WhenShowsExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var show = GenerateTvShow();
        await httpClient.PostAsJsonAsync("/tvshows", show);
        _createdId.Add(show.Id);
        var shows = new List<TvShow> { show };

        // Act
        var result = await httpClient.GetAsync("/tvshows");
        var returnedShows = await result.Content.ReadFromJsonAsync<List<TvShow>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedShows.Should().BeEquivalentTo(shows);
    }
    
    [Fact]
    public async Task GetAllFavourite_ReturnsAllShows_WhenShowsExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var show = GenerateTvShow();
        show.Favourite = 1;
        await httpClient.PostAsJsonAsync("/tvshows", show);
        _createdId.Add(show.Id);
        var shows = new List<TvShow> { show };

        // Act
        var result = await httpClient.GetAsync("/favouriteShows");
        var returnedShows = await result.Content.ReadFromJsonAsync<List<TvShow>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedShows.Should().BeEquivalentTo(shows);
    }
    
    [Fact]
    public async Task GetAllFavourite_ReturnsNoShows_WhenNoShowsExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();

        // Act
        var result = await httpClient.GetAsync("/favouriteShows");
        var returnedShows = await result.Content.ReadFromJsonAsync<List<TvShow>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedShows.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllShow_ReturnsNoShows_WhenNoShowsExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();

        // Act
        var result = await httpClient.GetAsync("/tvshows");
        var returnedShows = await result.Content.ReadFromJsonAsync<List<TvShow>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedShows.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchShows_ReturnsShows_WhenTitleMatches()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var show = GenerateTvShow();
        await httpClient.PostAsJsonAsync("/tvshows", show);
        _createdId.Add(show.Id);
        var shows = new List<TvShow> { show };

        // Act
        var result = await httpClient.GetAsync("/tvshows?title=back");
        var returnedShows = await result.Content.ReadFromJsonAsync<List<TvShow>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedShows.Should().BeEquivalentTo(shows);
    }
    
    [Fact]
    public async Task SearchShows_ReturnsShows_WhenGenreIsGiven()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var show = GenerateTvShow();
        await httpClient.PostAsJsonAsync("/tvshows", show);
        _createdId.Add(show.Id);
        var shows = new List<TvShow> { show };

        // Act
        var result = await httpClient.GetAsync("/tvshows?genre=Comedy");
        var returnedShows = await result.Content.ReadFromJsonAsync<List<TvShow>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedShows.Should().BeEquivalentTo(shows);
    }
    
    [Fact]
    public async Task SearchShows_ReturnsShows_WhenShowTypeIsGiven()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var show = GenerateTvShow();
        await httpClient.PostAsJsonAsync("/tvshows", show);
        _createdId.Add(show.Id);
        var shows = new List<TvShow> { show };

        // Act
        var result = await httpClient.GetAsync("/tvshows?showType=Documentary");
        var returnedShows = await result.Content.ReadFromJsonAsync<List<TvShow>>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedShows.Should().BeEquivalentTo(shows);
    }
    
    [Fact]
    public async Task SearchShows_ReturnsActors_WhenTitleMatches()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var show = GenerateTvShow();
        await httpClient.PostAsJsonAsync("/tvshows", show);
        _createdId.Add(show.Id);
        var actors = new List<string> { show.Actors };

        // Act
        var result = await httpClient.GetAsync("/tvshows?getActorsFromTitle=back");
        var returnedBooks = await result.Content.ReadFromJsonAsync<List<string>>();
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedBooks.Should().BeEquivalentTo(actors);
    }

    [Fact]
    public async Task UpdateShow_UpdatesShow_WhenDataIsCorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var show = GenerateTvShow();
        await httpClient.PostAsJsonAsync("/tvshows", show);
        _createdId.Add(show.Id);

        // Act
        show.Favourite = 1;
        var result = await httpClient.PutAsJsonAsync($"/tvshows/{show.Id}", show);
        var updatedBook = await result.Content.ReadFromJsonAsync<TvShow>();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedBook.Should().BeEquivalentTo(show);
    }

    [Fact]
    public async Task UpdateShow_DoesNotUpdatesShow_WhenDataIsIncorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var show = GenerateTvShow();
        await httpClient.PostAsJsonAsync("/tvshows", show);
        _createdId.Add(show.Id);

        // Act
        show.Title = string.Empty;
        var result = await httpClient.PutAsJsonAsync($"/tvshows/{show.Id}", show);
        var errors = await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>();
        var error = errors!.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Title");
        error.ErrorMessage.Should().Be("'Title' must not be empty.");
    }

    [Fact]
    public async Task UpdateShow_ReturnsNotFound_WhenShowDoesNotExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var show = GenerateTvShow();

        // Act
        var result = await httpClient.PutAsJsonAsync($"/tvshows/{show.Id}", show);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteShow_ReturnsNoContent_WhenShowDoesExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var show = GenerateTvShow();
        await httpClient.PostAsJsonAsync("/tvshows", show);
        _createdId.Add(show.Id);

        // Act
        var result = await httpClient.DeleteAsync($"/tvshows/{show.Id}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteShow_ReturnsNotFound_WhenShowDoesNotExist()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var id = GenerateId();

        // Act
        var result = await httpClient.DeleteAsync($"/tvshows/{id}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    private TvShow GenerateTvShow(int favourite = 0, string genre = "Comedy", 
        string showtype = "Documentary", string title = "There and Back Again") {
        return new TvShow {
            Id = GenerateId(),
            Favourite = favourite,
            Actors = "Keanu, Andre, Estevao",
            Genre = genre,
            Showtype = showtype,
            Title = title,
            ReleaseDate = DateTime.Now.AddYears(1)
        };
    }

    private int GenerateId() {
        return Random.Shared.Next(999);
    }

    public Task InitializeAsync()=>Task.CompletedTask;
    
    public async Task DisposeAsync() {
        var httpClient = _factory.CreateClient();
        foreach (var createdId in _createdId)
        {
            await httpClient.DeleteAsync($"/tvshows/{createdId}");
        }
    }
}