using System.Runtime.CompilerServices;

namespace Common.Extensions;

public static class NullableExtensions
{
    public static T ThrowIfNull<T>(
        this T? input, [CallerArgumentExpression(nameof(input))] string? description = null)
        where T : struct =>
        input ?? ThrowMustNotBeNull<T>(description);

    public static T ThrowIfNull<T>(
        this T? input, [CallerArgumentExpression(nameof(input))] string? description = null)
        where T : class =>
        input ?? ThrowMustNotBeNull<T>(description);

    public static T? ToNullable<T>(this object? source)
        where T : struct =>
        source is { } value
            ? (T)value
            : null;

    private static T ThrowMustNotBeNull<T>(string? description) =>
        throw new InvalidOperationException($"{description} must not be null");
}
