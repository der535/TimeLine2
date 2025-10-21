using EventBus;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using Zenject;

[ExecuteInEditMode]
public class WaveformRenderer : MonoBehaviour
{
    public AudioSource source;
    public float amplitudeScale = 1f;
    public int totalResolution = 2048;
    [ColorUsage(true, true)] // Добавляем атрибут для поддержки HDR цветов
    public Color waveColor = Color.white; // Новое свойство для цвета
    
    public GameObject[] segments;

    private Texture2D[] _dataTextures;
    private RawImage[] _segmentImages;
    private float[] _cachedSamples;
    private Color _lastColor; // Для отслеживания изменений цвета
    
    private GameEventBus _gameEventBus;

    [Inject]
    private void Construct(GameEventBus eventBus)
    {
        _gameEventBus = eventBus;
    }
    
    private void Start()
    {
        _gameEventBus.SubscribeTo(((ref MusicLoadedEvent data) =>
        {
            Init();
        }));
    }

    [Button]
    void Init()
    {
        InitializeSegments();
        CacheFullAudioData();
        GenerateWaveform();
        print("InitializeSegments");
        _lastColor = waveColor; // Инициализируем последний цвет
    }

    void Update()
    {
        // Проверяем изменение цвета в реальном времени
        if (waveColor != _lastColor)
        {
            UpdateColor();
            _lastColor = waveColor;
        }
    }

    private void InitializeSegments()
    {
        
        if (segments == null || segments.Length == 0) return;

        if (_segmentImages == null || _segmentImages.Length != segments.Length)
        {
            if (_dataTextures != null)
            {
                foreach (var tex in _dataTextures)
                    if (tex != null) DestroyImmediate(tex);
            }
            
            _segmentImages = new RawImage[segments.Length];
            _dataTextures = new Texture2D[segments.Length];
        }

        int resolutionPerSegment = Mathf.CeilToInt((float)totalResolution / segments.Length);

        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null) continue;

            _segmentImages[i] = segments[i].GetComponent<RawImage>();
            if (_segmentImages[i] == null) continue;

            if (_segmentImages[i].material == null || 
                _segmentImages[i].material.shader.name != "Custom/WaveformShaderMultiTex")
            {
                // Создаем новый материал с шейдером
                var material = new Material(Shader.Find("Custom/WaveformShaderMultiTex"));
                _segmentImages[i].material = material;
                
                // Инициализируем цвет материала
                material.SetColor("_Color", waveColor);
            }

            _segmentImages[i].rectTransform.anchorMin = Vector2.zero;
            _segmentImages[i].rectTransform.anchorMax = Vector2.one;
            _segmentImages[i].rectTransform.offsetMin = Vector2.zero;
            _segmentImages[i].rectTransform.offsetMax = Vector2.zero;

            if (_dataTextures[i] == null || _dataTextures[i].width != resolutionPerSegment)
            {
                if (_dataTextures[i] != null) DestroyImmediate(_dataTextures[i]);
                _dataTextures[i] = CreateTexture(resolutionPerSegment);
            }
        }
    }


    private Texture2D CreateTexture(int resolution)
    {
        return new Texture2D(resolution, 1, TextureFormat.RFloat, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
    }

    private void CacheFullAudioData()
    {
        print(source.clip);
        if (source.clip == null) return;
        
        _cachedSamples = new float[source.clip.samples * source.clip.channels];
        source.clip.GetData(_cachedSamples, 0);
    }

    public void GenerateWaveform()
    {
        if (source.clip == null || 
            _cachedSamples == null || 
            _dataTextures == null || 
            _segmentImages == null ||
            segments == null) 
            return;

        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null || _segmentImages[i] == null || _dataTextures[i] == null)
                continue;
                
            GenerateSegmentTexture(i, segments.Length);
            ApplyMaterialSettings(i);
        }
    }
    public void UpdateColor()
    {
        if (_segmentImages == null) return;
        
        foreach (var image in _segmentImages)
        {
            if (image != null && image.material != null)
            {
                image.material.SetColor("_Color", waveColor);
            }
        }
    }
    private void GenerateSegmentTexture(int segmentIndex, int segmentCount)
    {
        int res = _dataTextures[segmentIndex].width;
        int channels = source.clip.channels;
        int totalSamples = source.clip.samples;
        
        // Рассчитываем диапазон семплов для сегмента
        int startSample = Mathf.FloorToInt((float)segmentIndex / segmentCount * totalSamples);
        int endSample = Mathf.FloorToInt((float)(segmentIndex + 1) / segmentCount * totalSamples);
        
        int startValue = startSample * channels;
        int endValue = endSample * channels;
        int valueCount = endValue - startValue;
        
        float[] maxValues = new float[res];
        float valuesPerPixel = (float)valueCount / res;

        for (int i = 0; i < res; i++)
        {
            int startIdx = startValue + Mathf.FloorToInt(i * valuesPerPixel);
            int endIdx = Mathf.Min(
                endValue,
                startValue + Mathf.FloorToInt((i + 1) * valuesPerPixel)
            );

            // Обработка последнего пикселя
            if (i == res - 1) endIdx = endValue;

            float max = 0f;
            for (int j = startIdx; j < endIdx; j++)
            {
                float absValue = Mathf.Abs(_cachedSamples[j]);
                if (absValue > max) max = absValue;
            }
            
            maxValues[i] = max;
        }

        // Записываем данные в текстуру
        Color[] colors = new Color[res];
        for (int i = 0; i < res; i++)
            colors[i] = new Color(maxValues[i], 0, 0);
        
        _dataTextures[segmentIndex].SetPixels(colors);
        _dataTextures[segmentIndex].Apply();
    }

    private void ApplyMaterialSettings(int segmentIndex)
    {
        var mat = _segmentImages[segmentIndex].material;
        mat.SetTexture("_DataTex", _dataTextures[segmentIndex]);
        mat.SetFloat("_AmpScale", amplitudeScale);
        mat.SetColor("_Color", waveColor); // Устанавливаем текущий цвет
    }
}