using System;
using System.Collections.Generic;
using GenericEventBus;

public class EventBinder : IDisposable
{
    private readonly List<Action> _unsubscribers = new List<Action>();

    // Метод теперь возвращает сам EventBinder, что позволяет строить цепочку
    public EventBinder Add<TBase, TEvent>(GenericEventBus<TBase> bus, GenericEventBus<TBase>.EventHandler<TEvent> handler) 
        where TEvent : TBase
    {
        bus.SubscribeTo(handler);
        _unsubscribers.Add(() => bus.UnsubscribeFrom(handler));
        return this; // Возвращаем себя для следующего вызова
    }

    public void Dispose()
    {
        foreach (var unsub in _unsubscribers) unsub?.Invoke();
        _unsubscribers.Clear();
    }
}