using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using TimeLine;
using UnityEngine;

public class BranchCollection : MonoBehaviour
{
    public List<Branch> Branches { get; } = new List<Branch>();

    [Button]
    public void printTree()
    {
        foreach (var branch in Branches)
        {
            branch.PrintTree();
        }
    }

    public Branch AddBranch(string id, string name)
    {
        var newBranch = new Branch(id, name);
        Branches.Add(newBranch);
        return newBranch;
    }

    public Branch CopyBranch(Branch branch, string id)
    {
        var newBranch = new Branch(branch, id);
        Branches.Add(newBranch);
        return newBranch;
    }

    public Branch GetBranch(string id)
    {
        return Branches.FirstOrDefault(branch => branch.ID == id);
    }

    public TreeNode AddNodeToBranch(string branchId, string branchName, string path)
    {
        foreach (var branch in Branches)
        {
            if (branch.ID == branchId)
            {
                return branch.AddNode(path);
            }
        }

        // Если ветка не найдена - создаем новую
        var newBranch = AddBranch(branchId, branchName);
        return newBranch.AddNode(path);
    }

    // // 🔑 Новое: загрузка всей коллекции (вся логика здесь!)
    // public Branch LoadFromSaveData(BranchSaveData branchData)
    // {
    //     if (string.IsNullOrEmpty(branchData?.ID)) return null; 
    //
    //     // Создаём новую ветку
    //     var branch = new Branch(branchData.ID, branchData.Name);
    //     branch.Nodes.Clear(); // удаляем автоматически созданный Root
    //
    //     // Словарь для поиска узлов по Path
    //     var nodeMap = new Dictionary<string, TreeNode>();
    //
    //     // Создаём все узлы
    //     foreach (var nodeData in branchData.Nodes)
    //     {
    //         if (string.IsNullOrEmpty(nodeData?.Path) || string.IsNullOrEmpty(nodeData.Name)) continue;
    //
    //         var node = new TreeNode(nodeData.Name, nodeData.Path);
    //         nodeMap[node.Path] = node;
    //     }
    //
    //     // Восстанавливаем иерархию
    //     foreach (var nodeData in branchData.Nodes)
    //     {
    //         if (!nodeMap.TryGetValue(nodeData.Path, out TreeNode node)) continue;
    //
    //         // Определяем корень: Path == Name
    //         bool isRoot = nodeData.Path == nodeData.Name;
    //
    //         if (isRoot)
    //         {
    //             branch.Root = node;
    //         }
    //         else
    //         {
    //             // Находим родителя
    //             string parentPath = GetParentPathFromPath(nodeData.Path);
    //             if (nodeMap.TryGetValue(parentPath, out TreeNode parent))
    //             {
    //                 parent.Children.Add(node);
    //             }
    //             else
    //             {
    //                 Debug.LogWarning($"Parent not found for: {nodeData.Path}");
    //             }
    //         }
    //
    //         branch.Nodes.Add(node);
    //     }
    //
    //     Branches.Add(branch);
    //     return branch;
    // }

    // Вспомогательный метод (можно сделать private static)
    private static string GetParentPathFromPath(string fullPath)
    {
        int lastSlash = fullPath.LastIndexOf('/');
        return lastSlash > 0 ? fullPath.Substring(0, lastSlash) : "";
    }
}