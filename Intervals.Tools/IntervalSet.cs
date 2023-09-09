namespace Intervals.Tools;

using System;
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
/// IntervalSet is a collection for storing unique intervals where multiple add, remove and search operations can be done in efficient time.
/// </summary>
/// <remarks>
/// It's an implementation of Augmented Interval Tree abstract data structure,
/// using self-balancing Binary Search Tree (BST) - AA Tree.
/// It provides functionalities for add, remove, intersect, except, union and merge of intervals.
/// </remarks>
/// <seealso href="https://en.wikipedia.org/wiki/Interval_tree">Interval Tree</seealso>
/// <seealso href="https://en.wikipedia.org/wiki/Interval_tree#Augmented_tree">Augmented Interval Tree</seealso>
/// <seealso href="https://en.wikipedia.org/wiki/AA_tree">AA Tree</seealso>
/// <typeparam name="TLimit">Represents the limit type of start and end of interval</typeparam>
public class IntervalSet<TLimit> : ICollection<Interval<TLimit>>
{
    private IComparer<TLimit> _comparer;
    private AATree<Interval<TLimit>> _aaTree;

    /// <summary>
    /// Creates an empty IntervalSet.
    /// </summary>
    public IntervalSet()
        : this(Comparer<TLimit>.Default, Enumerable.Empty<Interval<TLimit>>().ToHashSet())
    { }

    /// <summary>
    /// Creates an IntervalSet with comparer.
    /// </summary>
    /// <param name="comparer">comparer</param>
    public IntervalSet(IComparer<TLimit> comparer)
        : this(comparer, Enumerable.Empty<Interval<TLimit>>().ToHashSet())
    { }

    /// <summary>
    /// Creates IntervalSet with given intervals.
    /// </summary>
    /// <param name="intervals">intervals</param>
    public IntervalSet(ISet<Interval<TLimit>> intervals)
        : this(Comparer<TLimit>.Default, intervals)
    { }

    /// <summary>
    /// Creates IntervalSet with given comparer and intervals.
    /// </summary>
    /// <param name="comparer">comparer</param>
    /// <param name="intervals">intervals</param>
    public IntervalSet(IComparer<TLimit> comparer, ISet<Interval<TLimit>> intervals)
        : this(comparer, intervals, areIntervalsSorted: false)
    { }

    /// <summary>
    /// Creates IntervalSet with given comparer and intervals and flag if intervals are sorted.
    /// </summary>
    /// <param name="comparer">comparer</param>
    /// <param name="intervals">intervals</param>
    private IntervalSet(IComparer<TLimit> comparer, IEnumerable<Interval<TLimit>> intervals, bool areIntervalsSorted)
    {
        _comparer = comparer;

        _aaTree = new AATree<Interval<TLimit>>(
            IntervalComparer<TLimit>.Create(comparer),
            intervals,
            areIntervalsSorted,
            OnChildChanged);
    }

    private void OnChildChanged(AATree<Interval<TLimit>>.Node parent)
    {
        var isLeftNull = parent.Left is null;
        var isRightNull = parent.Right is null;
        var parentValue = parent.Value;
        if (!isLeftNull && isRightNull)
        {
            var comparison = _comparer.Compare(parent.Value.End, parent.Left!.Value.MaxEnd);
            parentValue.MaxEnd = comparison > 0 ? parent.Value.End : parent.Left!.Value.MaxEnd;
            parent.Value = parentValue;
            return;
        }

        if (isLeftNull && !isRightNull)
        {
            var comparison = _comparer.Compare(parent.Value.End, parent.Right!.Value.MaxEnd);
            parentValue.MaxEnd = comparison > 0 ? parent.Value.End : parent.Right!.Value.MaxEnd;
            parent.Value = parentValue;
            return;
        }

        if (isLeftNull && isRightNull)
        {
            parentValue.MaxEnd = parent.Value.End;
            parent.Value = parentValue;
            return;
        }

        var leftRightComparison = _comparer.Compare(parent.Left!.Value.MaxEnd, parent.Right!.Value.MaxEnd);
        var childMaxEnd = leftRightComparison > 0 ? parent.Left!.Value.MaxEnd : parent.Right!.Value.MaxEnd;

        var maxChildComparison = _comparer.Compare(parent.Value.End, childMaxEnd);
        parentValue.MaxEnd = maxChildComparison > 0 ? parent.Value.End : childMaxEnd;
        parent.Value = parentValue;
    }

    /// <summary>
    /// Count of intervals.
    /// </summary>
    public int Count => _aaTree.Count;

    public bool IsReadOnly => false;

    /// <summary>
    /// Adds interval.
    /// </summary>
    /// <param name="item">item</param>
    void ICollection<Interval<TLimit>>.Add(Interval<TLimit> item) => Add(item);

    /// <summary>
    /// Adds interval.
    /// </summary>
    /// <param name="item"></param>
    /// <returns>true if interval is successfully added; otherwise, false.</returns>
    public bool Add(Interval<TLimit> item) => _aaTree.Add(item);

    /// <summary>
    /// Adds collection of intervals.
    /// </summary>
    /// <param name="intervals">intervals</param>
    public void AddRange(IEnumerable<Interval<TLimit>> intervals)
    {
        foreach (var interval in intervals)
        {
            Add(interval);
        }
    }

    /// <summary>
    /// Checks if an interval is present.
    /// </summary>
    /// <param name="item"></param>
    /// <returns>true if contains interval; otherwise, false.</returns>
    public bool Contains(Interval<TLimit> item) => _aaTree.Contains(item);

    /// <summary>
    /// Removes interval.
    /// </summary>
    /// <param name="item"></param>
    /// <returns>true if interval is successfully removed; otherwise, false.</returns>
    public bool Remove(Interval<TLimit> item)
    {
        return _aaTree.Remove(item);
    }

    /// <summary>
    /// Removes sequence of intervals.
    /// </summary>
    /// <param name="items"></param>
    /// <returns>true if all intervals are successfully removed; If any interval is not removed, false.</returns>
    public bool Remove(IEnumerable<Interval<TLimit>> items) => items
        .Select(item => Remove(item))
        .All(result => result);

    /// <summary>
    /// Removes intervals intersecting the limit.
    /// </summary>
    /// <param name="limit">Limit</param>
    /// <returns>true if any intervals are intersected and removed; otherwise, false.</returns>
    public bool Remove(TLimit limit)
    {
        var intervalsToRemove = Intersect(limit);
        return intervalsToRemove.Select(itv => Remove(itv)).Any();
    }

    /// <summary>
    /// Intersects the set with given interval by type of intersection.
    /// </summary>
    /// <param name="interval">interval</param>
    /// <param name="intersectionType"></param>
    /// <returns>intersected intervals.</returns>
    public IntervalSet<TLimit> Intersect(
        Interval<TLimit> interval, IntersectionType intersectionType = IntersectionType.Any)
    {
        var intersectedIntervals = new List<Interval<TLimit>>();
        IntersectRecursive(_aaTree.Root, interval, intersectionType, intersectedIntervals);
        return new IntervalSet<TLimit>(_comparer, intersectedIntervals, areIntervalsSorted: true);
    }

    /// <summary>
    /// Intersects the set with given limit.
    /// </summary>
    /// <param name="limit"></param>
    /// <returns>intersected intervals.</returns>
    public IntervalSet<TLimit> Intersect(TLimit limit) =>
        Intersect((limit, limit, IntervalType.Closed), IntersectionType.Any);

    private void IntersectRecursive(AATree<Interval<TLimit>>.Node? node,
        Interval<TLimit> interval, IntersectionType intersectionType, IList<Interval<TLimit>> result)
    {
        if (node is null)
        {
            return;
        }

        if (node!.Left is not null
            && _comparer.Compare(interval.Start, node!.Left.Value.MaxEnd) <= 0)
        {
            IntersectRecursive(node.Left, interval, intersectionType, result);
        }

        if ((intersectionType == IntersectionType.Any && IntervalTools.HasAnyIntersection(interval, node!.Value, _comparer))
            || (intersectionType == IntersectionType.Cover && IntervalTools.Covers(interval, node!.Value, _comparer))
            || (intersectionType == IntersectionType.Within && IntervalTools.Covers(node!.Value, interval, _comparer)))
        {
            result.Add(node.Value);
        }

        var startComparison = _comparer.Compare(interval.End, node!.Value.Start);
        if (node!.Right is not null
            && startComparison >= 0
            && _comparer.Compare(interval.Start, node!.Right.Value.MaxEnd) <= 0)
        {
            IntersectRecursive(node.Right, interval, intersectionType, result);
        }
    }

    /// <summary>
    /// Excepts the set with given exception interval.
    /// </summary>
    /// <param name="limit"></param>
    /// <returns>excepted intervals.</returns>
    public IntervalSet<TLimit> Except(Interval<TLimit> interval, IntersectionType intersectionType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Unions all unique intervals from the current and a given IntervalSet.
    /// </summary>
    /// <param name="other"></param>
    /// <returns>interval set of united intervals.</returns>
    public IntervalSet<TLimit> Union(IntervalSet<TLimit> other)
    {
        var result = new IntervalSet<TLimit>(_comparer, this, areIntervalsSorted: true);
        result.AddRange(other);

        return result;
    }

    /// <summary>
    /// Merges intersecting intervals into single intervals.
    /// </summary>
    /// <returns>interval set with merged intervals.</returns>
    public IntervalSet<TLimit> Merge()
    {
        var result = new List<Interval<TLimit>>();
        Merge(_aaTree.Root, result);

        return new IntervalSet<TLimit>(_comparer, result, areIntervalsSorted: true);
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
        var lastIndex = intervals.Count - 1;
        Interval<TLimit>? left = intervals.Count > 0 ? intervals[lastIndex] : null;
        var isMerged = TryMerge(left, node, out Interval<TLimit> interval);

        if (isMerged)
        {
            intervals[lastIndex] = interval;
            return;
        }

        intervals.Add(node!.Value);
    }

    private bool TryMerge(
        Interval<TLimit>? before, AATree<Interval<TLimit>>.Node? after, out Interval<TLimit> result)
    {
        if (before is not null
            && (IntervalTools.HasAnyIntersection(before.Value, after!.Value, _comparer)
                || IntervalTools.Touch(before.Value, after!.Value, _comparer)))
        {
            result = IntervalTools.Merge(before.Value, after!.Value, _comparer);
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
}
