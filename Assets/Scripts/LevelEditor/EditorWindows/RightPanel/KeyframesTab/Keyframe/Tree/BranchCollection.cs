using System.Collections.Generic;
using System.Linq;
using EventBus;
using NaughtyAttributes;
using TimeLine;
using UnityEngine;
using Zenject;

public class BranchCollection : MonoBehaviour
{
    public List<Branch> Branches { get; } = new ();


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

    // Вспомогательный метод (можно сделать private static)
    private static string GetParentPathFromPath(string fullPath)
    {
        int lastSlash = fullPath.LastIndexOf('/');
        return lastSlash > 0 ? fullPath.Substring(0, lastSlash) : "";
    }
}