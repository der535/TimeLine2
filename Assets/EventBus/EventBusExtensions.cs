using GenericEventBus;

public static class EventBusExtensions
{
    // Метод расширения "Sub" для краткой записи
    public static GenericEventBus<TBase> Sub<TBase, TEvent>(
        this GenericEventBus<TBase> bus, 
        GenericEventBus<TBase>.EventHandler<TEvent> handler) 
        where TEvent : TBase
    {
        bus.SubscribeTo(handler);
        return bus;
    }
}