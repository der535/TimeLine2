using DG.Tweening;
using UnityEngine;

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

        private string _compositionID;
        
        internal void Edit(GroupGameObjectSaveData compositionData)
        {
            _compositionID = compositionData.compositionID;
            trackObjectStorage.HideAll();
            var (trackObjectGroup, game, _) =
                trackObjectSpawner.LoadGroup(compositionData, compositionData.compositionID);
            groupSeparate.SeparateSingle((TrackObjectGroup)trackObjectStorage.GetTrackObjectData(game));
        }

        public void EndEdit()
        {
            TrackObjectGroup trackObjectGroup = groupCreater.Create(trackObjectStorage.GetAllActiveTrackData(), _compositionID);
            trackObjectStorage.ShowAll();
            composition.EditComposition(saveLevel.SaveGroup(trackObjectGroup), trackObjectGroup.compositionID);
            trackObjectRemover.SingleRemove(trackObjectGroup);
            compositionUpdater.UpdateCompositions();
        }
    }
}