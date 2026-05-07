using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System;

/// <summary>
/// Provides extension methods for string manipulation and formatting.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Masks a string by replacing characters with a mask character, leaving only the specified number of characters visible at the end.
    /// </summary>
    /// <param name="input">The input string to mask.</param>
    /// <param name="maskCharacter">The character to use for masking. Default is '*'.</param>
    /// <param name="visibleCharacters">The number of characters to keep visible at the end. Default is 4.</param>
    /// <returns>
    /// A masked string with the specified number of characters visible at the end, 
    /// or the original string if it's null, empty, or shorter than or equal to the visible character count.
    /// </returns>
    /// <example>
    /// <code>
    /// string creditCard = "1234567890123456";
    /// string masked = creditCard.Mask(); // Returns "************3456"
    /// </code>
    /// </example>
    public static string Mask(this string input, char maskCharacter = '*', int visibleCharacters = 4)
    {
        if (string.IsNullOrEmpty(input))
            return input ?? string.Empty;

        if (input.Length <= visibleCharacters)
            return input;

        var maskLength = input.Length - visibleCharacters;
        var mask = new string(maskCharacter, maskLength);
        var visiblePart = input.Substring(input.Length - visibleCharacters);

        return mask + visiblePart;
    }

    /// <summary>
    /// Masks a string by replacing characters in the middle with a mask character, leaving specified numbers of characters visible at both ends.
    /// </summary>
    /// <param name="input">The input string to mask.</param>
    /// <param name="visibleStartCharacters">The number of characters to keep visible at the beginning. Default is 4.</param>
    /// <param name="visibleEndCharacters">The number of characters to keep visible at the end. Default is 4.</param>
    /// <param name="maskCharacter">The character to use for masking. Default is '*'.</param>
    /// <returns>
    /// A masked string with the specified number of characters visible at both ends,
    /// or the original string if it's null, empty, or shorter than or equal to the total visible character count.
    /// </returns>
    /// <example>
    /// <code>
    /// string email = "john.doe@example.com";
    /// string masked = email.MaskStringBothEnds(2, 3); // Returns "jo*********com"
    /// </code>
    /// </example>
    public static string MaskStringBothEnds(this string input, int visibleStartCharacters = 4, int visibleEndCharacters = 4, char maskCharacter = '*')
    {
        if (string.IsNullOrEmpty(input))
            return input ?? string.Empty;

        var totalVisibleCharacters = visibleStartCharacters + visibleEndCharacters;
        if (input.Length <= totalVisibleCharacters)
            return input;

        var startPart = input.Substring(0, visibleStartCharacters);
        var endPart = input.Substring(input.Length - visibleEndCharacters);
        var maskLength = input.Length - totalVisibleCharacters;
        var mask = new string(maskCharacter, maskLength);

        return startPart + mask + endPart;
    }
}
