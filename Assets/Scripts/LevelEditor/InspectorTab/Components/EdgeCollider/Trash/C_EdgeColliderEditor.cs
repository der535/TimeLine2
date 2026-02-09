// using EventBus;
// using TimeLine.CustomInspector.Logic.Parameter;
// using TimeLine.EventBus.Events.EditroSceneCamera;
// using TimeLine.Installers;
// using TimeLine.LevelEditor.General;
// using UnityEngine;
// using Zenject;
//
// namespace TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components.EdgeCollider
// {
//     public class C_EdgeColliderEditor : MonoBehaviour
//     {
//         [SerializeField] private LineRenderer _lineRenderer;
//         
//         private GridScene _gridScene;
//         private GameEventBus _gameEventBus;
//         private SceneToRawImageConverter _sceneToRawImageConverter;
//         private CameraReferences _cameraReferences;
//
//         private C_EditColliderState _cEditState;
//         private C_EdgeColliderSelectionCube _cEdgeColliderSelectionCube;
//         
//         private M_EdgeColliderEditorGeometry _mEdgeColliderEditorGeometry;
//         private M_EdgeColliderEditorMousePosition _mEdgeColliderEditorMousePosition;
//         
//         private M_EdgeColliderEditorData _mEdgeColliderEditorData = new();
//         private M_EdgeColliderEditorView _mEdgeColliderEditorView;
//
//         [Inject]
//         void Construct(
//             GridScene gridScene, 
//             GameEventBus eventBus,
//             C_EdgeColliderSelectionCube cEdgeColliderSelectionCube,
//             C_EditColliderState cEditColliderState,
//             SceneToRawImageConverter sceneToRawImageConverter, 
//             CameraReferences references)
//         {
//             _cEditState = cEditColliderState;
//             _gridScene = gridScene;
//             _gameEventBus = eventBus;
//             _cEdgeColliderSelectionCube = cEdgeColliderSelectionCube;
//             _sceneToRawImageConverter = sceneToRawImageConverter;
//             _cameraReferences = references;
//         }
//
//
//
//         /// <summary>
//         /// Сетапит скрипты для работы с новым коллайдером
//         /// </summary>
//         /// <param name="parameter">Параметр хранящий точки коллайдера</param>
//         /// <param name="edgeCollider2DComponent">Скрипт коллайдера</param>
//         /// <param name="edgeColliderObject">Объект на который будут вешаться всяких дополнительные линии отрисовки колайдера</param>
//         /// <returns>Возвращает метод обновления визуализации колайдера</returns>
//         internal (M_EdgeColliderEditorView, M_EdgeColliderEditorData) Setup(ListVector2Parameter parameter, EdgeCollider2DComponent edgeCollider2DComponent, EdgeCollider2D edgeColliderComponent,
//             GameObject edgeColliderObject)
//         {
//             _mEdgeColliderEditorData = new M_EdgeColliderEditorData();
//             _mEdgeColliderEditorView = new M_EdgeColliderEditorView(_mEdgeColliderEditorData,_lineRenderer, edgeColliderObject);
//             _mEdgeColliderEditorMousePosition = new M_EdgeColliderEditorMousePosition(_sceneToRawImageConverter);
//             _mEdgeColliderEditorGeometry = new M_EdgeColliderEditorGeometry(_mEdgeColliderEditorData,
//                 _mEdgeColliderEditorMousePosition, _gridScene, edgeColliderObject);
//             _mEdgeColliderEditorGeometry.CalculateDynamicFields(_cameraReferences);
//             _mEdgeColliderEditorView.UpdateWidth();
//             _gameEventBus.SubscribeTo((ref EditorSceneCameraUpdateViewEvent data) =>
//             {
//                 _mEdgeColliderEditorGeometry.CalculateDynamicFields(_cameraReferences);
//                 _mEdgeColliderEditorView.UpdateWidth();
//             });
//             
//             _mEdgeColliderEditorView.Initialize();
//             _mEdgeColliderEditorData.ListVector2Parameter = parameter;
//             _mEdgeColliderEditorData.EdgeCollider2D = edgeColliderComponent;
//             _mEdgeColliderEditorData.EdgeCollider2DComponent = edgeCollider2DComponent;
//             _mEdgeColliderEditorView.UpdateOutline();
//             _mEdgeColliderEditorView.SetActiveLineRenderer(true);
//             return (_mEdgeColliderEditorView, _mEdgeColliderEditorData);
//         }
//
//         internal void SynchronizationListVector2Parameter()
//         {
//             _mEdgeColliderEditorGeometry.SynchronizationListVector2Parameter();
//         }
//         
//         private void Reset()
//         {
//             _mEdgeColliderEditorData.EdgeCollider2D.Reset();
//             _mEdgeColliderEditorView.UpdateOutline();
//             _mEdgeColliderEditorView.UpdateIntersectingSegments();
//             _mEdgeColliderEditorView.UpdateEdgeRadiusVisualization();
//             _mEdgeColliderEditorView.UpdateEdgeRadiusConnections();
//         }
//
//
//         void Update()
//         {
//             if (_mEdgeColliderEditorView == null || _mEdgeColliderEditorView.LineRenderer.enabled == false || _cEditState.GetState() == false) return;
//
//             Vector2 mouseWorldPos = _mEdgeColliderEditorMousePosition.GetMouseWorldPosition();
//             Vector2[] points = _mEdgeColliderEditorData.EdgeCollider2D.points;
//
//             // 1. Ищем САМУЮ близкую точку (вершину) среди всех существующих
//             int closestVertexIndex = -1;
//             float minVertexDistSqr = float.MaxValue;
//
//             for (int i = 0; i < points.Length; i++)
//             {
//                 float distSqr = Vector2.SqrMagnitude((Vector2)transform.TransformPoint(points[i]) - mouseWorldPos);
//                 if (distSqr < minVertexDistSqr)
//                 {
//                     minVertexDistSqr = distSqr;
//                     closestVertexIndex = i;
//                 }
//             }
//
//             // 2. Ищем ближайшую точку на ребрах (как и было)
//             var (segA, segB, minEdgeDistSqr, closestPointOnEdge) =
//                 _mEdgeColliderEditorGeometry.ClosestSegmentIndices(points, mouseWorldPos);
//
//             // Параметры для принятия решения
//             bool nearVertex = minVertexDistSqr <= _mEdgeColliderEditorData.DistanceToCornerDynamic *
//                 _mEdgeColliderEditorData.DistanceToCornerDynamic;
//             bool nearEdge = minEdgeDistSqr <= _mEdgeColliderEditorData.DistanceToEdgeDynamic *
//                 _mEdgeColliderEditorData.DistanceToEdgeDynamic;
//
//             // 3. Логика подсветки куба (приоритет вершине, если она близко)
//             if (nearVertex)
//             {
//                 _cEdgeColliderSelectionCube.SetActive(true);
//                 _cEdgeColliderSelectionCube.SetPosition(transform.TransformPoint(points[closestVertexIndex]));
//             }
//             else if (nearEdge)
//             {
//                 _cEdgeColliderSelectionCube.SetActive(true);
//                 _cEdgeColliderSelectionCube.SetPosition(closestPointOnEdge);
//             }
//             else
//             {
//                 _cEdgeColliderSelectionCube.SetActive(false);
//             }
//
//             // 4. Логика клика
//             if (!_mEdgeColliderEditorData.DraggedPointIndex.HasValue && UnityEngine.Input.GetMouseButtonDown(0))
//             {
//                 bool isCtrlPressed = UnityEngine.Input.GetKey(KeyCode.LeftControl) ||
//                                      UnityEngine.Input.GetKey(KeyCode.RightControl);
//
//                 if (nearVertex)
//                 {
//                     if (isCtrlPressed) _mEdgeColliderEditorGeometry.RemovePoint(closestVertexIndex);
//                     else _mEdgeColliderEditorGeometry.BeginDraggingPoint(closestVertexIndex);
//                 }
//                 else if (nearEdge)
//                 {
//                     // Если мы не у вершины, но у ребра — создаем новую точку
//                     int newIndex = _mEdgeColliderEditorGeometry.InsertPointBetween(segA, segB, closestPointOnEdge);
//                     if (newIndex >= 0) _mEdgeColliderEditorGeometry.BeginDraggingPoint(newIndex);
//                 }
//             }
//
//             _mEdgeColliderEditorView.UpdatePoint(_mEdgeColliderEditorGeometry.UpdateDraggedPoint());
//             _mEdgeColliderEditorView.UpdateIntersectingSegments();
//             _mEdgeColliderEditorView.UpdateEdgeRadiusVisualization();
//             _mEdgeColliderEditorView.UpdateEdgeRadiusConnections();
//         }
//
//
//         void OnDestroy()
//         {
//             foreach (var obj in _mEdgeColliderEditorData.IntersectingSegmentObjects)
//                 Destroy(obj);
//
//             foreach (var lr in _mEdgeColliderEditorView.RadiusVisualizers)
//                 Destroy(lr.gameObject);
//
//             foreach (var lr in _mEdgeColliderEditorView.EdgeConnectionVisualizers)
//                 Destroy(lr.gameObject);
//         }
//     }
// }