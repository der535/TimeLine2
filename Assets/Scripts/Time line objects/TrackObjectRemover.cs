using System;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class TrackObjectRemover : MonoBehaviour
    {
        private TrackObjectStorage _trackObjectStorage;
        private BranchCollection _branchCollection;
        private Main _main;
        private TrackStorage _trackStorage;
        private KeyframeTrackStorage _keyframeTrackStorage;

        [Inject]
        private void Construct(
            TrackObjectStorage trackObjectStorage,
            BranchCollection branchCollection,
            Main main,
            TrackStorage trackStorage,
            KeyframeTrackStorage keyframeTrackStorage)
        {
            _trackObjectStorage = trackObjectStorage;
            _branchCollection = branchCollection;
            _main = main;
            _trackStorage = trackStorage;
            _keyframeTrackStorage = keyframeTrackStorage;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.X))
            {
                Remove();
            }
        }

        internal void Remove()
        {
            //[x] Уделить все кейфрем треки 
            //[x] Удалить трек обжект 
            //[x] Удалить объект на сцене
            //[x] Удалить ветку 
            
            foreach (var nodes in _trackObjectStorage._selectedObject.branch.Nodes)
            {
                _keyframeTrackStorage.RemoveTrack(nodes);
            }
            
            Destroy(_trackObjectStorage._selectedObject.trackObject.gameObject);
            Destroy(_trackObjectStorage._selectedObject.sceneObject);
            
            _trackObjectStorage.Remove(_trackObjectStorage._selectedObject);
            
            // _trackObjectStorage._selectedObject;
            //
            // SceneTrackObject sceneTrackObject =
            //     _container.InstantiatePrefab(scenePrefab, root).GetComponent<SceneTrackObject>();
            // sceneTrackObject.Setup(trackObjectSO);
            //
            // TrackObject trackObject = _container.InstantiatePrefab(trackPrefab, _trackStorage.TrackLines[0].RectTransform).GetComponent<TrackObject>();
            //
            // trackObject.Setup(trackObjectSO, _trackStorage.TrackLines[0], _main.CurrentTime);
            //
            // Branch branch = _branchCollection.AddBranch(id, trackObjectSO.name);
            //
            // _trackObjectStorage.Add(sceneTrackObject.gameObject, trackObject, branch);
        }
    }
}