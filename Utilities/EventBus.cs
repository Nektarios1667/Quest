
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Utilities;

public static class EventBus
{
    private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    public static void Publish<TEvent>(TEvent eventData)
    {
        var eventType = typeof(TEvent);
        if (!_subscribers.TryGetValue(eventType, out var subscribers))
            return;

        foreach (var subscriber in subscribers)
        {
            ((Action<TEvent>)subscriber)(eventData);
        }
    }
    public static void Subscribe<TEvent>(Action<TEvent> handler)
    {
        var eventType = typeof(TEvent);
        if (!_subscribers.ContainsKey(eventType))
        {
            _subscribers[eventType] = new List<Delegate>();
        }
        _subscribers[eventType].Add(handler);
    }
    public static void Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        var eventType = typeof(TEvent);
        if (_subscribers.TryGetValue(eventType, out var subscribers))
        {
            subscribers.Remove(handler);
            if (subscribers.Count == 0)
            {
                _subscribers.Remove(eventType);
            }
        }
    }
}
