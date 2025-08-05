using System;
using EventBus;
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
        
        private TrackObjectStorage _trackObjectStorage;
        private BranchCollection _branchCollection;
        private Main _main;
        private TrackStorage _trackStorage;
        
        private DiContainer _container;

        [Inject]
        private void Construct(
            TrackObjectStorage trackObjectStorage,
            BranchCollection branchCollection,
            Main main,
            TrackStorage trackStorage,
            DiContainer container)
        {
            _trackObjectStorage = trackObjectStorage;
            _branchCollection = branchCollection;
            _main = main;
            _trackStorage = trackStorage;
            _container = container;
        }

        internal void Spawn(TrackObjectSO trackObjectSO)
        {
            string id = UniqueIDGenerator.GenerateUniqueID();
            
            
            
            SceneTrackObject sceneTrackObject =
                _container.InstantiatePrefab(scenePrefab, root).GetComponent<SceneTrackObject>();
            sceneTrackObject.Setup(trackObjectSO);
            
            TrackObject trackObject = _container.InstantiatePrefab(trackPrefab, _trackStorage.TrackLines[0].RectTransform).GetComponent<TrackObject>();
            //
            // gameEventListener.AddSubscription(onScroll, 0);
            // gameEventListener.AddSubscription(onScrollPan, 0);
            //
            //
            // var onScrollSubscription = gameEventListener.subscriptions.Find(s => s.gameEvent == onScroll);
            // onScrollSubscription.response.AddListener(trackObject.OnScroll);
            //
            // var onScrollPanSubscription = gameEventListener.subscriptions.Find(s => s.gameEvent == onScrollPan);
            // onScrollPanSubscription.response.AddListener(trackObject.OnScrollPan);
            
            
            trackObject.Setup(trackObjectSO, _trackStorage.TrackLines[0], _main.CurrentTime);

            Branch branch = _branchCollection.AddBranch(id, trackObjectSO.name);
            
            _trackObjectStorage.Add(sceneTrackObject.gameObject, trackObject, branch);
        }
    }
}