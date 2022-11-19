using System.Diagnostics.Eventing.Reader;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using TvShowAPI.Auth;
using TvShowAPI.Data;
using TvShowAPI.Models;
using TvShowAPI.Services;

var builder = WebApplication.CreateBuilder(args);

//Service registration starts here

//Deal with CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AnyOrigin", x => x.AllowAnyOrigin());
});

// builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
//     .AddScheme<ApiKeyAuthSchemeOptions, ApiKeuyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { });
// builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x => {
    x.EnableAnnotations();
});

builder.Services.AddSingleton<IDbConnectionFactory>(_ => 
    new SqliteConnectionFactory(
        builder.Configuration.GetValue<string>("Database:ConnectionString")));
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<ITvShowService, TvShowService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

//Service registration stops here

var app = builder.Build();

//Middleware registration starts here

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

// app.UseAuthorization();

// Insert to database, but needed the key to do that "SecretKey"
app.MapPost("tvshows", 
            //[Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]    
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
}).WithName("CreateTvShow")
    .WithMetadata(new SwaggerOperationAttribute("Create","Insert new tvshow in the database"))
    //.RequireAuthorization("ApiKeyScheme")
    .Accepts<TvShow>("application/json")
    .Produces<TvShow>(201)
    .Produces<IEnumerable<ValidationFailure>>(400)
    .WithTags("TvShows");



// Get favourite shows
// app.MapGet("tvshows/favourite", async (ITvShowService tvShowService) => {
//         
//         var favouriteTvshows = await tvShowService.GetByFavouriteAsync();
//         Results.Ok(favouriteTvshows);
//     }).WithName("GetFavourites")
//     .Produces<IEnumerable<TvShow>>(200)
//     .WithTags("TvShows")
//     .WithMetadata(new SwaggerOperationAttribute("List of Favourites","Return the list of favourites tvshows"));

// Get all or specific shows
app.MapGet("tvshows", async (ITvShowService tvshowService, string? title, 
    string? genre, string? showType, string? actors, bool? favourite) => {

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

    if (favourite is not null) {
        var matchedTvShows = await tvshowService.GetByFavouriteAsync();
        return Results.Ok(matchedTvShows);
    }
    
    var tvshows = await tvshowService.GetAllAsync();
    return Results.Ok(tvshows);
}).WithName("GetTvShows")
    .Produces<IEnumerable<TvShow>>(200)
    .WithMetadata(new SwaggerOperationAttribute("Get the Tvshow or Shows","Search any tvshow or shows but use only one search method.\n" +
                                                                          "No query string used than it retrieves all tvshows\n" +
                                                                          "Choose the favourite to true if you want the list of favourites"))
    .WithTags("TvShows");

// get show by id
app.MapGet("tvshows/{id}", async (int id, ITvShowService tvShowService) => {
    var tvshow = await tvShowService.GetByIdAsync(id);
    return tvshow is not null ? Results.Ok(tvshow) : Results.NotFound();
}).WithName("GetTvShow")
    .Produces<TvShow>(200)
    .Produces<TvShow>(404)
    .WithTags("TvShows")
    .WithMetadata(new SwaggerOperationAttribute("Search","Search a Tvshow by its id"));


// Update shows by their id (better with id than title, get only one), here we can set favourite or not a favourite
// but needs authorization "SecretKey"
app.MapPut("tvshows/{id}", 
    //[Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)] 
    async (int id, TvShow tvShow, ITvShowService tvShowService,
    IValidator<TvShow> validator) => {

    tvShow.Id = id;
    var validationResult = await validator.ValidateAsync(tvShow);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);

    var updated = await tvShowService.UpdateAsync(tvShow);
    return updated ? Results.Ok(tvShow) : Results.NotFound();

}).WithName("UpdateTvShow")
    //.RequireAuthorization()
    .Accepts<TvShow>("application/json")
    .Produces<TvShow>(200)
    .Produces<IEnumerable<ValidationFailure>>(400)
    .WithTags("TvShows")
    .WithMetadata(new SwaggerOperationAttribute("Update Tvshow","Update the Tvshow. With this we can specify if it is a favourite or not. \n" +
                                                                "1 - Favourite 0 - Not"));

app.MapDelete("books/{id}", [EndpointDescription("Delete")]async (int id, ITvShowService tvshowService) => {
        var deleted = await tvshowService.DeleteAsync(id);
        return deleted ? Results.NoContent() : Results.NotFound();
    }).WithName("DeleteTvShow")
    .WithTags("TvShows")
    .Produces(204)
    .Produces(404)
    .WithMetadata(new SwaggerOperationAttribute("Delete TvShow","Deletes the tvshow from the database given the id"));

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();
//Middleware registration stops here

app.Run();
