// using System.Collections.Generic;
// using TimeLine.CustomInspector.Logic;
// using TimeLine.CustomInspector.Logic.Parameter;
// using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
// using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
// using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
// using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
// using TimeLine.TimeLine;
// using UnityEngine;
// using Zenject;
//
// namespace TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Components
// {
//     public class PressEventComponent : BaseParameterComponent, IInitializedComponent
//     {
//         public CompositionParameter prefabPerfect = new("prefabPerfect", null, Color.blue);
//         public CompositionParameter prefabMiddel = new("prefabMiddel", null, Color.blue);
//         public CompositionParameter prefabMiss = new("prefabMiss", null, Color.blue);
//
//         public Vector2Parameter prefabPerfectPosition =
//             new Vector2Parameter("prefabPerfectPosition", "x", "y", Vector2.zero);
//         public Vector2Parameter prefabMiddelPosition =
//             new Vector2Parameter("prefabMiddelPosition", "x", "y", Vector2.zero);
//         public Vector2Parameter prefabMissPosition =
//             new Vector2Parameter("prefabMissPosition", "x", "y", Vector2.zero);
//
//         public FloatParameter perfectArea = new("Perfect_area", 30, Color.magenta);
//         public FloatParameter middleArea = new("Middle_area", 70, Color.magenta);
//
//         public KeyCodeParameter KeyCodeParameter = new KeyCodeParameter("Event key", KeyCode.Space, Color.green);
//
//         private TrackObjectStorage _trackObjectStorage;
//         private TrackObjectCustomizationRange _rerfectArea;
//         private TrackObjectCustomizationRange _middelArea;
//         private TrackObjectCustomizationRange _missArea;
//         private TrackObject _trackObject;
//         private Main _main;
//         private FacadeObjectSpawner _facadeObjectSpawner;
//
//         private TrackObjectPacket _trackObjectPacketPerfect;
//         private TrackObjectPacket _trackObjectPacketMiddel;
//         private TrackObjectPacket _trackObjectPacketMiss;
//
//         private TrackObjectRemover _trackObjectRemover;
//         private bool _isPressed;
//
//         [Inject]
//         private void Construct(TrackObjectStorage trackObjectStorage, DiContainer container, Main main,
//             FacadeObjectSpawner facadeObjectSpawner, TrackObjectRemover trackObjectRemover)
//         {
//             _trackObjectRemover = trackObjectRemover;
//             _trackObjectStorage = trackObjectStorage;
//             _facadeObjectSpawner = facadeObjectSpawner;
//             _main = main;
//         }
//
//         private void Start()
//         {
//             _trackObject = _trackObjectStorage.GetTrackObjectData(gameObject).trackObject.trackObject;
//             _missArea = _trackObject.CustomizationController().CreateRange(Color.red, 100);
//             _rerfectArea = _trackObject.CustomizationController().CreateRange(Color.yellow, middleArea.Value);
//             _middelArea = _trackObject.CustomizationController().CreateRange(Color.green, perfectArea.Value);
//
//             perfectArea.OnValueChanged += () => { _rerfectArea.UpdateImage(perfectArea.Value); };
//             middleArea.OnValueChanged += () => { _middelArea.UpdateImage(middleArea.Value); };
//
//             prefabPerfect.OnValueChanged += () =>
//             {
//                 if (_trackObjectPacketPerfect != null) _trackObjectRemover.SingleRemove(_trackObjectPacketPerfect);
//                 _trackObjectPacketPerfect = _facadeObjectSpawner
//                     .LoadComposition(prefabPerfect.Value, prefabPerfect.Value.compositionID, true).Item1;
//                 _trackObjectPacketPerfect.sceneObject.transform.position = prefabPerfectPosition.Value;
//                 _trackObjectPacketPerfect.trackObject.trackObject.Visual.SetActive(false);
//                 _trackObjectPacketPerfect.trackObject.trackObject.IsActive = false;
//                 _trackObjectPacketPerfect.trackObject.trackObject.IsTemp = true;
//             };
//             prefabMiddel.OnValueChanged += () =>
//             {
//                 if (_trackObjectPacketMiddel != null) _trackObjectRemover.SingleRemove(_trackObjectPacketMiddel);
//                 _trackObjectPacketMiddel = _facadeObjectSpawner
//                     .LoadComposition(prefabMiddel.Value, prefabMiddel.Value.compositionID, true).Item1;
//                 _trackObjectPacketMiddel.sceneObject.transform.position = prefabMiddelPosition.Value;
//                 _trackObjectPacketMiddel.trackObject.trackObject.Visual.SetActive(false);
//                 _trackObjectPacketMiddel.trackObject.trackObject.IsActive = false;
//                 _trackObjectPacketMiddel.trackObject.trackObject.IsTemp = true;
//             };
//             prefabMiss.OnValueChanged += () =>
//             {
//                 if (_trackObjectPacketMiss != null) _trackObjectRemover.SingleRemove(_trackObjectPacketMiss);
//                 _trackObjectPacketMiss = _facadeObjectSpawner
//                     .LoadComposition(prefabMiss.Value, prefabMiss.Value.compositionID, true).Item1;
//                 _trackObjectPacketMiss.sceneObject.transform.position = prefabMissPosition.Value;
//                 _trackObjectPacketMiss.trackObject.trackObject.Visual.SetActive(false);
//                 _trackObjectPacketMiss.trackObject.trackObject.IsActive = false;
//                 _trackObjectPacketMiss.trackObject.trackObject.IsTemp = true;
//             };
//
//             prefabPerfectPosition.OnValueChanged += () =>
//             {
//                 _trackObjectPacketPerfect.sceneObject.transform.position = prefabPerfectPosition.Value;
//             };
//             prefabMiddelPosition.OnValueChanged += () =>
//             {
//                 _trackObjectPacketMiddel.sceneObject.transform.position = prefabMiddelPosition.Value;
//             };
//             prefabMissPosition.OnValueChanged += () =>
//             {
//                 _trackObjectPacketMiss.sceneObject.transform.position = prefabMissPosition.Value;
//             };
//         }
//
//         private void Update()
//         {
//             if (UnityEngine.Input.GetKeyDown(KeyCodeParameter.Value) && !_isPressed)
//             {
//                 var center = _trackObject.StartTimeInTicks + _trackObject.TimeDuractionInTicks / 2;
//                 if (TimeLineConverter.Instance.TicksCurrentTime() <=
//                     center + _trackObject.TimeDuractionInTicks * (perfectArea.Value / 100 / 2) &&
//                     TimeLineConverter.Instance.TicksCurrentTime() >=
//                     center - _trackObject.TimeDuractionInTicks * (perfectArea.Value / 100 / 2))
//                 {
//                     if (prefabPerfect.Value != null)
//                     {
//                         _trackObjectPacketPerfect.trackObject.trackObject.SetTime(TimeLineConverter.Instance.TicksCurrentTime());
//                         _trackObjectPacketPerfect.trackObject.trackObject.IsActive = true;
//                     }
//
//                     print("HIT!");
//                 }
//                 else if (TimeLineConverter.Instance.TicksCurrentTime() <=
//                          center + _trackObject.TimeDuractionInTicks * (middleArea.Value / 100 / 2) &&
//                          TimeLineConverter.Instance.TicksCurrentTime() >=
//                          center - _trackObject.TimeDuractionInTicks * (middleArea.Value / 100 / 2))
//                 {
//                     if (prefabMiddel.Value != null)
//                     {
//                         _trackObjectPacketMiddel.trackObject.trackObject.SetTime(TimeLineConverter.Instance.TicksCurrentTime());
//                         _trackObjectPacketPerfect.trackObject.trackObject.IsActive = true;
//                     }
//
//                     print("Middel");
//                 }
//                 else
//                 {
//                     if (prefabMiss.Value != null)
//                     {
//                         _trackObjectPacketMiss.trackObject.trackObject.SetTime(TimeLineConverter.Instance.TicksCurrentTime());
//                         _trackObjectPacketPerfect.trackObject.trackObject.IsActive = true;
//                     }
//
//                     print("Miss");
//                 }
//
//                 _isPressed = true;
//             }
//         }
//
//         public override IEnumerable<InspectableParameter> GetParameters()
//         {
//             yield return perfectArea;
//         }
//
//         public void Initialized()
//         {
//             _isPressed = false;
//         }
//     }
// }