namespace NeatIntervals.Tests;

public class IntervalExtensionsTests
{
    [Fact]
    public void HasIntersection_IntersectingIntervals_ShouldReturnTrue()
    {
        Interval<int, int?> interval = (2, 5);
        Interval<int, int?> other = (4, 8);

        var result = interval.HasIntersection(other, IntersectionType.Any);

        result.Should().BeTrue();
    }

    [Fact]
    public void HasIntersection_NotIntersectingIntervals_ShouldReturnFalse()
    {
        Interval<int, int?> interval = (2, 5);
        Interval<int, int?> other = (6, 8);

        var result = interval.HasIntersection(other, IntersectionType.Any);

        result.Should().BeFalse();
    }

    [Fact]
    public void HasIntersection_IntersectingIntervalsWithCustomComparer_ShouldReturnTrue()
    {
        var comparer = Comparer<int>.Create((i1, i2) => i2 - i1);
        Interval<int, int?> interval = (5, 2, comparer);
        Interval<int, int?> other = (8, 4, comparer);

        var result = interval.HasIntersection(other, IntersectionType.Any, comparer);

        result.Should().BeTrue();
    }

    [Fact]
    public void HasIntersection_NotIntersectingIntervalsWithCustomComparer_ShouldReturnFalse()
    {
        var comparer = Comparer<int>.Create((i1, i2) => i2 - i1);
        Interval<int, int?> interval = (5, 2, comparer);
        Interval<int, int?> other = (8, 6, comparer);

        var result = interval.HasIntersection(other, IntersectionType.Any, comparer);

        result.Should().BeFalse();
    }
}