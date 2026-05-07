using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine;

/// <summary>
/// Provides extension methods for <see cref="ParseResult"/> to simplify option value retrieval.
/// </summary>
internal static class ParseResultExtensions
{
    /// <summary>
    /// Attempts to retrieve the value of the specified command-line option from the parse result.
    /// </summary>
    /// <typeparam name="T">The type of the option value.</typeparam>
    /// <param name="parseResult">The <see cref="ParseResult"/> instance containing the parsed command-line arguments.</param>
    /// <param name="option">The <see cref="Option{T}"/> whose value should be retrieved.</param>
    /// <param name="result">
    /// When this method returns <see langword="true"/>, contains the value of the option;
    /// otherwise, contains the default value for <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the option was present in the parse result and its value was successfully retrieved;
    /// <see langword="false"/> if the option was not found in the parse result.
    /// </returns>
    public static bool TryGetValue<T>(this ParseResult parseResult, Option<T> option, out T? result)
    {
        result = default(T);

        var optionArg = parseResult.GetResult(option);

        if (optionArg == null)
            return false;

        result = optionArg.GetValue<T>(option);
        return true;
    }
}
