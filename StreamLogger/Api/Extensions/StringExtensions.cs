using System;
using System.Collections.Generic;
using System.Linq;

namespace StreamLogger.Api.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string to PascalCase
        /// </summary>
        /// <param name="str">String to convert</param>
        public static string ToPascalCase(this string str){

            // Replace all non-letter and non-digits with an underscore and lowercase the rest.
            string sample = string.Join("", str?.Select(c => char.IsLetterOrDigit(c) ? c.ToString().ToLower() : "_").ToArray() ?? Array.Empty<string>());

            // Split the resulting string by underscore
            // Select first character, uppercase it and concatenate with the rest of the string
            IEnumerable<string> arr = sample
                .Split(new []{'_'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => $"{s[..1].ToUpper()}{s[1..]}");

            // Join the resulting collection
            sample = string.Join("", arr);

            return sample;
        }
    }
}