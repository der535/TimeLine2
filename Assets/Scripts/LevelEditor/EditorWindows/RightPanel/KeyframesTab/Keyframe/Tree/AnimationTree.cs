// using System;
// using System.Collections.Generic;
// using System.Text;
// using UnityEngine;
//
// public class AnimationTree : MonoBehaviour
// {
//     private TreeNode _root;
//     private readonly Dictionary<string, TreeNode> _nodeLookup = new Dictionary<string, TreeNode>();
//
//     public TreeNode Root => _root;
//     public Action OnTreeModified;
//     
//     void Awake()
//     {
//         InitializeTree();
//     }
//
//     private void InitializeTree()
//     {
//         _root = new TreeNode("Root", "Root");
//         _nodeLookup.Clear();
//         _nodeLookup.Add("Root", _root);
//     }
//
//     public TreeNode AddNode(string parentPath, string newNodeName)
//     {
//         TreeNode parentNode = ValidateAndCreatePath(parentPath);
//         if (parentNode == null)
//         {
//             Debug.LogError($"Invalid path: {parentPath}");
//             return null;
//         }
//
//         string newPath = $"{parentPath}/{newNodeName}";
//         if (_nodeLookup.ContainsKey(newPath))
//         {
//             Debug.LogWarning($"Node already exists: {newPath}");
//             return _nodeLookup[newPath];
//         }
//
//         TreeNode newNode = parentNode.AddChild(newNodeName);
//         _nodeLookup.Add(newPath, newNode);
//         OnTreeModified.Invoke();
//         return newNode;
//     }
//
//     private TreeNode ValidateAndCreatePath(string path)
//     {
//         if (path == "Root") return _root;
//         if (_nodeLookup.TryGetValue(path, out TreeNode existingNode)) 
//             return existingNode;
//
//         string[] parts = path.Split('/');
//         StringBuilder currentPath = new StringBuilder("Root");
//         TreeNode currentNode = _root;
//
//         for (int i = 1; i < parts.Length; i++)
//         {
//             currentPath.Append('/');
//             currentPath.Append(parts[i]);
//             string currentPathStr = currentPath.ToString();
//
//             if (!_nodeLookup.TryGetValue(currentPathStr, out TreeNode nextNode))
//             {
//                 nextNode = currentNode.AddChild(parts[i]);
//                 _nodeLookup.Add(currentPathStr, nextNode);
//             }
//             currentNode = nextNode;
//         }
//
//         OnTreeModified.Invoke();
//         return currentNode;
//     }
//
//     public bool PathExists(string path)
//     {
//         return _nodeLookup.ContainsKey(path);
//     }
//
//     public TreeNode GetNodeByPath(string path)
//     {
//         return _nodeLookup.TryGetValue(path, out TreeNode node) ? node : null;
//     }
// }