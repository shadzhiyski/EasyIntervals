# Intervals.Tools

A .NET package intended to simplify the work with sets of generic intervals, especially in use cases with large sets requiring often mutation (add, remove).

## Get Started

Intervals.Tools can be installed using the Nuget package manager or the `dotnet` CLI.

```Shell
dotnet add package Intervals.Tools
```

## Usage

### Interval Basics

An Interval can be created with start and end input parameters. By default type of interval is **Open**. Interval type can be **Closed**, **StartClosed**, **EndClosed** and **Open**.

```CSharp
var interval1 = new Interval<int>(10, 50); // open (10, 50)
var interval2 = new Interval<int>(10, 50, IntervalType.Closed); // closed [10, 50]
var interval3 = new Interval<int>(10, 50, IntervalType.StartClosed); // end closed [10, 50)
var interval4 = new Interval<int>(10, 50, IntervalType.EndClosed); // end closed (10, 50]
```

Interval can also be created from value tuple:

```CSharp
Interval<double> interval1 = (0.1d, 0.5d); // open (0.1, 0.5)
Interval<double> interval2 = (0.1d, 0.5d, IntervalType.Closed); // closed [0.1, 0.5]
```

Operations over specific intervals is done through the functions of **IntervalTools** class.

Here is an example how to check if 2 intervals intersect:

```CSharp
var result1 = IntervalTools.HasAnyIntersection((10, 20), (18, 30));
Console.WriteLine(result1);
// True

var result2 = IntervalTools.HasAnyIntersection((10, 20), (22, 30));
Console.WriteLine(result2);
// False
```

Here is an example how to check if an interval covers another interval:

```CSharp
var result1 = IntervalTools.Covers(interval: (10, 20), other: (12, 18));
Console.WriteLine(result1);
// True

var result2 = IntervalTools.HasAnyIntersection(interval: (10, 20), other: (10, 30));
Console.WriteLine(result2);
// False
```

Manipulation on sets of intervals is done with **IntervalSet** collection. You can do all basic operations on intervals set - **AddRange**, **Union**, **Intersect**, **Except**, **Merge**.

### Add Range

Adds all unique intervals from given input enumeration of intervals:

```CSharp
var intervalSet = new IntervalSet<int>
{
    (2, 5), // (2, 5)
    (3, 8, IntervalType.Open), // (3, 8)
    (7, 10), // (7, 10)
};

var inputIntervals = new List<Interval<int>>
{
    (3, 8, IntervalType.Closed), // [3, 8]
    (7, 10), // (7, 10)
    (11, 16, IntervalType.StartClosed), // [11, 16)
    (11, 14, IntervalType.EndClosed), // (11, 14]
};

intervalSet.AddRange(inputIntervals);
Console.WriteLine($"[{string.Join(", ", intervalSet)}]");
// [(2, 5), [3, 8], (3, 8), (7, 10), [11, 16), (11, 14]]
```

### Union

Unions all unique intervals from the current and input interval set:

```CSharp
var intervalSet1 = new IntervalSet<int>
{
    (2, 5), // (2, 5)
    (3, 8, IntervalType.Open), // (3, 8)
    (7, 10), // (7, 10)
};

var intervalSet2 = new IntervalSet<int>
{
    (3, 8, IntervalType.Closed), // [3, 8]
    (7, 10), // (7, 10)
    (11, 16, IntervalType.StartClosed), // [11, 16)
    (11, 14, IntervalType.EndClosed), // (11, 14]
};

var unionIntervalSet = intervalSet1.Union(intervalSet2);
Console.WriteLine($"[{string.Join(", ", unionIntervalSet)}]");
// [(2, 5), [3, 8], (3, 8), (7, 10), [11, 16), (11, 14]]
```

### Intersection

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
    (2, 5), // (2, 5)
    (3, 8, IntervalType.Open), // (3, 8)
    (3, 8, IntervalType.Closed), // [3, 8]
    (7, 10), // (7, 10)
    (11, 16, IntervalType.StartClosed), // [11, 16)
    (11, 14, IntervalType.EndClosed), // (11, 14]
};

var intersectionInterval = new Interval<int>(8, 11, IntervalType.Closed);
var intersectedIntervals = intervalSet
    .Intersect(intersectionInterval); // [8, 11]
Console.WriteLine($"[{string.Join(", ", intersectedIntervals)}]");
// [[3, 8], (7, 10), [11, 16)]
```

You can also specify the type of intersection between the given interval and the interval set. By default the intersection type is **Any**, searching for any kind of intersection between the intervals:

```CSharp
var intersectedIntervals = intervalSet
    .Intersect(intersectionInterval, IntersectionType.Any);
Console.WriteLine($"[{string.Join(", ", intersectedIntervals)}]");
// [[3, 8], (7, 10), [11, 16)]
```

There are 2 more type of intersection - **Cover** where the given interval covers intervals and **Within** where the given one is within intervals.

### Covering Intersection

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
var intersectionInterval = (5, 11, IntervalType.Closed);
var coveredIntervals = intervalSet
    .Intersect(intersectionInterval, IntersectionType.Cover);
Console.WriteLine(
    $"{intersectionInterval} covers [{string.Join(", ", coveredIntervals)}]");
// [5, 11] covers [[5, 10)]
```

### Within Intersection

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
var intersectionInterval = (5, 11, IntervalType.Closed);
var coveringIntervals = intervalSet
    .Intersect(intersectionInterval, IntersectionType.Within);
Console.WriteLine(
    $"{intersectionInterval} is within [{string.Join(", ", coveringIntervals)}]");
// [5, 11] is within [[3, 12]]
```

### Exception

Calling **Except** over the IntervalSet returns the not intersected (excepted) intervals:

```mermaid
gantt
    title Example Intervals
    dateFormat  X
    axisFormat %s

    section Intersection Interval
    [8, 11]   :active, 8, 11

    section Intervals
    (3, 8)   :crit, active, 3, 8
    [3, 8]   : 3, 8
    (2, 5)   :crit, active, 2, 5
    (7, 10)    : 7, 10
    (11, 14]    :crit, active, 11, 14
    [11, 16)    : 11, 16
```

Code:

```CSharp
var intersectionInterval = (8, 11, IntervalType.Closed);
var exceptedIntervals = intervalSet
    .Except(intersectionInterval);
Console.WriteLine($"[{string.Join(", ", exceptedIntervals)}]");
// [(2, 5), (3, 8), (11, 14]]
```

### Covering Exception

Exception of interval set with an input interval that doesn't cover excepted intervals:

```mermaid
gantt
    title Example Intervals
    dateFormat  X
    axisFormat %s

    section Exception Interval
    [5, 11]   :active, 5, 11

    section Intervals
    (2, 5)   :crit, active, 2, 5
    [5, 10)   : 5, 10
    [3, 12]   :crit, active, 3, 12
    [11, 16)    :crit, active, 11, 16
```

Code:

```CSharp
var coveringInterval = (5, 11, IntervalType.Closed);
var notCoveredIntervals = intervalSet
    .Except(coveringInterval, IntersectionType.Cover);
Console.WriteLine(
    $"{coveringInterval} doesn't cover [{string.Join(", ", notCoveredIntervals)}]");
// [5, 11] doesn't cover [(2, 5), [3, 12], [11, 16)]
```

### Within Exception

Exception of interval set with an input interval that is not within excepted intervals:

```mermaid
gantt
    title Example Intervals
    dateFormat  X
    axisFormat %s

    section Exception Interval
    [5, 11]   :active, 5, 11

    section Intervals
    (2, 5)   :crit, active, 2, 5
    [5, 10)   :crit, active, 5, 10
    [3, 12]   : 3, 12
    [11, 16)    :crit, active, 11, 16
```

Code:

```CSharp
var withinInterval = (5, 11, IntervalType.Closed);
var notCoveringIntervals = intervalSet
    .Except(withinInterval, IntersectionType.Within);
Console.WriteLine(
    $"{withinInterval} is not within [{string.Join(", ", notCoveringIntervals)}]");
// [5, 11] is not within [(2, 5), [5, 10), [11, 16)]
```

### Merge

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
