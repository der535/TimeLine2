using System.Collections.Generic;
using TimeLine.LevelEditor.ECS.Components;
using TimeLine.LevelEditor.ECS.Services;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using BoxCollider = Unity.Physics.BoxCollider;
using Collider = Unity.Physics.Collider;
using Material = Unity.Physics.Material;
using MeshCollider = Unity.Physics.MeshCollider;

public partial struct UpdateColliderSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Используем .WithChangeFilter<LocalToWorld>(), чтобы цикл работал 
        // ТОЛЬКО когда объект переместился или изменил масштаб
        foreach (var (colliderRef, boxColliderTag, ltw, entity) in SystemAPI
                     .Query<RefRW<PhysicsCollider>, RefRO<BoxColliderData>, RefRO<LocalToWorld>>()
                     .WithChangeFilter<LocalToWorld, BoxColliderData>().WithEntityAccess())
                     
        {
            float3 scaleObject = GetScaleFromMatrix.Get(ltw.ValueRO.Value);

            var geometry = new BoxGeometry
            {
                Center = boxColliderTag.ValueRO.boxCenter,
                Size = boxColliderTag.ValueRO.boxSize * scaleObject,
                Orientation = quaternion.identity,
                BevelRadius = 0.02f
            };

            var material = new Material
            {
                CollisionResponse = boxColliderTag.ValueRO.isTrigger
                    ? CollisionResponsePolicy.RaiseTriggerEvents
                    : CollisionResponsePolicy.CollideRaiseCollisionEvents,
                Friction = 0.5f,
                Restitution = 0f
            };

            // Создаем новый. 
            // УДАЛЯЕМ ручной .Dispose() старого colliderRef.ValueRO.Value!
            // Unity сама очистит память, когда вы перезапишете ссылку ниже.
            var newCollider = BoxCollider.Create(geometry, CollisionFilter.Default, material);

            // Присваиваем новую ссылку
            colliderRef.ValueRW.Value = newCollider;

            // Логика тега опасности
            bool hasTag = SystemAPI.HasComponent<DangerousObjectTag>(entity);
            if (boxColliderTag.ValueRO.isDangerous && !hasTag)
                ecb.AddComponent<DangerousObjectTag>(entity);
            else if (!boxColliderTag.ValueRO.isDangerous && hasTag)
                ecb.RemoveComponent<DangerousObjectTag>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

public partial struct UpdateCircleColliderSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Используем EntityCommandBuffer для безопасного изменения сущностей
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (colliderRef, circleColliderTag, transformMatrix, entity) in SystemAPI
                     .Query<RefRW<PhysicsCollider>, RefRO<CircleColliderData>, RefRO<LocalToWorld>>()  
                     .WithChangeFilter<LocalToWorld, CircleColliderData>().WithEntityAccess())
            
        {
            // ОПТИМИЗАЦИЯ: обновляем коллайдер ТОЛЬКО если размер изменился
            // (Вам нужно хранить старый размер в компоненте, чтобы сравнивать)

            float3 scaleObject = GetScaleFromMatrix.Get(transformMatrix.ValueRO.Value);

            var geometry = new SphereGeometry()
            {
                Center = circleColliderTag.ValueRO.center,
                Radius = circleColliderTag.ValueRO.radius * math.max(scaleObject.x, scaleObject.y)
            };

            // 1. Создаем материал и помечаем его как триггер
            var material = new Material
            {
                // Это превращает коллайдер в триггер
                CollisionResponse = circleColliderTag.ValueRO.isTrigger
                    ? CollisionResponsePolicy.RaiseTriggerEvents
                    : CollisionResponsePolicy.CollideRaiseCollisionEvents,
                Friction = 0.5f,
                Restitution = 0f,
                CustomTags = 0
            };

            var filter = CollisionFilter.Default; // Или ваш кастомный

            // 1. Создаем НОВЫЙ BlobAsset
            var newCollider = Unity.Physics.SphereCollider.Create(geometry, filter, material);


            // 3. Присваиваем новый
            colliderRef.ValueRW.Value = newCollider;
            
            bool hasTag = SystemAPI.HasComponent<DangerousObjectTag>(entity);
            if (circleColliderTag.ValueRO.isDangerous && !hasTag)
                ecb.AddComponent<DangerousObjectTag>(entity);
            else if (!circleColliderTag.ValueRO.isDangerous && hasTag)
                ecb.RemoveComponent<DangerousObjectTag>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

public partial struct UpdatePolygonColliderSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (tag, collider, ltw, entity) in SystemAPI
                     .Query<RefRW<PolygonColliderData>, RefRW<PhysicsCollider>, RefRO<LocalToWorld>>()
                     .WithChangeFilter<LocalToWorld, PolygonColliderData>().WithEntityAccess())
        {
            float3 scale = new float3(
                math.length(ltw.ValueRO.Value.c0.xyz),
                math.length(ltw.ValueRO.Value.c1.xyz),
                math.length(ltw.ValueRO.Value.c2.xyz)
            );

            if (collider.ValueRO.Value.IsCreated)
            {
                collider.ValueRW.Value.Dispose();
            }

            var points = tag.ValueRO.PointsReference.Value.Points.ToArray();
            float3[] scaledPoints = new float3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                scaledPoints[i] = new float3(points[i].x * scale.x, points[i].y * scale.y, 0);
            }

            // ВАЖНО: Здесь вам нужно получить индексы треугольников (триангуляция).
            // Это массив индексов точек из scaledPoints, которые образуют треугольники (по 3 на каждый).
            int[] triangles = TriangulatePolygon(scaledPoints);

            // Создаем вогнутый меш-коллайдер
            collider.ValueRW.Value = InstallConcaveMesh(scaledPoints, triangles, tag.ValueRO.IsTrigger);
            
            bool hasTag = SystemAPI.HasComponent<DangerousObjectTag>(entity);
            if (tag.ValueRO.IsDangerous && !hasTag)
                ecb.AddComponent<DangerousObjectTag>(entity);
            else if (!tag.ValueRO.IsDangerous && hasTag)
                ecb.RemoveComponent<DangerousObjectTag>(entity);
        }
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    public BlobAssetReference<Collider> InstallConcaveMesh(float3[] points, int[] triangles2D, bool isTrigger)
    {
        int pointCount = points.Length;
        int triangleCount2D = triangles2D.Length / 3;

        // Вершины: передняя грань (Z = 0.05f) и задняя грань (Z = -0.05f)
        int totalVertices = pointCount * 2;
        var vertices = new NativeArray<float3>(totalVertices, Allocator.Temp);

        for (int i = 0; i < pointCount; i++)
        {
            vertices[i] = new float3(points[i].x, points[i].y, 0.05f); // Передние
            vertices[i + pointCount] = new float3(points[i].x, points[i].y, -0.05f); // Задние
        }

        // Индексы для MeshCollider (int3 представляет один треугольник)
        // Количество треугольников: передняя + задняя стенки + боковые стенки (по 2 треугольника на ребро)
        int totalTriangles = (triangleCount2D * 2) + (pointCount * 2);
        var indices = new NativeArray<int3>(totalTriangles, Allocator.Temp);

        int indexOffset = 0;

        // 1. Передняя стенка (сохраняем порядок индексов из триангуляции)
        for (int i = 0; i < triangleCount2D; i++)
        {
            indices[indexOffset++] = new int3(
                triangles2D[i * 3],
                triangles2D[i * 3 + 1],
                triangles2D[i * 3 + 2]
            );
        }

        // 2. Задняя стенка (меняем порядок индексов (x, z, y) чтобы нормали смотрели наружу)
        for (int i = 0; i < triangleCount2D; i++)
        {
            indices[indexOffset++] = new int3(
                triangles2D[i * 3] + pointCount,
                triangles2D[i * 3 + 2] + pointCount,
                triangles2D[i * 3 + 1] + pointCount
            );
        }

        // 3. Боковые стенки (закрываем периметр)
        for (int i = 0; i < pointCount; i++)
        {
            int next = (i + 1) % pointCount;

            // Вершины квада (i, next, next + pointCount, i + pointCount)
            int v1 = i;
            int v2 = next;
            int v3 = next + pointCount;
            int v4 = i + pointCount;

            // Первый треугольник боковины
            indices[indexOffset++] = new int3(v1, v2, v3);
            // Второй треугольник боковины
            indices[indexOffset++] = new int3(v1, v3, v4);
        }

        var material = new Material
        {
            // Это превращает коллайдер в триггер
            CollisionResponse = isTrigger
                ? CollisionResponsePolicy.RaiseTriggerEvents
                : CollisionResponsePolicy.CollideRaiseCollisionEvents,
            Friction = 0.5f,
            Restitution = 0f,
            CustomTags = 0
        };

        // Создаем сам коллайдер
        var newCollider = MeshCollider.Create(vertices, indices, CollisionFilter.Default, material);

        vertices.Dispose();
        indices.Dispose();

        return newCollider;
    }

    private int[] TriangulatePolygon(float3[] points)
    {
        List<int> indices = new List<int>();
        int n = points.Length;
        if (n < 3) return indices.ToArray();

        // Создаем список индексов точек
        List<int> indexList = new List<int>();
        for (int i = 0; i < n; i++) indexList.Add(i);

        // Проверяем направление обхода (нужно CCW или CW для корректной логики)
        bool isClockwise = IsPolygonClockwise(points);

        int iterations = 0;
        while (indexList.Count > 3)
        {
            bool earFound = false;

            for (int i = 0; i < indexList.Count; i++)
            {
                int prev = indexList[(i + indexList.Count - 1) % indexList.Count];
                int curr = indexList[i];
                int next = indexList[(i + 1) % indexList.Count];

                if (IsEar(prev, curr, next, points, indexList, isClockwise))
                {
                    // Нашли ухо — записываем треугольник
                    indices.Add(prev);
                    indices.Add(curr);
                    indices.Add(next);

                    // Удаляем вершину уха
                    indexList.RemoveAt(i);
                    earFound = true;
                    break;
                }
            }

            // Страховка от бесконечного цикла (если геометрия сломана)
            iterations++;
            if (!earFound || iterations > n * n) break;
        }

        // Добавляем последний оставшийся треугольник
        indices.Add(indexList[0]);
        indices.Add(indexList[1]);
        indices.Add(indexList[2]);

        return indices.ToArray();
    }

    private bool IsEar(int p, int c, int n, float3[] points, List<int> indexList, bool isClockwise)
    {
        float2 a = points[p].xy;
        float2 b = points[c].xy;
        float2 d = points[n].xy;

        // 1. Проверяем, является ли угол выпуклым
        float crossProduct = (b.x - a.x) * (d.y - a.y) - (b.y - a.y) * (d.x - a.x);
        bool isConvex = isClockwise ? crossProduct <= 0 : crossProduct >= 0;

        if (!isConvex) return false;

        // 2. Проверяем, нет ли других точек внутри этого треугольника
        for (int i = 0; i < indexList.Count; i++)
        {
            int index = indexList[i];
            if (index == p || index == c || index == n) continue;

            if (IsPointInTriangle(points[index].xy, a, b, d)) return false;
        }

        return true;
    }

    private bool IsPointInTriangle(float2 p, float2 a, float2 b, float2 c)
    {
        float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
        float s = 1 / (2 * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
        float t = 1 / (2 * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
        return s > 0 && t > 0 && (1 - s - t) > 0;
    }

    private bool IsPolygonClockwise(float3[] points)
    {
        float sum = 0;
        for (int i = 0; i < points.Length; i++)
        {
            float3 p1 = points[i];
            float3 p2 = points[(i + 1) % points.Length];
            sum += (p2.x - p1.x) * (p2.y + p1.y);
        }

        return sum > 0;
    }
}