using System;
using System.Collections.Generic;

//Event Bus system
//Usage exameple
//EventBus.Publish(new GameOverEvent())
//EventBus.Subscribe<GameOverEvent>((x)=>DoSomething(x))

public static class EventBus
{
    private static Dictionary<Type, List<Delegate>> subs = new();

    public static void Subscribe<T>(Action<T> handler) where T : IGameEvent { 
        Type type = typeof(T);
        if (!subs.TryGetValue(type, out List<Delegate> list)) {
            list = new();
            subs[type] = list;
        }
        list.Add(handler);
    }
    public static void UnSubscribe<T>(Action<T> handler) where T : IGameEvent
    {
        Type type = typeof(T);
        if (!subs.TryGetValue(type, out List<Delegate> list))
        {
            list.Remove(handler);
            if(list.Count == 0) subs.Remove(type);
        }
    }
    public static void Publish<T>(T e) where T : IGameEvent {
        Type type = typeof(T);
        if(subs.TryGetValue(type,out List<Delegate> list))
        {
            Delegate[] array = list.ToArray();
            foreach(Delegate d in array)
            {
                ((Action<T>)d).Invoke(e);   
            }
        }
    }
}
public interface IGameEvent { }