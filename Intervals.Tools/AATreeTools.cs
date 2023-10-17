namespace Intervals.Tools;

internal static class AATreeTools
{
    public static AATree<T>.Node? InitializeTree<T>(
        IEnumerable<T> elements, bool areSorted, bool areUnique, IComparer<T> comparer, Action<AATree<T>.Node> onChildChanged)
    {
        if (elements.Count() == 0)
        {
            return default;
        }

        var orderedElements = elements.ToArray();
        if (!areSorted)
        {
            Array.Sort(orderedElements, comparer);
        }

        var uniqueElementsCount = orderedElements.Length;
        if (!areUnique)
        {
            uniqueElementsCount = ShiftUniqueElementsToBeginning(orderedElements, comparer);
        }

        var orderedNodes = orderedElements.Take(uniqueElementsCount)
            .Select(n => new AATree<T>.Node(n, onChildChanged))
            .ToArray();

        if (orderedNodes.Length % 2 == 0)
        {
            orderedNodes[orderedNodes.Length - 2].Right = orderedNodes[orderedNodes.Length - 1];
        }

        var treeDepth = (int)Math.Ceiling(Math.Log2(orderedNodes.Length));
        var startIndex = 0;
        for (int iteration = 2; iteration <= treeDepth; iteration++)
        {
            var step = 1 << iteration;
            var childStep = step >> 2;
            var halfStep = step >> 1;
            startIndex = halfStep - 1;
            if (startIndex >= (double)orderedNodes.Length / 2)
            {
                startIndex = (step >> 2) - 1;
                break;
            }

            var index = startIndex;
            for (; index <= orderedNodes.Length - (childStep << 1); index += step)
            {
                SetChildren(orderedNodes, iteration, childStep, index);
            }

            index -= halfStep;
            if (index < orderedNodes.Length - 1
                && orderedNodes[index].Left is null
                && orderedNodes[index].Right is null)
            {
                SetChildren(orderedNodes, iteration, childStep, index);

                orderedNodes[index - halfStep].Right = orderedNodes[index];
            }
        }

        return orderedNodes[startIndex];
    }

    private static void SetChildren<T>(AATree<T>.Node[] orderedNodes, int iteration, int childStep, int index)
    {
        orderedNodes[index].Level = iteration;
        orderedNodes[index].Left = orderedNodes[index - childStep];
        orderedNodes[index].Right = orderedNodes[index + childStep];
    }

    public static int ShiftUniqueElementsToBeginning<T>(T[] orderedElements, IComparer<T> comparer)
    {
        var index = 1;
        for (int i = 1; i < orderedElements.Length; i++)
        {
            if (comparer.Compare(orderedElements[i], orderedElements[i - 1]) != 0)
            {
                orderedElements[index++] = orderedElements[i];
            }
        }

        return index;
    }
}