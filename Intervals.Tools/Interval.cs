namespace Intervals.Tools;

/// <summary>
/// Type of interval.
/// </summary>
public enum IntervalType
{
    Open = 0,
    StartClosed = 1,
    EndClosed = 2,
    Closed = StartClosed | EndClosed,
}

/// <summary>
/// Interval.
/// </summary>
public struct Interval<TLimit> : IEquatable<Interval<TLimit>>
{
    public Interval(TLimit start, TLimit end, IntervalType type = IntervalType.Open, IComparer<TLimit>? comparer = null)
    {
        comparer ??= Comparer<TLimit>.Default;
        if (comparer.Compare(start, end) > 0)
        {
            throw new ArgumentException("Start must not be greater than end.");
        }

        Start = start;
        End = end;
        Type = type;
        MaxEnd = End;
    }

    public TLimit Start { get; init; }

    public TLimit End { get; init; }

    public IntervalType Type { get; init; }

    internal TLimit MaxEnd { get; set; }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End, Type);
    }

    public override bool Equals(object? obj)
    {
        if (obj is Interval<TLimit> other)
        {
            return Equals(other);
        }

        return false;
    }

    public bool Equals(Interval<TLimit> other)
    {
        var result = Type.Equals(other!.Type)
            && (Start?.Equals(other.Start) ?? other.Start is null)
            && (End?.Equals(other.End) ?? other.End is null);
        if (!result)
        {
            return false;
        }

        return result;
    }

    public override string ToString()
    {
        var startBracket = (Type & IntervalType.StartClosed) == IntervalType.StartClosed ? '[' : '(';
        var endBracket = (Type & IntervalType.EndClosed) == IntervalType.EndClosed ? ']' : ')';
        return $"{startBracket}{Start}, {End}{endBracket}";
    }

    public static implicit operator Interval<TLimit>((TLimit Start, TLimit End) interval) =>
        new Interval<TLimit>(interval.Start, interval.End);

    public static implicit operator Interval<TLimit>((TLimit Start, TLimit End, IntervalType Type) interval) =>
        new Interval<TLimit>(interval.Start, interval.End, interval.Type);
}