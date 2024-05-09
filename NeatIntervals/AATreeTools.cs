namespace NeatIntervals;

internal static class AATreeTools
{
    public static (AATree<T>.Node?, int) InitializeTree<T>(
        IEnumerable<T> elements, bool areSorted, bool areUnique, IComparer<T> comparer, Action<AATree<T>.Node> onChildChanged)
    {
        if (!elements.Any())
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

        var root = ConstructTreeFromSortedArray(orderedElements, 0, uniqueElementsCount - 1, null, onChildChanged);
        return (root, uniqueElementsCount);
    }

    private static AATree<T>.Node? ConstructTreeFromSortedArray<T>(
        T[] elements, int startIndex, int endIndex, AATree<T>.Node? leftmostNode, Action<AATree<T>.Node> onChildChanged)
    {
        int size = endIndex - startIndex + 1;
        AATree<T>.Node? node;

        switch (size)
        {
            case 0:
                node = null; break;
            case 1:
                node = new AATree<T>.Node(elements[startIndex], onChildChanged);
                if (leftmostNode is not null)
                {
                    leftmostNode.Right = node;
                    node = leftmostNode;
                }
                break;
            case 2:
                node = new AATree<T>.Node(elements[startIndex], onChildChanged);
                node.Right = new AATree<T>.Node(elements[endIndex], onChildChanged);
                if (leftmostNode is not null)
                {
                    node.Left = leftmostNode;
                    node.Level = 2;
                }
                break;
            case 3:
                node = new AATree<T>.Node(elements[startIndex + 1], onChildChanged);
                node.Level = 2;
                node.Left = new AATree<T>.Node(elements[startIndex], onChildChanged);
                node.Right = new AATree<T>.Node(elements[endIndex], onChildChanged);
                if (leftmostNode is not null)
                {
                    leftmostNode.Right = node.Left;
                    node.Left = leftmostNode;
                }
                break;
            default:
                int middleIndex = (startIndex + endIndex) / 2;
                node = new AATree<T>.Node(elements[middleIndex], onChildChanged);
                node.Left = ConstructTreeFromSortedArray(elements, startIndex, middleIndex - 1, leftmostNode, onChildChanged);
                node.Right = size % 2 == 0 ?
                    ConstructTreeFromSortedArray(elements, middleIndex + 2, endIndex, new AATree<T>.Node(elements[middleIndex + 1], onChildChanged), onChildChanged) :
                    ConstructTreeFromSortedArray(elements, middleIndex + 1, endIndex, null, onChildChanged);
                node.Level = node.Right?.Level ?? node.Level;
                if (node.Left?.Level == node.Right?.Level)
                {
                    node.Level++;
                }
                else if (node.Left?.Level > node.Right?.Level)
                {
                    var leftNode = node.Left;
                    var middleNode = leftNode.Right;

                    node.Left = middleNode;
                    leftNode.Right = node;
                    node.Level = leftNode.Level;

                    node = leftNode;
                }
                break;
        }

        return node;
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