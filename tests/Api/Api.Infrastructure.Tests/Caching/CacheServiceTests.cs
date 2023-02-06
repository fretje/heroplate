using FluentAssertions;
using Heroplate.Api.Application.Common.Caching;
using Xunit;

namespace Api.Infrastructure.Tests.Caching;

public abstract class CacheServiceTests
{
    private sealed record TestRecord(Guid Id, string StringValue, DateTime DateTimeValue);

    private const string _testKey = "testkey";
    private const string _testValue = "testvalue";

    private readonly ICacheService _sut;

    protected CacheServiceTests(ICacheService cacheService) => _sut = cacheService;

    [Fact]
    public void ThrowsGivenNullKey()
    {
        var action = () => { string? result = _sut.Get<string>(null!); };

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ReturnsNullGivenNonExistingKey()
    {
        string? result = _sut.Get<string>(_testKey);

        result.Should().BeNull();
    }

    public static object[][] ValueData =>
        new[]
        {
            new object[] { _testKey, _testValue },
            new object[] { "integer", 1 },
            new object[] { "long", 1L },
            new object[] { "double", 1.0 },
            new object[] { "bool", true },
            new object[] { "date", new DateTime(2022, 1, 1) },
        };

    [Theory]
    [MemberData(nameof(ValueData))]
    public void ReturnsExistingValueGivenExistingKey<T>(string testKey, T testValue)
    {
        _sut.Set(testKey, testValue);
        var result = _sut.Get<T>(testKey);

        result.Should().Be(testValue);
    }

    [Fact]
    public void ReturnsExistingObjectGivenExistingKey()
    {
        var expected = new TestRecord(Guid.NewGuid(), _testValue, DateTime.UtcNow);

        _sut.Set(_testKey, expected);
        var result = _sut.Get<TestRecord>(_testKey);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task ReturnsNullGivenAnExpiredKey()
    {
        _sut.Set(_testKey, _testValue, TimeSpan.FromMilliseconds(200));

        string? result = _sut.Get<string>(_testKey);
        Assert.Equal(_testValue, result);

        await Task.Delay(250);
        result = _sut.Get<string>(_testKey);

        result.Should().BeNull();
    }
}