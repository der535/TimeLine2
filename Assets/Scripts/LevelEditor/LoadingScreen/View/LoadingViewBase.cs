using UnityEngine;

namespace TimeLine.LevelEditor.LoadingScreen.View
{
    public abstract class LoadingViewBase : MonoBehaviour
    {
        public abstract void UpdateUI(float progress, string status);
        public abstract void ShowLoadingScreen();
        public abstract void HideLoadingScreen();
    }
}