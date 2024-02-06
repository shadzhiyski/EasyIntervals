namespace EasyIntervals.Tests;

public class AATreeTests
{

    private int[] input = {
            18,
            13,
            1,
            7,
            42,
            73,
            56,
            24,
            6,
            2,
            74,
            69,
            55
        };

    private (int, int, int)[] intervalsInput = new [] {
        (18, 34, 34),
        (13, 18, 18),
        (1, 2, 2),
        (7, 12, 12),
        (42, 68, 68),
        (73, 90, 90),
        (56, 65, 65),
        (24, 56, 56),
        (6, 11, 11),
        (2, 4, 4),
        (74, 80, 80),
        (69, 92, 92),
        (55, 58, 58)
    };

    private AATree<T> CreateAATree<T>(IEnumerable<T> input)
    {
        var aaTree = new AATree<T>(input);

        return aaTree;
    }

    private void FillAATree<T>(AATree<T> aaTree, IEnumerable<T> input)
    {
        foreach (var element in input)
        {
            aaTree.Add(element);
        }
    }

    [Fact]
    public void Add_NonExistingElement_ShouldReturnTrue()
    {
        var aaTree = new AATree<int>();

        var result = aaTree.Add(24);

        result.Should().BeTrue();
    }

    [Fact]
    public void Add_NonExistingElement_ShouldIncreaseCount()
    {
        var aaTree = new AATree<int>();

        aaTree.Add(24);

        aaTree.Count.Should().Be(1);
    }

    [Fact]
    public void Add_ExistingElement_ShouldReturnFalse()
    {
        var aaTree = new AATree<int>();
        aaTree.Add(24);

        var result = aaTree.Add(24);

        result.Should().BeFalse();
    }

    [Fact]
    public void Add_ExistingElement_ShouldKeepCount()
    {
        var aaTree = new AATree<int>();
        aaTree.Add(24);

        aaTree.Add(24);

        aaTree.Count.Should().Be(1);
    }

    [Fact]
    public void Add_NonExistingElement_ShouldBeEnumerated()
    {
        var aaTree = CreateAATree(input);

        aaTree.Add(99);

        aaTree.Should().Contain(99);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(9)]
    [InlineData(70)]
    public void Add_NotExistingElement_ShouldRemainValidAATree(int element)
    {
        var aaTree = CreateAATree(input);

        aaTree.Add(element);

        VerifyAATree(aaTree);
    }

    [Fact]
    public void Add_OnChildChangedSet_ShouldUpdateNodes()
    {
        // Arrange
        var expected = new [] {
            (1, 2, 4),
            (2, 4, 4),
            (6, 11, 12),
            (7, 12, 12),
            (13, 18, 92),
            (18, 34, 56),
            (24, 56, 56),
            (55, 58, 58),
            (42, 68, 68),
            (56, 65, 92),
            (69, 92, 92),
            (73, 90, 92),
            (74, 80, 80)
        };
        var aaTree = new AATree<(int Start, int End, int MaxEnd)>(
            Comparer<(int Start, int End, int MaxEnd)>.Create((a, b) => a.Start.CompareTo(b.Start)),
            (parent) =>
            {
                parent.Value = (
                    parent.Value.Start,
                    parent.Value.End,
                    Math.Max(
                        Math.Max(parent.Value.End, parent.Left?.Value.MaxEnd ?? 0),
                        Math.Max(parent.Value.End, parent.Right?.Value.MaxEnd ?? 0)
                    )
                );
            }
        );

        // Act
        FillAATree(aaTree, intervalsInput);

        // Assert
        aaTree.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Contains_ExistingElement_ShouldReturnTrue()
    {
        var aaTree = CreateAATree(input);

        var result = aaTree.Contains(24);

        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_NonExistingElement_ShouldReturnFalse()
    {
        var aaTree = CreateAATree(input);

        var result = aaTree.Contains(-9);

        result.Should().BeFalse();
    }

    [Fact]
    public void Count_WithManyElements_ShouldBeCorrect()
    {
        var aaTree = CreateAATree(input);

        aaTree.Count.Should().Be(input.Length);
    }

    [Fact]
    public void Count_EmptyCollection_ShouldBeZero()
    {
        var aaTree = new AATree<int>();

        aaTree.Count.Should().Be(0);
    }

    [Fact]
    public void Remove_ExistingElement_ShouldReturnTrue()
    {
        var aaTree = CreateAATree(input);

        var result = aaTree.Remove(24);

        result.Should().BeTrue();
    }

    [Fact]
    public void Remove_NotExistingElement_ShouldReturnFalse()
    {
        var aaTree = CreateAATree(input);

        var result = aaTree.Remove(199);

        result.Should().BeFalse();
    }

    [Fact]
    public void Remove_NonExistingElement_ShouldNotChangeCount()
    {
        var aaTree = CreateAATree(input);
        var expectedCount = aaTree.Count;

        aaTree.Remove(199);

        aaTree.Count.Should().Be(expectedCount);
    }

    [Theory]
    [InlineData(24)] // Leaf
    [InlineData(42)] // Low Level (< 2) Inner Element
    [InlineData(56)] // High Level (>= 2) Inner Element
    public void Remove_ExistingElement_ShouldNotBeEnumerated(int element)
    {
        var aaTree = CreateAATree(input);

        aaTree.Remove(element);

        aaTree.Should().BeEquivalentTo(input.Where(e => e != element));
    }

    [Fact]
    public void Remove_NonExistingElement_ShouldEnumerateFullCollection()
    {
        var aaTree = CreateAATree(input);

        aaTree.Remove(199);

        aaTree.Should().BeEquivalentTo(input);
    }

    [Fact]
    public void Remove_ExistingElement_ShouldDecrementCount()
    {
        var aaTree = CreateAATree(input);
        var expectedCount = aaTree.Count - 1;

        aaTree.Remove(24);

        aaTree.Count.Should().Be(expectedCount);
    }

    [Fact]
    public void Remove_NonEqualExistingElement_ShouldNotRemoveElement()
    {
        var aaTree = new AATree<Interval<int, int?>>(
            Enumerable.Empty<Interval<int, int?>>(),
            IntervalComparer<int, int?>.Create(Comparer<int>.Default), (_) => {});
        aaTree.Add((10, 20, 2));

        aaTree.Remove((10, 20, 5));

        aaTree.Count.Should().Be(1);
    }

    [Fact]
    public void Remove_EqualExistingElement_ShouldRemoveElement()
    {
        var aaTree = new AATree<Interval<int, int?>>(
            Enumerable.Empty<Interval<int, int?>>(),
            IntervalComparer<int, int?>.Create(Comparer<int>.Default), (_) => {});
        aaTree.Add((10, 20, 2));

        aaTree.Remove((10, 20, 2));

        aaTree.Count.Should().Be(0);
    }

    [Theory]
    [InlineData(24)] // Leaf
    [InlineData(42)] // Low Level (< 2) Inner Element
    [InlineData(56)] // High Level (>= 2) Inner Element
    public void Remove_ExistingElement_ShouldRemainValidAATree(int element)
    {
        var aaTree = CreateAATree(input);

        aaTree.Remove(element);

        VerifyAATree(aaTree);
    }

    [Fact]
    public void Remove_LeafNodeAndOnChildChangedSet_ShouldUpdateNodes()
    {
        // Arrange
        var expected = new [] {
            (1, 2, 4),
            (2, 4, 4),
            (6, 11, 12),
            (7, 12, 12),
            (13, 18, 92),
            (18, 34, 34),
            // (24, 56, 56),
            (55, 58, 58),
            (42, 68, 68),
            (56, 65, 92),
            (69, 92, 92),
            (73, 90, 92),
            (74, 80, 80)
        };
        var aaTree = new AATree<(int Start, int End, int MaxEnd)>(
            Comparer<(int Start, int End, int MaxEnd)>.Create((a, b) => a.Start.CompareTo(b.Start)),
            (parent) =>
            {
                parent.Value = (
                    parent.Value.Start,
                    parent.Value.End,
                    Math.Max(
                        Math.Max(parent.Value.End, parent.Left?.Value.MaxEnd ?? 0),
                        Math.Max(parent.Value.End, parent.Right?.Value.MaxEnd ?? 0)
                    )
                );
            }
        );
        FillAATree(aaTree, intervalsInput);

        // Act
        aaTree.Remove((24, 56, 56));

        // Assert
        aaTree.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Remove_LowInnerNodeAndOnChildChangedSet_ShouldUpdateNodes()
    {
        // Arrange
        var expected = new [] {
            (1, 2, 4),
            (2, 4, 4),
            (6, 11, 12),
            (7, 12, 12),
            (13, 18, 92),
            (18, 34, 34),
            (24, 56, 58),
            (55, 58, 58),
            // (42, 68, 68),
            (56, 65, 92),
            (69, 92, 92),
            (73, 90, 92),
            (74, 80, 80)
        };
        var aaTree = new AATree<(int Start, int End, int MaxEnd)>(
            Comparer<(int Start, int End, int MaxEnd)>.Create((a, b) => a.Start.CompareTo(b.Start)),
            (parent) =>
            {
                parent.Value = (
                    parent.Value.Start,
                    parent.Value.End,
                    Math.Max(
                        Math.Max(parent.Value.End, parent.Left?.Value.MaxEnd ?? 0),
                        Math.Max(parent.Value.End, parent.Right?.Value.MaxEnd ?? 0)
                    )
                );
            }
        );
        FillAATree(aaTree, intervalsInput);

        // Act
        aaTree.Remove((42, 68, 68));

        // Assert
        aaTree.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Remove_HighInnerNodeAndOnChildChangedSet_ShouldUpdateNodes()
    {
        // Arrange
        var expected = new [] {
            (1, 2, 4),
            (2, 4, 4),
            (6, 11, 12),
            (7, 12, 12),
            (13, 18, 92),
            (18, 34, 56),
            (24, 56, 56),
            (55, 58, 58),
            (42, 68, 92),
            // (56, 65, 92),
            (69, 92, 92),
            (73, 90, 90),
            (74, 80, 80)
        };
        var aaTree = new AATree<(int Start, int End, int MaxEnd)>(
            Comparer<(int Start, int End, int MaxEnd)>.Create((a, b) => a.Start.CompareTo(b.Start)),
            (parent) =>
            {
                parent.Value = (
                    parent.Value.Start,
                    parent.Value.End,
                    Math.Max(
                        Math.Max(parent.Value.End, parent.Left?.Value.MaxEnd ?? 0),
                        Math.Max(parent.Value.End, parent.Right?.Value.MaxEnd ?? 0)
                    )
                );
            }
        );
        FillAATree(aaTree, intervalsInput);

        // Act
        aaTree.Remove((56, 65, 92));

        // Assert
        aaTree.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Enumerate_WithManyElements_ShouldBeSorted()
    {
        List<int> expected = input.OrderBy(x => x).ToList();
        var aaTree = CreateAATree(input);

        List<int> numbers = aaTree.ToList();

        numbers.Should().ContainInOrder(expected);
    }

    [Fact]
    public void Initialize_RepeatingElements_ShouldHaveCountOfUniqueElements()
    {
        var evenCountElements = new [] { 18, 13, 1, 7, 42, 7 };

        var aaTree = new AATree<int>(evenCountElements);

        aaTree.Count.Should().Be(5);
    }

    /// <summary>
    ///
    /// Full Tree Structure:
    /// [3]:                20
    ///                 /        \
    /// [2]:        10              30
    ///           /    \          /    \
    /// [1]:    5       15     25        45
    ///
    /// </summary>
    [Fact]
    public void Initialize_CountOfElementsForBalancedTree_ShouldCreateValidAATree()
    {
        var elements = new int[] { 5, 10, 15, 20, 25, 30, 35 };

        var aaTree = new AATree<int>(elements);

        VerifyAATree(aaTree);
    }

    /// <summary>
    ///
    /// Subtree:
    ///    use case      =>  updated child
    /// +----------------+------------------+
    ///      .. 30       | .. 30            |
    ///            \     |       \          |
    /// [1]:    *35  40  |         35 -> 40 |
    /// +----------------+------------------+
    ///
    /// Full Tree Structure:
    /// [2]:       30
    ///          /    \ ---------+
    /// [1]:  25      | 35 -> 40 |
    ///               +----------+
    /// </summary>
    [Fact]
    public void Initialize_HaveLeftmostNextToLeafChild_ShouldUpdateLeafChild()
    {
        var elements = new int[] { 25, 30, 35, 40 };

        var aaTree = new AATree<int>(elements);

        aaTree.Root!.Right!.Value.Should().Be(35);
    }

    [Fact]
    public void Initialize_HaveLeftmostNextToLeafChild_ShouldCreateValidAATree()
    {
        var elements = new int[] { 25, 30, 35, 40 };

        var aaTree = new AATree<int>(elements);

        VerifyAATree(aaTree);
    }

    /// <summary>
    ///
    /// Subtree:
    ///          use case       => updated child
    /// +-----------------------+-----------------+
    /// [2]:            35      |        35       |
    ///               /    \    |     /      \    |
    /// [1]:  *25   30      40  |  25 -> 30    40 |
    /// +-----------------------+-----------------+
    ///
    /// Full Tree Structure:
    /// [3]:               20
    ///                 /       \
    /// [2]:        10      +------ 35 ------+
    ///           /    \    |    /      \    |
    /// [1]:    5       15  | 25 -> 30    40 |
    ///                     +----------------+
    /// </summary>
    [Fact]
    public void Initialize_HaveLeftmostChildOnSubtreeWithThreeNodes_ShouldUpdateLeftChild()
    {
        var elements = new int[] { 5, 10, 15, 20, 25, 30, 35, 40 };

        var aaTree = new AATree<int>(elements);

        var subtree = aaTree.Root!.Right;
        subtree!.Left!.Value.Should().Be(25);
    }

    [Fact]
    public void Initialize_HaveLeftmostChildOnThreeNodeTree_ShouldCreateValidAATree()
    {
        var elements = new int[] { 5, 10, 15, 20, 25, 30, 35, 40 };

        var aaTree = new AATree<int>(elements);

        VerifyAATree(aaTree);
    }

    /// <summary>
    ///
    /// Subtree:
    ///            use case            =>         skewed
    /// +------------------------------+---------------------------+
    /// [2]:      40  <-  50           |     40  ->  50            |
    ///         /    \      \          |    /      /    \          |
    /// [1]:  35     45      55 -> 50  |  35    45        55 -> 60 |
    /// +------------------------------+---------------------------+
    ///
    /// Full Tree Structure:
    /// [3]:                 30
    ///                 /          \ -------------------------+
    /// [2]:        15             |    40  ->  50            |
    ///         /       \          |   /      /    \          |
    /// [1]:  5 -> 10    20 -> 25  | 35    45        55 -> 60 |
    ///                            +--------------------------+
    /// </summary>
    [Fact]
    public void Initialize_LeftChildHaveHigherLevelThanRightChild_ShouldUpdateParent()
    {
        var elements = new int[] { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60 };

        var aaTree = new AATree<int>(elements);

        var subtree = aaTree.Root!.Right;
        subtree!.Value.Should().Be(40);
    }

    [Fact]
    public void Initialize_LeftChildHaveHigherLevelThanRightChild_ShouldCreateValidAATree()
    {
        var elements = new int[] { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60 };

        var aaTree = new AATree<int>(elements);

        VerifyAATree(aaTree);
    }

    private static void VerifyAATree<T>(AATree<T> tree) where T : struct
    {
        var queue = new Queue<AATree<T>.Node>();
        queue.Enqueue(tree.Root!);

        int traversedNodesCount = 0;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            traversedNodesCount++;

            VerifySingleNode(current);

            if (current.Left is not null)
            {
                queue.Enqueue(current.Left);
            }

            if (current.Right is not null)
            {
                queue.Enqueue(current.Right);
            }
        }

        traversedNodesCount.Should().Be(tree.Count);
    }

    private static void VerifySingleNode<T>(AATree<T>.Node node) where T : struct
    {
        if (node.Left is not null && node.Right is not null)
        {
            node.Level.Should().BeGreaterThan(1, "Violation! If node has both children it's level must be greater than 1. Rule not satisfied. "
                + $"Node (value: {node.Value}; level {node.Level}; left-value: {node.Left.Value}; right-value: {node.Right.Value}).");
        }

        if (node.Left is null ^ node.Right is null)
        {
            node.Right.Should().NotBeNull("Violation! If node has child it must be right child. Rule not satisfied. "
                + $"Node (value: {node.Value}; level {node.Level}; left-value: {node?.Left?.Value}; right-value: {node?.Right?.Value}).");
            node!.Level.Should().Be(1, "Violation! If node has only one (right) child, it's level must be 1. Rule not satisfied. "
                + $"Node (value: {node.Value}; level {node.Level}; left-value: {node?.Left?.Value}; right-value: {node?.Right?.Value}).");
            Assert.True(node!.Level == node!.Right?.Level,
                $"Violation! Single child node (value: {node!.Value}; level {node.Level}) must have same level 1 as "
                + $"it's right child (value: {node!.Right?.Value}; level {node!.Right?.Level}). Rule not satisfied.");
        }

        if (node.Left is null && node.Right is null)
        {
            node.Level.Should().Be(1, "Violation! If node has no children, it's level must be 1. Rule not satisfied. "
                + $"Node (value: {node.Value}; level {node.Level}; left-value: {node?.Left?.Value}; right-value: {node?.Right?.Value}).");
        }

        Assert.True(node?.Level > (node?.Left?.Level ?? 0),
            $"Violation! Node (value: {node?.Value}; level {node?.Level}) must have greater level than "
            + $"it's left child (value: {node?.Left?.Value}; level {node?.Left?.Level ?? 0}). Rule not satisfied.");

        Assert.True(node?.Level >= (node?.Right?.Level ?? 0),
            $"Violation! Node (value: {node?.Value}; level {node?.Level}) must have greater or equal level than "
            + $"it's right child (value: {node?.Right?.Value}; level {node?.Right?.Level ?? 0}). Rule not satisfied.");

        Assert.True(!(node?.Level == (node?.Right?.Level ?? 0) && node?.Level == (node?.Right?.Right?.Level ?? 0)),
            $"Violation! Cannot have more than 2 nodes on the same level. Rule not satisfied. "
            + $"Node (value: {node?.Value}), node->right (value: {node?.Right?.Value}), "
            + $"node->right->right (value: {node?.Right?.Right?.Value}).");
    }
}