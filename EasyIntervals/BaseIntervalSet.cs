namespace EasyIntervals;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class BaseIntervalSet<TIntervalSet, TInterval, TLimit> : ISet<TInterval>
    where TIntervalSet : BaseIntervalSet<TIntervalSet, TInterval, TLimit>
    where TInterval : struct, IInterval<TLimit>, INodeInterval<TLimit>
{
    internal readonly AATree<TInterval> _aaTree;
    protected readonly IComparer<TLimit> _limitComparer;
    protected readonly IComparer<TInterval> _comparer;

    public BaseIntervalSet(
        IEnumerable<TInterval> intervals,
        bool areIntervalsSorted,
        bool areIntervalsUnique,
        IComparer<TInterval> comparer,
        IComparer<TLimit> limitComparer)
    {
        _limitComparer = limitComparer;
        _comparer = comparer;

        _aaTree = new AATree<TInterval>(
            intervals,
            areIntervalsSorted,
            areIntervalsUnique,
            _comparer,
            OnChildChanged);
    }

    private void OnChildChanged(AATree<TInterval>.Node parent)
    {
        var parentValue = parent.Value;
        parentValue.MaxEnd = GetMaxValue(in parentValue, parent.Left, parent.Right);
        parent.Value = parentValue;
    }

    private TLimit GetMaxValue(
        in TInterval nodeValue, AATree<TInterval>.Node? leftNode, AATree<TInterval>.Node? rightNode)
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

    protected abstract TIntervalSet CreateInstance(
        IEnumerable<TInterval> intervals,
        bool areIntervalsSorted,
        bool areIntervalsUnique,
        IComparer<TLimit> limitComparer);

    /// <summary>
    /// Count of intervals.
    /// </summary>
    public int Count => _aaTree.Count;

    public bool IsReadOnly => false;

    /// <summary>
    /// Adds <c>interval</c>.
    /// </summary>
    /// <param name="interval">interval</param>
    void ICollection<TInterval>.Add(TInterval interval) => Add(interval);

    /// <summary>
    /// Adds <c>interval</c>.
    /// </summary>
    /// <param name="interval">interval</param>
    /// <returns>true if interval is successfully added; otherwise, false.</returns>
    public bool Add(TInterval interval) => _aaTree.Add(interval);

    /// <summary>
    /// Adds collection of <c>intervals</c>.
    /// </summary>
    /// <param name="intervals">intervals</param>
    public void AddRange(IEnumerable<TInterval> intervals)
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
    public bool Contains(TInterval interval) => _aaTree.Contains(interval);

    /// <summary>
    /// Removes <c>interval</c>.
    /// </summary>
    /// <param name="interval"></param>
    /// <returns>true if interval is successfully removed; otherwise, false.</returns>
    public bool Remove(TInterval interval)
    {
        return _aaTree.Remove(interval);
    }

    /// <summary>
    /// Removes <c>intervals</c>.
    /// </summary>
    /// <param name="intervals"></param>
    /// <returns>true if all intervals are successfully removed; If any interval is not removed, false.</returns>
    public bool Remove(IEnumerable<TInterval> intervals) => intervals
        .Select(item => Remove(item))
        .All(result => result);

    /// <summary>
    /// Intersects the set with <c>interval</c> by <c>intersectionType</c>.
    /// </summary>
    /// <param name="interval">interval</param>
    /// <param name="intersectionType">intersectionType</param>
    /// <returns>interval set with intersected intervals.</returns>
    public TIntervalSet Intersect(
        TInterval interval, IntersectionType intersectionType = IntersectionType.Any)
    {
        var intersectedIntervals = new List<TInterval>();
        IntersectRecursive(_aaTree.Root, interval, intersectionType, intersectedIntervals);
        return CreateInstance(intersectedIntervals, areIntervalsSorted: true, areIntervalsUnique: true, _limitComparer);
    }

    private void IntersectRecursive(AATree<TInterval>.Node? node,
        in TInterval interval, IntersectionType intersectionType, IList<TInterval> result)
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
    public TIntervalSet Except(
        TInterval interval, IntersectionType intersectionType = IntersectionType.Any)
    {
        var intersectedIntervals = new List<TInterval>();
        ExceptRecursive(_aaTree.Root, interval, intersectionType, intersectedIntervals);
        return CreateInstance(intersectedIntervals, areIntervalsSorted: true, areIntervalsUnique: true, _limitComparer);
    }

    private void ExceptRecursive(AATree<TInterval>.Node? node,
        in TInterval interval, IntersectionType intersectionType, IList<TInterval> result)
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
    public TIntervalSet Union(TIntervalSet other)
    {
        if (_limitComparer.GetType() != other._limitComparer.GetType())
        {
            throw new ArgumentException("Comparers types differ.");
        }

        var otherCount = other.Count;
        if (otherCount < Count / 2)
        {
            var result = CreateInstance(this, areIntervalsSorted: true, areIntervalsUnique: true, _limitComparer);
            result.AddRange(other);
            return result;
        }

        var unionIntervals = UnionSortedIntervals(this, other, _comparer);
        return CreateInstance(unionIntervals, areIntervalsSorted: true, areIntervalsUnique: true, _limitComparer);
    }

    public IEnumerator<TInterval> GetEnumerator() => _aaTree.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Clear() => _aaTree.Clear();

    public void CopyTo(TInterval[] array, int arrayIndex)
    {
        foreach (var interval in this)
        {
            array[arrayIndex++] = interval;
        }
    }

    public void ExceptWith(IEnumerable<TInterval> other) => Remove(other);

    public void IntersectWith(IEnumerable<TInterval> other) => _aaTree
        .Reset(
            other.Where(interval => Contains(interval))
        );

    public bool IsProperSubsetOf(IEnumerable<TInterval> other)
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

    public bool IsProperSupersetOf(IEnumerable<TInterval> other) =>
        Count >= other.Count() && !other.Any(interval => !Contains(interval));

    public bool IsSubsetOf(IEnumerable<TInterval> other) => IsProperSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<TInterval> other) => IsProperSupersetOf(other);

    public bool Overlaps(IEnumerable<TInterval> other) => other.Any(interval => Contains(interval));

    public bool SetEquals(IEnumerable<TInterval> other) =>
        Count == other.Count() && !other.Any(interval => !Contains(interval));

    public void SymmetricExceptWith(IEnumerable<TInterval> other) => ExceptWith(other);

    public void UnionWith(IEnumerable<TInterval> other) => AddRange(other);

    private static IEnumerable<TInterval> UnionSortedIntervals(
        IEnumerable<TInterval> current,
        IEnumerable<TInterval> other,
        IComparer<TInterval> intervalComparer)
    {
        TInterval? GetNextIntervalOrNull(IEnumerator<TInterval> enumerator) =>
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
        var unionIntervals = new List<TInterval>(unionIntervalsCount);
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