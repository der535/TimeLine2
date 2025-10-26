using DG.Tweening;
using EventBus;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class CompositionEdit : MonoBehaviour
    {
        [SerializeField] private SaveComposition composition;
        [SerializeField] private SaveLevel saveLevel;
        [SerializeField] private TrackObjectSpawner trackObjectSpawner;
        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [SerializeField] private GroupCreater groupCreater;
        [SerializeField] private GroupSeparate groupSeparate;
        [SerializeField] private TrackObjectRemover trackObjectRemover;
        [SerializeField] private CompositionUpdater compositionUpdater;
        
        private GameEventBus _gameEventBus;
        private string _compositionID;

        [Inject]
        void Construct(GameEventBus gameEventBus)
        {
            _gameEventBus = gameEventBus;
        }
        
        internal void Edit(GroupGameObjectSaveData compositionData)
        {
            _gameEventBus.Raise(new StartCompositionEdit(compositionData));
            
            _compositionID = compositionData.compositionID;
            trackObjectStorage.HideAll();
            var (trackObjectGroup, game, _) =
                trackObjectSpawner.LoadGroupNew(compositionData, compositionData.compositionID);
            groupSeparate.SeparateSingle((TrackObjectGroup)trackObjectStorage.GetTrackObjectData(game));
        }

        public void EndEdit()
        {
            TrackObjectGroup trackObjectGroup = groupCreater.Create(trackObjectStorage.GetAllActiveTrackData(), _compositionID);
            trackObjectStorage.ShowAll();
            composition.EditComposition(saveLevel.SaveGroup(trackObjectGroup), trackObjectGroup.compositionID);
            trackObjectRemover.SingleRemove(trackObjectGroup);
            compositionUpdater.UpdateCompositions();
            
            _gameEventBus.Raise(new EndCompositionEdit());
        }
    }
}