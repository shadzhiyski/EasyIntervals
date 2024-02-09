namespace EasyIntervals;

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
/// <typeparam name="TValue">Represents the type of the value of interval</typeparam>
public class IntervalSet<TLimit, TValue> : ISet<Interval<TLimit, TValue>>
{
    private readonly AATree<Interval<TLimit, TValue>> _aaTree;
    private readonly IComparer<TLimit> _limitComparer;
    private readonly IComparer<Interval<TLimit, TValue>> _comparer;

    /// <summary>
    /// Creates an empty IntervalSet.
    /// </summary>
    public IntervalSet()
        : this(Enumerable.Empty<Interval<TLimit, TValue>>(), Comparer<TLimit>.Default)
    { }

    /// <summary>
    /// Creates an IntervalSet with limit <c>comparison</c>.
    /// </summary>
    /// <param name="comparison">comparison</param>
    public IntervalSet(Comparison<TLimit> comparison)
        : this(Enumerable.Empty<Interval<TLimit, TValue>>(), Comparer<TLimit>.Create(comparison))
    { }

    /// <summary>
    /// Creates an IntervalSet with limit <c>comparer</c>.
    /// </summary>
    /// <param name="comparer">comparer</param>
    public IntervalSet(IComparer<TLimit> comparer)
        : this(Enumerable.Empty<Interval<TLimit, TValue>>(), comparer)
    { }

    /// <summary>
    /// Creates IntervalSet with <c>intervals</c>.
    /// </summary>
    /// <param name="intervals">intervals</param>
    public IntervalSet(IEnumerable<Interval<TLimit, TValue>> intervals)
        : this(intervals, Comparer<TLimit>.Default)
    { }

    /// <summary>
    /// Creates IntervalSet with limit <c>comparison</c> and <c>intervals</c>.
    /// </summary>
    /// <param name="comparison">comparison</param>
    /// <param name="intervals">intervals</param>
    public IntervalSet(IEnumerable<Interval<TLimit, TValue>> intervals, Comparison<TLimit> comparison)
        : this(intervals, areIntervalsSorted: false, areIntervalsUnique: false, Comparer<TLimit>.Create(comparison))
    { }

    /// <summary>
    /// Creates IntervalSet with limit <c>comparer</c> and <c>intervals</c>.
    /// </summary>
    /// <param name="comparer">comparer</param>
    /// <param name="intervals">intervals</param>
    public IntervalSet(IEnumerable<Interval<TLimit, TValue>> intervals, IComparer<TLimit> comparer)
        : this(intervals, areIntervalsSorted: false, areIntervalsUnique: false, comparer)
    { }

    /// <summary>
    /// Creates IntervalSet with limit <c>comparer</c>, <c>intervals</c>,
    /// <c>areIntervalsSorted</c> and <c>areIntervalsUnique</c> flags defining if intervals are sorted or unique.
    /// </summary>
    /// <param name="comparer">comparer</param>
    /// <param name="areIntervalsSorted">areIntervalsSorted</param>
    /// <param name="areIntervalsUnique">areIntervalsUnique</param>
    /// <param name="intervals">intervals</param>
    private IntervalSet(
        IEnumerable<Interval<TLimit, TValue>> intervals, bool areIntervalsSorted, bool areIntervalsUnique, IComparer<TLimit> comparer)
    {
        if (intervals is IntervalSet<TLimit, TValue> inputIntervalSet
            && !AreEqualComparers(comparer, inputIntervalSet._limitComparer))
        {
            throw new ArgumentException("The given comparer argument must be of the same type as the comparer of the given intervals argument.");
        }

        _limitComparer = comparer;
        _comparer = IntervalComparer<TLimit, TValue>.Create(comparer);

        _aaTree = new AATree<Interval<TLimit, TValue>>(
            intervals,
            areIntervalsSorted,
            areIntervalsUnique,
            _comparer,
            OnChildChanged);
    }

    private static bool AreEqualComparers(IComparer<TLimit> comparer1, IComparer<TLimit> comparer2) => comparer1 == comparer2 || comparer1.Equals(comparer2);

    private void OnChildChanged(AATree<Interval<TLimit, TValue>>.Node parent)
    {
        var parentValue = parent.Value;
        parentValue.MaxEnd = GetMaxValue(in parentValue, parent.Left, parent.Right);
        parent.Value = parentValue;
    }

    private TLimit GetMaxValue(
        in Interval<TLimit, TValue> nodeValue, AATree<Interval<TLimit, TValue>>.Node? leftNode, AATree<Interval<TLimit, TValue>>.Node? rightNode)
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
    void ICollection<Interval<TLimit, TValue>>.Add(Interval<TLimit, TValue> interval) => Add(interval);

    /// <summary>
    /// Adds <c>interval</c>.
    /// </summary>
    /// <param name="interval">interval</param>
    /// <returns>true if interval is successfully added; otherwise, false.</returns>
    public bool Add(Interval<TLimit, TValue> interval) => _aaTree.Add(interval);

    /// <summary>
    /// Checks if <c>interval</c> is present.
    /// </summary>
    /// <param name="interval"></param>
    /// <returns>true if contains interval; otherwise, false.</returns>
    public bool Contains(Interval<TLimit, TValue> interval) => _aaTree.Contains(interval);

    /// <summary>
    /// Removes <c>interval</c>.
    /// </summary>
    /// <param name="interval"></param>
    /// <returns>true if interval is successfully removed; otherwise, false.</returns>
    public bool Remove(Interval<TLimit, TValue> interval)
    {
        return _aaTree.Remove(interval);
    }

    /// <summary>
    /// Removes <c>intervals</c>.
    /// </summary>
    /// <param name="intervals"></param>
    /// <returns>true if all intervals are successfully removed; If any interval is not removed, false.</returns>
    public bool Remove(IEnumerable<Interval<TLimit, TValue>> intervals) => intervals
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
    public IntervalSet<TLimit, TValue> Intersect(
        Interval<TLimit, TValue> interval, IntersectionType intersectionType = IntersectionType.Any)
    {
        var intersectedIntervals = new List<Interval<TLimit, TValue>>();
        IntersectRecursive(_aaTree.Root, interval, intersectionType, intersectedIntervals);
        return new IntervalSet<TLimit, TValue>(intersectedIntervals, areIntervalsSorted: true, areIntervalsUnique: true, _limitComparer);
    }

    /// <summary>
    /// Intersects the set with <c>limit</c>.
    /// </summary>
    /// <param name="limit"></param>
    /// <returns>interval set with intersected intervals.</returns>
    public IntervalSet<TLimit, TValue> Intersect(TLimit limit) =>
        Intersect((limit, limit, IntervalType.Closed), IntersectionType.Any);

    private void IntersectRecursive(AATree<Interval<TLimit, TValue>>.Node? node,
        in Interval<TLimit, TValue> interval, IntersectionType intersectionType, IList<Interval<TLimit, TValue>> result)
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
    public IntervalSet<TLimit, TValue> Except(
        Interval<TLimit, TValue> interval, IntersectionType intersectionType = IntersectionType.Any)
    {
        var intersectedIntervals = new List<Interval<TLimit, TValue>>();
        ExceptRecursive(_aaTree.Root, interval, intersectionType, intersectedIntervals);
        return new IntervalSet<TLimit, TValue>(intersectedIntervals, areIntervalsSorted: true, areIntervalsUnique: true, _limitComparer);
    }

    private void ExceptRecursive(AATree<Interval<TLimit, TValue>>.Node? node,
        in Interval<TLimit, TValue> interval, IntersectionType intersectionType, IList<Interval<TLimit, TValue>> result)
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
    public IntervalSet<TLimit, TValue> Union(IntervalSet<TLimit, TValue> other)
    {
        if (_limitComparer.GetType() != other._limitComparer.GetType())
        {
            throw new ArgumentException("Comparers types differ.");
        }

        var otherCount = other.Count;
        if (otherCount < Count / 2)
        {
            var result = new IntervalSet<TLimit, TValue>(this, areIntervalsSorted: true, areIntervalsUnique: true, _limitComparer);
            result.UnionWith(other);
            return result;
        }

        var unionIntervals = UnionSortedIntervals(this, other, _comparer);
        return new IntervalSet<TLimit, TValue>(unionIntervals, areIntervalsSorted: true, areIntervalsUnique: true, _limitComparer);
    }

    /// <summary>
    /// Merges intersecting intervals.
    /// </summary>
    /// <returns>interval set with merged intervals.</returns>
    public IntervalSet<TLimit, TValue> Merge(Func<Interval<TLimit, TValue>, Interval<TLimit, TValue>, TValue>? mergeFunction = null)
    {
        var result = new List<Interval<TLimit, TValue>>();
        Merge(_aaTree.Root, mergeFunction, result);

        return new IntervalSet<TLimit, TValue>(result, areIntervalsSorted: true, areIntervalsUnique: true, _limitComparer);
    }

    private void Merge(
        AATree<Interval<TLimit, TValue>>.Node? node,
        Func<Interval<TLimit, TValue>, Interval<TLimit, TValue>, TValue>? mergeFunction,
        IList<Interval<TLimit, TValue>> intervals)
    {
        if (node is null)
        {
            return;
        }

        Merge(node.Left, mergeFunction, intervals);

        MergeCurrent(node, mergeFunction, intervals);

        Merge(node.Right, mergeFunction, intervals);
    }

    private void MergeCurrent(
        AATree<Interval<TLimit, TValue>>.Node? node,
        Func<Interval<TLimit, TValue>, Interval<TLimit, TValue>, TValue>? mergeFunction,
        IList<Interval<TLimit, TValue>> intervals)
    {
        if (intervals.Count > 0)
        {
            var lastIndex = intervals.Count - 1;
            var precedingInterval = intervals[lastIndex];
            var isMerged = TryMerge(precedingInterval, node!.Value, mergeFunction, out Interval<TLimit, TValue> mergedInterval);

            if (isMerged)
            {
                intervals[lastIndex] = mergedInterval;
                return;
            }
        }

        intervals.Add(node!.Value);
    }

    private bool TryMerge(
        in Interval<TLimit, TValue> precedingInterval,
        in Interval<TLimit, TValue> followingInterval,
        Func<Interval<TLimit, TValue>, Interval<TLimit, TValue>, TValue>? mergeFunction,
        out Interval<TLimit, TValue> result)
    {
        if (IntervalTools.HasAnyIntersection(precedingInterval, followingInterval, _limitComparer)
                || IntervalTools.Touch(precedingInterval, followingInterval, _limitComparer))
        {
            if (mergeFunction is not null)
            {
                result = IntervalTools.Merge(precedingInterval, followingInterval, mergeFunction, _limitComparer);
            }
            else
            {
                result = IntervalTools.Merge(precedingInterval, followingInterval, _limitComparer);
            }

            return true;
        }

        result = default!;
        return false;
    }

    public IEnumerator<Interval<TLimit, TValue>> GetEnumerator() => _aaTree.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Clear() => _aaTree.Clear();

    public void CopyTo(Interval<TLimit, TValue>[] array, int arrayIndex)
    {
        foreach (var interval in this)
        {
            array[arrayIndex++] = interval;
        }
    }

    public void ExceptWith(IEnumerable<Interval<TLimit, TValue>> other) => Remove(other);

    public void IntersectWith(IEnumerable<Interval<TLimit, TValue>> other) => _aaTree
        .Reset(
            other.Where(interval => Contains(interval))
        );

    public bool IsProperSubsetOf(IEnumerable<Interval<TLimit, TValue>> other)
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

    public bool IsProperSupersetOf(IEnumerable<Interval<TLimit, TValue>> other) =>
        Count >= other.Count() && !other.Any(interval => !Contains(interval));

    public bool IsSubsetOf(IEnumerable<Interval<TLimit, TValue>> other) => IsProperSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<Interval<TLimit, TValue>> other) => IsProperSupersetOf(other);

    public bool Overlaps(IEnumerable<Interval<TLimit, TValue>> other) => other.Any(interval => Contains(interval));

    public bool SetEquals(IEnumerable<Interval<TLimit, TValue>> other) =>
        Count == other.Count() && !other.Any(interval => !Contains(interval));

    public void SymmetricExceptWith(IEnumerable<Interval<TLimit, TValue>> other) => ExceptWith(other);

    public void UnionWith(IEnumerable<Interval<TLimit, TValue>> others)
    {
        var otherCount = others.Count();
        if (otherCount < Count / 2)
        {
            foreach (var interval in others)
            {
                Add(interval);
            }

            return;
        }

        var orderedOther = others.ToArray();
        Array.Sort(orderedOther, _comparer);
        var otherUniqueCount = AATreeTools.ShiftUniqueElementsToBeginning(orderedOther, _comparer);

        var unionIntervals = UnionSortedIntervals(this, orderedOther.Take(otherUniqueCount), _comparer);
        _aaTree.Reset(unionIntervals, areElementsSorted: true, areElementsUnique: true);
    }

    private static IEnumerable<Interval<TLimit, TValue>> UnionSortedIntervals(
        IEnumerable<Interval<TLimit, TValue>> current,
        IEnumerable<Interval<TLimit, TValue>> other,
        IComparer<Interval<TLimit, TValue>> intervalComparer)
    {
        Interval<TLimit, TValue>? GetNextIntervalOrNull(IEnumerator<Interval<TLimit, TValue>> enumerator) =>
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
        var unionIntervals = new List<Interval<TLimit, TValue>>(unionIntervalsCount);
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
