using Unity.Physics;
using Unity.Mathematics;

public static class PhysicsExtensions
{
    // Теперь ты сможешь писать: if(myCollider.TryGetBox(out var geometry)) ...
    public static unsafe bool TryGetBox(this PhysicsCollider collider, out BoxGeometry geometry)
    {
        geometry = default;
        if (!collider.Value.IsCreated) return false;
        
        // Получаем указатель на данные
        Collider* ptr = (Collider*)collider.Value.GetUnsafePtr();
        
        if (ptr->Type == ColliderType.Box)
        {
            // Извлекаем геометрию напрямую
            geometry = ((BoxCollider*)ptr)->Geometry;
            return true;
        }

        return false;
    }
    
    public static unsafe bool TryGetSphere(this PhysicsCollider collider, out SphereGeometry geometry)
    {
        geometry = default;
        if (!collider.Value.IsCreated) return false;
        
        // Получаем указатель на данные
        Collider* ptr = (Collider*)collider.Value.GetUnsafePtr();
        
        if (ptr->Type == ColliderType.Sphere)
        {
            // Извлекаем геометрию напрямую
            geometry = ((SphereCollider*)ptr)->Geometry;
            return true;
        }

        return false;
    }
}