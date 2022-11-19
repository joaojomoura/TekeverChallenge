using FluentValidation;
using TvShowAPI.Models;

namespace TvShowAPI.Validators; 

public class TvshowValidator : AbstractValidator<TvShow> {

    public TvshowValidator() {
        RuleFor(tvshow => tvshow.Id).GreaterThan(0);
        RuleFor(tvshow => tvshow.Title).NotEmpty();
        RuleFor(tvshow => tvshow.ReleaseDate).NotEmpty();
        RuleFor(tvshow => tvshow.Favourite)
            .InclusiveBetween(0,1)
            .WithMessage("Value must be 0 or 1");

    }
}