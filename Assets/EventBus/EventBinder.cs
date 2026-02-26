using System;
using System.Collections.Generic;
using GenericEventBus;

public class EventBinder : IDisposable
{
    // Храним действия по отписке. 
    // Замыкание внутри лямбды гарантирует, что мы вызовем отписку именно того экземпляра, 
    // который был создан при подписке.
    private readonly List<Action> _unsubscribers = new List<Action>();

    public EventBinder Add<TBase, TEvent>(
        GenericEventBus<TBase> bus, 
        GenericEventBus<TBase>.EventHandler<TEvent> handler) 
        where TEvent : TBase
    {
        // Подписываем конкретный экземпляр handler
        bus.SubscribeTo(handler);

        // Сохраняем замыкание, которое точно знает, от какой шины и какого хендлера отписываться
        _unsubscribers.Add(() => bus.UnsubscribeFrom(handler));
        
        return this;
    }

    public void Dispose()
    {
        // Вызываем все сохраненные действия отписки
        foreach (var unsub in _unsubscribers)
        {
            unsub?.Invoke();
        }
        _unsubscribers.Clear();
    }
}