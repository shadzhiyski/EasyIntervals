namespace EasyIntervals.Tests;

public class IntervalToolsTests
{
    [Fact]
    public void HasAnyIntersection_IntersectingIntervals_ShouldReturnTrue()
    {
        var start = (2, 5);
        var end = (4, 8);

        var result = IntervalTools.HasAnyIntersection<int, int?>(start, end, Comparer<int>.Default);

        result.Should().BeTrue();
    }

    [Fact]
    public void HasAnyIntersection_NotIntersectingIntervals_ShouldReturnFalse()
    {
        var start = (2, 5);
        var end = (6, 8);

        var result = IntervalTools.HasAnyIntersection<int, int?>(start, end, Comparer<int>.Default);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(2, 8, IntervalType.Open, 2, 8, IntervalType.Open)]
    [InlineData(2, 8, IntervalType.StartClosed, 2, 8, IntervalType.Open)]
    [InlineData(2, 8, IntervalType.EndClosed, 2, 8, IntervalType.Open)]
    public void Covers_IntervalCoversOther_ShouldReturnTrue(
        int intervalStart, int intervalEnd, IntervalType intervalType,
        int otherIntervalStart, int otherIntervalEnd, IntervalType otherIntervalType)
    {
        var interval = (intervalStart, intervalEnd, intervalType);
        var other = (otherIntervalStart, otherIntervalEnd, otherIntervalType);

        var result = IntervalTools.Covers<int, int?>(interval, other, Comparer<int>.Default);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(3, 5, IntervalType.Open, 2, 8, IntervalType.Open)]
    [InlineData(2, 8, IntervalType.Open, 2, 8, IntervalType.StartClosed)]
    [InlineData(2, 8, IntervalType.Open, 2, 8, IntervalType.EndClosed)]
    public void Covers_IntervalNotCoversOther_ShouldReturnTrue(
        int intervalStart, int intervalEnd, IntervalType intervalType,
        int otherIntervalStart, int otherIntervalEnd, IntervalType otherIntervalType)
    {
        var interval = (intervalStart, intervalEnd, intervalType);
        var other = (otherIntervalStart, otherIntervalEnd, otherIntervalType);

        var result = IntervalTools.Covers<int, int?>(interval, other, Comparer<int>.Default);

        result.Should().BeFalse();
    }
}