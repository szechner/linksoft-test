using FluentValidation;

namespace SocialNetworkAnalyzer.App.WebApi.Validators;

/// <summary>
/// Validator for testing text files
/// </summary>
public class TextFileValidator : AbstractValidator<IFormFile>
{
    public TextFileValidator()
    {
        RuleFor(p => p.ContentType).Must(p => p.StartsWith("text/")).WithMessage("File is not a text file");
    }
}