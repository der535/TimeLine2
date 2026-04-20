using System.Collections.Generic;
using TimeLine.LevelEditor.ECS.Services;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine.LevelEditor.TransformationSquare.Service
{
    public class TransformationSquareUpdateSquare
    {
        private SceneToRawImageConverter _sceneToRawImageConverter;
        private TransformationSquareView _view;
        private TransformationSquareData _data;

        public TransformationSquareUpdateSquare(SceneToRawImageConverter sceneToRawImageConverter,
            TransformationSquareView view, TransformationSquareData data)
        {
            _sceneToRawImageConverter = sceneToRawImageConverter;
            _view = view;
            _data = data;
        }

        public void UpdateGroupOBB(List<Entity> _selectedEntities, bool writeRecaculatedPitot = true)
        {
            if( _selectedEntities == null || _selectedEntities.Count == 0) return;
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
            if(!em.Exists(_selectedEntities[^1])) return;
            
            LocalTransform activeLT = em.GetComponentData<LocalTransform>(_selectedEntities[^1]);

            // Создаем матрицу, которая "обнуляет" поворот для расчетов
            // Мы берем позицию центра активного объекта и его поворот
            var pivotToWorldMatrix = float4x4.TRS(activeLT.Position, activeLT.Rotation, new float3(1));
            var worldToPivotMatrix  = math.inverse(pivotToWorldMatrix);
            
            
            float3 min = new float3(float.MaxValue);
            float3 max = new float3(float.MinValue);

            foreach (var entity in _selectedEntities)
            {
                LocalTransform lt = em.GetComponentData<LocalTransform>(entity);
                PostTransformMatrix ptm = em.GetComponentData<PostTransformMatrix>(entity);
                float3 scale = GetScaleFromMatrix.Get(ptm.Value);

                // Углы текущего объекта в мировом пространстве
                float3[] worldCorners = GetWorldCorners(lt, scale);

                foreach (var wc in worldCorners)
                {
                    // Переводим мировой угол в локальное пространство нашей рамки
                    float3 localP = math.transform(worldToPivotMatrix, wc);

                    min = math.min(min, localP);
                    max = math.max(max, localP);
                }
            }
            
            if (writeRecaculatedPitot)
            {
                _data.PivotToWorldMatrix = pivotToWorldMatrix;
                _data.WorldToPivotMatrix = worldToPivotMatrix;
                _data.CurrentLocalMin = min; // Сохраняем вычисленный min
                _data.CurrentLocalMax = max; // Сохраняем вычисленный max
            }

            
            
            // Теперь у нас есть min/max в локальном пространстве. 
            // Переводим 4 точки рамки обратно в мир, а затем в UI.
            float3[] obbCorners = new float3[4]
            {
                new(min.x, max.y, 0), // TL
                new(max.x, max.y, 0), // TR
                new(max.x, min.y, 0), // BR
                new(min.x, min.y, 0) // BL
            };

            Vector2[] uiPoints = new Vector2[5];
            RectTransform rectTrans = (RectTransform)_view.lineRenderer.transform;

            for (int i = 0; i < 4; i++)
            {
                float3 worldP = math.transform(pivotToWorldMatrix, obbCorners[i]);
                uiPoints[i] = _sceneToRawImageConverter.WorldToUIAnchoredPosition(worldP, rectTrans);
            }

            uiPoints[4] = uiPoints[0];

            _view.lineRenderer.SetPoints(uiPoints);
            _data.UIPoints = uiPoints; // Сохраняем для логики MouseInResizeArea

            _view.circleLeftTop.anchoredPosition = uiPoints[0];
            _view.circleRightTop.anchoredPosition = uiPoints[1];
            _view.circleRightBottom.anchoredPosition = uiPoints[2];
            _view.circleLeftBottom.anchoredPosition = uiPoints[3];
            
            float3 localCenter = (min + max) * 0.5f;
            _data.GroupCenter = math.transform(_data.PivotToWorldMatrix, localCenter);

        }

// Вспомогательная функция для получения 8 (или 4 для 2D) углов объекта в мире
        private float3[] GetWorldCorners(LocalTransform lt, float3 scale)
        {
            float3 h = scale * 0.5f;
            return new float3[]
            {
                lt.Position + math.rotate(lt.Rotation, new float3(-h.x, h.y, 0)),
                lt.Position + math.rotate(lt.Rotation, new float3(h.x, h.y, 0)),
                lt.Position + math.rotate(lt.Rotation, new float3(h.x, -h.y, 0)),
                lt.Position + math.rotate(lt.Rotation, new float3(-h.x, -h.y, 0))
            };
        }
    }
}