using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine.LevelEditor.LoadingScreen.View
{
    public class LoadingStepsView : LoadingViewBase
    {
        [SerializeField] private RectTransform loadingScreen;
        [Space] [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI statusText;

        public override void UpdateUI(float progress, string status)
        {
            progressBar.value = progress;
            statusText.text = status;
        }

        public override void ShowLoadingScreen()
        {
            loadingScreen.gameObject.SetActive(true);
        }

        public override void HideLoadingScreen()
        {
            loadingScreen.gameObject.SetActive(false);
        }
    }
}