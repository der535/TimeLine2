using System;
using System.Collections.Generic;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe;
using TimeLine.LevelEditor.ValueEditor;
using TimeLine.LevelEditor.ValueEditor.NodeLogic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst; // Добавь это для скорости!
using UnityEngine;
using Zenject;

public enum OpCode
{
    Constant,
    Add,
    Multiply,
    DirectPass,
    PlayerPos,
    Initialize,
    Input // Добавил для ComponentFieldLogic
}

namespace TimeLine.LevelEditor.Optimization
{
    // Обертка для хранения запеченных данных одного графа
    public class BakedGraph : IDisposable
    {
        public NativeArray<JobNode> Instructions;
        public NativeArray<float> ResultsBuffer;
        public List<(int index, ComponentFieldLogic logic)> DynamicInputs;

        public void Dispose()
        {
            if (Instructions.IsCreated) Instructions.Dispose();
            if (ResultsBuffer.IsCreated) ResultsBuffer.Dispose();
        }
    }


    public class KeyframeGraphCalculator : MonoBehaviour
    {
        private KeyframeTrackStorage _keyframeTrackStorage;
        private Dictionary<AnimationData, BakedGraph> _runtimeGraphs = new();

        private NativeArray<JobNode> _globalGraphNodes;
        private NativeArray<float> _globalGraphResults;
        private List<DynamicInputData> _allDynamicInputs = new();

        public struct DynamicInputData
        {
            public int globalOffset; 
            public NodeLogic logic;
            public int outputIndex; // 0=X, 1=Y, 2=Z и т.д.
        }

        [Inject]
        private void Construct(KeyframeTrackStorage keyframeTrackStorage)
        {
            _keyframeTrackStorage = keyframeTrackStorage;
        }

        public void BakeAll()
        {
            foreach (var g in _runtimeGraphs.Values) g.Dispose();
            _runtimeGraphs.Clear();

            foreach (var track in _keyframeTrackStorage.GetTracks())
            {
                foreach (var key in track.Track.Keyframes)
                {
                    var data = key.GetData();
                    if (data.Logic != null)
                    {
                        _runtimeGraphs.Add(data, BakeSingleGraph(data.Logic));
                    }
                }
            }
        }

        private BakedGraph BakeSingleGraph(NodeLogic root)
        {
            var baked = new BakedGraph();
            var sortedNodes = new List<NodeLogic>();
            FillSortedNodes(root, sortedNodes, new HashSet<NodeLogic>());

            baked.Instructions = new NativeArray<JobNode>(sortedNodes.Count, Allocator.Persistent);
            baked.ResultsBuffer = new NativeArray<float>(sortedNodes.Count, Allocator.Persistent);
            baked.DynamicInputs = new List<(int, ComponentFieldLogic)>();

            for (int i = 0; i < sortedNodes.Count; i++)
            {
                var logic = sortedNodes[i];
                var inst = new JobNode();
                MapLogicToJob(logic, ref inst);

                inst.InputIdxA = GetInputIndex(logic, 0, sortedNodes);
                inst.InputIdxB = GetInputIndex(logic, 1, sortedNodes);
                baked.Instructions[i] = inst;

                if (logic is ComponentFieldLogic compLogic)
                    baked.DynamicInputs.Add((i, compLogic));
            }

            return baked;
        }

        private void MapLogicToJob(NodeLogic logic, ref JobNode inst)
        {
            if (logic is FloatLogic fl)
            {
                inst.Op = OpCode.Constant;
                inst.Value = fl.Value;
            }
            else if (logic is ComponentFieldLogic)
            {
                inst.Op = OpCode.Input; // Помечаем как вход
            }
            else if (logic is AddLogic)
            {
                inst.Op = OpCode.Add;
            }
            else if (logic is OutputLogic)
            {
                inst.Op = OpCode.DirectPass;
            }
        }

        private int GetInputIndex(NodeLogic logic, int portIndex, List<NodeLogic> sortedNodes)
        {
            if (logic.ConnectedInputs.TryGetValue(portIndex, out var connection))
                return sortedNodes.IndexOf(connection.node);
            return -1;
        }

        private (int nodeIdx, int portIdx) GetInputConnection(NodeLogic node, int portIndex,
            List<NodeLogic> sortedNodes)
        {
            if (node.ConnectedInputs.TryGetValue(portIndex, out var connection))
            {
                return (sortedNodes.IndexOf(connection.node), connection.outputIndex);
            }

            return (-1, 0);
        }

        private void FillSortedNodes(NodeLogic current, List<NodeLogic> sortedNodes, HashSet<NodeLogic> visited)
        {
            if (current == null || visited.Contains(current)) return;
            visited.Add(current);
            foreach (var inputPair in current.ConnectedInputs)
                FillSortedNodes(inputPair.Value.node, sortedNodes, visited);
            sortedNodes.Add(current);
        }

        public float Evaluate(AnimationData keyframe)
        {
            if (!_runtimeGraphs.TryGetValue(keyframe, out var baked)) return 0;

            // Заполняем входы из компонентов
            foreach (var input in baked.DynamicInputs)
                baked.ResultsBuffer[input.index] = Convert.ToSingle(input.logic.GetValue());

            var job = new GraphEvalJob
            {
                Nodes = baked.Instructions,
                Results = baked.ResultsBuffer
            };

            job.Run(); // Синхронный запуск на воркере через Burst

            return baked.ResultsBuffer[baked.ResultsBuffer.Length - 1];
        }

        private void OnDestroy()
        {
            foreach (var g in _runtimeGraphs.Values) g.Dispose();
        }
    }
}

[BurstCompile] // Магия ускорения
public struct GraphEvalJob : IJob
{
    [ReadOnly] public NativeArray<JobNode> Nodes;
    public NativeArray<float> Results;

    public void Execute()
    {
        for (int i = 0; i < Nodes.Length; i++)
        {
            var node = Nodes[i];
            float a = (node.InputIdxA != -1) ? Results[node.InputIdxA] : node.Value;
            float b = (node.InputIdxB != -1) ? Results[node.InputIdxB] : 0;

            switch (node.Op)
            {
                case OpCode.Constant:
                    Results[i] = node.Value;
                    break;
                case OpCode.Input:
                    // Значение уже записано в Results[i] перед стартом
                    break;
                case OpCode.Add:
                    Results[i] = a + b;
                    break;
                case OpCode.Multiply:
                    Results[i] = a * b;
                    break;
                case OpCode.DirectPass:
                    Results[i] = a;
                    break;
            }
        }
    }
}

public struct JobNode
{
    public OpCode Op;
    public float Value;
    public int InputIdxA;

    public int InputIdxB;

    // Новые поля: указывают, какой именно выход предыдущей ноды нам нужен
    public int InputPortA;
    public int InputPortB;
}