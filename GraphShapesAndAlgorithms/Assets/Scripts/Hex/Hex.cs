using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
public readonly struct Hex
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
    public readonly int q;
    public readonly int r;
    public readonly int s;

    public Hex(int q, int r, int s)
    {
        if (q + r + s != 0)
            throw new ArgumentException("q + r + s must be 0");

        this.q = q;
        this.r = r;
        this.s = s;
    }

    #region Hex Operator Overrieds
    public static bool operator ==(Hex a, Hex b)
    {
        return a.q == b.q && a.r == b.r && a.s == b.s;
    }
    public static bool operator !=(Hex a, Hex b)
    {
        return !(a == b);
    }
    public static Hex operator +(Hex a, Hex b)
    {
        return new Hex(a.q + b.q, a.r + b.r, a.s + b.s);
    }
    public static Hex operator -(Hex a, Hex b)
    {
        return new Hex(a.q - b.q, a.r - b.r, a.s - b.s);
    }
    public static Hex operator *(Hex a, int k)
    {
        return new Hex(a.q * k, a.r * k, a.s * k);
    }
    public override int GetHashCode()
    {
        int hq = q.GetHashCode();
        int hr = r.GetHashCode();
        return 31 * hq + hr;
    }
    #endregion
}

public struct FractionalHex
{
    public readonly double q, r, s;
    public FractionalHex(double q, double r, double s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
    }
}
