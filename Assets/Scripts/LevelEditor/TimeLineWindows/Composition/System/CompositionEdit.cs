using System.Collections.Generic;
using DG.Tweening;
using EventBus;
using Newtonsoft.Json;
using NUnit.Framework;
using TimeLine.LevelEditor.Save;
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

        [FormerlySerializedAs("objectSpawner")] [FormerlySerializedAs("trackObjectSpawner")] [SerializeField]
        private FacadeObjectSpawner facadeObjectSpawner;

        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [SerializeField] private GroupCreater groupCreater;
        [SerializeField] private GroupSeparate groupSeparate;
        [SerializeField] private TrackObjectRemover trackObjectRemover;
        [SerializeField] private CompositionUpdater compositionUpdater;

        private SetPositionInTimeline _setPositionInTimeline;

        private GameEventBus _gameEventBus;
        private string _compositionID;
        private string _savedName;
        
        List<TrackObjectPacket> editObjects = new List<TrackObjectPacket>();

        [Inject]
        void Construct(GameEventBus gameEventBus, SetPositionInTimeline setPositionInTimeline)
        {
            _gameEventBus = gameEventBus;
            _setPositionInTimeline = setPositionInTimeline;
        }

        internal void Edit(GroupGameObjectSaveData compositionData)
        {
            _gameEventBus.Raise(new StartCompositionEdit(compositionData));

            _compositionID = compositionData.compositionID;
            _savedName = compositionData.gameObjectName;
            trackObjectStorage.HideAll();
            
            editObjects = facadeObjectSpawner.LoadObjects(compositionData.children);
            
            // var (_, game, _) =
            //     facadeObjectSpawner.LoadComposition(compositionData, compositionData.compositionID, false);
            //
            // var trackObject = (TrackObjectGroup)trackObjectStorage.GetTrackObjectData(game);
            //
            // List<TrackObjectPacket> datas = groupSeparate.SeparateSingle((TrackObjectGroup)trackObjectStorage.GetTrackObjectData(game));

            double sum = 0;
            
            foreach (var VARIABLE in editObjects)
            {
                sum += VARIABLE.components.Data.StartTimeInTicks;
            }
            sum /= editObjects.Count;
            

            _setPositionInTimeline.SetPosition((float)sum);
        }

        public void EndEdit()
        {
            TrackObjectGroup trackObjectGroup =
                groupCreater.Create(editObjects, _compositionID);
            // trackObjectGroup.sceneObject.GetComponent<NameComponent>().Name.Value = _savedName;
            trackObjectStorage.ShowAll();
            composition.EditComposition(saveLevel.SaveGroup(trackObjectGroup), trackObjectGroup.compositionID);
            trackObjectRemover.ListRemove(trackObjectGroup);
            foreach (var ob in editObjects)
            {
                trackObjectRemover.SingleRemove(ob);
            }
            compositionUpdater.UpdateCompositions(trackObjectGroup.compositionID);
            _gameEventBus.Raise(new EndCompositionEdit());
        }
    }
}