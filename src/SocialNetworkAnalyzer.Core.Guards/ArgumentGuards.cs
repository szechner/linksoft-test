using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SocialNetworkAnalyzer.Core.Guards;

/// <summary>
/// Guards for arguments
/// </summary>
public static class ArgumentGuards
{
    /// <summary>
    /// Test if the input is not null
    /// </summary>
    /// <returns>Not null result of <typeparamref name="T"/></returns>
    /// <exception cref="ArgumentNullException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNull]
    public static T ArgumentNotNull<T>(this IGuard _, T input, [CallerMemberName] string? parameterName = null, string? message = null)
    {
        if (input != null)
        {
            return input;
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentNullException(parameterName);
        }

        throw new ArgumentNullException(parameterName, message);
    }
}