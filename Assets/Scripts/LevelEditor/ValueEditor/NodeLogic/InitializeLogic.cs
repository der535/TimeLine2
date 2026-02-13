using UnityEngine;

namespace TimeLine.LevelEditor.ValueEditor.NodeLogic
{
    public class InitializeLogic : global::NodeLogic, IInitializedNode
    {
        private float value;
        
        public InitializeLogic()
        {
            InputDefinitions = new()
            {
                ("Dynamic value", DataType.Float),
            };
            OutputDefinitions = new()
            {
                ("Initialized value", DataType.Float),
            };
        }

        public override object GetValue(int outputIndex = 0)
        {
            return value;
        }

        public void Initialized()
        {
            value = (float)GetInputValue(0,0);
        }
    }
}