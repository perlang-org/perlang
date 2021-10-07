using System.Collections.Generic;

namespace Perlang.Interpreter.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IDictionary{TKey,TValue}"/> where `TKey` is <see cref="string"/>.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Returns a value from a dictionary, returning `defaultValue` (defaults to null) if the key cannot be found.
        ///
        /// This method only works for value types/`struct`s.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <typeparam name="T">The type of values in the dictionary.</typeparam>
        /// <returns>The value from the dictionary, or `defaultValue` if the key is not found.</returns>
        internal static T? TryGetStructValue<T>(this IDictionary<string, T> dictionary, string key, T? defaultValue = null)
            where T : struct
        {
            return dictionary.TryGetValue(key, out T value) ? value : defaultValue;
        }

        /// <summary>
        /// Returns a value from a dictionary, returning `defaultValue` (defaults to null) if the key cannot be found.
        ///
        /// This method is mostly suited for reference types. It can still be used with value types, but `defaultValue`
        /// cannot be null in those cases.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <returns>The value from the dictionary, or `defaultValue` if the key is not found.</returns>
        internal static TValue TryGetObjectValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        {
            return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
        }
    }
}
