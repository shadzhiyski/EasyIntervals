namespace NeatIntervals.Tests;

public class IntervalSetTests
{
    /// Test AA Tree Structure:
    ///
    /// [3]:                (7, 12]     -->    (42, 68)
    ///                  /                  /           \
    /// [2]:     (2, 4)           [18, 13]                (56, 65) --> (73, 90)
    ///         /     \          /       \               /            /       \
    /// [1]:  (1, 2) (6, 11)  [13, 18] (24, 56)    (55, 58)        (69, 92) (74, 80)
    private ISet<Interval<int, int?>> input = new HashSet<Interval<int, int?>> {
            (18, 34, IntervalType.Closed),
            (13, 18, IntervalType.Closed),
            (1, 2, IntervalType.Open),
            (7, 12, IntervalType.Closed),
            (42, 68, IntervalType.Open),
            (73, 90, IntervalType.Open),
            (56, 65, IntervalType.Open),
            (24, 56, IntervalType.Open),
            (6, 11, IntervalType.Open),
            (2, 4, IntervalType.Open),
            (74, 80, IntervalType.Open),
            (69, 92, IntervalType.Open),
            (55, 58, IntervalType.Open)
        };

    private IntervalSet<int, int?> CreateIntervalSet(ISet<Interval<int, int?>> input)
    {
        var intervalSet = new IntervalSet<int, int?>(input);

        return intervalSet;
    }

    [Fact]
    public void Initialize_FromAnotherIntervalSetWithDifferentComparer_ShouldThrowArgumentException()
    {
        var intervals = new IntervalSet<int, int?>(input);

        // Act
        Action act = () => new IntervalSet<int, int?>(intervals, (a, b) => b - a);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Add_NonExistingInterval_ShouldIncreaseCount()
    {
        var intervalSet = new IntervalSet<int, int?>
        {
            (5, 10)
        };

        intervalSet.Count.Should().Be(1);
    }

    [Fact]
    public void Add_ExistingInterval_ShouldNotIncreaseCount()
    {
        var intervalSet = new IntervalSet<int, int?>
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

        var result = intervalSet.Contains((1, 2, IntervalType.Open));

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
        var expected = new Interval<int, int?>[] {
            (1, 2, IntervalType.Open),
            (2, 4, IntervalType.Open),
            (6, 12, IntervalType.StartOpen),
            (13, 68, IntervalType.EndOpen),
            (69, 92, IntervalType.Open),
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
        var expected = new Interval<int, int?>[] {
            (1, 2, IntervalType.Open),
            (2, 4, IntervalType.Open),
            (6, 12, IntervalType.StartOpen),
            (13, 68, IntervalType.EndOpen),
            (69, 92, IntervalType.Open),
        };

        // Act
        var result = intervalSet.Merge();

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Merge_NonEmptyCollection_ShouldReturnMergedIntervalsByMergeFunction()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, decimal?>
        {
            (6, 11, 20m, IntervalType.Closed),
            (7, 12, 10m, IntervalType.EndOpen)
        };
        var expected = new Interval<int, decimal?>(6, 12, 30m, IntervalType.EndOpen);

        // Act
        var result = intervalSet.Merge((itv1, itv2) => itv1.Value + itv2.Value);

        // Assert
        result.Should().ContainEquivalentOf(expected);
    }

    [Fact]
    public void Merge_EmptyCollection_ShouldReturnEmptyIntervalSet()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>();

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
    public void Merge_PrecedingClosedFollowingEndOpen_ShouldReturnCorrectEndOpen()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (6, 11, IntervalType.Closed),
            (7, 12, IntervalType.EndOpen)
        };
        var expected = new Interval<int, int?>(6, 12, IntervalType.EndOpen);

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
    public void Merge_PrecedingEndOpenFollowingStartOpen_ShouldReturnCorrectClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (6, 11, IntervalType.EndOpen),
            (7, 12, IntervalType.StartOpen)
        };
        var expected = new Interval<int, int?>(6, 12, IntervalType.Closed);

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
    public void Merge_PrecedingOpenFollowingStartOpen_ShouldReturnCorrectStartOpen()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (6, 11, IntervalType.Open),
            (7, 12, IntervalType.StartOpen)
        };
        var expected = new Interval<int, int?>(6, 12, IntervalType.StartOpen);

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
    public void Merge_PrecedingOpenFollowingEndOpen_ShouldReturnCorrectOpen()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (6, 11, IntervalType.Open),
            (7, 12, IntervalType.EndOpen)
        };
        var expected = new Interval<int, int?>(6, 12, IntervalType.Open);

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
    public void Merge_SameStartPrecedingOpenFollowingStartOpen_ShouldReturnCorrectStartOpen()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (7, 11, IntervalType.Open),
            (7, 12, IntervalType.StartOpen)
        };
        var expected = new Interval<int, int?>(7, 12, IntervalType.StartOpen);

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
    public void Merge_SameStartPrecedingEndOpenFollowingStartOpen_ShouldReturnCorrectClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (7, 11, IntervalType.EndOpen),
            (7, 12, IntervalType.StartOpen)
        };
        var expected = new Interval<int, int?>(7, 12, IntervalType.Closed);

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
    public void Merge_SameStartPrecedingEndOpenFollowingClosed_ShouldReturnCorrectClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (7, 11, IntervalType.EndOpen),
            (7, 12, IntervalType.Closed)
        };
        var expected = new Interval<int, int?>(7, 12, IntervalType.Closed);

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
    public void Merge_SameEndPrecedingOpenFollowingStartOpen_ShouldReturnCorrectStartOpen()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (6, 12, IntervalType.Open),
            (7, 12, IntervalType.StartOpen)
        };
        var expected = new Interval<int, int?>(6, 12, IntervalType.StartOpen);

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
    public void Merge_SameEndPrecedingEndOpenFollowingStartOpen_ShouldReturnCorrectClosed()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (6, 12, IntervalType.EndOpen),
            (7, 12, IntervalType.StartOpen)
        };
        var expected = new Interval<int, int?>(6, 12, IntervalType.Closed);

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
    public void Merge_SameEndPrecedingEndOpenFollowingOpen_ShouldReturnCorrectEndOpen()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (6, 12, IntervalType.EndOpen),
            (7, 12, IntervalType.Open)
        };
        var expected = new Interval<int, int?>(6, 12, IntervalType.EndOpen);

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
        var intervalSet = new IntervalSet<int, int?>();
        var precedingInterval = new Interval<int, int?>(6, 8, IntervalType.Open);
        var followingInterval = new Interval<int, int?>(8, 12, IntervalType.Open);
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
    public void Merge_TouchingIntervalsPrecedingStartOpenFollowingOpen_ShouldMergeIntervals()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>();
        var precedingInterval = new Interval<int, int?>(6, 8, IntervalType.StartOpen);
        var followingInterval = new Interval<int, int?>(8, 12, IntervalType.Open);
        intervalSet.Add(precedingInterval);
        intervalSet.Add(followingInterval);
        var expected = new Interval<int, int?>(6, 12, IntervalType.Open);

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
    public void Merge_TouchingIntervalsPrecedingOpenFollowingEndOpen_ShouldMergeIntervals()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>();
        var precedingInterval = new Interval<int, int?>(6, 8, IntervalType.Open);
        var followingInterval = new Interval<int, int?>(8, 12, IntervalType.EndOpen);
        intervalSet.Add(precedingInterval);
        intervalSet.Add(followingInterval);
        var expected = new Interval<int, int?>(6, 12, IntervalType.Open);

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
    /// 2--3-----5-----7--8-----10-11-------14----16-
    /// Expected result: true
    /// </summary>
    [Fact]
    public void HasIntersection_GivenIntervalHasAnyIntersection_ShouldReturnTrue()
    {
        // Arrange
        var expected = new Interval<int, int?>[] {
            (3, 8, IntervalType.Closed),
            (11, 16, IntervalType.EndOpen),
            (7, 10),
        };

        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5), // (2, 5)
            (7, 10), // (7, 10)
            (3, 8, IntervalType.Open), // (3, 8)
            (3, 8, IntervalType.Closed), // [3, 8]
            (11, 16, IntervalType.EndOpen), // [11, 16)
            (11, 14, IntervalType.StartOpen) // (11, 14]
        };

        // Act
        var result = intervalSet.HasIntersection((8, 11, IntervalType.Closed));

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Intersection interval:
    ///                   (--------)
    /// Set:
    ///    [--------------]
    ///    (--------------)        (--------]
    /// (--------)                 [--------------)
    /// 2--3-----5--------8--------11-------14----16-
    ///  Expected result: false
    /// </summary>
    [Fact]
    public void HasIntersection_GivenIntervalHasNoIntersection_ShouldReturnFalse()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5, IntervalType.Open), // (2, 5)
            (3, 8, IntervalType.Open), // (3, 8)
            (3, 8, IntervalType.Closed), // [3, 8]
            (11, 16, IntervalType.EndOpen), // [11, 16)
            (11, 14, IntervalType.StartOpen) // (11, 14]
        };

        // Act
        var result = intervalSet.HasIntersection((8, 11, IntervalType.Open));

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Intersection interval:
    ///          [--------------------------------]
    /// Set:
    ///    [--------------------------]
    /// (--------)
    ///          [--------------)  (--------]
    /// 2--3-----5--------------10-11-12----14----16-
    /// Expected result: true
    /// </summary>
    [Fact]
    public void HasIntersection_GivenIntervalHasCoveringIntersection_ShouldReturnIntersectedIntervals()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (11, 14, IntervalType.StartOpen)
        };

        // Act
        var result = intervalSet.HasIntersection((5, 16, IntervalType.Closed), IntersectionType.Cover);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Intersection interval:
    ///          (--------------)
    /// Set:
    ///    [--------------------------]
    /// (--------)
    ///          [--------------)  (--------]
    /// 2--3-----5--------------10-11-12----14----16-
    /// Expected result: false
    /// </summary>
    [Fact]
    public void HasIntersection_GivenIntervalHasNoCoveringIntersection_ShouldReturnEmptyCollection()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5, IntervalType.Open),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (11, 16, IntervalType.EndOpen)
        };

        // Act
        var result = intervalSet.HasIntersection((5, 10, IntervalType.Open), IntersectionType.Cover);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Intersection interval:
    ///          [-----------------]
    /// Set:
    ///    [--------------------------]
    /// (--------)
    ///          [--------------)  (--------]
    /// 2--3-----5--------------10-11-12----14----16-
    /// Expected result: true
    /// </summary>
    [Fact]
    public void HasIntersection_GivenIntervalHasWithinIntersection_ShouldReturnIntersectedIntervals()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (11, 16, IntervalType.EndOpen)
        };

        // Act
        var result = intervalSet.HasIntersection((5, 11, IntervalType.Closed), IntersectionType.Within);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Intersection interval:
    ///          (--------------------------------)
    /// Set:
    ///    [--------------------------]
    /// (--------)
    ///          [--------------)  (--------]
    /// 2--3-----5--------------10-11-12----14----16-
    /// Expected result: false
    /// </summary>
    [Fact]
    public void HasIntersection_GivenIntervalHasNoWithinIntersection_ShouldReturnEmptyCollection()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (11, 16, IntervalType.EndOpen)
        };

        // Act
        var result = intervalSet.HasIntersection((5, 16), IntersectionType.Within);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Intersection intervals:
    ///                   [--------]                ... [20, 30] ... [40, 50]
    /// Set:
    ///    [--------------]
    ///    (--------------)        (--------]
    /// (--------)     (--------)  [--------------)
    /// 2--3-----5-----7--8-----10-11-------14----16-
    /// Expected result: true
    /// </summary>
    [Fact]
    public void HasIntersection_AnyGivenIntervalHasAnyIntersection_ShouldReturnTrue()
    {
        // Arrange
        var expected = new Interval<int, int?>[] {
            (3, 8, IntervalType.Closed),
            (11, 16, IntervalType.EndOpen),
            (7, 10),
        };

        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5), // (2, 5)
            (7, 10), // (7, 10)
            (3, 8, IntervalType.Open), // (3, 8)
            (3, 8, IntervalType.Closed), // [3, 8]
            (11, 16, IntervalType.EndOpen), // [11, 16)
            (11, 14, IntervalType.StartOpen) // (11, 14]
        };

        // Act
        var result = intervalSet.HasIntersection(new Interval<int, int?>[]
        {
            (8, 11, IntervalType.Closed), (20, 30, IntervalType.Closed), (40, 50, IntervalType.Closed)
        });

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Intersection interval:
    ///                   (--------)               ... [20, 30] ... [40, 50]
    /// Set:
    ///    [--------------]
    ///    (--------------)        (--------]
    /// (--------)                 [--------------)
    /// 2--3-----5--------8--------11-------14----16-
    ///  Expected result: false
    /// </summary>
    [Fact]
    public void HasIntersection_NoGivenIntervalHasNoIntersection_ShouldReturnFalse()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5), // (2, 5)
            (3, 8, IntervalType.Open), // (3, 8)
            (3, 8, IntervalType.Closed), // [3, 8]
            (11, 16, IntervalType.EndOpen), // [11, 16)
            (11, 14, IntervalType.StartOpen) // (11, 14]
        };

        // Act
        var result = intervalSet.HasIntersection(new Interval<int, int?>[]
        {
            (8, 11, IntervalType.Open), (20, 30), (40, 50)
        });

        // Assert
        result.Should().BeFalse();
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
        var expected = new Interval<int, int?>[] {
            (3, 8, IntervalType.Closed),
            (11, 16, IntervalType.EndOpen),
            (7, 10),
        };

        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5), // (2, 5)
            (7, 10), // (7, 10)
            (3, 8, IntervalType.Open), // (3, 8)
            (3, 8, IntervalType.Closed), // [3, 8]
            (11, 16, IntervalType.EndOpen), // [11, 16)
            (11, 14, IntervalType.StartOpen) // (11, 14]
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
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5, IntervalType.Open), // (2, 5)
            (3, 8, IntervalType.Open), // (3, 8)
            (3, 8, IntervalType.Closed), // [3, 8]
            (11, 16, IntervalType.EndOpen), // [11, 16)
            (11, 14, IntervalType.StartOpen) // (11, 14]
        };

        // Act
        var result = intervalSet.Intersect((8, 11, IntervalType.Open));

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
        var expected = new Interval<int, int?>[] {
            (5, 10, IntervalType.EndOpen),
            (11, 14, IntervalType.StartOpen)
        };

        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (11, 14, IntervalType.StartOpen)
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
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5, IntervalType.Open),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (11, 16, IntervalType.EndOpen)
        };

        // Act
        var result = intervalSet.Intersect((5, 10, IntervalType.Open), IntersectionType.Cover);

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
        var expected = new Interval<int, int?>[] {
            (3, 12, IntervalType.Closed)
        };

        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (11, 16, IntervalType.EndOpen)
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
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (11, 16, IntervalType.EndOpen)
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
        var expected = new Interval<int, int?>[] {
            (3, 8, IntervalType.Closed),
            (11, 16, IntervalType.EndOpen),
            (11, 14, IntervalType.StartOpen),
            (12, 29, IntervalType.Closed),
            (21, 42, IntervalType.Closed),
            (22, 25, IntervalType.Closed)
        };

        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5), // (2, 5)
            (3, 8, IntervalType.Open), // (3, 8)
            (3, 8, IntervalType.Closed), // [3, 8]
            (11, 14, IntervalType.StartOpen), // (11, 14]
            (11, 16, IntervalType.EndOpen), // [11, 16)
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

    /// <summary>
    /// Intersection interval:
    ///                   [--------]
    /// Set:
    ///    [--------------]
    ///    (--------------)        (--------]
    /// (--------)     (--------)  [--------------)
    /// Expected set:
    /// (--------)
    ///    (--------------)        (--------]
    /// 2--3-----5-----7--8-----10-11-------14----16-
    /// </summary>
    [Fact]
    public void Except_GivenIntervalHasAnyIntersection_ShouldReturnExceptedIntervals()
    {
        // Arrange
        var expected = new Interval<int, int?>[] {
            (2, 5),
            (3, 8, IntervalType.Open),
            (11, 14, IntervalType.StartOpen)
        };

        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5), // (2, 5)
            (7, 10), // (7, 10)
            (3, 8, IntervalType.Open), // (3, 8)
            (3, 8, IntervalType.Closed), // [3, 8]
            (11, 16, IntervalType.EndOpen), // [11, 16)
            (11, 14, IntervalType.StartOpen) // (11, 14]
        };

        // Act
        var result = intervalSet.Except((8, 11, IntervalType.Closed));

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    /// <summary>
    /// Intersection interval:
    ///    (--------------------------------)
    /// Set:
    ///    [--------------]
    ///    (--------------)        (--------]
    /// (--------)                 [--------------)
    /// Expected set:
    ///
    /// 2--3-----5--------8--------11-------14----16-
    /// </summary>
    [Fact]
    public void Except_GivenIntervalHasAllIntersected_ShouldReturnEmptyCollection()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5), // (2, 5)
            (3, 8, IntervalType.Open), // (3, 8)
            (3, 8, IntervalType.Closed), // [3, 8]
            (11, 16, IntervalType.EndOpen), // [11, 16)
            (11, 14, IntervalType.StartOpen) // (11, 14]
        };

        // Act
        var result = intervalSet.Except((3, 14));

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
    ///    [--------------------------]
    /// (--------)
    /// 2--3-----5--------------10-11-12----14----16-
    /// </summary>
    [Fact]
    public void Except_GivenIntervalHasCoveringIntersection_ShouldReturnExceptedIntervals()
    {
        // Arrange
        var expected = new Interval<int, int?>[] {
            (2, 5),
            (3, 12, IntervalType.Closed)
        };

        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (11, 14, IntervalType.StartOpen)
        };

        // Act
        var result = intervalSet.Except((5, 16, IntervalType.Closed), IntersectionType.Cover);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    /// <summary>
    /// Intersection interval:
    /// [-----------------------------------------)
    /// Set:
    ///    [--------------------------]
    /// (--------)
    ///          [--------------)  (--------]
    /// Expected set:
    ///
    /// 2--3-----5--------------10-11-12----14----16-
    /// </summary>
    [Fact]
    public void Except_GivenIntervalHasAllCoveringIntersection_ShouldReturnEmptyCollection()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (11, 16, IntervalType.EndOpen)
        };

        // Act
        var result = intervalSet.Except((2, 16, IntervalType.EndOpen), IntersectionType.Cover);

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
    /// (--------)
    ///          [--------------)  (--------]
    /// 2--3-----5--------------10-11-12----14----16-
    /// </summary>
    [Fact]
    public void Except_GivenIntervalHasWithinIntersection_ShouldReturnExceptedIntervals()
    {
        // Arrange
        var expected = new Interval<int, int?>[] {
            (2, 5),
            (5, 10, IntervalType.EndOpen),
            (11, 14, IntervalType.EndOpen)
        };

        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 5),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (11, 14, IntervalType.EndOpen)
        };

        // Act
        var result = intervalSet.Except((5, 11, IntervalType.Closed), IntersectionType.Within);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }

    /// <summary>
    /// Intersection interval:
    ///          (--------------)
    /// Set:
    ///    [--------------------------]
    /// (-----------------------)
    ///          [--------------)
    /// Expected set:
    ///
    /// 2--3-----5--------------10-11-12----14----16-
    /// </summary>
    [Fact]
    public void Except_GivenIntervalHasAllWithinIntersection_ShouldReturnEmptyCollection()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 10, IntervalType.Open),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed)
        };

        // Act
        var result = intervalSet.Except((5, 10, IntervalType.Open), IntersectionType.Within);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Union_OtherHasMatchingIntervals_ShouldUnionIntervalSetsWithoutRepetition()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed)
        };

        var otherIntervalSet = new IntervalSet<int, int?>
        {
            (3, 12, IntervalType.Closed),
            (25, 30),
            (33, 35)
        };

        var expectedIntervals = new Interval<int, int?>[]
        {
            (2, 10),
            (3, 12, IntervalType.Closed),
            (5, 10, IntervalType.EndOpen),
            (25, 30),
            (33, 35)
        };

        // Act
        var result = intervalSet.Union(otherIntervalSet);

        // Assert
        result.Should().BeEquivalentTo(expectedIntervals);
    }

    [Fact]
    public void Union_IntervalSetsHasIntervalsInBetween_ShouldUnionIntervalSets()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (3, 12, IntervalType.Closed),
            (25, 30),
        };

        var otherIntervalSet = new IntervalSet<int, int?>
        {
            (5, 10, IntervalType.EndOpen),
            (33, 35)
        };

        var expectedIntervals = new Interval<int, int?>[]
        {
            (2, 10),
            (3, 12, IntervalType.Closed),
            (5, 10, IntervalType.EndOpen),
            (25, 30),
            (33, 35)
        };

        // Act
        var result = intervalSet.Union(otherIntervalSet);

        // Assert
        result.Should().BeEquivalentTo(expectedIntervals);
    }

    [Fact]
    public void Union_OtherEmpty_ShouldUnionOnlyIntervalSet()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (3, 12, IntervalType.Closed),
            (5, 10, IntervalType.EndOpen)
        };

        var otherIntervalSet = new IntervalSet<int, int?>();

        var expectedIntervals = new Interval<int, int?>[]
        {
            (2, 10),
            (3, 12, IntervalType.Closed),
            (5, 10, IntervalType.EndOpen)
        };

        // Act
        var result = intervalSet.Union(otherIntervalSet);

        // Assert
        result.Should().BeEquivalentTo(expectedIntervals);
    }

    [Fact]
    public void Union_CurrentEmpty_ShouldUnionOnlyOtherIntervalSet()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>();

        var otherIntervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (3, 12, IntervalType.Closed),
            (5, 10, IntervalType.EndOpen)
        };

        var expectedIntervals = new Interval<int, int?>[]
        {
            (2, 10),
            (3, 12, IntervalType.Closed),
            (5, 10, IntervalType.EndOpen)
        };

        // Act
        var result = intervalSet.Union(otherIntervalSet);

        // Assert
        result.Should().BeEquivalentTo(expectedIntervals);
    }

    [Fact]
    public void Union_DifferentComparers_ShouldThrowArgumentException()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>();

        var otherIntervalSet = new IntervalSet<int, int?>((a, b) => b - a);

        // Act
        Action act = () => intervalSet.Union(otherIntervalSet);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SymmetricExceptWith_HasIntersection_ShouldExceptIntervalSet()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed)
        };

        var exceptIntervalSet = new IntervalSet<int, int?>
        {
            (5, 10, IntervalType.EndOpen),
            (25, 30),
            (33, 35)
        };

        var expectedIntervals = new Interval<int, int?>[]
        {
            (2, 10),
            (3, 12, IntervalType.Closed)
        };

        // Act
        intervalSet.SymmetricExceptWith(exceptIntervalSet);

        // Assert
        intervalSet.Should().BeEquivalentTo(expectedIntervals);
    }

    [Fact]
    public void IntersectWith_HasIntersection_ShouldIntersectIntervalSet()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed)
        };

        var exceptIntervalSet = new IntervalSet<int, int?>
        {
            (5, 10, IntervalType.EndOpen),
            (25, 30),
            (33, 35)
        };

        var expectedIntervals = new Interval<int, int?>[]
        {
            (5, 10, IntervalType.EndOpen),
        };

        // Act
        intervalSet.IntersectWith(exceptIntervalSet);

        // Assert
        intervalSet.Should().BeEquivalentTo(expectedIntervals);
    }

    [Fact]
    public void UnionWith_OtherWithDifferentIntervals_ShouldUnionIntervalSet()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed)
        };

        var otherIntervalSet = new IntervalSet<int, int?>
        {
            (25, 30),
            (33, 35)
        };

        var expectedIntervals = new Interval<int, int?>[]
        {
            (2, 10),
            (3, 12, IntervalType.Closed),
            (5, 10, IntervalType.EndOpen),
            (25, 30),
            (33, 35)
        };

        // Act
        intervalSet.UnionWith(otherIntervalSet);

        // Assert
        intervalSet.Should().BeEquivalentTo(expectedIntervals);
    }

    [Fact]
    public void UnionWith_OtherWithRepeatingIntervals_ShouldUnionIntervalSetWithoutRepetition()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed)
        };

        var otherIntervalSet = new IntervalSet<int, int?>
        {
            (25, 30),
            (25, 30),
            (33, 35)
        };

        var expectedIntervals = new Interval<int, int?>[]
        {
            (2, 10),
            (3, 12, IntervalType.Closed),
            (5, 10, IntervalType.EndOpen),
            (25, 30),
            (33, 35)
        };

        // Act
        intervalSet.UnionWith(otherIntervalSet);

        // Assert
        intervalSet.Should().BeEquivalentTo(expectedIntervals);
    }

    [Fact]
    public void IsSubsetOf_AllElementsAreInOtherIntervalSet_ShouldReturnTrue()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed)
        };

        var otherIntervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (25, 30),
            (33, 35)
        };

        // Act
        var result = intervalSet.IsSubsetOf(otherIntervalSet);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSubsetOf_NotAllElementsAreInOtherIntervalSet_ShouldReturnFalse()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed)
        };

        var otherIntervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (25, 30),
            (33, 35)
        };

        // Act
        var result = intervalSet.IsSubsetOf(otherIntervalSet);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSupersetOf_AllOtherElementsAreInIntervalSet_ShouldReturnTrue()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (25, 30),
            (33, 35)
        };

        var otherIntervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed)
        };

        // Act
        var result = intervalSet.IsSupersetOf(otherIntervalSet);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSupersetOf_NotAllOtherElementsAreInIntervalSet_ShouldReturnFalse()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (25, 30),
            (33, 35)
        };

        var otherIntervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed)
        };

        // Act
        var result = intervalSet.IsSupersetOf(otherIntervalSet);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_HasMatchingIntervals_ShouldReturnTrue()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (25, 30),
            (33, 35)
        };

        var otherIntervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (8, 14, IntervalType.Closed)
        };

        // Act
        var result = intervalSet.Overlaps(otherIntervalSet);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_HasNoMatchingIntervals_ShouldReturnFalse()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (25, 30),
            (33, 35)
        };

        var otherIntervalSet = new IntervalSet<int, int?>
        {
            (5, 10),
            (5, 10, IntervalType.StartOpen),
            (8, 14, IntervalType.Closed)
        };

        // Act
        var result = intervalSet.Overlaps(otherIntervalSet);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SetEquals_HasSameIntervals_ShouldReturnTrue()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (25, 30),
            (33, 35)
        };

        var otherIntervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (25, 30),
            (33, 35)
        };

        // Act
        var result = intervalSet.SetEquals(otherIntervalSet);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SetEquals_HasNotSameIntervals_ShouldReturnFalse()
    {
        // Arrange
        var intervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10, IntervalType.EndOpen),
            (3, 12, IntervalType.Closed),
            (25, 30),
            (33, 35)
        };

        var otherIntervalSet = new IntervalSet<int, int?>
        {
            (2, 10),
            (5, 10),
            (3, 12, IntervalType.Closed),
            (25, 30, IntervalType.EndOpen),
            (33, 35)
        };

        // Act
        var result = intervalSet.SetEquals(otherIntervalSet);

        // Assert
        result.Should().BeFalse();
    }
}
