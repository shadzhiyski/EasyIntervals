# Intervals.Tools

This .NET package provides basic set of tools for working with generic intervals.

`// TODO: Enhance with Invert(); Slice(itv); Union(itv[]); HasIntersection(itv/limit)`

## About

> [!NOTE]
> This package is intended to simplify the work with large set of intervals in use cases requiring often mutation (add, remove, update).

There aren't many packages available on [NuGet](https://www.nuget.org/), providing these functionalities with flexible and friendly enough programming API. The goal of this package is to provide useful and rich library for convenient work with intervals.

## Usage

### Interval\<TLimit\>

**Interval** represents an interval with `Start` and `End` limits and `Type` of interval. There are four interval types - **Open**, **StartClosed**, **EndClosed**, **Closed**. Here are examples for each interval type:

```
(2, 5) - Open - open on both limits;
[2, 5) - StartClosed - closed on Start and open on end;
(2, 5] - EndClosed - open on Start and closed on end;
[2, 5] - Closed - closed on both limits;
```

An **Interval** can be created with `start` and `end` input parameters. By default `type` of interval is **Open**.

```CSharp
var interval = new Interval<int>(10, 50); // open (10, 50)
var intervalWithCustomType = new Interval<int>(10, 50, IntervalType.Closed); // closed [10, 50]
```

Intervals can be created from value tuple:

```CSharp
Interval<double> interval1 = (0.1d, 0.5d);
Interval<double> interval2 = (0.1d, 0.5d, IntervalType.Closed);
```

### IntervalSet\<TLimit\>

**IntervalSet** is a collection for storing large amount of unique intervals where multiple add, remove and search operations can be done in efficient time. It's inspired by [IntervalTree](https://github.com/mbuchetics/RangeTree) and has similarities with [System.Collections.Generic.SortedSet\<T\>](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.sortedset-1).

Here is an example for intersection over IntervalSet:

```mermaid
gantt
    title Example Intervals
    dateFormat  X
    axisFormat %s

    section Intersection Interval
    [8, 11]   :active, 8, 11

    section Intervals
    (3, 8)   : 3, 8
    [3, 8]   :crit, active, 3, 8
    (2, 5)   :after, 2, 5
    (7, 10)    :crit, active, 7, 10
    (11, 14]    : 11, 14
    [11, 16)    :crit, active, 11, 16
```

Code:

```CSharp
var intervalSet = new IntervalSet<int>
{
    (2, 5), // (2, 5) - if interval type is not specified, interval is open by default
    (3, 8, IntervalType.Open), // (3, 8)
    (3, 8, IntervalType.Closed), // [3, 8]
    (7, 10), // (7, 10)
    (11, 16, IntervalType.StartClosed), // [11, 16)
    (11, 14, IntervalType.EndClosed), // (11, 14]
};

var intersectionInterval = (8, 11, IntervalType.Closed);
var intersectedIntervals = intervalSet
    .Intersect(intersectionInterval); // [8, 11]
Console.WriteLine($"[{string.Join(", ", intersectedIntervals)}]");
// [[3, 8], (7, 10), [11, 16)]
```

Calling Except over the IntervalSet returns the not intersected (excepted) intervals:

```CSharp
var exceptedIntervals = intervalSet
    .Except((8, 11, IntervalType.Closed)); // [8, 11]
Console.WriteLine($"[{string.Join(", ", exceptedIntervals)}]");
// [(2, 5), (3, 8), (11, 14]]
```

You can also specify the type of intersection between the given interval and the interval set. By default the intersection type is **Any**, searching for any kind of intersection between the intervals:

```mermaid
gantt
    title Example Intervals
    dateFormat  X
    axisFormat %s

    section Intersection Interval
    [5, 11]   :active, 5, 11

    section Intervals
    (2, 5)   : 2, 5
    [5, 10)   :crit, active, 5, 10
    [3, 12]   :crit, active, 3, 12
    [11, 16)    :crit, active, 11, 16
```

Code:

```CSharp
var intervalSet = new IntervalSet<int>
{
    (2, 5),
    (5, 10, IntervalType.StartClosed),
    (3, 12, IntervalType.Closed),
    (11, 16, IntervalType.StartClosed)
};

var intersectionInterval = (5, 11, IntervalType.Closed);
var intersectedIntervals = intervalSet
    .Intersect(intersectionInterval, IntersectionType.Any);
Console.WriteLine(
    $"{intersectionInterval} has any intersection with [{string.Join(", ", intersectedIntervals)}]");
// [5, 11] has any intersection with [[3, 12], [5, 10), [11, 16)]
```

There are 2 more type of intersection - **Cover** where the given interval covers intervals and **Within** where the given one is within intervals.

Intersection of interval set with an input interval that covers intersected intervals:

```mermaid
gantt
    title Example Intervals
    dateFormat  X
    axisFormat %s

    section Intersection Interval
    [5, 11]   :active, 5, 11

    section Intervals
    (2, 5)   : 2, 5
    [5, 10)   :crit, active, 5, 10
    [3, 12]   : 3, 12
    [11, 16)    : 11, 16
```

Code:

```CSharp
var coveredIntervals = intervalSet
    .Intersect(intersectionInterval, IntersectionType.Cover);
Console.WriteLine(
    $"{intersectionInterval} covers [{string.Join(", ", coveredIntervals)}]");
// [5, 11] covers [[5, 10)]
```

Intersection of interval set with an input interval that is within intersected intervals:

```mermaid
gantt
    title Example Intervals
    dateFormat  X
    axisFormat %s

    section Intersection Interval
    [5, 11]   :active, 5, 11

    section Intervals
    (2, 5)   : 2, 5
    [5, 10)   : 5, 10
    [3, 12]   :crit, active, 3, 12
    [11, 16)    : 11, 16
```

Code:

```CSharp
var coveringIntervals = intervalSet
    .Intersect(intersectionInterval, IntersectionType.Within);
Console.WriteLine(
    $"{intersectionInterval} is within [{string.Join(", ", coveringIntervals)}]");
// [5, 11] is within [[3, 12]]
```

You can also merge intervals. Calling **Merge** returns IntervalSet with the merged intervals.

```mermaid
---
displayMode: compact
---
gantt
    title Example Intervals
    dateFormat  YYYY-MM-DD
    axisFormat %Y-%m-%d

    section Intervals
    (2.9.2023 - 5.9.2023)     : 2023-09-02, 2023-09-05
    [5.9.2023 - 10.9.2023)    : 2023-09-05, 2023-09-10
    [12.9.2023 - 16.9.2023]   : 2023-09-12, 2023-09-16
    [14.9.2023 - 26.9.2023)   : 2023-09-14, 2023-09-26

    section Merged Intervals
    (2.9.2023 - 10.9.2023)    : 2023-09-02, 2023-09-10
    [12.9.2023 - 26.9.2023)   : 2023-09-12, 2023-09-26
```

Code:

```CSharp
var intervalSet = new IntervalSet<DateOnly>
{
    (new DateOnly(2023, 09, 2), new DateOnly(2023, 09, 5)), // (2 - 5)
    (new DateOnly(2023, 09, 5), new DateOnly(2023, 09, 10), IntervalType.StartClosed), // [5 - 10)
    (new DateOnly(2023, 09, 12), new DateOnly(2023, 09, 16), IntervalType.Closed), // [12 - 16]
    (new DateOnly(2023, 09, 14), new DateOnly(2023, 09, 26), IntervalType.StartClosed) // [14 - 26)
};

var mergedIntervalSet = intervalSet.Merge();
Console.WriteLine($"[{string.Join(", ", mergedIntervalSet)}]");
// [(9/2/2023, 9/10/2023), [9/12/2023, 9/26/2023)]
```

## License

// TODO: add MIT license

## Donate

// TODO: enable GitHub Sponsorship
