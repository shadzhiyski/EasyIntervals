namespace EasyIntervals.Tests;

public class IntervalTests
{
    [Fact]
    public void Initialization_StartGreaterThanEnd_ShouldThrowArgumentException()
    {
        var start = 5;
        var end = 2;

        var act = () => new Interval<int, int?>(start, end);

        act.Should().Throw<ArgumentException>().WithMessage("Start must not be greater than end.");
    }

    [Theory]
    [InlineData(IntervalType.Open)]
    [InlineData(IntervalType.EndOpen)]
    [InlineData(IntervalType.StartOpen)]
    public void Initialization_EqualLimitsNotClosedIntervalType_ShouldThrowArgumentException(IntervalType intervalType)
    {
        var start = 5;
        var end = 5;

        var act = () => new Interval<int, int?>(start, end, intervalType);

        act.Should().Throw<ArgumentException>().WithMessage("Equal limits must be combined only with Closed interval type.");
    }

    [Fact]
    public void Initialization_ValidLimits_ShouldCreateInterval()
    {
        var start = 2;
        var end = 5;

        var act = () => new Interval<int, int?>(start, end);

        act.Should().NotThrow<ArgumentException>();
    }

    [Fact]
    public void Equals_DifferentIntervals_ShouldBeFalse()
    {
        var interval1 = new Interval<int, int?>(10, 20, IntervalType.Open);
        var interval2 = new Interval<int, int?>(1, 2, IntervalType.Open);

        var result = interval1.Equals(interval2);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_SameIntervals_ShouldBeTrue()
    {
        var interval1 = new Interval<int, int?>(1, 2, 5, IntervalType.Open);
        var interval2 = new Interval<int, int?>(1, 2, 5, IntervalType.Open);

        var result = interval1.Equals(interval2);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_SameIntervalsDifferentMaxEnd_ShouldBeTrue()
    {
        var interval1 = new Interval<int, int?>(1, 2, IntervalType.Open);
        var interval2 = new Interval<int, int?>(1, 2, IntervalType.Open);
        interval2.MaxEnd = 22;

        var result = interval1.Equals(interval2);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(2, 5, null, IntervalType.Open, "(2, 5)")]
    [InlineData(2, 5, null, IntervalType.Closed, "[2, 5]")]
    [InlineData(2, 5, null, IntervalType.StartOpen, "(2, 5]")]
    [InlineData(2, 5, null, IntervalType.EndOpen, "[2, 5)")]
    [InlineData(2, 5, 10, IntervalType.Open, "(2, 5): 10")]
    [InlineData(2, 5, 10, IntervalType.Closed, "[2, 5]: 10")]
    public void ToString_ShouldPrintCorrect(int start, int end, int? value, IntervalType intervalType, string expectedValue)
    {
        var interval = new Interval<int, int?>(start, end, value, intervalType);

        interval.ToString().Should().BeEquivalentTo(expectedValue);
    }
}