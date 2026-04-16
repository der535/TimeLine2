using System.Collections.Generic;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe;
using TimeLine.LevelEditor.ValueEditor.NodeLogic;
using TimeLine.LevelEditor.ValueEditor.Save;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor.Test
{
    public class SaveGraphToKeyFrame : MonoBehaviour
    {
        private OpenValueEditor _openValueEditor;
        private NodeCreator _nodeCreator;
        private SaveNodes _saveNodes;
        
        [Inject]
        void Construct(OpenValueEditor openValueEditor, NodeCreator nodeCreator, SaveNodes saveNodes)
        {
            _openValueEditor = openValueEditor;
            _nodeCreator = nodeCreator;
            _saveNodes = saveNodes;
        }
        
        public void Save()
        {
            Save(_openValueEditor.GetEditKeyframe().GetEntityData(), _nodeCreator.GetInitializedNodes(), _saveNodes.SaveGraphToJson(_nodeCreator.GetNodes()));
        }
        
        private void Save(EntityAnimationData keyframe, List<IInitializedNode> initialized, GraphSaveData saveJson)
        {
            keyframe.Graph = saveJson;
            keyframe.initializedNodes = initialized;
        }
    }
}