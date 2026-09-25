using LaneReady.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace LaneReady.Domain.Tests;

public class EoriNumberTests
{
    [Fact]
    public void Create_WithValidGbEori_Succeeds()
    {
        var eori = new EoriNumber("GB123456789012");
        eori.Value.Should().Be("GB123456789012");
        eori.IsGbEori.Should().BeTrue();
        eori.IsXiEori.Should().BeFalse();
    }

    [Fact]
    public void Create_WithValidXiEori_Succeeds()
    {
        var eori = new EoriNumber("XI123456789012");
        eori.IsXiEori.Should().BeTrue();
        eori.IsGbEori.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("GB12345")]
    [InlineData("EU123456789012")]
    [InlineData("GB12345678901X")]
    public void Create_WithInvalidEori_Throws(string value)
    {
        var act = () => new EoriNumber(value);
        act.Should().Throw<Exception>();
    }
}

public class CommodityCodeTests
{
    [Theory]
    [InlineData("12345678")]
    [InlineData("1234567890")]
    public void Create_WithValidCode_Succeeds(string code)
    {
        var cc = new CommodityCode(code);
        cc.Value.Should().Be(code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234")]
    [InlineData("123456789")]
    [InlineData("1234567X")]
    public void Create_WithInvalidCode_Throws(string code)
    {
        var act = () => new CommodityCode(code);
        act.Should().Throw<Exception>();
    }
}
