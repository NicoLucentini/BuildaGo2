using System;
using System.Collections.Generic;

public class ObjectPool<T>
{

    private readonly Func<T> _factoryMethod;
    private readonly Action<T> _resetMethod;
    private readonly Stack<T> _pool;
    private readonly string poolName;

    public int Count => _pool.Count;

    public ObjectPool(string poolName, Func<T> factoryMethod, Action<T> resetMethod = null, int initialSize = 0)
    {
        _factoryMethod = factoryMethod ?? throw new ArgumentNullException(nameof(factoryMethod));
        _resetMethod = resetMethod;
        _pool = new Stack<T>(initialSize);
        this.poolName = poolName;

        // Pre-fill the pool
        for (int i = 0; i < initialSize; i++)
        {
            _pool.Push(_factoryMethod());
        }
    }

    public T Get()
    {
        return _pool.Count > 0 ? _pool.Pop() : _factoryMethod();
    }

    public void Release(T obj)
    {
        _resetMethod?.Invoke(obj);
        _pool.Push(obj);
    }


    public static Dictionary<string, object> pools = new();
    public static ObjectPool<T> Create<T>(string poolName, int size, Func<T> fact, Action<T> reset) where T : class{
        ObjectPool<T> pool = new ObjectPool<T>(poolName, fact, reset, size);
        
        RegisterPool<T>(pool, poolName);
        return pool;
    }
    public static ObjectPool<T> GetPool<T>(string poolName) where T : class
    {
        Type type = typeof(T);

        if (!pools.TryGetValue(poolName, out object poolObj))
        {
            throw new Exception($"Pool for type {type} does not exist. Call RegisterPool<T>() first.");
        }

        return (ObjectPool<T>)poolObj;
    }

    public static void RegisterPool<T>(ObjectPool<T> pool, String poolName) where T : class
    {
        Type type = typeof(T);

        if (pools.ContainsKey(poolName))
            throw new Exception($"Pool for type {type} already registered");

        pools[poolName] = pool;
    }

    public static void ClearAllPools()
    {
        pools.Clear();
    }
}
public static class TestPool {

    static void Main() {

        Talent t = new Talent();
        var op = ObjectPool<Talent>.Create("Talents",5, Talent.Create, Talent.Reset);
    }
}
class Talent 
{
    public static Talent Create()
    {
        throw new NotImplementedException();
    }

    public static void Reset(Talent t)
    {
        throw new NotImplementedException();
    }
}