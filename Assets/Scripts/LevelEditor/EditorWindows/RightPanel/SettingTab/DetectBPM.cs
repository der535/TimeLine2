using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


namespace TimeLine
{
    public class DetectBPM : MonoBehaviour
    {
        [SerializeField] private TMP_InputField bpmInputField;
        [SerializeField] private Button button;

        private M_AudioPlaybackService _audioPlaybackService;

        [Inject]
        private void Construct(M_AudioPlaybackService audioPlaybackService)
        {
            _audioPlaybackService = audioPlaybackService;
        }
        
        private void Start()
        {
            button.onClick.AddListener((() =>
            {
                Debug.Log(_audioPlaybackService.Clip);
                var bpm = UniBpmAnalyzer.AnalyzeBpm(_audioPlaybackService.Clip);
                Debug.Log(bpm);
                Debug.Log(bpmInputField);
                bpmInputField.text = bpm.ToString();
            }));
        }

    }
}
