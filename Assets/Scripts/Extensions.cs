using DG.Tweening;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
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

    public static int ToInt(this bool value) => value ? 1 : 0;

    /// <summary>
    /// возвращает компонент SpriteRenderer у объекта
    /// </summary>
    public static SpriteRenderer GetSR(this GameObject go) => go.GetComponent<SpriteRenderer>();

    public static Collider GetCollider(this GameObject go) => go.GetComponent<Collider>();

    public static Collider2D GetCollider2D(this GameObject go) => go.GetComponent<Collider2D>();

    public static void SetLocalY(this Transform transform, float targetY)
    {
        var localPos = transform.localPosition;
        localPos.y = targetY;

        transform.localPosition = localPos;
    }

    public static void SetLocalX(this Transform transform, float targetX)
    {
        var localPos = transform.localPosition;
        localPos.x = targetX;

        transform.localPosition = localPos;
    }
}
