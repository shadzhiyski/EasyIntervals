namespace Intervals.Tools;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents intersection type between two intervals.
/// </summary>
public enum IntersectionType
{
    /// <summary>
    /// Given interval intersects in any way another interval.
    /// </summary>
    Any,

    /// <summary>
    /// Given interval covers another interval.
    /// </summary>
    Cover,

    /// <summary>
    /// Given interval is within another interval.
    /// </summary>
    Within,
}

/// <summary>
/// IntervalSet is a collection for storing large amount of unique intervals
/// where multiple add, remove and search operations can be done in efficient time.
/// </summary>
/// <remarks>
/// It's an implementation of <see href="https://en.wikipedia.org/wiki/Interval_tree#Augmented_tree">Augmented Interval Tree</see>
/// abstract data structure, using self-balancing Binary Search Tree (BST) - <see href="https://en.wikipedia.org/wiki/AA_tree">AA Tree</see>.
/// It provides functionalities for add, remove, intersect, except, union and merge of intervals.
/// </remarks>
/// <typeparam name="TLimit">Represents the limit type of start and end of interval</typeparam>
public class IntervalSet<TLimit> : ISet<Interval<TLimit>>
{
    private readonly AATree<Interval<TLimit>> _aaTree;
    private readonly IComparer<TLimit> _limitComparer;
    private readonly IComparer<Interval<TLimit>> _comparer;

    /// <summary>
    /// Creates an empty IntervalSet.
    /// </summary>
    public IntervalSet()
        : this(Enumerable.Empty<Interval<TLimit>>(), Comparer<TLimit>.Default)
    { }

    /// <summary>
    /// Creates an IntervalSet with limit <c>comparison</c>.
    /// </summary>
    /// <param name="comparison">comparison</param>
    public IntervalSet(Comparison<TLimit> comparison)
        : this(Enumerable.Empty<Interval<TLimit>>(), Comparer<TLimit>.Create(comparison))
    { }

    /// <summary>
    /// Creates an IntervalSet with limit <c>comparer</c>.
    /// </summary>
    /// <param name="comparer">comparer</param>
    public IntervalSet(IComparer<TLimit> comparer)
        : this(Enumerable.Empty<Interval<TLimit>>(), comparer)
    { }

    /// <summary>
    /// Creates IntervalSet with <c>intervals</c>.
    /// </summary>
    /// <param name="intervals">intervals</param>
    public IntervalSet(IEnumerable<Interval<TLimit>> intervals)
        : this(intervals, Comparer<TLimit>.Default)
    { }

    /// <summary>
    /// Creates IntervalSet with limit <c>comparer</c> and <c>intervals</c>.
    /// </summary>
    /// <param name="comparer">comparer</param>
    /// <param name="intervals">intervals</param>
    public IntervalSet(IEnumerable<Interval<TLimit>> intervals, IComparer<TLimit> comparer)
        : this(intervals, areIntervalsSorted: false, areIntervalsUnique: false, comparer)
    { }

    /// <summary>
    /// Creates IntervalSet with limit <c>comparer</c>, <c>intervals</c> and flag if intervals are sorted.
    /// </summary>
    /// <param name="comparer">comparer</param>
    /// <param name="intervals">intervals</param>
    private IntervalSet(
        IEnumerable<Interval<TLimit>> intervals, bool areIntervalsSorted, bool areIntervalsUnique, IComparer<TLimit> comparer)
    {
        _limitComparer = comparer;
        _comparer = IntervalComparer<TLimit>.Create(comparer);

        _aaTree = new AATree<Interval<TLimit>>(
            intervals,
            areIntervalsSorted,
            areIntervalsUnique,
            _comparer,
            OnChildChanged);
    }

    private void OnChildChanged(AATree<Interval<TLimit>>.Node parent)
    {
        var parentValue = parent.Value;
        parentValue.MaxEnd = GetMaxValue(in parentValue, parent.Left, parent.Right);
        parent.Value = parentValue;
    }

    private TLimit GetMaxValue(
        in Interval<TLimit> nodeValue, AATree<Interval<TLimit>>.Node? leftNode, AATree<Interval<TLimit>>.Node? rightNode)
    {
        var isLeftNull = leftNode is null;
        var isRightNull = rightNode is null;
        if (!isLeftNull && isRightNull)
        {
            var comparison = _limitComparer.Compare(nodeValue.End, leftNode!.Value.MaxEnd);
            return comparison > 0 ? nodeValue.End : leftNode!.Value.MaxEnd;
        }

        if (isLeftNull && !isRightNull)
        {
            var comparison = _limitComparer.Compare(nodeValue.End, rightNode!.Value.MaxEnd);
            return comparison > 0 ? nodeValue.End : rightNode!.Value.MaxEnd;
        }

        if (isLeftNull && isRightNull)
        {
            return nodeValue.End;
        }

        var leftRightComparison = _limitComparer.Compare(leftNode!.Value.MaxEnd, rightNode!.Value.MaxEnd);
        var childMaxEnd = leftRightComparison > 0 ? leftNode!.Value.MaxEnd : rightNode!.Value.MaxEnd;

        var maxChildComparison = _limitComparer.Compare(nodeValue.End, childMaxEnd);
        return maxChildComparison > 0 ? nodeValue.End : childMaxEnd;
    }

    /// <summary>
    /// Count of intervals.
    /// </summary>
    public int Count => _aaTree.Count;

    public bool IsReadOnly => false;

    /// <summary>
    /// Adds <c>interval</c>.
    /// </summary>
    /// <param name="interval">interval</param>
    void ICollection<Interval<TLimit>>.Add(Interval<TLimit> interval) => Add(interval);

    /// <summary>
    /// Adds <c>interval</c>.
    /// </summary>
    /// <param name="interval">interval</param>
    /// <returns>true if interval is successfully added; otherwise, false.</returns>
    public bool Add(Interval<TLimit> interval) => _aaTree.Add(interval);

    /// <summary>
    /// Adds collection of <c>intervals</c>.
    /// </summary>
    /// <param name="intervals">intervals</param>
    public void AddRange(IEnumerable<Interval<TLimit>> intervals)
    {
        var otherCount = intervals.Count();
        if (otherCount < Count / 2)
        {
            foreach (var interval in intervals)
            {
                Add(interval);
            }

            return;
        }

        var orderedOther = intervals.ToArray();
        Array.Sort(orderedOther, _comparer);
        var otherUniqueCount = AATreeTools.ShiftUniqueElementsToBeginning(orderedOther, _comparer);

        var unionIntervals = UnionSortedIntervals(this, orderedOther.Take(otherUniqueCount), _comparer);
        _aaTree.Reset(unionIntervals, areElementsSorted: true, areElementsUnique: true);
    }

    /// <summary>
    /// Checks if <c>interval</c> is present.
    /// </summary>
    /// <param name="interval"></param>
    /// <returns>true if contains interval; otherwise, false.</returns>
    public bool Contains(Interval<TLimit> interval) => _aaTree.Contains(interval);

    /// <summary>
    /// Removes <c>interval</c>.
    /// </summary>
    /// <param name="interval"></param>
    /// <returns>true if interval is successfully removed; otherwise, false.</returns>
    public bool Remove(Interval<TLimit> interval)
    {
        return _aaTree.Remove(interval);
    }

    /// <summary>
    /// Removes <c>intervals</c>.
    /// </summary>
    /// <param name="intervals"></param>
    /// <returns>true if all intervals are successfully removed; If any interval is not removed, false.</returns>
    public bool Remove(IEnumerable<Interval<TLimit>> intervals) => intervals
        .Select(item => Remove(item))
        .All(result => result);

    /// <summary>
    /// Removes intervals intersecting <c>limit</c>.
    /// </summary>
    /// <param name="limit">Limit</param>
    /// <returns>true if any intervals are intersected and removed; otherwise, false.</returns>
    public bool Remove(TLimit limit)
    {
        var intervalsToRemove = Intersect(limit);
        return intervalsToRemove.Select(itv => Remove(itv)).Any();
    }

    /// <summary>
    /// Intersects the set with <c>interval</c> by <c>intersectionType</c>.
    /// </summary>
    /// <param name="interval">interval</param>
    /// <param name="intersectionType">intersectionType</param>
    /// <returns>interval set with intersected intervals.</returns>
    public IntervalSet<TLimit> Intersect(
        Interval<TLimit> interval, IntersectionType intersectionType = IntersectionType.Any)
    {
        var intersectedIntervals = new List<Interval<TLimit>>();
        IntersectRecursive(_aaTree.Root, interval, intersectionType, intersectedIntervals);
        return new IntervalSet<TLimit>(intersectedIntervals, areIntervalsSorted: true, areIntervalsUnique: true, _limitComparer);
    }

    /// <summary>
    /// Intersects the set with <c>limit</c>.
    /// </summary>
    /// <param name="limit"></param>
    /// <returns>interval set with intersected intervals.</returns>
    public IntervalSet<TLimit> Intersect(TLimit limit) =>
        Intersect((limit, limit, IntervalType.Closed), IntersectionType.Any);

    private void IntersectRecursive(AATree<Interval<TLimit>>.Node? node,
        in Interval<TLimit> interval, IntersectionType intersectionType, IList<Interval<TLimit>> result)
    {
        if (node is null
            || _limitComparer.Compare(interval.Start, node.Value.MaxEnd) > 0)
        {
            return;
        }

        IntersectRecursive(node.Left, interval, intersectionType, result);

        if ((intersectionType == IntersectionType.Any && IntervalTools.HasAnyIntersection(interval, node!.Value, _limitComparer))
            || (intersectionType == IntersectionType.Cover && IntervalTools.Covers(interval, node!.Value, _limitComparer))
            || (intersectionType == IntersectionType.Within && IntervalTools.Covers(node!.Value, interval, _limitComparer)))
        {
            result.Add(node.Value);
        }

        var endStartComparison = _limitComparer.Compare(interval.End, node!.Value.Start);
        if (endStartComparison >= 0)
        {
            IntersectRecursive(node.Right, interval, intersectionType, result);
        }
    }

    /// <summary>
    /// Excepts the set with <c>interval</c> by <c>intersectionType</c>.
    /// </summary>
    /// <param name="interval">interval</param>
    /// <param name="intersectionType">intersectionType</param>
    /// <returns>interval set with excepted intervals.</returns>
    public IntervalSet<TLimit> Except(
        Interval<TLimit> interval, IntersectionType intersectionType = IntersectionType.Any)
    {
        var intersectedIntervals = new List<Interval<TLimit>>();
        ExceptRecursive(_aaTree.Root, interval, intersectionType, intersectedIntervals);
        return new IntervalSet<TLimit>(intersectedIntervals, areIntervalsSorted: true, areIntervalsUnique: true, _limitComparer);
    }

    private void ExceptRecursive(AATree<Interval<TLimit>>.Node? node,
        in Interval<TLimit> interval, IntersectionType intersectionType, IList<Interval<TLimit>> result)
    {
        if (node is null)
        {
            return;
        }

        ExceptRecursive(node.Left, interval, intersectionType, result);

        if (!((intersectionType == IntersectionType.Any && IntervalTools.HasAnyIntersection(interval, node!.Value, _limitComparer))
            || (intersectionType == IntersectionType.Cover && IntervalTools.Covers(interval, node!.Value, _limitComparer))
            || (intersectionType == IntersectionType.Within && IntervalTools.Covers(node!.Value, interval, _limitComparer))))
        {
            result.Add(node.Value);
        }

        var startComparison = _limitComparer.Compare(interval.Start, node!.Value.Start);
        if (node!.Right is not null
            && !(intersectionType != IntersectionType.Within
                && startComparison <= 0
                && _limitComparer.Compare(interval.End, node!.Right.Value.MaxEnd) >= 0))
        {
            ExceptRecursive(node.Right, interval, intersectionType, result);
        }
    }

    /// <summary>
    /// Unions all unique intervals from the current and <c>other</c> interval set.
    /// </summary>
    /// <param name="other"></param>
    /// <returns>interval set with united intervals.</returns>
    public IntervalSet<TLimit> Union(IntervalSet<TLimit> other)
    {
        if (_limitComparer.GetType() != other._limitComparer.GetType())
        {
            throw new ArgumentException("Comparers types differ.");
        }

        var otherCount = other.Count;
        if (otherCount < Count / 2)
        {
            var result = new IntervalSet<TLimit>(this, areIntervalsSorted: true, areIntervalsUnique: true, _limitComparer);
            result.AddRange(other);
            return result;
        }

        var unionIntervals = UnionSortedIntervals(this, other, _comparer);
        return new IntervalSet<TLimit>(unionIntervals, areIntervalsSorted: true, areIntervalsUnique: true, _limitComparer);
    }

    /// <summary>
    /// Merges intersecting intervals.
    /// </summary>
    /// <returns>interval set with merged intervals.</returns>
    public IntervalSet<TLimit> Merge()
    {
        var result = new List<Interval<TLimit>>();
        Merge(_aaTree.Root, result);

        return new IntervalSet<TLimit>(result, areIntervalsSorted: true, areIntervalsUnique: true, _limitComparer);
    }

    private void Merge(
        AATree<Interval<TLimit>>.Node? node, IList<Interval<TLimit>> intervals)
    {
        if (node is null)
        {
            return;
        }

        Merge(node.Left, intervals);

        MergeCurrent(node, intervals);

        Merge(node.Right, intervals);
    }

    private void MergeCurrent(AATree<Interval<TLimit>>.Node? node, IList<Interval<TLimit>> intervals)
    {
        if (intervals.Count > 0)
        {
            var lastIndex = intervals.Count - 1;
            var precedingInterval = intervals[lastIndex];
            var isMerged = TryMerge(precedingInterval, node!.Value, out Interval<TLimit> mergedInterval);

            if (isMerged)
            {
                intervals[lastIndex] = mergedInterval;
                return;
            }
        }

        intervals.Add(node!.Value);
    }

    private bool TryMerge(
        in Interval<TLimit> precedingInterval, in Interval<TLimit> followingInterval, out Interval<TLimit> result)
    {
        if (IntervalTools.HasAnyIntersection(precedingInterval, followingInterval, _limitComparer)
                || IntervalTools.Touch(precedingInterval, followingInterval, _limitComparer))
        {
            result = IntervalTools.Merge(precedingInterval, followingInterval, _limitComparer);
            return true;
        }

        result = default!;
        return false;
    }

    public IEnumerator<Interval<TLimit>> GetEnumerator() => _aaTree.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Clear() => _aaTree.Clear();

    public void CopyTo(Interval<TLimit>[] array, int arrayIndex)
    {
        foreach (var interval in this)
        {
            array[arrayIndex++] = interval;
        }
    }

    public void ExceptWith(IEnumerable<Interval<TLimit>> other) => Remove(other);

    public void IntersectWith(IEnumerable<Interval<TLimit>> other) => _aaTree
        .Reset(
            other.Where(interval => Contains(interval))
        );

    public bool IsProperSubsetOf(IEnumerable<Interval<TLimit>> other)
    {
        if (Count > other.Count())
        {
            return false;
        }

        var matchesCount = 0;
        foreach (var interval in other)
        {
            if (Contains(interval))
            {
                matchesCount++;
            }
        }

        return Count == matchesCount;
    }

    public bool IsProperSupersetOf(IEnumerable<Interval<TLimit>> other) =>
        Count >= other.Count() && !other.Any(interval => !Contains(interval));

    public bool IsSubsetOf(IEnumerable<Interval<TLimit>> other) => IsProperSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<Interval<TLimit>> other) => IsProperSupersetOf(other);

    public bool Overlaps(IEnumerable<Interval<TLimit>> other) => other.Any(interval => Contains(interval));

    public bool SetEquals(IEnumerable<Interval<TLimit>> other) =>
        Count == other.Count() && !other.Any(interval => !Contains(interval));

    public void SymmetricExceptWith(IEnumerable<Interval<TLimit>> other) => ExceptWith(other);

    public void UnionWith(IEnumerable<Interval<TLimit>> other) => AddRange(other);

    private static IEnumerable<Interval<TLimit>> UnionSortedIntervals(
        IEnumerable<Interval<TLimit>> current,
        IEnumerable<Interval<TLimit>> other,
        IComparer<Interval<TLimit>> intervalComparer)
    {
        Interval<TLimit>? GetNextIntervalOrNull(IEnumerator<Interval<TLimit>> enumerator) =>
            enumerator.MoveNext()
                ? enumerator.Current
                : null;

        var currentEnumerator = current.GetEnumerator();
        var otherEnumerator = other.GetEnumerator();
        var currentInterval = GetNextIntervalOrNull(currentEnumerator);
        var otherInterval = GetNextIntervalOrNull(otherEnumerator);

        var currentCount = current.Count();
        var otherCount = other.Count();
        var unionIntervalsCount = otherCount + currentCount;
        var unionIntervals = new List<Interval<TLimit>>(unionIntervalsCount);
        for (int i = 0; i < unionIntervalsCount; i++)
        {
            if (currentInterval is null)
            {
                unionIntervals.Add(otherInterval!.Value);
                otherInterval = GetNextIntervalOrNull(otherEnumerator);
                continue;
            }

            if (otherInterval is null)
            {
                unionIntervals.Add(currentInterval!.Value);
                currentInterval = GetNextIntervalOrNull(currentEnumerator);
                continue;
            }

            var currentOtherComparison = intervalComparer.Compare(currentInterval!.Value, otherInterval!.Value);
            if (currentOtherComparison < 0)
            {
                unionIntervals.Add(currentInterval!.Value);
                currentInterval = GetNextIntervalOrNull(currentEnumerator);
            }
            else if (currentOtherComparison > 0)
            {
                unionIntervals.Add(otherInterval!.Value);
                otherInterval = GetNextIntervalOrNull(otherEnumerator);
            }
            else
            {
                unionIntervals.Add(currentInterval!.Value);
                currentInterval = GetNextIntervalOrNull(currentEnumerator);
                otherInterval = GetNextIntervalOrNull(otherEnumerator);

                unionIntervalsCount--;
            }
        }

        return unionIntervals;
    }
}
