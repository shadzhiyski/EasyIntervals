namespace EasyIntervals.Tests;

public class IntervalComparerTests
{
    [Fact]
    public void SameStartFirstOpenSecondEndOpen_ShouldReturnOne()
    {
        var intervalComparer = IntervalComparer<int, int?>.Create(Comparer<int>.Default);

        var result = intervalComparer.Compare((5, 12), (5, 10, IntervalType.EndOpen));

        result.Should().Be(1);
    }

    [Fact]
    public void SameStartFirstEndOpenSecondOpen_ShouldReturnMinusOne()
    {
        var intervalComparer = IntervalComparer<int, int?>.Create(Comparer<int>.Default);

        var result = intervalComparer.Compare((5, 12, IntervalType.EndOpen), (5, 10, IntervalType.Open));

        result.Should().Be(-1);
    }

    [Fact]
    public void SameStartSameStartTypeFirstGreaterEnd_ShouldReturnOne()
    {
        var intervalComparer = IntervalComparer<int, int?>.Create(Comparer<int>.Default);

        var result = intervalComparer.Compare((5, 12, IntervalType.EndOpen), (5, 10, IntervalType.EndOpen));

        result.Should().Be(1);
    }

    [Fact]
    public void SameStartSameStartTypeFirstLowerEnd_ShouldReturnMinusOne()
    {
        var intervalComparer = IntervalComparer<int, int?>.Create(Comparer<int>.Default);

        var result = intervalComparer.Compare((5, 9, IntervalType.EndOpen), (5, 10, IntervalType.EndOpen));

        result.Should().Be(-1);
    }

    [Fact]
    public void SameLimitsSameStartTypeFirstStartOpen_ShouldReturnOne()
    {
        var intervalComparer = IntervalComparer<int, int?>.Create(Comparer<int>.Default);

        var result = intervalComparer.Compare((5, 10, IntervalType.Closed), (5, 10, IntervalType.EndOpen));

        result.Should().Be(1);
    }

    [Fact]
    public void SameLimitsSameStartTypeSecondStartOpen_ShouldReturnMinusOne()
    {
        var intervalComparer = IntervalComparer<int, int?>.Create(Comparer<int>.Default);

        var result = intervalComparer.Compare((5, 10, IntervalType.EndOpen), (5, 10, IntervalType.Closed));

        result.Should().Be(-1);
    }

    [Fact]
    public void SameLimitsSameTypes_ShouldReturnZero()
    {
        var intervalComparer = IntervalComparer<int, int?>.Create(Comparer<int>.Default);

        var result = intervalComparer.Compare((5, 10, IntervalType.EndOpen), (5, 10, IntervalType.EndOpen));

        result.Should().Be(0);
    }
}