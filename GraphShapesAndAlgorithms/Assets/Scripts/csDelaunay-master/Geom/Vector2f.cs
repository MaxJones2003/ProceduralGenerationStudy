﻿using System;
using UnityEngine;

// Recreation of the UnityEngine.Vector3, so it can be used in other thread
[System.Serializable]
public struct Vector2f {
	
	public float x, y;
	
	public static readonly Vector2f zero = new Vector2f(0,0);
	public static readonly Vector2f one = new Vector2f(1,1);

	public static readonly Vector2f right = new Vector2f(1,0);
	public static readonly Vector2f left = new Vector2f(-1,0);
	
	public static readonly Vector2f up = new Vector2f(0,1);
	public static readonly Vector2f down = new Vector2f(0,-1);
	
	public Vector2f(float x, float y) {
		this.x = x;
		this.y = y;
	}
	public Vector2f(double x, double y) {
		this.x = (float)x;
		this.y = (float)y;
	}
	
	public float magnitude {
		get{
			return (float)Math.Sqrt(x*x + y*y);
		}
	}
    public float sqrMagnitude
    {
        get
        {
            return (x * x + y * y);
        }
    }

    public void Normalize() {
		float magnitude = this.magnitude;
		x /= magnitude;
		y /= magnitude;
	}

	public static Vector2f Normalize(Vector2f a) {
		float magnitude = a.magnitude;
		return new Vector2f(a.x/magnitude, a.y/magnitude);
	}

	public static Vector2f Interpolate(Vector2f a, Vector2f b, float weight)
	{
		Vector2 lerped = Vector2.Lerp(new Vector2(a.x, a.y), new Vector2(b.x, b.y), weight);
		return new Vector2f(lerped.x, lerped.y);
	}

	public static float Distance(Vector2f a, Vector2f b)
	{
		return (float)Math.Sqrt(Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2));
	}
	
	public override bool Equals(object other) {
		if (!(other is Vector2f)) {
			return false;
		}
		Vector2f v = (Vector2f) other;
		return x == v.x &&
			y == v.y;
	}

    public override string ToString () {
		return string.Format ("[Vector2f]"+x+","+y);
	}
	
	// Indexer to access elements by index
    public float this[int index]
    {
        get
        {
            return index == 0 ? x : y;
        }
        set
        {
            if (index == 0)
                x = value;
            else if (index == 1)
                y = value;
            else
                throw new IndexOutOfRangeException("Vector2f index must be 0 or 1.");
        }
    }
	public override int GetHashCode () {
		return x.GetHashCode () ^ y.GetHashCode () << 2;
	}

	public float DistanceSquare(Vector2f v) {
		return Vector2f.DistanceSquare(this, v);
	}
	public static float DistanceSquare(Vector2f a, Vector2f b) {
		float cx = b.x - a.x;
		float cy = b.y - a.y;
		return cx*cx + cy*cy;
	}
	
	public static bool operator == (Vector2f a, Vector2f b) {
		return a.x == b.x && 
			a.y == b.y;
	}
	
	public static bool operator != (Vector2f a, Vector2f b) {
		return a.x != b.x ||
			a.y != b.y;
	}
	
	public static Vector2f operator - (Vector2f a, Vector2f b) {
		return new Vector2f( a.x-b.x, a.y-b.y);
	}
	
	public static Vector2f operator + (Vector2f a, Vector2f b) {
		return new Vector2f( a.x+b.x, a.y+b.y);
	}

	public static Vector2f operator * (Vector2f a, int i) {
		return new Vector2f(a.x*i, a.y*i);
	}

	public static Vector2f operator * (Vector2f a, float i) {
		return new Vector2f(a.x*i, a.y*i);
	}

	public static Vector2f operator / (Vector2f a, int i) {
		return new Vector2f(a.x/i, a.y/i);
	}

	public static Vector2f operator / (Vector2f a, float i) {
		return new Vector2f(a.x*i, a.y*i);
	}

    public static explicit operator Vector2(Vector2f v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector2f Min(Vector2f a, Vector2f b) {
		return new Vector2f(Math.Min(a.x, b.x), Math.Min(a.y, b.y));
	}
	public static Vector2f Max(Vector2f a, Vector2f b) {
		return new Vector2f(Math.Max(a.x, b.x), Math.Max(a.y, b.y));
	}
}
