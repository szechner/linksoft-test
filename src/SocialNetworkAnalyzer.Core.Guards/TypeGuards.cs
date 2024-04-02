using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SocialNetworkAnalyzer.Core.Guards;

public static class TypeGuards
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNull]
    public static T TypeOf<T>(this IGuard _, object? input, [CallerMemberName] string? parameterName = null, string? message = null)
    {
        if (input is T result)
        {
            return result;
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new InvalidCastException($"Cannot cast {input?.GetType().Name ?? "input"} to {typeof(T).Name}. {parameterName} is not of type {typeof(T).Name}.");
        }

        throw new InvalidCastException(message);
    }
}