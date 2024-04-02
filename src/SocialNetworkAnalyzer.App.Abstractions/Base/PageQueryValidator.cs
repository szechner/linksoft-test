using FluentValidation;

namespace SocialNetworkAnalyzer.App.Abstractions.Base;

/// <summary>
/// Validator for paged queries 
/// </summary>
public class PageQueryValidator<T> : AbstractValidator<T>
    where T : IPaged
{
    protected PageQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).GreaterThan(0);
    }
}