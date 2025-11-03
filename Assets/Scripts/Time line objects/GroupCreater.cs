using System.Collections.Generic;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Installers;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class GroupCreater : MonoBehaviour
    {
        [SerializeField] private Transform root;
        [Space] [SerializeField] private GameObject scenePrefab;
        [SerializeField] private GameObject trackPrefab;

        private SelectObjectController _selectObjectController;
        private TrackObjectStorage _trackObjectStorage;
        private BranchCollection _branchCollection;
        private TrackStorage _trackStorage;
        private KeyframeTrackStorage _keyframeTrackStorage;
        private Main _main;

        private DiContainer _container;
        private MainObjects _mainObjects;

        [Inject]
        private void Construct(
            TrackObjectStorage trackObjectStorage,
            BranchCollection branchCollection,
            Main main,
            TrackStorage trackStorage,
            DiContainer container,
            KeyframeTrackStorage keyframeTrackStorage,
            SelectObjectController selectObjectController,
            MainObjects mainObjects)
        {
            _trackObjectStorage = trackObjectStorage;
            _branchCollection = branchCollection;
            _trackStorage = trackStorage;
            _container = container;
            _selectObjectController = selectObjectController;
            _keyframeTrackStorage = keyframeTrackStorage;
            _mainObjects = mainObjects;
            _main = main;
        }

        public static (double minTime, double maxTime) CalculateMinAndMaxTime(List<TrackObjectData> trackObjectData)
        {
            double minTime = double.MaxValue;
            double maxTime = double.MinValue;

            foreach (var selectObject in trackObjectData)
            {
                double startTime = selectObject.trackObject.StartTimeInTicks;
                double endTime = startTime + selectObject.trackObject.TimeDuractionInTicks;

                if (startTime < minTime)
                    minTime = startTime;

                if (endTime > maxTime)
                    maxTime = endTime;
            }

            return (minTime, maxTime);
        }

        public static (double minTime, double maxTime) CalculateMinAndMaxTime(List<GameObjectSaveData> trackObjectData)
        {
            double minTime = double.MaxValue;
            double maxTime = double.MinValue;

            foreach (var selectObject in trackObjectData)
            {
                double startTime = selectObject.startTime;
                double endTime = startTime + selectObject.duractionTime;

                if (startTime < minTime)
                    minTime = startTime;

                if (endTime > maxTime)
                    maxTime = endTime;
            }

            return (minTime, maxTime);
        }

        public void Create()
        {
            if (_selectObjectController.SelectObjects.Count <= 0) return;

            TrackObject trackObject = _container
                .InstantiatePrefab(trackPrefab, _trackStorage.TrackLines[0].RectTransform).GetComponent<TrackObject>();

            var (minTime, maxTime) = CalculateMinAndMaxTime(_selectObjectController.SelectObjects);


            
            var currentTime = _main.TicksCurrentTime();
            _main.SetTimeInTicks(minTime);

            
            GameObject sceneObject = _container.InstantiatePrefab(scenePrefab, root);

            sceneObject.GetComponent<NameComponent>().Name.Value = "Group";

            foreach (var selectObject in _selectObjectController.SelectObjects)
            {
                selectObject.trackObject.GroupOffset(minTime);
                selectObject.trackObject.GroupOffsetTrack(trackObject);

                if (selectObject.sceneObject.transform.parent == null ||
                    selectObject.sceneObject.transform.parent.transform == _mainObjects.SceneObjectParent)
                {
                    var position = selectObject.sceneObject.transform.localPosition;
                    var rotation = selectObject.sceneObject.transform.rotation;
                    var scale = selectObject.sceneObject.transform.localScale;
                    selectObject.sceneObject.transform.SetParent(sceneObject.transform);
                    selectObject.sceneObject.transform.localPosition = position;
                    selectObject.sceneObject.transform.localRotation = rotation;
                    selectObject.sceneObject.transform.localScale = scale;
                }

                foreach (var node in selectObject.branch.Nodes)
                {
                    foreach (var node2 in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(node2)?.SetParent(trackObject);
                    }
                }
            }

            trackObject.Setup(maxTime - minTime, "Group", _trackStorage.TrackLines[0], minTime, true);

            Branch branch = _branchCollection.AddBranch(UniqueIDGenerator.GenerateUniqueID(), "Group");

            _trackObjectStorage.AddGroup(sceneObject, trackObject, branch, _selectObjectController.SelectObjects,
                UniqueIDGenerator.GenerateUniqueID(), UniqueIDGenerator.GenerateUniqueID());
            
            
            _main.SetTimeInTicks(currentTime);
        }

        public TrackObjectGroup Create(List<TrackObjectData> trackObjects, string compositionID = null)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();

            TrackObject trackObject = _container
                .InstantiatePrefab(trackPrefab, _trackStorage.TrackLines[0].RectTransform).GetComponent<TrackObject>();

            var (minTime, maxTime) = CalculateMinAndMaxTime(trackObjects);
            
            _main.SetTimeInTicks(minTime);

            var sceneObject = _container.InstantiatePrefab(scenePrefab, root);

            foreach (var selectObject in trackObjects)
            {
                selectObject.trackObject.GroupOffset(minTime);
                selectObject.trackObject.GroupOffsetTrack(trackObject);

                if (selectObject.sceneObject.transform.parent == null ||
                    selectObject.sceneObject.transform.parent.transform == _mainObjects.SceneObjectParent)
                {
                    var position = selectObject.sceneObject.transform.localPosition;
                    var rotation = selectObject.sceneObject.transform.rotation;
                    var scale = selectObject.sceneObject.transform.localScale;
                    selectObject.sceneObject.transform.SetParent(sceneObject.transform);
                    selectObject.sceneObject.transform.localPosition = position;
                    selectObject.sceneObject.transform.localRotation = rotation;
                    selectObject.sceneObject.transform.localScale = scale;
                }

                foreach (var node in selectObject.branch.Nodes)
                {
                    foreach (var node2 in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(node2)?.SetParent(trackObject);
                    }
                }
            }

            trackObject.Setup(maxTime - minTime, "Group", _trackStorage.TrackLines[0], minTime, true);

            Branch branch = _branchCollection.AddBranch(id, scenePrefab.name);

            if (string.IsNullOrEmpty(compositionID))
                compositionID = UniqueIDGenerator.GenerateUniqueID();

            return _trackObjectStorage.AddGroup(sceneObject, trackObject, branch, trackObjects,
                UniqueIDGenerator.GenerateUniqueID(), compositionID);
        }

        // public void Create(List<TrackObjectData> trackObjects, TrackObject groupTrackObject)
        // {
        //     string id = UniqueIDGenerator.GenerateUniqueID();
        //
        //     var (minTime, _) = CalculateMinAndMaxTime(trackObjects);
        //     
        //     _main.SetTimeInTicks(minTime);
        //
        //     GameObject sceneObject = _container.InstantiatePrefab(scenePrefab, root);
        //
        //     foreach (var selectObject in trackObjects)
        //     {
        //         selectObject.trackObject.GroupOffset(minTime);
        //         selectObject.trackObject.GroupOffsetTrack(groupTrackObject);
        //
        //         if (selectObject.sceneObject.transform.parent.transform == null ||
        //             selectObject.sceneObject.transform.parent.transform == _mainObjects.SceneObjectParent)
        //         {
        //             var position = selectObject.sceneObject.transform.localPosition;
        //             var rotation = selectObject.sceneObject.transform.rotation;
        //             var scale = selectObject.sceneObject.transform.localScale;
        //             selectObject.sceneObject.transform.SetParent(sceneObject.transform);
        //             selectObject.sceneObject.transform.localPosition = position;
        //             selectObject.sceneObject.transform.localRotation = rotation;
        //             selectObject.sceneObject.transform.localScale = scale;
        //         }
        //
        //         foreach (var node in selectObject.branch.Nodes)
        //         {
        //             foreach (var node2 in node.Children)
        //             {
        //                 _keyframeTrackStorage.GetTrack(node2)?.SetParent(groupTrackObject);
        //             }
        //         }
        //     }
        //
        //     Branch branch = _branchCollection.AddBranch(id, scenePrefab.name);
        //
        //     _trackObjectStorage.AddGroup(sceneObject, groupTrackObject, branch, trackObjects,
        //         UniqueIDGenerator.GenerateUniqueID(), UniqueIDGenerator.GenerateUniqueID());
        // }
    }
}