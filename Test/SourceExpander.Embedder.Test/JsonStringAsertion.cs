using System.Runtime.CompilerServices;
using TUnit.Assertions.Conditions;
using TUnit.Assertions.Core;
using TUnit.Assertions.Exceptions;
using TUnit.Assertions.Should.Core;

namespace SourceExpander;

internal abstract class JsonSourcesAsertion(AssertionContext<string> context, IEnumerable<SourceFileInfo> expected) : Assertion<string>(context)
{
    protected readonly IEnumerable<SourceFileInfo> _expected = expected;

    protected abstract SourceFileInfo[] Deserialize(string json);
    protected abstract string MethodName { get; }

    protected override string GetExpectation() => "to be equivalent to expected souces";
    protected override async Task<AssertionResult> CheckAsync(EvaluationMetadata<string> metadata)
    {
        var value = metadata.Value;
        var exception = metadata.Exception;

        if (exception != null)
            return AssertionResult.Failed($"threw {exception.GetType().Name}");

        if (value == null)
            return AssertionResult.Failed("value was null");


        SourceFileInfo[] dec;
        try
        {
            dec = Deserialize(value);
        }
        catch (Exception e)
        {
            return AssertionResult.Failed($"Failed to deserialize: {e.GetType().Name}");
        }
        var inner = new IsEquivalentToAssertion<IEnumerable<SourceFileInfo>, SourceFileInfo>(
            new AssertionContext<IEnumerable<SourceFileInfo>>(dec, Context.ExpressionBuilder), _expected, TestUtil.SourceFileInfoEqualityComparer);

        try
        {
            await inner.AssertAsync();
        }
        catch (AssertionException e)
        {
            return AssertionResult.Failed($"The object returned by {MethodName} is not expected", e);
        }

        return AssertionResult.Passed;
    }
}

internal class SystemTextJsonSourcesAsertion(AssertionContext<string> context, IEnumerable<SourceFileInfo> expected) : JsonSourcesAsertion(context, expected)
{
    protected override string MethodName => "System.Text.Json.JsonSerializer.Deserialize";

    protected override SourceFileInfo[] Deserialize(string json) => System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(json);
}

internal class NewtonsoftJsonSourcesAsertion(AssertionContext<string> context, IEnumerable<SourceFileInfo> expected) : JsonSourcesAsertion(context, expected)
{
    protected override string MethodName => "Newtonsoft.Json.JsonConvert.DeserializeObject";
    protected override SourceFileInfo[] Deserialize(string json) => Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(json);
}

internal static class JsonSoucesExtension
{
    extension(IAssertionSource<string> source)
    {
        public JsonSourcesAsertion IsEquivalentToJsonSources(IEnumerable<SourceFileInfo> expected)
             => source.IsEquivalentToNewtonsoftJsonSources(expected).And.IsEquivalentToSystemTextJsonSources(expected);
        private SystemTextJsonSourcesAsertion IsEquivalentToSystemTextJsonSources(IEnumerable<SourceFileInfo> expected)
        {
            source.Context.ExpressionBuilder.Append(nameof(IsEquivalentToSystemTextJsonSources) + "(expected)");
            return new(source.Context, expected);
        }
        private NewtonsoftJsonSourcesAsertion IsEquivalentToNewtonsoftJsonSources(IEnumerable<SourceFileInfo> expected)
        {
            source.Context.ExpressionBuilder.Append(nameof(IsEquivalentToNewtonsoftJsonSources) + "(expected)");
            return new(source.Context, expected);
        }
    }
    extension(IShouldSource<string> source)
    {
        public ShouldAssertion<string> BeEquivalentToJsonSources(IEnumerable<SourceFileInfo> expected,
            [CallerArgumentExpression(nameof(expected))] string expectedExpression = null)
        {
            var innerContext = source.Context;
            innerContext.ExpressionBuilder.Append('.').Append(nameof(BeEquivalentToJsonSources)).Append('(');
            var __added = false;
            if (expectedExpression is not null)
            {
                if (__added) innerContext.ExpressionBuilder.Append(", ");
                innerContext.ExpressionBuilder.Append(expectedExpression);
                __added = true;
            }
            innerContext.ExpressionBuilder.Append(')');
            Assertion<string> inner = new SystemTextJsonSourcesAsertion(innerContext, expected).And.IsEquivalentToSystemTextJsonSources(expected);
            var __tunit_should_because = source.ConsumeBecauseMessage();
            if (__tunit_should_because is not null)
            {
                inner.Because(__tunit_should_because);
            }
            return new ShouldAssertion<string>(innerContext, inner);
        }
    }
}
