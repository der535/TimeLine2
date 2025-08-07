// using EventBus;
// using TimeLine;
// using TimeLine.EventBus.Events.Input;
// using TimeLine.Installers;
// using TimeLine.TimeLine;
// using UnityEngine;
// using Zenject;
//
// public class TimeLineKeyFrameRenderer : MonoBehaviour
// {
//     [SerializeField] private RectTransform keyframeRootObject;
//     
//     private TimeLineConverter _timeLineConverter;
//     private TimeLineRenderer _timeLineRenderer;
//     private GameEventBus _gameEventBus;
//     
//     private float _oldPan;
//
//     [Inject]
//     private void Construct(MainObjects mainObjects, TimeLineConverter timeLineConverter,
//         TimeLineRenderer timeLineRenderer, GameEventBus gameEventBus)
//     {
//         _timeLineConverter = timeLineConverter;
//         _timeLineRenderer = timeLineRenderer;
//         _gameEventBus = gameEventBus;
//     }
//
//     private void Start()
//     {
//         _gameEventBus.SubscribeTo((ref ScrollTimeLineKeyframeEvent scrollEvent) =>
//         {
//             keyframeRootObject.offsetMin += new Vector2(scrollEvent.ScrollOffset, 0); //Left
//             keyframeRootObject.offsetMax += new Vector2(scrollEvent.ScrollOffset, 0); //Right
//         }, 1);
//     }
//     //
//     // public void SetPosition(float position)
//     // {
//     //     _mainObjects.ContentRectTransform.offsetMin = new Vector2(position, 0); //Left
//     //     _mainObjects.ContentRectTransform.offsetMax = new Vector2(position, 0); //Right
//     //     _mainObjects.NotifyContentRectChanged();
//     // }
// }