using DG.Tweening;
using EventBus;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine
{
    public class CompositionEdit : MonoBehaviour
    {
        [SerializeField] private SaveComposition composition;
        [SerializeField] private SaveLevel saveLevel;
        [FormerlySerializedAs("objectSpawner")] [FormerlySerializedAs("trackObjectSpawner")] [SerializeField] private FacadeObjectSpawner facadeObjectSpawner;
        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [SerializeField] private GroupCreater groupCreater;
        [SerializeField] private GroupSeparate groupSeparate;
        [SerializeField] private TrackObjectRemover trackObjectRemover;
        [SerializeField] private CompositionUpdater compositionUpdater;
        
        private GameEventBus _gameEventBus;
        private string _compositionID;
        private string _savedName;

        [Inject]
        void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }
        
        internal void Edit(GroupGameObjectSaveData compositionData)
        {
            _gameEventBus.Raise(new StartCompositionEdit(compositionData));
            
            _compositionID = compositionData.compositionID;
            _savedName = compositionData.gameObjectName;
            trackObjectStorage.HideAll();
            var (trackObjectGroup, game, _) =
                facadeObjectSpawner.LoadComposition(compositionData, compositionData.compositionID);
            groupSeparate.SeparateSingle((TrackObjectGroup)trackObjectStorage.GetTrackObjectData(game));
        }

        public void EndEdit()
        {
            TrackObjectGroup trackObjectGroup = groupCreater.Create(trackObjectStorage.GetAllActiveTrackData(), _compositionID);
            trackObjectGroup.sceneObject.GetComponent<NameComponent>().Name.Value = _savedName;
            trackObjectStorage.ShowAll();
            composition.EditComposition(saveLevel.SaveGroup(trackObjectGroup), trackObjectGroup.compositionID);
            trackObjectRemover.SingleRemove(trackObjectGroup, false);
            compositionUpdater.UpdateCompositions();
            
            _gameEventBus.Raise(new EndCompositionEdit());
        }
    }
}