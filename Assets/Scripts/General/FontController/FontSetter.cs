using TimeLine.General.Installers;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class FontSetter : MonoBehaviour
    {
        [SerializeField] private FontNames fontNames;
        
        private FontStorage _fontStorage;
        
        [Inject]
        private void Constructor(FontStorage fontStorage)
        {
            _fontStorage = fontStorage;
            gameObject.GetComponent<TextMeshProUGUI>().font =_fontStorage.GetFont(fontNames);
        }
    }
}
