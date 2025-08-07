using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static T Choice<T>(this System.Random rand, T[] array)
    {
        return array[rand.Next(array.Length)];
    }

    public static T Choice<T>(this System.Random rand, IList<T> list)
    {
        return list[rand.Next(list.Count)];
    }

    public static Vector3 Negative(this Vector3 vector)
    {
        return new Vector3(x: -vector.x, y: -vector.y, z: -vector.z);
    }

    public static int ToInt(this bool value) => value? 1 : 0;
}
