using UnityEngine;

namespace TimeLine
{
    public class treetest : MonoBehaviour
    {
        [SerializeField] private GameObject keyframe;
        [SerializeField] private BranchCollection branchCollection;
        [SerializeField] private TreeViewUI treeView;
        [Space]
        [SerializeField] private string path;
        [SerializeField] private string element;

        void Start()
        {
            string id = UniqueIDGenerator.GenerateUniqueID();
            // Создаем ветку для Куба
            TreeNode node = branchCollection.AddNodeToBranch(id, "Куб", "Трансформ", "Позиция");
            treeView.BuildBranch(branchCollection.Branches[0]);

            foreach (var node2 in treeView.NodeObjects)
            {
                if (node2.LogicalNode == node)
                {
                    Instantiate(keyframe, node2.RootRect);
                }
            }
        }
    }
}