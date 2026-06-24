using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SourceExpander;

internal static class CollectionExtensions
{
    extension<T>(IEnumerable<T> collection)
    {
        public IEnumerable<T> Do(Action action)
            => collection.Select(v => { action(); return v; });
        public IEnumerable<T> Do(Action<T> action)
            => collection.Select(v => { action(v); return v; });
        public IEnumerable<T> TryParallel(bool isConcurrentBuild, CancellationToken cancellationToken)
            => isConcurrentBuild
            ? collection.Do(cancellationToken.ThrowIfCancellationRequested)
            : collection.AsParallel().WithCancellation(cancellationToken);
    }
}
