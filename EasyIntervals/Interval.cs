namespace EasyIntervals;

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

public interface IInterval<TLimit>
{
    public TLimit Start { get; }

    public TLimit End { get; }

    public IntervalType Type { get; }
}

public interface INodeInterval<TLimit>
{
    public TLimit MaxEnd { get; set; }
}

/// <summary>
/// Interval.
/// </summary>
public struct Interval<TLimit> : IInterval<TLimit>, INodeInterval<TLimit>, IEquatable<Interval<TLimit>>
{
    public Interval(TLimit start, TLimit end, IntervalType type = IntervalType.Open, IComparer<TLimit>? comparer = null)
    {
        comparer ??= Comparer<TLimit>.Default;
        var startEndComparison = comparer.Compare(start, end);
        if (startEndComparison > 0)
        {
            throw new ArgumentException("Start must not be greater than end.");
        }

        if (startEndComparison == 0 && type != IntervalType.Closed)
        {
            throw new ArgumentException("Equal limits must be combined only with Closed interval type.");
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

    TLimit INodeInterval<TLimit>.MaxEnd
    {
        get => MaxEnd;
        set => MaxEnd = value;
    }

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

    public static implicit operator Interval<TLimit>((TLimit Start, TLimit End, IComparer<TLimit> Comparer) interval) =>
        new Interval<TLimit>(interval.Start, interval.End, IntervalType.Open, interval.Comparer);

    public static implicit operator Interval<TLimit>((TLimit Start, TLimit End, IntervalType Type, IComparer<TLimit> Comparer) interval) =>
        new Interval<TLimit>(interval.Start, interval.End, interval.Type, interval.Comparer);
}

/// <summary>
/// Interval with value.
/// </summary>
public struct Interval<TLimit, TValue> : IInterval<TLimit>, INodeInterval<TLimit>, IEquatable<Interval<TLimit, TValue>>
{
    public Interval(TLimit start, TLimit end, TValue? value, IntervalType type = IntervalType.Open, IComparer<TLimit>? comparer = null)
    {
        comparer ??= Comparer<TLimit>.Default;
        var startEndComparison = comparer.Compare(start, end);
        if (startEndComparison > 0)
        {
            throw new ArgumentException("Start must not be greater than end.");
        }

        if (startEndComparison == 0 && type != IntervalType.Closed)
        {
            throw new ArgumentException("Equal limits must be combined only with Closed interval type.");
        }

        Start = start;
        End = end;
        Value = value;
        Type = type;
        MaxEnd = End;
    }

    public TLimit Start { get; init; }

    public TLimit End { get; init; }

    public TValue? Value { get; init; }

    public IntervalType Type { get; init; }

    internal TLimit MaxEnd { get; set; }

    TLimit INodeInterval<TLimit>.MaxEnd
    {
        get => MaxEnd;
        set => MaxEnd = value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End, Type, Value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is Interval<TLimit, TValue> other)
        {
            return Equals(other);
        }

        return false;
    }

    public bool Equals(Interval<TLimit, TValue> other)
    {
        var result = Type.Equals(other!.Type)
            && (Start?.Equals(other.Start) ?? other.Start is null)
            && (End?.Equals(other.End) ?? other.End is null)
            && (Value?.Equals(other.Value) ?? other.Value is null);
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

    public static implicit operator Interval<TLimit, TValue>((TLimit Start, TLimit End, TValue? Value) interval) =>
        new Interval<TLimit, TValue>(interval.Start, interval.End, interval.Value);

    public static implicit operator Interval<TLimit, TValue>((TLimit Start, TLimit End, TValue? Value, IntervalType Type) interval) =>
        new Interval<TLimit, TValue>(interval.Start, interval.End, interval.Value, interval.Type);

    public static implicit operator Interval<TLimit, TValue>((TLimit Start, TLimit End, TValue? Value, IComparer<TLimit> Comparer) interval) =>
        new Interval<TLimit, TValue>(interval.Start, interval.End, interval.Value, IntervalType.Open, interval.Comparer);

    public static implicit operator Interval<TLimit, TValue>((TLimit Start, TLimit End, TValue? Value, IntervalType Type, IComparer<TLimit> Comparer) interval) =>
        new Interval<TLimit, TValue>(interval.Start, interval.End, interval.Value, interval.Type, interval.Comparer);
}