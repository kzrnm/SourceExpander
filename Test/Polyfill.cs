using System.Threading;

namespace Xunit;

internal class TestContext
{
    public static C Current
    {
        get
        {
            if (typeof(Xunit.FactAttribute).Assembly.GetName().Name.Contains("v3"))
                throw new System.Exception();
            return default;
        }
    }
    public record struct C(CancellationToken CancellationToken);
}
