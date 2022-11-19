using System.Diagnostics.Eventing.Reader;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using TvShowAPI.Auth;
using TvShowAPI.Data;
using TvShowAPI.Models;
using TvShowAPI.Services;

var builder = WebApplication.CreateBuilder(args);

//Service registration starts here

builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
    .AddScheme<ApiKeyAuthSchemeOptions, ApiKeuyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDbConnectionFactory>(_ => 
    new SqliteConnectionFactory(
        builder.Configuration.GetValue<string>("Database:ConnectionString")));
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<ITvShowService, TvShowService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

//Service registration stops here

var app = builder.Build();

//Middleware registration starts here

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

// Insert to database, but needed the key to do that "SecretKey"
app.MapPost("tvshows", 
            [Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]    
    async (TvShow tvShow, ITvShowService tvShowService,
    IValidator<TvShow> validator) => {

    var validationResult = await validator.ValidateAsync(tvShow);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);
    
    var created = await tvShowService.CreateAsync(tvShow);
    if (!created) {
        return Results.BadRequest(new List<ValidationFailure> {
            new ("Id", "A tvShow with this Id already created")
        });
    }

    return Results.CreatedAtRoute("GetTvShow", new { id = tvShow.Id },tvShow);
    //return Results.Created($"/tvshows/{tvShow.Id}", tvShow);
}).WithName("CreateTvShow");

// Get all or specific shows
app.MapGet("tvshows", async (ITvShowService tvshowService, string? title, 
    string? genre, string? showType, string? actors) => {

    if (title is not null && !string.IsNullOrWhiteSpace(title)) {
        var matchedTvShows = await tvshowService.SearchByTitleAsync(title);
        return Results.Ok(matchedTvShows);
    }
    
    if (genre is not null && !string.IsNullOrWhiteSpace(genre)) {
        var matchedTvShows = await tvshowService.GetByGenreAsync(genre);
        return Results.Ok(matchedTvShows);
    }
    
    if (showType is not null && !string.IsNullOrWhiteSpace(showType)) {
        var matchedTvShows = await tvshowService.GetByShowTypeAsync(showType);
        return Results.Ok(matchedTvShows);
    }
    
    if (actors is not null && !string.IsNullOrWhiteSpace(actors)) {
        var matchedTvShows = await tvshowService.SearchByTitle_GetActors(actors);
        return Results.Ok(matchedTvShows);
    }
    
    var tvshows = await tvshowService.GetAllAsync();
    return Results.Ok(tvshows);
}).WithName("GetTvShows");

// get show by id
app.MapGet("tvshows/{id}", async (int id, ITvShowService tvShowService) => {
    var tvshow = await tvShowService.GetByIdAsync(id);
    return tvshow is not null ? Results.Ok(tvshow) : Results.NotFound();
}).WithName("GetTvShow");

// Get favourite shows
app.MapGet("tvshows/favourite", async (ITvShowService tvShowService) => {
    var favouriteTvshows = await tvShowService.GetByFavouriteAsync();
    Results.Ok(favouriteTvshows);
}).WithName("GetFavourites");

// Update shows by their id (better with id than title, get only one), here we can set favourite or not a favourite
// but needs authorization "SecretKey"
app.MapPut("tvshows/{id}", 
    [Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)] 
    async (int id, TvShow tvShow, ITvShowService tvShowService,
    IValidator<TvShow> validator) => {

    tvShow.Id = id;
    var validationResult = await validator.ValidateAsync(tvShow);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);

    var updated = await tvShowService.UpdateAsync(tvShow);
    return updated ? Results.Ok(tvShow) : Results.NotFound();

}).WithName("UpdateTvShow");

app.MapDelete("books/{id}", async (int id, ITvShowService tvshowService) => {
    var deleted = await tvshowService.DeleteAsync(id);
    return deleted ? Results.NoContent(): Results.NotFound();
}).WithName("DeleteTvShow");

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();
//Middleware registration stops here

app.Run();
