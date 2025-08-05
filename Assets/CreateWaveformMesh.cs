using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class CreateWaveformMesh : Graphic
{
    // [Serialized fields remain unchanged]
    [SerializeField] AudioSource _source;
    [SerializeField, Min(100)] int _sizeCompressedSamples = 1000;
    [Header("Editor Settings")]
    [SerializeField] bool _editorPreview = true;
    [SerializeField, Range(0.1f, 5f)] float _editorUpdateDelay = 1f;
    [SerializeField] bool _autoUpdateInEditor = false;
    [Header("Waveform Settings")]
    [SerializeField, Range(0f, 1f)] private float startPart;
    [SerializeField, Range(0f, 1f)] private float endPart = 1f;

    // Private variables remain unchanged
    private float[] _samplesPacked;
    private float[] _cachedMonoSamples;
    private float _width;
    private float _height;
    private int _oldSizeCompressedSamples;
    private AudioClip _oldAudioClip;
    private float _oldStartPart;
    private float _oldEndPart;
    private float _lastUpdateTime;
    private bool _requiresUpdate = true;
    private int _totalMonoSamples;
    private float _lastEditorUpdateTime;
    private bool _editorChangesPending;

    protected override void Start()
    {
        base.Start();
        if (_source != null && _source.clip != null)
            CacheAudioData(_source.clip);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        
        if (_source == null || _source.clip == null || _samplesPacked == null || _samplesPacked.Length == 0)
            return;

        _width = rectTransform.rect.width;
        _height = rectTransform.rect.height;

        // Create vertices
        for (int i = 0; i < _samplesPacked.Length; i++)
        {
            float xPos = Mathf.Lerp(-_width / 2, _width / 2, (float)i / (_samplesPacked.Length - 1));
            float yValue = _samplesPacked[i];
            
            // Top vertex
            AddVertex(vh, new Vector2(xPos, yValue * _height / 2));
            
            // Bottom vertex
            AddVertex(vh, new Vector2(xPos, -yValue * _height / 2));
        }

        // Create triangles
        for (int i = 0; i < _samplesPacked.Length - 1; i++)
        {
            int index = i * 2;
            vh.AddTriangle(index, index + 2, index + 1);
            vh.AddTriangle(index + 1, index + 2, index + 3);
        }
    }

    private void Update()
    {
        if (_source == null || _source.clip == null)
            return;
        
        bool inPlayMode = Application.isPlaying;
        
#if UNITY_EDITOR
        // Editor-time updating
        if (!inPlayMode)
        {
            // Check for editor changes
            bool clipChanged = _oldAudioClip != _source.clip;
            bool sizeChanged = _oldSizeCompressedSamples != _sizeCompressedSamples;
            bool rangeChanged = !Mathf.Approximately(_oldStartPart, startPart) || 
                               !Mathf.Approximately(_oldEndPart, endPart);

            if (clipChanged || sizeChanged || rangeChanged)
            {
                _editorChangesPending = true;
                _lastEditorUpdateTime = (float)EditorApplication.timeSinceStartup;
            }

            // Process pending changes with delay
            if (_editorChangesPending && 
                (float)EditorApplication.timeSinceStartup - _lastEditorUpdateTime > _editorUpdateDelay)
            {
                _requiresUpdate = true;
                _editorChangesPending = false;
            }

            // Auto-update in editor if enabled
            if (_autoUpdateInEditor && _editorPreview)
            {
                _requiresUpdate = true;
            }
        }
        else
#endif
        {
            // Play mode updating: fixed update rate
            if (Time.time - _lastUpdateTime > 0.1f)
            {
                _lastUpdateTime = Time.time;
                _requiresUpdate = true;
            }
        }

        // Check for critical changes
        bool clipChangedCritical = _oldAudioClip != _source.clip;
        bool sizeChangedCritical = _oldSizeCompressedSamples != _sizeCompressedSamples;
        bool rangeChangedCritical = !Mathf.Approximately(_oldStartPart, startPart) || 
                                   !Mathf.Approximately(_oldEndPart, endPart);

        if (clipChangedCritical || sizeChangedCritical || rangeChangedCritical || _requiresUpdate)
        {
            _oldAudioClip = _source.clip;
            _oldSizeCompressedSamples = _sizeCompressedSamples;
            _oldStartPart = startPart;
            _oldEndPart = endPart;
            
            if (clipChangedCritical) 
                CacheAudioData(_source.clip);
            
            UpdateWaveform();
            
            // Only set dirty if we're in play mode or editor preview is enabled
            if (inPlayMode || _editorPreview)
            {
                SetVerticesDirty();
            }
            _requiresUpdate = false;
        }
    }

    private void CacheAudioData(AudioClip clip)
    {
        if (clip == null) return;

        float[] multiChannelData = new float[clip.samples * clip.channels];
        clip.GetData(multiChannelData, 0);

        _cachedMonoSamples = new float[clip.samples];
        _totalMonoSamples = clip.samples;

        for (int i = 0; i < clip.samples; i++)
        {
            float sum = 0;
            for (int c = 0; c < clip.channels; c++)
            {
                int index = i * clip.channels + c;
                if (index < multiChannelData.Length)
                    sum += multiChannelData[index];
            }
            _cachedMonoSamples[i] = sum / clip.channels;
        }
    }

    private void UpdateWaveform()
    {
        if (_source == null || _source.clip == null || _cachedMonoSamples == null)
        {
            _samplesPacked = new float[_sizeCompressedSamples];
            return;
        }

        int sampleRate = _source.clip.frequency;
        float clipLength = _source.clip.length;

        int startSample = Mathf.FloorToInt(startPart * clipLength * sampleRate);
        int endSample = Mathf.FloorToInt(endPart * clipLength * sampleRate);

        startSample = Mathf.Clamp(startSample, 0, _totalMonoSamples - 1);
        endSample = Mathf.Clamp(endSample, startSample, _totalMonoSamples - 1);

        int sampleCount = endSample - startSample;
        if (sampleCount <= 0)
        {
            _samplesPacked = new float[_sizeCompressedSamples];
            return;
        }

        int compressedSampleCount = _sizeCompressedSamples;
        _samplesPacked = new float[compressedSampleCount];

        for (int i = 0; i < compressedSampleCount; i++)
        {
            int segmentStart = startSample + (int)((long)i * sampleCount / compressedSampleCount);
            int segmentEnd = startSample + (int)((long)(i + 1) * sampleCount / compressedSampleCount);
            if (segmentEnd > endSample)
                segmentEnd = endSample;

            float max = 0f;
            for (int j = segmentStart; j < segmentEnd; j++)
            {
                float absValue = Mathf.Abs(_cachedMonoSamples[j]);
                if (absValue > max)
                    max = absValue;
            }
            _samplesPacked[i] = max;
        }
    }

    private void AddVertex(VertexHelper vh, Vector2 position)
    {
        var vertex = UIVertex.simpleVert;
        vertex.position = position;
        vertex.color = color;
        vh.AddVert(vertex);
    }

#if UNITY_EDITOR
    [ContextMenu("Update Waveform")]
    private void ManualUpdate()
    {
        if (_source == null || _source.clip == null) return;
        CacheAudioData(_source.clip);
        UpdateWaveform();
        SetVerticesDirty();
    }

    private void OnValidate()
    {
        if (startPart > endPart)
            endPart = startPart;
    }
#endif
}