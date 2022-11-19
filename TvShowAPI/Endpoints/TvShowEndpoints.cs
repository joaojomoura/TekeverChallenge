using FluentValidation;
using FluentValidation.Results;
using Swashbuckle.AspNetCore.Annotations;
using TvShowAPI.Endpoints.Internal;
using TvShowAPI.Models;
using TvShowAPI.Services;

namespace TvShowAPI.Endpoints; 

public class TvShowEndpoints : IEndpoints {

    private const string ContentType = "application/json";
    private const string Tag = "TvShows";
    private const string BaseRoute = "tvshows";
    public static void DefineEndpoints(IEndpointRouteBuilder app) {

        // Insert to database, but needed the key to do that "SecretKey", not implemented yet
        app.MapPost(BaseRoute, 
                //[Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]    
        CreateTvshowAsync)
            .WithName("CreateTvShow")
            .WithMetadata(new SwaggerOperationAttribute("Create","Insert new tvshow in the database"))
            //.RequireAuthorization("ApiKeyScheme")
            .Accepts<TvShow>(ContentType)
            .Produces<TvShow>(201)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags(Tag);



        // Get favourite shows
        app.MapGet("favouriteShows",GetFavouritesAsync)
            .WithName("Favourites")
            .WithTags(Tag)
            .Produces<IEnumerable<TvShow>>(200)
            .WithDisplayName("Favourite")
            .WithMetadata(new SwaggerOperationAttribute("List of Favourites","Return the list of favourites tvshows"));


        // Get all or specific shows
        app.MapGet(BaseRoute, GetTvShowsOrTvShowAsync) 
            .WithName("GetTvShows")
            .Produces<IEnumerable<TvShow>>(200)
            .WithMetadata(new SwaggerOperationAttribute("Get the Tvshow or Shows","Search any tvshow or shows but use only one search method.\n" +
                                                                              "If no query string used than it retrieves all tvshows"))
            .WithTags("TvShows");

        // get show by id
        app.MapGet($"{BaseRoute}/{{id:int}}", GetShowbyIDAsync)
            .WithName("GetTvShow")
            .Produces<TvShow>(200)
            .Produces<TvShow>(404)
            .WithTags("TvShows")
            .WithMetadata(new SwaggerOperationAttribute("Search","Search a Tvshow by its id"));


        // Update shows by their id (better with id than title, get only one), here we can set favourite or not a favourite
        // but needs authorization "SecretKey", not implemented yet
        app.MapPut($"{BaseRoute}/{{id:int}}", 
        //[Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)] 
        UpdateAsync)
            .WithName("UpdateTvShow")
            //.RequireAuthorization()
            .Accepts<TvShow>(ContentType)
            .Produces<TvShow>(200)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags(Tag)
            .WithMetadata(new SwaggerOperationAttribute("Update Tvshow","Update the Tvshow. With this we can specify if it is a favourite or not. \n" +
                                                                    "1 - Favourite 0 - Not"));

    app.MapDelete($"{BaseRoute}/{{id:int}}", DeleteAsync)
        .WithName("DeleteTvShow")
        .WithTags(Tag)
        .Produces(204)
        .Produces(404)
        .WithMetadata(new SwaggerOperationAttribute("Delete TvShow","Deletes the tvshow from the database given the id"));

    }
    


    internal static async Task<IResult> CreateTvshowAsync(TvShow tvShow, ITvShowService tvShowService,
        IValidator<TvShow> validator) {
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
    }

    internal static async Task<IResult> GetFavouritesAsync(ITvShowService tvshowService) {
        var favourite = await tvshowService.GetByFavouriteAsync();
        return Results.Ok(favourite);
    }

    internal static async Task<IResult> GetTvShowsOrTvShowAsync(ITvShowService tvshowService, string? title,
        string? genre, string? showType, string? getActorsFromTitle) {
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
        
        if (getActorsFromTitle is not null && !string.IsNullOrWhiteSpace(getActorsFromTitle)) {
            var matchedTvShows = await tvshowService.SearchByTitle_GetActors(getActorsFromTitle);
            return Results.Ok(matchedTvShows);
        }
        
        
        var tvshows = await tvshowService.GetAllAsync();
        return Results.Ok(tvshows);
    }

    internal static async Task<IResult> GetShowbyIDAsync(int id, ITvShowService tvShowService) {
        var tvshow = await tvShowService.GetByIdAsync(id);
        return tvshow is not null ? Results.Ok(tvshow) : Results.NotFound();
    }

    internal static async Task<IResult> UpdateAsync(int id, TvShow tvShow, ITvShowService tvShowService,
        IValidator<TvShow> validator) {
        tvShow.Id = id;
        var validationResult = await validator.ValidateAsync(tvShow);
        if (!validationResult.IsValid)
            return Results.BadRequest(validationResult.Errors);

        var updated = await tvShowService.UpdateAsync(tvShow);
        return updated ? Results.Ok(tvShow) : Results.NotFound();
    }

    internal static async Task<IResult> DeleteAsync(int id, ITvShowService tvshowService) {
        var deleted = await tvshowService.DeleteAsync(id);
        return deleted ? Results.NoContent() : Results.NotFound();
    }

    public static void AddServices(IServiceCollection services, IConfiguration configuration) {
        services.AddSingleton<ITvShowService, TvShowService>();
    }
}