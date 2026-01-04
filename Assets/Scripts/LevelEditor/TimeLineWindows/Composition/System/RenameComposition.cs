
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine
{
    public class RenameComposition : MonoBehaviour
    {
        [SerializeField] private RectTransform renameCompositionPanel;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private SaveComposition saveComposition;
        [Space]
        [SerializeField] private Button ok;

        public RectTransform RenameCompositionPanel => renameCompositionPanel;
        
        
        internal void Setup(string compositionID, string compositionName)
        {
            ok.onClick.RemoveAllListeners();
            inputField.text = compositionName;
            ok.onClick.AddListener(() =>
            {
                RenameCompositionPanel.gameObject.SetActive(false);
                saveComposition.Rename(inputField.text, compositionID);
                saveComposition.UpdateCompositionCards();
            });
        }
    }
}
