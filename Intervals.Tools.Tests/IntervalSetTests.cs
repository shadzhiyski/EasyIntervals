namespace Intervals.Tools.Tests;

public class IntervalSetTests
{
    /// Test AA Tree Structure:
    ///
    /// [3]:                (7, 12]     -->    (42, 68)
    ///                  /                  /           \
    /// [2]:     (2, 4)           [18, 13]                (56, 65) --> (73, 90)
    ///         /     \          /       \               /            /       \
    /// [1]:  (1, 2) (6, 11)  [13, 18] (24, 56)    (55, 58)        (69, 92) (74, 80)
    private ISet<Interval<int>> input = new HashSet<Interval<int>> {
            (18, 34, IntervalType.Closed),
            (13, 18, IntervalType.Closed),
            (1, 2),
            (7, 12, IntervalType.Closed),
            (42, 68),
            (73, 90),
            (56, 65),
            (24, 56),
            (6, 11),
            (2, 4),
            (74, 80),
            (69, 92),
            (55, 58)
        };

    private IntervalSet<int> CreateIntervalSet(ISet<Interval<int>> input)
    {
        var intervalSet = new IntervalSet<int>(input);

        return intervalSet;
    }

    [Fact]
    public void Add_NonExistingInterval_ShouldIncreaseCount()
    {
        var intervalSet = new IntervalSet<int>();

        intervalSet.Add((5, 10));

        intervalSet.Count.Should().Be(1);
    }

    [Fact]
    public void Add_ExistingInterval_ShouldNotIncreaseCount()
    {
        var intervalSet = new IntervalSet<int>();
        intervalSet.Add((5, 10));

        intervalSet.Add((5, 10));

        intervalSet.Count.Should().Be(1);
    }

    [Fact]
    public void Contains_ExistingInterval_ShouldReturnTrue()
    {
        var intervalSet = CreateIntervalSet(input);

        var result = intervalSet.Contains((1, 2));

        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_NotExistingInterval_ShouldReturnFalse()
    {
        var intervalSet = CreateIntervalSet(input);

        var result = intervalSet.Contains((199, 200));

        result.Should().BeFalse();
    }

    [Fact]
    public void Merge_NonEmptyCollection_ShouldReturnMergedIntervalsSorted()
    {
        // Arrange
        var intervalSet = CreateIntervalSet(input);
        var expected = new Interval<int>[] {
            (1, 2),
            (2, 4),
            (6, 12, IntervalType.EndClosed),
            (13, 68, IntervalType.StartClosed),
            (69, 92),
        };

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainInOrder(expected);
    }

    [Fact]
    public void Merge_NonEmptyCollection_ShouldReturnIntervalSetWithMergedIntervals()
    {
        // Arrange
        var intervalSet = CreateIntervalSet(input);
        var expected = new Interval<int>[] {
            (1, 2),
            (2, 4),
            (6, 12, IntervalType.EndClosed),
            (13, 68, IntervalType.StartClosed),
            (69, 92),
        };

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Merge_EmptyCollection_ShouldReturnEmptyIntervalSet()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Merge_BeforeClosedAfterStartClosed_ShouldReturnCorrectStartClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();
        intervalSet.Add((6, 11, IntervalType.Closed));
        intervalSet.Add((7, 12, IntervalType.StartClosed));
        var expected = new Interval<int>(6, 12, IntervalType.StartClosed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    [Fact]
    public void Merge_BeforeStartClosedAfterEndClosed_ShouldReturnCorrectClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();
        intervalSet.Add((6, 11, IntervalType.StartClosed));
        intervalSet.Add((7, 12, IntervalType.EndClosed));
        var expected = new Interval<int>(6, 12, IntervalType.Closed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    [Fact]
    public void Merge_BeforeOpenAfterEndClosed_ShouldReturnCorrectEndClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();
        intervalSet.Add((6, 11, IntervalType.Open));
        intervalSet.Add((7, 12, IntervalType.EndClosed));
        var expected = new Interval<int>(6, 12, IntervalType.EndClosed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    [Fact]
    public void Merge_BeforeOpenAfterStartClosed_ShouldReturnCorrectOpen()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();
        intervalSet.Add((6, 11, IntervalType.Open));
        intervalSet.Add((7, 12, IntervalType.StartClosed));
        var expected = new Interval<int>(6, 12, IntervalType.Open);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    [Fact]
    public void Merge_SameStartBeforeOpenAfterEndClosed_ShouldReturnCorrectEndClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();
        intervalSet.Add((7, 11, IntervalType.Open));
        intervalSet.Add((7, 12, IntervalType.EndClosed));
        var expected = new Interval<int>(7, 12, IntervalType.EndClosed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    [Fact]
    public void Merge_SameStartBeforeStartClosedAfterEndClosed_ShouldReturnCorrectClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();
        intervalSet.Add((7, 11, IntervalType.StartClosed));
        intervalSet.Add((7, 12, IntervalType.EndClosed));
        var expected = new Interval<int>(7, 12, IntervalType.Closed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    [Fact]
    public void Merge_SameStartBeforeStartClosedAfterClosed_ShouldReturnCorrectClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();
        intervalSet.Add((7, 11, IntervalType.StartClosed));
        intervalSet.Add((7, 12, IntervalType.Closed));
        var expected = new Interval<int>(7, 12, IntervalType.Closed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    [Fact]
    public void Merge_SameEndBeforeOpenAfterEndClosed_ShouldReturnCorrectEndClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();
        intervalSet.Add((6, 12, IntervalType.Open));
        intervalSet.Add((7, 12, IntervalType.EndClosed));
        var expected = new Interval<int>(6, 12, IntervalType.EndClosed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    [Fact]
    public void Merge_SameEndBeforeStartClosedAfterEndClosed_ShouldReturnCorrectClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();
        intervalSet.Add((6, 12, IntervalType.StartClosed));
        intervalSet.Add((7, 12, IntervalType.EndClosed));
        var expected = new Interval<int>(6, 12, IntervalType.Closed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    [Fact]
    public void Merge_SameEndBeforeStartClosedAfterOpen_ShouldReturnCorrectStartClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();
        intervalSet.Add((6, 12, IntervalType.StartClosed));
        intervalSet.Add((7, 12, IntervalType.Open));
        var expected = new Interval<int>(6, 12, IntervalType.StartClosed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    [Fact]
    public void Merge_TouchingIntervalsBeforeOpenAfterOpen_ShouldNotMergeIntervals()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();
        var before = new Interval<int>(6, 8, IntervalType.Open);
        var after = new Interval<int>(8, 12, IntervalType.Open);
        intervalSet.Add(before);
        intervalSet.Add(after);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().BeEquivalentTo(new [] { before, after });
    }

    [Fact]
    public void Merge_TouchingIntervalsBeforeEndClosedAfterOpen_ShouldMergeIntervals()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();
        var before = new Interval<int>(6, 8, IntervalType.EndClosed);
        var after = new Interval<int>(8, 12, IntervalType.Open);
        intervalSet.Add(before);
        intervalSet.Add(after);
        var expected = new Interval<int>(6, 12, IntervalType.Open);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    [Fact]
    public void Merge_TouchingIntervalsBeforeOpenAfterStartClosed_ShouldMergeIntervals()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();
        var before = new Interval<int>(6, 8, IntervalType.Open);
        var after = new Interval<int>(8, 12, IntervalType.StartClosed);
        intervalSet.Add(before);
        intervalSet.Add(after);
        var expected = new Interval<int>(6, 12, IntervalType.Open);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    [Fact]
    public void Intersect_HasAnyIntersection_ShouldReturnIntersectedIntervals()
    {
        // Arrange
        var expected = new Interval<int>[] {
            (3, 8, IntervalType.Closed),
            (11, 16, IntervalType.StartClosed)
        };

        var intervalSet = new IntervalSet<int>();

        intervalSet.Add((2, 5)); // (2, 5)
        intervalSet.Add((3, 8, IntervalType.Open)); // (3, 8)
        intervalSet.Add((3, 8, IntervalType.Closed)); // [3, 8]
        intervalSet.Add((11, 16, IntervalType.StartClosed)); // [11, 16)
        intervalSet.Add((11, 14, IntervalType.EndClosed)); // (11, 14]

        // Act
        var result = intervalSet.Intersect((8, 11, IntervalType.Closed));

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Intersect_NoAnyIntersection_ShouldReturnEmptyCollection()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();

        intervalSet.Add((2, 5)); // (2, 5)
        intervalSet.Add((3, 8, IntervalType.Open)); // (3, 8)
        intervalSet.Add((3, 8, IntervalType.Closed)); // [3, 8]
        intervalSet.Add((11, 16, IntervalType.StartClosed)); // [11, 16)
        intervalSet.Add((11, 14, IntervalType.EndClosed)); // (11, 14]

        // Act
        var result = intervalSet.Intersect((8, 11));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Intersect_HasCoveringIntersection_ShouldReturnIntersectedIntervals()
    {
        // Arrange
        var expected = new Interval<int>[] {
            (5, 10, IntervalType.StartClosed)
        };

        var intervalSet = new IntervalSet<int>();

        intervalSet.Add((2, 5));
        intervalSet.Add((5, 10, IntervalType.StartClosed));
        intervalSet.Add((3, 12, IntervalType.Closed));
        intervalSet.Add((11, 16, IntervalType.StartClosed));

        // Act
        var result = intervalSet.Intersect((5, 11, IntervalType.Closed), IntersectionType.Cover);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Intersect_NoCoveringIntersection_ShouldReturnEmptyCollection()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();

        intervalSet.Add((2, 5));
        intervalSet.Add((5, 10, IntervalType.StartClosed));
        intervalSet.Add((3, 12, IntervalType.Closed));
        intervalSet.Add((11, 16, IntervalType.StartClosed));

        // Act
        var result = intervalSet.Intersect((8, 11), IntersectionType.Cover);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Intersect_HasWithinIntersection_ShouldReturnIntersectedIntervals()
    {
        // Arrange
        var expected = new Interval<int>[] {
            (3, 12, IntervalType.Closed)
        };

        var intervalSet = new IntervalSet<int>();

        intervalSet.Add((2, 5));
        intervalSet.Add((5, 10, IntervalType.StartClosed));
        intervalSet.Add((3, 12, IntervalType.Closed));
        intervalSet.Add((11, 16, IntervalType.StartClosed));

        // Act
        var result = intervalSet.Intersect((5, 11, IntervalType.Closed), IntersectionType.Within);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Intersect_NoWithinIntersection_ShouldReturnEmptyCollection()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();

        intervalSet.Add((2, 5));
        intervalSet.Add((5, 10, IntervalType.StartClosed));
        intervalSet.Add((3, 12, IntervalType.Closed));
        intervalSet.Add((11, 16, IntervalType.StartClosed));

        // Act
        var result = intervalSet.Intersect((5, 17), IntersectionType.Within);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Intersect_HasManyIntersectedIntervals_ShouldReturnIntersectedIntervalsSorted()
    {
        // Arrange
        var expected = new Interval<int>[] {
            (3, 8, IntervalType.Closed),
            (11, 16, IntervalType.StartClosed),
            (11, 14, IntervalType.EndClosed),
            (12, 29, IntervalType.Closed),
            (21, 42, IntervalType.Closed),
            (22, 25, IntervalType.Closed)
        };

        var intervalSet = new IntervalSet<int>();

        intervalSet.Add((2, 5)); // (2, 5)
        intervalSet.Add((3, 8, IntervalType.Open)); // (3, 8)
        intervalSet.Add((3, 8, IntervalType.Closed)); // [3, 8]
        intervalSet.Add((11, 14, IntervalType.EndClosed)); // (11, 14]
        intervalSet.Add((11, 16, IntervalType.StartClosed)); // [11, 16)
        intervalSet.Add((22, 25, IntervalType.Closed)); // [22, 25]
        intervalSet.Add((45, 50, IntervalType.Closed)); // [45, 50]
        intervalSet.Add((21, 42, IntervalType.Closed)); // [21, 42]
        intervalSet.Add((12, 29, IntervalType.Closed)); // [12, 29]

        // Act
        var result = intervalSet.Intersect((8, 22, IntervalType.Closed));

        // Assert
        result.Should().ContainInOrder(expected);
    }
}