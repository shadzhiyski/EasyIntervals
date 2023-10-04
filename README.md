# Intervals.Tools

This .NET package provides basic set of tools for working with generic intervals.

`// TODO: Enhance with Invert(); Slice(itv); Union(itv[]); HasIntersection(itv/limit)`

## About

> [!NOTE]
> This package is intended to simplify work with large set of intervals in use cases requiring often mutation (add, remove, update).

There aren't many packages available on [NuGet](https://www.nuget.org/), providing these functionalities with flexible and friendly enough programming API. The goal of this package is to provide useful and rich library for convenient work with intervals.

## Usage

### IntervalSet

IntervalSet is a generic collection providing basic functionalities over intervals of data.

Here is an example for intersection over IntervalSet:

[![](https://mermaid.ink/img/pako:eNptUs1unDAQfhXLUqVEsnaxYWHhnETKIadcqpYcJtgslgAjGFa7XfHuHRvapFW4MPP9Ipsbr5w2vOAn6BHLntGDFlvDHi_QDfR-7tGMZ2inldSA5smNHSBj31cILnbaoG-kWsHJVGhdv9r_WShrlfw8CiblG00FEH82ggXky4iPL7iLSXfvXcxPW5YfQ1Q1WhTsT-CH4k4Jdgg2qClPML9vVEa1kef-twdiE0lJS_IWVGxdtu6wpF_5V4YL3hk6H6vpoG_eVHJsTGdKXtCoTQ1ziyUv-4WkMKN7vfYVL3CcjeDz4M_8wcJphI4XNR0EoUZbdOPLennhDgUfoOfFjV94IeNdmidxpFSSp3meyUzwK8FZvsuTw-GookglkYrVIvgv5yhWBvuPMH_ufQw9f2tbB9qMvgWvQ_hv7IRUXbm-tiePz2NLcIM4TMV-7-ndyWIzv-8q1-0nqxsYsTnn6T5V6RFUbNIshkMc6-pd5sdaJbLWWSQV8GVZfgMPyMUH?type=png)](https://mermaid.live/edit#pako:eNptUs1unDAQfhXLUqVEsnaxYWHhnETKIadcqpYcJtgslgAjGFa7XfHuHRvapFW4MPP9Ipsbr5w2vOAn6BHLntGDFlvDHi_QDfR-7tGMZ2inldSA5smNHSBj31cILnbaoG-kWsHJVGhdv9r_WShrlfw8CiblG00FEH82ggXky4iPL7iLSXfvXcxPW5YfQ1Q1WhTsT-CH4k4Jdgg2qClPML9vVEa1kef-twdiE0lJS_IWVGxdtu6wpF_5V4YL3hk6H6vpoG_eVHJsTGdKXtCoTQ1ziyUv-4WkMKN7vfYVL3CcjeDz4M_8wcJphI4XNR0EoUZbdOPLennhDgUfoOfFjV94IeNdmidxpFSSp3meyUzwK8FZvsuTw-GookglkYrVIvgv5yhWBvuPMH_ufQw9f2tbB9qMvgWvQ_hv7IRUXbm-tiePz2NLcIM4TMV-7-ndyWIzv-8q1-0nqxsYsTnn6T5V6RFUbNIshkMc6-pd5sdaJbLWWSQV8GVZfgMPyMUH)

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

[![](https://mermaid.ink/img/pako:eNptkk9PhDAQxb8KmcREE7JLCwsLZzXx4MmLUTyMtCxNgJIybHbd8N0dYKPrHy5Mf_PmPdpygsIqDRnssCXKW48fMlRr7-6ATcfvh5a022PdL02FpO-ta5A873lBeDD9GV2xaoG9LsjYdhn_sWCvRfK68T0h3rjKkPt77Xsz-dfi-wuuJetupilvqi68gpkWzpDvXToGZ03ItXz7q5n5WSMEL-LJ6Ldq6YAPjea9GsWHdpqGcqBKNzqHjEulSxxqyiFvR5biQPbp2BaQkRu0D0M3nd-twZ3DBrKSN8VUK0PWPS4XMd-HDx22kJ3gAJkIV3EahYGUURqnaSISH46Mk3SVRpvNVgaBjAIZytGHD2vZVszjL3N9mXs353zF1haVdlMKHbv5HzA9cXRh29LsJj64mnFF1PXZej21VztD1fC-Kmyz7o2q0FG1T-N1LOMtylDHSYibMFTFu0i3pYxEqZJASIRxHD8Blo-6lg?type=png)](https://mermaid.live/edit#pako:eNptkk9PhDAQxb8KmcREE7JLCwsLZzXx4MmLUTyMtCxNgJIybHbd8N0dYKPrHy5Mf_PmPdpygsIqDRnssCXKW48fMlRr7-6ATcfvh5a022PdL02FpO-ta5A873lBeDD9GV2xaoG9LsjYdhn_sWCvRfK68T0h3rjKkPt77Xsz-dfi-wuuJetupilvqi68gpkWzpDvXToGZ03ItXz7q5n5WSMEL-LJ6Ldq6YAPjea9GsWHdpqGcqBKNzqHjEulSxxqyiFvR5biQPbp2BaQkRu0D0M3nd-twZ3DBrKSN8VUK0PWPS4XMd-HDx22kJ3gAJkIV3EahYGUURqnaSISH46Mk3SVRpvNVgaBjAIZytGHD2vZVszjL3N9mXs353zF1haVdlMKHbv5HzA9cXRh29LsJj64mnFF1PXZej21VztD1fC-Kmyz7o2q0FG1T-N1LOMtylDHSYibMFTFu0i3pYxEqZJASIRxHD8Blo-6lg)

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

[![](https://mermaid.ink/img/pako:eNptUcGOmzAQ_RVrpEpdCSXYEAicuyvtYU-9rFp6mMUmWAKMzBAljfj3HUzUplK5-M2bN-_Zww1qpw2UcMKBqBoEf2SpM-L5gv3I5-tAxp-xm7amRjIvzvdIQrxvFF7sdKe-sGojJ1OTdcM2_k_BXpvk5yESUv5iVCL3zyYSgfmvxd8bfFWse1qnxIoevOLA1t5SJB4d47smYaxCngjwTkvJRbbOcmMrIILe8Ius5tXcVl0F1JreVFAy1KbBuaMKqmFhKc7kvl-HGkrys4lgHtctfbN48thD2fDVmTXakvNv27rD1iMYcYDyBhcoZbLLijSJlUqLrChymUdwZTovdkV6OBxVHKs0VolaIvjtHNvKMP4j4Mfc55DzJ7ZzqI1fU-g6hj9tJ-Lo2g2NPa387DumW6JxKvf7tb07WWrnj13t-v1kdYue2nOR7TOVHVElJssTPCSJrj9kcWxUKhudx1IhLMvyCVBMsUo?type=png)](https://mermaid.live/edit#pako:eNptUcGOmzAQ_RVrpEpdCSXYEAicuyvtYU-9rFp6mMUmWAKMzBAljfj3HUzUplK5-M2bN-_Zww1qpw2UcMKBqBoEf2SpM-L5gv3I5-tAxp-xm7amRjIvzvdIQrxvFF7sdKe-sGojJ1OTdcM2_k_BXpvk5yESUv5iVCL3zyYSgfmvxd8bfFWse1qnxIoevOLA1t5SJB4d47smYaxCngjwTkvJRbbOcmMrIILe8Ius5tXcVl0F1JreVFAy1KbBuaMKqmFhKc7kvl-HGkrys4lgHtctfbN48thD2fDVmTXakvNv27rD1iMYcYDyBhcoZbLLijSJlUqLrChymUdwZTovdkV6OBxVHKs0VolaIvjtHNvKMP4j4Mfc55DzJ7ZzqI1fU-g6hj9tJ-Lo2g2NPa387DumW6JxKvf7tb07WWrnj13t-v1kdYue2nOR7TOVHVElJssTPCSJrj9kcWxUKhudx1IhLMvyCVBMsUo)

Code:

```CSharp
var coveredIntervals = intervalSet
    .Intersect(intersectionInterval, IntersectionType.Cover);
Console.WriteLine(
    $"{intersectionInterval} covers [{string.Join(", ", coveredIntervals)}]");
// [5, 11] covers [[5, 10)]
```

Intersection of interval set with an input interval that is within intersected intervals:

[![](https://mermaid.ink/img/pako:eNptkc1uqzAQhV8FjVSplVCCDYHAuq3URVfdXPXSxRSbYAkwMkOU3Ih372By-yOVDWc-n5ljhgtUVmko4IA9UdkH_JChVgcPJ-wGfj_1pN0R23E9VEj60boOKQj-rAhPZryiG3atcNQVGduv7T8KnrVa_u7CQIg3VgXy-VGHgSe_jvi6wa1k393SFSzq26xopV5eccxa-ojKGQqD_0GeXz1CcJEuvdy8FhBCp_mLjOLVXBZfCdToTpdQsFS6xqmlEsp-ZitOZF_OfQUFuUmHMA3Llu4NHhx2UNR8daZaGbLueV2333oIA_ZQXOAEhYg3aZ7EkZRJnuZ5JrIQzoyzfJMnu91eRpFMIhnLOYR_1vJY4dtfvf6e--BzPmNbi0q7JYXOg__TZiSOrmxfm8PCJ9cyboiGsdhul-PNwVAzvW8q221Hoxp01BzzdJvKdI8y1mkW4y6OVfUu8n0tE1GrLBISYZ7nDx_isUo?type=png)](https://mermaid.live/edit#pako:eNptkc1uqzAQhV8FjVSplVCCDYHAuq3URVfdXPXSxRSbYAkwMkOU3Ih372By-yOVDWc-n5ljhgtUVmko4IA9UdkH_JChVgcPJ-wGfj_1pN0R23E9VEj60boOKQj-rAhPZryiG3atcNQVGduv7T8KnrVa_u7CQIg3VgXy-VGHgSe_jvi6wa1k393SFSzq26xopV5eccxa-ojKGQqD_0GeXz1CcJEuvdy8FhBCp_mLjOLVXBZfCdToTpdQsFS6xqmlEsp-ZitOZF_OfQUFuUmHMA3Llu4NHhx2UNR8daZaGbLueV2333oIA_ZQXOAEhYg3aZ7EkZRJnuZ5JrIQzoyzfJMnu91eRpFMIhnLOYR_1vJY4dtfvf6e--BzPmNbi0q7JYXOg__TZiSOrmxfm8PCJ9cyboiGsdhul-PNwVAzvW8q221Hoxp01BzzdJvKdI8y1mkW4y6OVfUu8n0tE1GrLBISYZ7nDx_isUo)

Code:

```CSharp
var coveringIntervals = intervalSet
    .Intersect(intersectionInterval, IntersectionType.Within);
Console.WriteLine(
    $"{intersectionInterval} is within [{string.Join(", ", coveringIntervals)}]");
// [5, 11] is within [[3, 12]]
```

You can also merge intervals. Calling **Merge** returns IntervalSet with the merged intervals.

[![](https://mermaid.ink/img/pako:eNptkkuL2zAUhf-KEBQm4CSW_Iq17hS6yGo2Q-su7liyLbAlI18PSYP_e2V5GoYw2ujwnat79LrR2kpFBd3v95WRehp7uJ49EqS2wwg1ViZYLRj0mviBGntFni8wjH7-aVC5d-inzZSA6od1AyAhrxuCi54-0DdftcFJ1aiteVz-xCOS7VZFBFn1hn9nEWHxbsNBf3Dma1j-J_BN_zfSiPB8txlBPyaflWuV_GoD9yT-kHRvGDSN6KD8sbT093dbyyqKnRpURYWXUjUw91jRyiy-FGa0L1dTU4FuVhGdx_WqvmtoHQxUNH4Hniqp0brz9ibhaSI6gqHiRi9UsOSQl2kSc56WeVkWrIjo1eOiPJRplp14HPM05glfIvrXWt-WheW_gv6c-xxy7rG9BancmoLXcY1u9YQ-uram0e3KZ9d73CGOkzgeV_vQauzmt4P_J8dJyw4cdu9lfsx5fgKeqLxIIEsSWb-x8tTwlDWyiBkHuizLP1gTuAs?type=png)](https://mermaid.live/edit#pako:eNptkkuL2zAUhf-KEBQm4CSW_Iq17hS6yGo2Q-su7liyLbAlI18PSYP_e2V5GoYw2ujwnat79LrR2kpFBd3v95WRehp7uJ49EqS2wwg1ViZYLRj0mviBGntFni8wjH7-aVC5d-inzZSA6od1AyAhrxuCi54-0DdftcFJ1aiteVz-xCOS7VZFBFn1hn9nEWHxbsNBf3Dma1j-J_BN_zfSiPB8txlBPyaflWuV_GoD9yT-kHRvGDSN6KD8sbT093dbyyqKnRpURYWXUjUw91jRyiy-FGa0L1dTU4FuVhGdx_WqvmtoHQxUNH4Hniqp0brz9ibhaSI6gqHiRi9UsOSQl2kSc56WeVkWrIjo1eOiPJRplp14HPM05glfIvrXWt-WheW_gv6c-xxy7rG9BancmoLXcY1u9YQ-uram0e3KZ9d73CGOkzgeV_vQauzmt4P_J8dJyw4cdu9lfsx5fgKeqLxIIEsSWb-x8tTwlDWyiBkHuizLP1gTuAs)

Code:

```CSharp
var intervalSet = new IntervalSet<int>
{
    (2, 5), // (2, 5)
    (5, 10, IntervalType.StartClosed), // [5, 10)
    (12, 16, IntervalType.Closed), // [12, 16]
    (14, 26, IntervalType.StartClosed) // [14, 26)
};

var mergedIntervalSet = intervalSet.Merge();
Console.WriteLine($"[{string.Join(", ", intersectedIntervalSet3)}]");
// [(2, 10), [12, 26)]
```

## License

// TODO: add MIT license

## Donate

// TODO: enable GitHub Sponsorship
