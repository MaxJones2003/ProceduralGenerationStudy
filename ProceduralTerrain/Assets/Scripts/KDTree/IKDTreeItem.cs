using System;
using System.Collections.Generic;

public interface IKDTreeItem
{
    Vector2f Position { get; }
    float this[int dimension] { get; }
}

public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
{
    public int Compare(TKey x, TKey y)
    {
        int result = x.CompareTo(y);

        if (result == 0)
            return 1;   // Handle equality as being greater
        else
            return result;
    }
}
