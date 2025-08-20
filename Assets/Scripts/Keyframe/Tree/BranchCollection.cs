using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
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
    
    public TreeNode AddNodeToBranch(string branchId, string branchName, string path, string nodeName)
    {
        foreach (var branch in Branches)
        {
            if (branch.ID == branchId)
            {
                return branch.AddNode(path, nodeName);
            }
        }
        
        // Если ветка не найдена - создаем новую
        var newBranch = AddBranch(branchId, branchName);
        return newBranch.AddNode(path, nodeName);
    }
}