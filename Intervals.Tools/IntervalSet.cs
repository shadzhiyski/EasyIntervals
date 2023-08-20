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
/// It's using self-balancing Binary Search Tree (BST) - AA Tree.
/// </summary>
/// <remarks>
/// It provides functionalities for add, remove, intersect, except, union and merge of intervals.
/// </remarks>
/// <typeparam name="TLimit">Represents the limit type of start and end of interval</typeparam>
public class IntervalSet<TLimit> : ICollection<Interval<TLimit>>
{
    private IComparer<TLimit> _comparer;
    private AATree<Interval<TLimit>> _aaTree;

    /// <summary>
    /// Creates an empty IntervalSet.
    /// </summary>
    public IntervalSet()
        : this(Comparer<TLimit>.Default, Enumerable.Empty<Interval<TLimit>>())
    { }

    /// <summary>
    /// Creates an IntervalSet with comparer.
    /// </summary>
    /// <param name="comparer">comparer</param>
    public IntervalSet(IComparer<TLimit> comparer)
        : this(comparer, Enumerable.Empty<Interval<TLimit>>())
    { }

    /// <summary>
    /// Creates IntervalSet with given intervals.
    /// </summary>
    /// <param name="elements">intervals</param>
    public IntervalSet(IEnumerable<Interval<TLimit>> intervals)
        : this(Comparer<TLimit>.Default, intervals)
    { }

    /// <summary>
    /// Creates IntervalSet with given comparer and intervals.
    /// </summary>
    /// <param name="elements">intervals</param>
    /// <param name="comparer">comparer</param>
    public IntervalSet(IComparer<TLimit> comparer, IEnumerable<Interval<TLimit>> intervals)
    {
        _comparer = comparer;

        _aaTree = new AATree<Interval<TLimit>>(
            IntervalComparer<TLimit>.Create(comparer),
            intervals,
            (parent) =>
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
            });
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
        var queue = new Queue<AATree<Interval<TLimit>>.Node?>();
        queue.Enqueue(_aaTree.Root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if ((intersectionType == IntersectionType.Any && IntersectAny(interval, current!.Value))
                || (intersectionType == IntersectionType.Cover && IsCovering(interval, current!.Value))
                || (intersectionType == IntersectionType.Within && IsCovering(current!.Value, interval)))
            {
                intersectedIntervals.Add(current.Value);
            }

            if (current!.Left is not null
                && _comparer.Compare(interval.Start, current!.Left.Value.MaxEnd) <= 0)
            {
                queue.Enqueue(current.Left);
            }

            var startComparison = _comparer.Compare(interval.End, current!.Value.Start);
            if (current!.Right is not null
                && startComparison >= 0
                && _comparer.Compare(interval.Start, current!.Right.Value.MaxEnd) <= 0)
            {
                queue.Enqueue(current.Right);
            }
        }

        return new IntervalSet<TLimit>(_comparer, intersectedIntervals);
    }

    /// <summary>
    /// Intersects the set with given limit.
    /// </summary>
    /// <param name="limit"></param>
    /// <returns>intersected intervals.</returns>
    public IntervalSet<TLimit> Intersect(TLimit limit) =>
        Intersect((limit, limit, IntervalType.Closed), IntersectionType.Any);

    private bool IsCovering(Interval<TLimit> interval, Interval<TLimit> intervalToBeCovered)
    {
        var startsComparison = _comparer.Compare(interval.Start, intervalToBeCovered.Start);
        var endsComparison = _comparer.Compare(interval.End, intervalToBeCovered.End);
        return (startsComparison < 0
                || (startsComparison == 0 && (interval.Type & IntervalType.StartClosed) <= (intervalToBeCovered.Type & IntervalType.StartClosed)))
            && (endsComparison > 0
                || (endsComparison == 0 && (interval.Type & IntervalType.EndClosed) >= (intervalToBeCovered.Type & IntervalType.EndClosed)));
    }

    private bool IntersectAny(Interval<TLimit> interval1, Interval<TLimit> interval2)
    {
        var startEndComparison = _comparer.Compare(interval1.Start, interval2.End);
        var endStartComparison = _comparer.Compare(interval1.End, interval2.Start);
        if (startEndComparison > 0 || endStartComparison < 0)
        {
            return false;
        }

        var overlapsStartEnd = startEndComparison < 0
            || ((interval1.Type & IntervalType.StartClosed) > 0
                && (interval2.Type & IntervalType.EndClosed) > 0
                && startEndComparison == 0);
        var overlapsEndStart = endStartComparison > 0
            || ((interval1.Type & IntervalType.EndClosed) > 0
                && (interval2.Type & IntervalType.StartClosed) > 0
                && endStartComparison == 0);
        return overlapsStartEnd && overlapsEndStart;
    }

    private bool Touch(Interval<TLimit> before, Interval<TLimit> after) =>
        _comparer.Compare(before.End, after.Start) == 0
            && ((before.Type & IntervalType.EndClosed) | (after.Type & IntervalType.StartClosed)) > 0;


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
        var result = new IntervalSet<TLimit>(_comparer, this);
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

        return new IntervalSet<TLimit>(_comparer, result);
    }

    private void Merge(
        AATree<Interval<TLimit>>.Node? node, IList<Interval<TLimit>> intervals)
    {
        if (node is null)
        {
            return;
        }

        if (node.Left is null && node.Right is null)
        {
            MergeCurrent(node, intervals);
            return;
        }

        Merge(node.Left, intervals);

        MergeCurrent(node, intervals);

        Merge(node.Right, intervals);
    }

    private void MergeCurrent(AATree<Interval<TLimit>>.Node? node, IList<Interval<TLimit>> intervals)
    {
        var lastIndex = intervals.Count - 1;
        Interval<TLimit>? left = intervals.Count > 0 ? intervals[lastIndex] : default;
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
            && (IntersectAny(before.Value, after!.Value) || Touch(before.Value, after!.Value)))
        {
            result = MergeIntervals(before.Value, after!.Value);
            return true;
        }

        result = default!;
        return false;
    }

    private Interval<TLimit> MergeIntervals(Interval<TLimit> before, Interval<TLimit> after)
    {
        var startComparison = _comparer.Compare(after.Start, before.Start);
        var startIntervalType = startComparison == 0
            ? (after.Type | before.Type) & IntervalType.StartClosed
            : before.Type & IntervalType.StartClosed;
        var endComparison = _comparer.Compare(after.End, before.End);
        var endIntervalType = endComparison > 0
            ? after.Type & IntervalType.EndClosed
            : endComparison < 0
                ? before.Type & IntervalType.EndClosed
                : (after.Type | before.Type) & IntervalType.EndClosed;
        return (
            before.Start,
            endComparison > 0 ? after.End : before.End,
            startIntervalType | endIntervalType
        );
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