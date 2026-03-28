using Unity.Entities;
using Unity.Mathematics;

namespace TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components
{
    // Это структура данных ВНУТРИ Blob-файла (в памяти)
    public struct PolygonPointsBlob
    {
        public BlobArray<float2> Points;
    }
    // Это компонент, который вешается на Entity
    public struct PolygonColliderData : IComponentData
    {
        public BlobAssetReference<PolygonPointsBlob> PointsReference;
        public bool IsTrigger;
        public bool IsDangerous;
    }
}