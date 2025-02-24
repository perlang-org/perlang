using System.Collections.Generic;

namespace Perlang;

public static class DictionaryExtensions
{
    public static StringTokenTypeDictionary ToPerlangStringTokenTypeDictionary(this Dictionary<string, TokenType> dictionary)
    {
        using var mutableDictionary = new MutableStringTokenTypeDictionary();

        foreach ((string key, TokenType value) in dictionary)
        {
            mutableDictionary.Add(key, value);
        }

        var result = new StringTokenTypeDictionary(mutableDictionary);

        return result;
    }
}
