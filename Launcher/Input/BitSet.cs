using System.Collections;
using System.Collections.Generic;

namespace Launcher.Input;

public struct BitSet : IEnumerable<int>
{
    uint Value;
    const int Width = 32;

    public BitSet() { }

    public bool this[int idx]
    {
        readonly get => (Value & 1u << idx) != 0u;
        set
        {
            if (value)
            {
                Value |= 1u << idx;
            }
            else
            {
                Value &= ~(1u << idx);
            }
        }
    }

    public void Add(int value)
    {
        this[value] = true;
    }

    public void Remove(int value)
    {
        this[value] = false;
    }

    public void Clear()
    {
        Value = 0;
    }

    public static BitSet operator |(BitSet lhs, BitSet rhs) => lhs.Union(rhs);
    public static BitSet operator &(BitSet lhs, BitSet rhs) => lhs.Intersect(rhs);

    public readonly BitSet Union(BitSet other)
    {
        BitSet result = new()
        {
            Value = this.Value | other.Value
        };
        return result;
    }

    public readonly BitSet Intersect(BitSet rhs)
    {
        BitSet result = new()
        {
            Value = rhs.Value & Value
        };
        return result;
    }

    public readonly BitSet Diff(BitSet rhs)
    {
        BitSet result = new()
        {
            Value = rhs.Value & ~Value
        };
        return result;
    }

    public readonly bool IsEmpty()
    {
        return Value == 0;
    }

    public override readonly string ToString()
    {
        return $"[{string.Join(", ", this)}]";
    }

    public readonly IEnumerator<int> GetEnumerator()
    {
        for (int i = 0; i < Width; ++i)
        {
            if (this[i]) yield return i;
        }
    }

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
