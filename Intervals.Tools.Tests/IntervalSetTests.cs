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
        var intervalSet = new IntervalSet<int>
        {
            (5, 10)
        };

        intervalSet.Count.Should().Be(1);
    }

    [Fact]
    public void Add_ExistingInterval_ShouldNotIncreaseCount()
    {
        var intervalSet = new IntervalSet<int>
        {
            (5, 10),
            (5, 10)
        };

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

    /// <summary>
    /// Set:
    ///         [-----------------------------)
    ///   [-----------------------------]
    /// Expected set:
    ///   [-----------------------------------)
    /// --6-----7-----------------------11----12-------
    /// </summary>
    [Fact]
    public void Merge_PrecedingClosedFollowingStartClosed_ShouldReturnCorrectStartClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>
        {
            (6, 11, IntervalType.Closed),
            (7, 12, IntervalType.StartClosed)
        };
        var expected = new Interval<int>(6, 12, IntervalType.StartClosed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    /// <summary>
    /// Set:
    ///         (-----------------------------]
    ///   [-----------------------------)
    /// Expected set:
    ///   [-----------------------------------]
    /// --6-----7-----------------------11----12-------
    /// </summary>
    [Fact]
    public void Merge_PrecedingStartClosedFollowingEndClosed_ShouldReturnCorrectClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>
        {
            (6, 11, IntervalType.StartClosed),
            (7, 12, IntervalType.EndClosed)
        };
        var expected = new Interval<int>(6, 12, IntervalType.Closed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    /// <summary>
    /// Set:
    ///         (-----------------------------]
    ///   (-----------------------------)
    /// Expected set:
    ///   (-----------------------------------]
    /// --6-----7-----------------------11----12-------
    /// </summary>
    [Fact]
    public void Merge_PrecedingOpenFollowingEndClosed_ShouldReturnCorrectEndClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>
        {
            (6, 11, IntervalType.Open),
            (7, 12, IntervalType.EndClosed)
        };
        var expected = new Interval<int>(6, 12, IntervalType.EndClosed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    /// <summary>
    /// Set:
    ///         [-----------------------------)
    ///   (-----------------------------)
    /// Expected set:
    ///   (-----------------------------------)
    /// --6-----7-----------------------11----12-------
    /// </summary>
    [Fact]
    public void Merge_PrecedingOpenFollowingStartClosed_ShouldReturnCorrectOpen()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>
        {
            (6, 11, IntervalType.Open),
            (7, 12, IntervalType.StartClosed)
        };
        var expected = new Interval<int>(6, 12, IntervalType.Open);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    /// <summary>
    /// Set:
    ///   (-----------------------)
    ///   (-----------------------------]
    /// Expected set:
    ///   (-----------------------------]
    /// --7-----------------------11----12-------------
    /// </summary>
    [Fact]
    public void Merge_SameStartPrecedingOpenFollowingEndClosed_ShouldReturnCorrectEndClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>
        {
            (7, 11, IntervalType.Open),
            (7, 12, IntervalType.EndClosed)
        };
        var expected = new Interval<int>(7, 12, IntervalType.EndClosed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    /// <summary>
    /// Set:
    ///   [-----------------------)
    ///   (-----------------------------]
    /// Expected set:
    ///   [-----------------------------]
    /// --7-----------------------11----12-------------
    /// </summary>
    [Fact]
    public void Merge_SameStartPrecedingStartClosedFollowingEndClosed_ShouldReturnCorrectClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>
        {
            (7, 11, IntervalType.StartClosed),
            (7, 12, IntervalType.EndClosed)
        };
        var expected = new Interval<int>(7, 12, IntervalType.Closed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    /// <summary>
    /// Set:
    ///   [-----------------------)
    ///   [-----------------------------]
    /// Expected set:
    ///   [-----------------------------]
    /// --7-----------------------11----12-------------
    /// </summary>
    [Fact]
    public void Merge_SameStartPrecedingStartClosedFollowingClosed_ShouldReturnCorrectClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>
        {
            (7, 11, IntervalType.StartClosed),
            (7, 12, IntervalType.Closed)
        };
        var expected = new Interval<int>(7, 12, IntervalType.Closed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    /// <summary>
    /// Set:
    ///           (-------------------]
    ///    (--------------------------)
    /// Expected set:
    ///    (--------------------------]
    /// ---6------7-------8-----------12-------------
    /// </summary>
    [Fact]
    public void Merge_SameEndPrecedingOpenFollowingEndClosed_ShouldReturnCorrectEndClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>
        {
            (6, 12, IntervalType.Open),
            (7, 12, IntervalType.EndClosed)
        };
        var expected = new Interval<int>(6, 12, IntervalType.EndClosed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    /// <summary>
    /// Set:
    ///           (-------------------]
    ///    [--------------------------)
    /// Expected set:
    ///    [--------------------------]
    /// ---6------7-------8-----------12-------------
    /// </summary>
    [Fact]
    public void Merge_SameEndPrecedingStartClosedFollowingEndClosed_ShouldReturnCorrectClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>
        {
            (6, 12, IntervalType.StartClosed),
            (7, 12, IntervalType.EndClosed)
        };
        var expected = new Interval<int>(6, 12, IntervalType.Closed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    /// <summary>
    /// Set:
    ///           (-------------------)
    ///    [--------------------------)
    /// Expected set:
    ///    [--------------------------)
    /// ---6------7-------8-----------12-------------
    /// </summary>
    [Fact]
    public void Merge_SameEndPrecedingStartClosedFollowingOpen_ShouldReturnCorrectStartClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>
        {
            (6, 12, IntervalType.StartClosed),
            (7, 12, IntervalType.Open)
        };
        var expected = new Interval<int>(6, 12, IntervalType.StartClosed);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    /// <summary>
    /// Set:
    ///                   (-----------)
    ///    (--------------]
    /// Expected set:
    ///                   (-----------)
    ///    (--------------]
    /// ---6--------------8-----------12-------------
    /// </summary>
    [Fact]
    public void Merge_TouchingIntervalsPrecedingOpenFollowingOpen_ShouldNotMergeIntervals()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();
        var precedingInterval = new Interval<int>(6, 8, IntervalType.Open);
        var followingInterval = new Interval<int>(8, 12, IntervalType.Open);
        intervalSet.Add(precedingInterval);
        intervalSet.Add(followingInterval);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().BeEquivalentTo(new [] { precedingInterval, followingInterval });
    }

    /// <summary>
    /// Set:
    ///                   (-----------)
    ///    (--------------]
    /// Expected set:
    ///    (--------------------------)
    /// ---6--------------8-----------12-------------
    /// </summary>
    [Fact]
    public void Merge_TouchingIntervalsPrecedingEndClosedFollowingOpen_ShouldMergeIntervals()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();
        var precedingInterval = new Interval<int>(6, 8, IntervalType.EndClosed);
        var followingInterval = new Interval<int>(8, 12, IntervalType.Open);
        intervalSet.Add(precedingInterval);
        intervalSet.Add(followingInterval);
        var expected = new Interval<int>(6, 12, IntervalType.Open);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    /// <summary>
    /// Set:
    ///                   [-----------)
    ///    (--------------)
    /// Expected set:
    ///    (--------------------------)
    /// ---6--------------8-----------12-------------
    /// </summary>
    [Fact]
    public void Merge_TouchingIntervalsPrecedingOpenFollowingStartClosed_ShouldMergeIntervals()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>();
        var precedingInterval = new Interval<int>(6, 8, IntervalType.Open);
        var followingInterval = new Interval<int>(8, 12, IntervalType.StartClosed);
        intervalSet.Add(precedingInterval);
        intervalSet.Add(followingInterval);
        var expected = new Interval<int>(6, 12, IntervalType.Open);

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    /// <summary>
    /// Intersection interval:
    ///                   [--------]
    /// Set:
    ///    [--------------]
    ///    (--------------)        (--------]
    /// (--------)     (--------)  [--------------)
    /// Expected set:
    ///                (--------)
    ///    [--------------]        [--------------)
    /// 2--3-----5-----7--8-----10-11-------14----16-
    /// </summary>
    [Fact]
    public void Intersect_GivenIntervalHasAnyIntersection_ShouldReturnIntersectedIntervals()
    {
        // Arrange
        var expected = new Interval<int>[] {
            (3, 8, IntervalType.Closed),
            (11, 16, IntervalType.StartClosed),
            (7, 10),
        };

        var intervalSet = new IntervalSet<int>
        {
            (2, 5), // (2, 5)
            (7, 10), // (7, 10)
            (3, 8, IntervalType.Open), // (3, 8)
            (3, 8, IntervalType.Closed), // [3, 8]
            (11, 16, IntervalType.StartClosed), // [11, 16)
            (11, 14, IntervalType.EndClosed) // (11, 14]
        };

        // Act
        var result = intervalSet.Intersect((8, 11, IntervalType.Closed));

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    /// <summary>
    /// Intersection interval:
    ///                   (--------)
    /// Set:
    ///    [--------------]
    ///    (--------------)        (--------]
    /// (--------)                 [--------------)
    /// Expected set:
    ///
    /// 2--3-----5--------8--------11-------14----16-
    /// </summary>
    [Fact]
    public void Intersect_GivenIntervalHasNoIntersection_ShouldReturnEmptyCollection()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>
        {
            (2, 5), // (2, 5)
            (3, 8, IntervalType.Open), // (3, 8)
            (3, 8, IntervalType.Closed), // [3, 8]
            (11, 16, IntervalType.StartClosed), // [11, 16)
            (11, 14, IntervalType.EndClosed) // (11, 14]
        };

        // Act
        var result = intervalSet.Intersect((8, 11));

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Intersection interval:
    ///          [--------------------------------]
    /// Set:
    ///    [--------------------------]
    /// (--------)
    ///          [--------------)  (--------]
    /// Expected set:
    ///          [--------------)  (--------]
    /// 2--3-----5--------------10-11-12----14----16-
    /// </summary>
    [Fact]
    public void Intersect_GivenIntervalHasCoveringIntersection_ShouldReturnIntersectedIntervals()
    {
        // Arrange
        var expected = new Interval<int>[] {
            (5, 10, IntervalType.StartClosed),
            (11, 14, IntervalType.EndClosed)
        };

        var intervalSet = new IntervalSet<int>
        {
            (2, 5),
            (5, 10, IntervalType.StartClosed),
            (3, 12, IntervalType.Closed),
            (11, 14, IntervalType.EndClosed)
        };

        // Act
        var result = intervalSet.Intersect((5, 16, IntervalType.Closed), IntersectionType.Cover);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    /// <summary>
    /// Intersection interval:
    ///          (--------------)
    /// Set:
    ///    [--------------------------]
    /// (--------)
    ///          [--------------)  (--------]
    /// Expected set:
    ///
    /// 2--3-----5--------------10-11-12----14----16-
    /// </summary>
    [Fact]
    public void Intersect_GivenIntervalHasNoCoveringIntersection_ShouldReturnEmptyCollection()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>
        {
            (2, 5),
            (5, 10, IntervalType.StartClosed),
            (3, 12, IntervalType.Closed),
            (11, 16, IntervalType.StartClosed)
        };

        // Act
        var result = intervalSet.Intersect((5, 10), IntersectionType.Cover);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Intersection interval:
    ///          [-----------------]
    /// Set:
    ///    [--------------------------]
    /// (--------)
    ///          [--------------)  (--------]
    /// Expected set:
    ///    [--------------------------]
    /// 2--3-----5--------------10-11-12----14----16-
    /// </summary>
    [Fact]
    public void Intersect_GivenIntervalHasWithinIntersection_ShouldReturnIntersectedIntervals()
    {
        // Arrange
        var expected = new Interval<int>[] {
            (3, 12, IntervalType.Closed)
        };

        var intervalSet = new IntervalSet<int>
        {
            (2, 5),
            (5, 10, IntervalType.StartClosed),
            (3, 12, IntervalType.Closed),
            (11, 16, IntervalType.StartClosed)
        };

        // Act
        var result = intervalSet.Intersect((5, 11, IntervalType.Closed), IntersectionType.Within);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    /// <summary>
    /// Intersection interval:
    ///          (--------------------------------)
    /// Set:
    ///    [--------------------------]
    /// (--------)
    ///          [--------------)  (--------]
    /// Expected set:
    ///
    /// 2--3-----5--------------10-11-12----14----16-
    /// </summary>
    [Fact]
    public void Intersect_GivenIntervalHasNoWithinIntersection_ShouldReturnEmptyCollection()
    {
        // Arrange
        var intervalSet = new IntervalSet<int>
        {
            (2, 5),
            (5, 10, IntervalType.StartClosed),
            (3, 12, IntervalType.Closed),
            (11, 16, IntervalType.StartClosed)
        };

        // Act
        var result = intervalSet.Intersect((5, 16), IntersectionType.Within);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Intersect_GivenIntervalHasManyIntersectedIntervals_ShouldReturnIntersectedIntervalsSorted()
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

        var intervalSet = new IntervalSet<int>
        {
            (2, 5), // (2, 5)
            (3, 8, IntervalType.Open), // (3, 8)
            (3, 8, IntervalType.Closed), // [3, 8]
            (11, 14, IntervalType.EndClosed), // (11, 14]
            (11, 16, IntervalType.StartClosed), // [11, 16)
            (22, 25, IntervalType.Closed), // [22, 25]
            (45, 50, IntervalType.Closed), // [45, 50]
            (21, 42, IntervalType.Closed), // [21, 42]
            (12, 29, IntervalType.Closed) // [12, 29]
        };

        // Act
        var result = intervalSet.Intersect((8, 22, IntervalType.Closed));

        // Assert
        result.Should().ContainInOrder(expected);
    }
}
