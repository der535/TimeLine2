using System.Collections.Generic;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class GroupCreater : MonoBehaviour
    {
        [SerializeField] private Transform root;
        [Space] 
        [SerializeField] private GameObject scenePrefab;
        [SerializeField] private GameObject trackPrefab;
        [Space]
        [SerializeField] private TrackObjectSO groupObjectSO;
        
        private SelectObjectController _selectObjectController;
        private TrackObjectStorage _trackObjectStorage;
        private BranchCollection _branchCollection;
        private TrackStorage _trackStorage;
        private KeyframeTrackStorage _keyframeTrackStorage;
        
        private DiContainer _container;

        [Inject]
        private void Construct(
            TrackObjectStorage trackObjectStorage,
            BranchCollection branchCollection,
            Main main,
            TrackStorage trackStorage,
            DiContainer container,
            KeyframeTrackStorage keyframeTrackStorage,
            SelectObjectController selectObjectController)
        {
            _trackObjectStorage = trackObjectStorage;
            _branchCollection = branchCollection;
            _trackStorage = trackStorage;
            _container = container;
            _selectObjectController = selectObjectController;    
            _keyframeTrackStorage = keyframeTrackStorage;
        }

        public void Create()
        {
            if(_selectObjectController.SelectObjects.Count <= 0) return;
                
            string id = UniqueIDGenerator.GenerateUniqueID();
            
            TrackObject trackObject = _container.InstantiatePrefab(trackPrefab, _trackStorage.TrackLines[0].RectTransform).GetComponent<TrackObject>();
            
            double minTime = float.MaxValue;
            double maxTime = -1;
            
            double maxTimeTemp = -1;
            foreach (var selectObject in _selectObjectController.SelectObjects)
            {
                if(selectObject.trackObject.StartTimeInTicks < minTime)
                    minTime = selectObject.trackObject.StartTimeInTicks;

                if (selectObject.trackObject.StartTimeInTicks > maxTimeTemp)
                {
                    maxTimeTemp = selectObject.trackObject.StartTimeInTicks;
                    maxTime = selectObject.trackObject.StartTimeInTicks + selectObject.trackObject.TimeDuractionInTicks;
                }
            }
            
            SceneTrackObject sceneTrackObject =
                _container.InstantiatePrefab(scenePrefab, root).GetComponent<SceneTrackObject>();
            sceneTrackObject.Setup(new TrackObjectSO()
            {
                name = "Group 1", 
                startLiveTime = (float)(maxTime-minTime),
            });
            
            print(minTime);
            
            foreach (var selectObject in _selectObjectController.SelectObjects)
            {
                selectObject.trackObject.GroupOffset(minTime);
                selectObject.sceneObject.transform.SetParent(sceneTrackObject.gameObject.transform);

                foreach (var node in selectObject.branch.Nodes)
                {
                    foreach (var node2 in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(node2)?.SetParent(trackObject);
                    }
                }
            }
            
            trackObject.Setup(maxTime-minTime, "Group 1", _trackStorage.TrackLines[0], minTime, true);

            Branch branch = _branchCollection.AddBranch(id, groupObjectSO.name);
            
            _trackObjectStorage.AddGroup(sceneTrackObject.gameObject, trackObject, branch, _selectObjectController.SelectObjects);
        }
        
        public void Create(List<TrackObjectData> trackObjects)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();
            
            TrackObject trackObject = _container.InstantiatePrefab(trackPrefab, _trackStorage.TrackLines[0].RectTransform).GetComponent<TrackObject>();
            
            double minTime = float.MaxValue;
            double maxTime = -1;
            
            double maxTimeTemp = -1;
            foreach (var selectObject in trackObjects)
            {
                if(selectObject.trackObject.StartTimeInTicks < minTime)
                    minTime = selectObject.trackObject.StartTimeInTicks;

                if (selectObject.trackObject.StartTimeInTicks > maxTimeTemp)
                {
                    maxTimeTemp = selectObject.trackObject.StartTimeInTicks;
                    maxTime = selectObject.trackObject.StartTimeInTicks + selectObject.trackObject.TimeDuractionInTicks;
                }
            }
            
            SceneTrackObject sceneTrackObject =
                _container.InstantiatePrefab(scenePrefab, root).GetComponent<SceneTrackObject>();
            sceneTrackObject.Setup(new TrackObjectSO()
            {
                name = "Group 1", 
                startLiveTime = (float)(maxTime-minTime),
            });
            
            print(minTime);
            
            foreach (var selectObject in trackObjects)
            {
                selectObject.trackObject.GroupOffset(minTime);
                selectObject.sceneObject.transform.SetParent(sceneTrackObject.gameObject.transform);

                foreach (var node in selectObject.branch.Nodes)
                {
                    foreach (var node2 in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(node2)?.SetParent(trackObject);
                    }
                }
            }
            
            trackObject.Setup(maxTime-minTime, "Group 1", _trackStorage.TrackLines[0], minTime, true);

            Branch branch = _branchCollection.AddBranch(id, groupObjectSO.name);
            
            _trackObjectStorage.AddGroup(sceneTrackObject.gameObject, trackObject, branch, trackObjects);
        }
    }
}