namespace Intervals.Tools.Tests;

public class IntervalTests
{
    [Theory]
    [InlineData(2, 5, IntervalType.Open, "(2, 5)")]
    [InlineData(2, 5, IntervalType.Closed, "[2, 5]")]
    [InlineData(2, 5, IntervalType.EndClosed, "(2, 5]")]
    [InlineData(2, 5, IntervalType.StartClosed, "[2, 5)")]
    public void ToString_ShouldPrintCorrect(int start, int end, IntervalType intervalType, string expectedValue)
    {
        var interval = new Interval<int>(start, end, intervalType);

        interval.ToString().Should().BeEquivalentTo(expectedValue);
    }

    [Fact]
    public void Equals_DifferentIntervals_ShouldBeFalse()
    {
        var interval1 = new Interval<int>(10, 20, IntervalType.Open);
        var interval2 = new Interval<int>(1, 2, IntervalType.Open);

        var result = interval1.Equals(interval2);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_SameIntervals_ShouldBeTrue()
    {
        var interval1 = new Interval<int>(1, 2, IntervalType.Open);
        var interval2 = new Interval<int>(1, 2, IntervalType.Open);

        var result = interval1.Equals(interval2);

        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_SameIntervalsDifferentMaxEnd_ShouldBeTrue()
    {
        var interval1 = new Interval<int>(1, 2, IntervalType.Open);
        var interval2 = new Interval<int>(1, 2, IntervalType.Open);
        interval2.MaxEnd = 22;

        var result = interval1.Equals(interval2);

        result.Should().BeTrue();
    }
}