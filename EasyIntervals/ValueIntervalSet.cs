namespace EasyIntervals;

using System.Collections.Generic;
using System.Linq;

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
/// <typeparam name="TValue">Represents the value type of the value of interval</typeparam>
public class IntervalSet<TLimit, TValue> : BaseIntervalSet<IntervalSet<TLimit, TValue>, Interval<TLimit, TValue>, TLimit>
{

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
    /// Creates IntervalSet with limit <c>comparer</c> and <c>intervals</c>.
    /// </summary>
    /// <param name="comparer">comparer</param>
    /// <param name="intervals">intervals</param>
    public IntervalSet(IEnumerable<Interval<TLimit, TValue>> intervals, IComparer<TLimit> comparer)
        : this(intervals, areIntervalsSorted: false, areIntervalsUnique: false, comparer)
    { }

    /// <summary>
    /// Creates IntervalSet with limit <c>comparer</c>, <c>intervals</c> and flag if intervals are sorted.
    /// </summary>
    /// <param name="comparer">comparer</param>
    /// <param name="intervals">intervals</param>
    private IntervalSet(
        IEnumerable<Interval<TLimit, TValue>> intervals, bool areIntervalsSorted, bool areIntervalsUnique, IComparer<TLimit> comparer)
        : base(intervals, areIntervalsSorted, areIntervalsUnique, IntervalComparer<TLimit, TValue>.Create(comparer), comparer)
    { }

    protected override IntervalSet<TLimit, TValue> CreateInstance(
        IEnumerable<Interval<TLimit, TValue>> intervals,
        bool areIntervalsSorted,
        bool areIntervalsUnique,
        IComparer<TLimit> limitComparer) => new IntervalSet<TLimit, TValue>(intervals, areIntervalsSorted: true, areIntervalsUnique: true, limitComparer);

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
    /// Intersects the set with <c>limit</c>.
    /// </summary>
    /// <param name="limit"></param>
    /// <returns>interval set with intersected intervals.</returns>
    public IntervalSet<TLimit, TValue> Intersect(TLimit limit) =>
        Intersect((limit, limit, default, IntervalType.Closed), IntersectionType.Any);

    /// <summary>
    /// Merges intersecting intervals.
    /// </summary>
    /// <returns>interval set with merged intervals.</returns>
    public IntervalSet<TLimit, TValue> Merge(Func<Interval<TLimit, TValue>, Interval<TLimit, TValue>, TValue> mergeFunction)
    {
        var result = new List<Interval<TLimit, TValue>>();
        Merge(_aaTree.Root, result, mergeFunction);

        return new IntervalSet<TLimit, TValue>(result, areIntervalsSorted: true, areIntervalsUnique: true, _limitComparer);
    }

    private void Merge(
        AATree<Interval<TLimit, TValue>>.Node? node,
        IList<Interval<TLimit, TValue>> intervals,
        Func<Interval<TLimit, TValue>, Interval<TLimit, TValue>, TValue> mergeFunction)
    {
        if (node is null)
        {
            return;
        }

        Merge(node.Left, intervals, mergeFunction);

        MergeCurrent(node, intervals, mergeFunction);

        Merge(node.Right, intervals, mergeFunction);
    }

    private void MergeCurrent(
        AATree<Interval<TLimit, TValue>>.Node? node,
        IList<Interval<TLimit, TValue>> intervals,
        Func<Interval<TLimit, TValue>, Interval<TLimit, TValue>, TValue> mergeFunction)
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
        Func<Interval<TLimit, TValue>, Interval<TLimit, TValue>, TValue> mergeFunction,
        out Interval<TLimit, TValue> result)
    {
        if (IntervalTools.HasAnyIntersection(precedingInterval, followingInterval, _limitComparer)
                || IntervalTools.Touch(precedingInterval, followingInterval, _limitComparer))
        {
            result = IntervalTools.Merge(precedingInterval, followingInterval, mergeFunction, _limitComparer);
            return true;
        }

        result = default!;
        return false;
    }
}
