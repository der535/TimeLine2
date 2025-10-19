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
        [Space] 
        [SerializeField] private GameObject scenePrefab;
        [SerializeField] private GameObject trackPrefab;
        
        private SelectObjectController _selectObjectController;
        private TrackObjectStorage _trackObjectStorage;
        private BranchCollection _branchCollection;
        private TrackStorage _trackStorage;
        private KeyframeTrackStorage _keyframeTrackStorage;
        
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

            GameObject sceneObject = _container.InstantiatePrefab(scenePrefab, root);
            
            foreach (var selectObject in _selectObjectController.SelectObjects)
            {
                selectObject.trackObject.GroupOffset(minTime);
                
                if(selectObject.sceneObject.transform.parent == null || selectObject.sceneObject.transform.parent.transform == _mainObjects.SceneObjectParent)
                    selectObject.sceneObject.transform.SetParent(sceneObject.transform);

                foreach (var node in selectObject.branch.Nodes)
                {
                    foreach (var node2 in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(node2)?.SetParent(trackObject);
                    }
                }
            }
            
            trackObject.Setup(maxTime-minTime, "Group 1", _trackStorage.TrackLines[0], minTime, true);

            Branch branch = _branchCollection.AddBranch(id, scenePrefab.name);
            
            _trackObjectStorage.AddGroup(sceneObject, trackObject, branch, _selectObjectController.SelectObjects);
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
            
            GameObject sceneObject = _container.InstantiatePrefab(scenePrefab, root);
            
            foreach (var selectObject in trackObjects)
            {
                selectObject.trackObject.GroupOffset(minTime);
                
                if(selectObject.sceneObject.transform.parent.transform == null || selectObject.sceneObject.transform.parent.transform == _mainObjects.SceneObjectParent)
                    selectObject.sceneObject.transform.SetParent(sceneObject.transform);

                foreach (var node in selectObject.branch.Nodes)
                {
                    foreach (var node2 in node.Children)
                    {
                        _keyframeTrackStorage.GetTrack(node2)?.SetParent(trackObject);
                    }
                }
            }
            
            trackObject.Setup(maxTime-minTime, "Group 1", _trackStorage.TrackLines[0], minTime, true);

            Branch branch = _branchCollection.AddBranch(id, scenePrefab.name);
            
            _trackObjectStorage.AddGroup(sceneObject, trackObject, branch, trackObjects);
        }
    }
}