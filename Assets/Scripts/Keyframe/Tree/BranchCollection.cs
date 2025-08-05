using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BranchCollection : MonoBehaviour
{
    public List<Branch> Branches { get; } = new List<Branch>();

    public Branch AddBranch(string id, string name)
    {
        var newBranch = new Branch(id, name);
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