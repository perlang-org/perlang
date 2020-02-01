using System.Collections.Generic;

namespace Perlang.Interpreter.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Returns a value from a dictionary, returning <see cref="defaultValue"/> (defaults to null) if the key
        /// cannot be found
        ///
        /// This method only works for value types/structs.
        /// </summary>
        /// <param name="dictionary">the dictionary</param>
        /// <param name="key">the key</param>
        /// <param name="defaultValue">the default value</param>
        /// <typeparam name="T">the type of values in the dictionary</typeparam>
        /// <returns>the value from the dictionary, or defaultValue if the key is not found</returns>
        internal static T? TryGetStructValue<T>(this IDictionary<string, T> dictionary, string key,
            T? defaultValue = null) where T : struct
        {
            return dictionary.TryGetValue(key, out T value) ? value : defaultValue;
        }

        /// <summary>
        /// Returns a value from a dictionary, returning <see cref="defaultValue"/> (defaults to null) if the key
        /// cannot be found
        ///
        /// This method is mostly suited for reference types. It can still be used with value types, but
        /// <see cref="defaultValue"/> cannot be null in those cases.
        /// </summary>
        /// <param name="dictionary">the dictionary</param>
        /// <param name="key">the key</param>
        /// <param name="defaultValue">the default value</param>
        /// <typeparam name="T">the type of values in the dictionary</typeparam>
        /// <returns>the value from the dictionary, or defaultValue if the key is not found</returns>
        internal static T TryGetObjectValue<T>(this IDictionary<string, T> dictionary, string key, T defaultValue = default)
        {
            return dictionary.TryGetValue(key, out T value) ? value : defaultValue;
        }
    }
}
