using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TimeLine
{
    public class OpenEditorScene : MonoBehaviour
    {
        [SerializeField] SceneField editorScene;


        public void Open()
        {
            DOTween.KillAll();
            SceneManager.LoadScene(editorScene.SceneName);
        }

    }
}
