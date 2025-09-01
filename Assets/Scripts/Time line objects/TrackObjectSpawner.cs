using System.Collections.Generic;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class TrackObjectSpawner : MonoBehaviour
    {
        [SerializeField] private Transform root;
        [Space] 
        [SerializeField] private GameObject scenePrefab;
        [SerializeField] private GameObject trackPrefab;
        
        private KeyframeTrackStorage _keyframeTrackStorage;
        private TrackObjectStorage _trackObjectStorage;
        private BranchCollection _branchCollection;
        private TrackStorage _trackStorage;
        private Main _main;
        
        private DiContainer _container;

        [Inject]
        private void Construct(
            TrackObjectStorage trackObjectStorage,
            BranchCollection branchCollection,
            Main main,
            TrackStorage trackStorage,
            DiContainer container,
            KeyframeTrackStorage keyframeTrackStorage)
        {
            _trackObjectStorage = trackObjectStorage;
            _branchCollection = branchCollection;
            _main = main;
            _trackStorage = trackStorage;
            _container = container;
            _keyframeTrackStorage = keyframeTrackStorage;
        }

        internal void Spawn(TrackObjectSO trackObjectSO)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();
            
            SceneTrackObject sceneTrackObject =
                _container.InstantiatePrefab(scenePrefab, root).GetComponent<SceneTrackObject>();
            sceneTrackObject.Setup(trackObjectSO);
            
            TrackObject trackObject = _container.InstantiatePrefab(trackPrefab, _trackStorage.TrackLines[0].RectTransform).GetComponent<TrackObject>();
            
            trackObject.Setup(trackObjectSO, _trackStorage.TrackLines[0], _main.TicksCurrentTime());

            Branch branch = _branchCollection.AddBranch(id, trackObjectSO.name);
            
            _trackObjectStorage.Add(sceneTrackObject.gameObject, trackObject, branch);
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                CopyObject(_trackObjectStorage._selectedObject);
            }
        }
        
        public static List<ICopyableComponent> GetAllCopyableComponents(GameObject gameObject)
        {
            var result = new List<ICopyableComponent>();
            var allComponents = gameObject.GetComponents<Component>();
            
            foreach (var component in allComponents)
            {
                if (component is ICopyableComponent copyable)
                {
                    result.Add(copyable);
                }
            }
            
            return result;
        }

        internal void CopyObject(TrackObjectData trackObjectData)
        {
            string id = UniqueIDGenerator.GenerateUniqueID(); // Создаём новый ID
            
            TrackObjectSO trackObjectSo = trackObjectData.sceneObject.GetComponent<SceneTrackObject>().Copy(); // Копируем данные с объекта на сцене
            
            SceneTrackObject sceneTrackObject =
                _container.InstantiatePrefab(scenePrefab, root).GetComponent<SceneTrackObject>();
            sceneTrackObject.Setup(trackObjectSo); // Создаём новый объект
            
            List<ICopyableComponent> sourceComponents = GetAllCopyableComponents(trackObjectData.sceneObject);

            foreach (var component in sourceComponents)
            {
                component.Copy(sceneTrackObject.gameObject);
            }
            
            
            TrackObject trackObject = _container.InstantiatePrefab(trackPrefab, _trackStorage.TrackLines[0].RectTransform).GetComponent<TrackObject>();
            
            
            trackObject.Setup(new TrackObjectSO()
            {
                name = trackObjectData.trackObject.Name,
                startLiveTime = (float)trackObjectData.trackObject.BeatDuraction
            }, _trackStorage.TrackLines[0], _main.TicksCurrentTime());
            
            Branch branch = _branchCollection.CopyBranch(trackObjectData.branch, id);

            foreach (var nodes in trackObjectData.branch.Nodes)
            {
                var track = _keyframeTrackStorage.GetTrack(nodes);
                print(nodes.Path);
                print(track);
                
                string[] split = nodes.Path.Split('/');

                if (track != null)
                {
                    TreeNode node;
                    if(split.Length > 1)
                        node = branch.AddNode(split[0], split[1]);
                    else
                        node = branch.AddNode("", split[0]);
                    
                    _keyframeTrackStorage.AddTrack(node, track.Copy(sceneTrackObject.gameObject), trackObject);
                }
            }
            
            _trackObjectStorage.Add(sceneTrackObject.gameObject, trackObject, branch);
        }
    }
}