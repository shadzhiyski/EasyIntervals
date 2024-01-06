namespace EasyIntervals.Tests;

public class IntervalComparerTests
{
    [Fact]
    public void SameStartFirstOpenSecondStartClosed_ShouldReturnOne()
    {
        var intervalComparer = IntervalComparer<int>.Create(Comparer<int>.Default);

        var result = intervalComparer.Compare((5, 12), (5, 10, IntervalType.StartClosed));

        result.Should().Be(1);
    }

    [Fact]
    public void SameStartFirstStartClosedSecondOpen_ShouldReturnMinusOne()
    {
        var intervalComparer = IntervalComparer<int>.Create(Comparer<int>.Default);

        var result = intervalComparer.Compare((5, 12, IntervalType.StartClosed), (5, 10));

        result.Should().Be(-1);
    }

    [Fact]
    public void SameStartSameStartTypeFirstGreaterEnd_ShouldReturnOne()
    {
        var intervalComparer = IntervalComparer<int>.Create(Comparer<int>.Default);

        var result = intervalComparer.Compare((5, 12, IntervalType.StartClosed), (5, 10, IntervalType.StartClosed));

        result.Should().Be(1);
    }

    [Fact]
    public void SameStartSameStartTypeFirstLowerEnd_ShouldReturnMinusOne()
    {
        var intervalComparer = IntervalComparer<int>.Create(Comparer<int>.Default);

        var result = intervalComparer.Compare((5, 9, IntervalType.StartClosed), (5, 10, IntervalType.StartClosed));

        result.Should().Be(-1);
    }

    [Fact]
    public void SameLimitsSameStartTypeFirstEndClosed_ShouldReturnOne()
    {
        var intervalComparer = IntervalComparer<int>.Create(Comparer<int>.Default);

        var result = intervalComparer.Compare((5, 10, IntervalType.Closed), (5, 10, IntervalType.StartClosed));

        result.Should().Be(1);
    }

    [Fact]
    public void SameLimitsSameStartTypeSecondEndClosed_ShouldReturnMinusOne()
    {
        var intervalComparer = IntervalComparer<int>.Create(Comparer<int>.Default);

        var result = intervalComparer.Compare((5, 10, IntervalType.StartClosed), (5, 10, IntervalType.Closed));

        result.Should().Be(-1);
    }

    [Fact]
    public void SameLimitsSameTypes_ShouldReturnZero()
    {
        var intervalComparer = IntervalComparer<int>.Create(Comparer<int>.Default);

        var result = intervalComparer.Compare((5, 10, IntervalType.StartClosed), (5, 10, IntervalType.StartClosed));

        result.Should().Be(0);
    }
}