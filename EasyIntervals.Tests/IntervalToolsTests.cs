namespace EasyIntervals.Tests;

public class IntervalToolsTests
{
    [Fact]
    public void HasAnyIntersection_IntersectingIntervals_ShouldReturnTrue()
    {
        var interval1 = (2, 5);
        var interval2 = (4, 8);

        var result = IntervalTools.HasAnyIntersection<int, int?>(interval1, interval2, Comparer<int>.Default);

        result.Should().BeTrue();
    }

    [Fact]
    public void HasAnyIntersection_NotIntersectingIntervals_ShouldReturnFalse()
    {
        var interval1 = (6, 8);
        var interval2 = (2, 5);

        var result = IntervalTools.HasAnyIntersection<int, int?>(interval1, interval2, Comparer<int>.Default);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasAnyIntersection_IntersectingIntervalsWithCustomComparer_ShouldReturnTrue()
    {
        var comparer = Comparer<int>.Create((i1, i2) => i2 - i1);
        var interval = (5, 2, comparer);
        var other = (8, 4, comparer);

        var result = IntervalTools.HasAnyIntersection<int, int?>(interval, other, comparer);

        result.Should().BeTrue();
    }

    [Fact]
    public void HasAnyIntersection_NotIntersectingIntervalsWithCustomComparer_ShouldReturnFalse()
    {
        var comparer = Comparer<int>.Create((i1, i2) => i2 - i1);
        var interval = (5, 2, comparer);
        var other = (8, 6, comparer);

        var result = IntervalTools.HasAnyIntersection<int, int?>(interval, other, comparer);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(2, 8, IntervalType.Open, 2, 8, IntervalType.Open)]
    [InlineData(2, 8, IntervalType.EndOpen, 2, 8, IntervalType.Open)]
    [InlineData(2, 8, IntervalType.StartOpen, 2, 8, IntervalType.Open)]
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
    [InlineData(2, 8, IntervalType.Open, 2, 8, IntervalType.EndOpen)]
    [InlineData(2, 8, IntervalType.Open, 2, 8, IntervalType.StartOpen)]
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