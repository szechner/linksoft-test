using FluentValidation;
using SocialNetworkAnalyzer.Core.Guards;

namespace SocialNetworkAnalyzer.App.Abstractions.Validators;

/// <summary>
/// Validator to test files
/// </summary>
public static class FileValidator
{
    /// <summary>
    /// Rule to test if a file exists 
    /// </summary>
    public static void FileMustExists<T, TElement>(this IRuleBuilder<T, TElement> ruleBuilder)
    {
        ruleBuilder.Must((_, fileNameElement, context) =>
            {
                var fileName = Guard.Require.TypeOf<string>(fileNameElement);
                context.MessageFormatter.AppendArgument("fileName", fileName);
                return File.Exists(fileName);
            }
        ).WithMessage("Temporary file {fileName} does not exists.");
    }
}