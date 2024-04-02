using FluentValidation;
using SocialNetworkAnalyzer.App.Abstractions.Validators;

namespace SocialNetworkAnalyzer.App.DataSet.CreateDataSet;

/// <summary>
/// Validator for <see cref="CreateDataSetCommand"/>
/// </summary>
public sealed class CreateDataSetCommandValidator : AbstractValidator<CreateDataSetCommand>
{
    public CreateDataSetCommandValidator()
    {
        RuleFor(p => p.Name).NotNull().NotEmpty().MaximumLength(100);
        RuleFor(p => p.TmpFilePath).NotNull().NotEmpty().DependentRules(
            () => RuleFor(p => p.TmpFilePath).FileMustExists()
        ).WithMessage("File is empty");
    }
}