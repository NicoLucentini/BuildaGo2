using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CustomExtensions
{
    
}

public static class StringHelper {

    public static string WithColor(this string msg, Color col) {
        return "<color=#" + ColorUtility.ToHtmlStringRGBA(col) + ">" + msg + "</color>";
    }
    public static string StringColor(string msg, Color col)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGBA(col) + ">" + msg + "</color>";
    }
    public static string Enter(this string msg) {
        var sb = new StringBuilder(msg.Length  + Environment.NewLine.Length);
        sb.Append(msg);
        sb.Append(Environment.NewLine);
        return sb.ToString();
    }
}
public static class Vector3Extension {

    public static Vector3 WithY(this Vector3 v, float y) {
        return new Vector3(v.x, y, v.z);
    }
    public static Vector3 WithX(this Vector3 v, float x)
    {
        return new Vector3(x, v.y, v.z);
    }
    public static Vector3 WithZ(this Vector3 v, float z)
    {
        return new Vector3(v.x, v.y, z);
    }
    public static Vector3 WithOffset(this Vector3 v, Vector3 offset) {
        return new Vector3(v.x, v.y, v.z) + offset;
    }
    public static Vector3 WithOffset(this Vector3 v,float x, float y,  float z)
    {
        return new Vector3(v.x +x, v.y+y, v.z+z) ;
    }
    public static String ToStringValues(this Vector3 v) {
        return $"[{v.x},{v.y},{v.z}]";
    }
}
public static class IntExtension
{
   
}
public static class MathHelper
{
    public static float Map(float value, float min, float max, float mapMin, float mapMax)
    {
        if (value < min) return mapMin;
        else if (value > max) return mapMax;
        else
        {
            float coef = value / max;
            return mapMin + (mapMax - mapMin) * coef;
        }

    }
    public static float Map2(float value, float min, float max, float mapMin, float mapMax)
    {
        if(value<min) return mapMin;
        if (value > max) return mapMax;

        float coef = (value - min) / max;
        return mapMin + (mapMax - mapMin) * coef;
    }
}
public static class MonobehaviourExtension
{

    public static List<T> GetComponentsInChildrenOnly<T>(this GameObject component) where T : Component
    {
        var comps = component.GetComponentsInChildren<T>().ToList();
        comps.RemoveAll(x => x.gameObject == component.gameObject);
        return comps;
    }
}
public static class ListExtension {

    public static void Shuffle<T>(this IList<T> list)
    {
        var rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1); // random index between 0 and n
            (list[n], list[k]) = (list[k], list[n]); // swap
        }
    }
    public static void DestroyAndClearList<T>(this List<T> items, bool inmediate = false) where T : Component
    {
        if (items == null || items.Count == 0) return;

        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i] == null) continue;

            if(inmediate)
                UnityEngine.Object.DestroyImmediate(items[i].gameObject);
            else
                UnityEngine.Object.Destroy(items[i].gameObject);
        }
        items.Clear();
    }
    public static void DestroyAndClearList<T>(this List<GameObject> items, bool inmediate = false) 
    {
        if (items == null || items.Count == 0) return;

        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i] == null) continue;
            
            if (inmediate)
                UnityEngine.Object.DestroyImmediate(items[i]);
            else
                UnityEngine.Object.Destroy(items[i]);
        }
        items.Clear();
    }
}
public static class ColorExtension
{
    public static Color ToRgb(this Color color)
    {
        color.r = color.r / 255f;
        color.g = color.g / 255f;
        color.b = color.b / 255f;
        color.a = color.a / 255f;
        return color;
    }
    public static Color WithAlpha(this Color color, int alpha) {

        color.a = alpha;
        return color;
    }
}
public static class LineRendererExtension {

    public static void SetColor(this LineRenderer lr, Color start, Color end) {
        lr.startColor = start;
        lr.endColor = end;
    }
}
public static class DictionaryExtension {
    public static void AddOrUpgrade(this Dictionary<LaborType, Data<int>> dic, LaborType type, int amount) {
        if (dic.ContainsKey(type))
        {
            dic[type].Value += amount;
        }
        else {
            dic.Add(type, new Data<int>(amount));
        }
    }
    public static void AddOrUpgrade<T>(this Dictionary<T, int> dic, T type, int amount)
    {
        if (dic.ContainsKey(type))
        {
            dic[type] += amount;
        }
        else
        {
            dic.Add(type, amount);
        }
    }
}

public static class CustomRandom
{
    public static bool Get(int chance)
    {
        int random = UnityEngine.Random.Range(0, 100);
        return chance > random;
    }

    public static int Random(this Vector2Int val)
    {
        return UnityEngine.Random.Range(val.x, val.y);
    }
    public static T Random<T>(this List<T> val)
    {
        if (val.Count > 0)
        {
            return val[UnityEngine.Random.Range(0, val.Count)];
        }
        else
        {
            return default(T);
        }
    }
    public static T Random<T>(this T[] val)
    {
        if (val.Length > 0)
        {
            return val[UnityEngine.Random.Range(0, val.Length)];
        }
        else
        {
            return default(T);
        }
    }
    public static List<T> RandomUniques<T>(this List<T> val, int count, bool repeatIfMax = false)
    {
        List<T> newList = new List<T>();
        List<T> copyList = new List<T>(val);
        int realC = val.Count >= count ? count : val.Count;
        bool hasToRepeat = false;
        if (val.Count < count && repeatIfMax)
            hasToRepeat = true;

        for (int i = 0; i < realC; i++) {
            var r = copyList.Random();
            newList.Add(r);
            if(!hasToRepeat)
                copyList.Remove(r);
        }
        return newList;
    }
    public static T GetRandomEnum<T>() where T : Enum
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }
    public static List<T> GetEnumsAsList<T>() where T : Enum
    {
        List<T> res = new();
        var values = Enum.GetValues(typeof(T));
        foreach (var v in values) {
            res.Add((T)v);
        }
        return res;
    }
    public static int RandomRoulette(List<int> chances)
    {

        int total = chances.Sum();
        int currentSum = 0;
        int randomValue = UnityEngine.Random.Range(0, total); ;
        for (int i = 0; i < chances.Count; i++)
        {
            currentSum += chances[i];
            if (randomValue < currentSum)
            {
                return i;
            }
        }
        return chances[chances.Count - 1];
    }

}

