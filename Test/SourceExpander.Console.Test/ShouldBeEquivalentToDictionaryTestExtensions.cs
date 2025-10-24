using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Shouldly;

public static class ShouldBeEquivalentToDictionaryTestExtensions
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ShouldContainKeyAndBeEquivalentValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue val)
        where TKey : notnull
    {
        dictionary.ShouldContainKey(key);
        var actual = dictionary[key];
        actual.ShouldBeEquivalentTo(val);
    }
}
