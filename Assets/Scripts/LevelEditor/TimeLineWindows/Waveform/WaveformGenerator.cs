using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;

namespace TimeLine
{
    public class WaveformGenerator : Graphic
    {
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private int offsetSamples;
        
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            
            if (audioClip == null || audioClip.samples == 0)
                return;

            int numChannels = audioClip.channels;
            int totalSamples = audioClip.samples * numChannels;
            var originalSamples = new NativeArray<float>(totalSamples, Allocator.Temp);
            audioClip.GetData(originalSamples, offsetSamples);

            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;

            // Handle invalid dimensions
            if (width <= 0 || height <= 0)
            {
                originalSamples.Dispose();
                return;
            }

            // Downsample with channel-aware processing
            float[] downsampled = DownsampleWithChannels(originalSamples, numChannels, width);
            originalSamples.Dispose();

            // Generate mesh
            GenerateWaveformMesh(vh, downsampled, width, height);
        }

        private float[] DownsampleWithChannels(NativeArray<float> samples, int numChannels, float width)
        {
            int totalFrames = samples.Length / numChannels;
            int stepFrames = Mathf.Max(1, Mathf.FloorToInt(totalFrames / width));
            int downsampledFrames = (totalFrames + stepFrames - 1) / stepFrames;
            float[] downsampled = new float[downsampledFrames];

            for (int i = 0; i < downsampledFrames; i++)
            {
                int frameIndex = i * stepFrames;
                int sampleIndex = frameIndex * numChannels;
                
                if (sampleIndex >= samples.Length) 
                    break;

                float sum = 0;
                for (int c = 0; c < numChannels; c++)
                {
                    int idx = sampleIndex + c;
                    if (idx < samples.Length)
                        sum += samples[idx];
                }
                downsampled[i] = sum / numChannels;
            }
            return downsampled;
        }

        private void GenerateWaveformMesh(VertexHelper vh, float[] samples, float width, float height)
        {
            if (samples.Length == 0) 
                return;

            UIVertex vert = UIVertex.simpleVert;
            vert.color = color;
            int vertexCount = 0;

            for (int i = 0; i < samples.Length; i++)
            {
                // Calculate position in the UI
                float t = (samples.Length > 1) ? (float)i / (samples.Length - 1) : 0.5f;
                float x = -width / 2 + t * width;
                float yTop = samples[i] * (height / 2);
                float yBottom = -height / 2;

                // Add top vertex
                vert.position = new Vector3(x, yTop, 0);
                vh.AddVert(vert);
                
                // Add bottom vertex
                vert.position = new Vector3(x, yBottom, 0);
                vh.AddVert(vert);

                vertexCount += 2;

                // Connect segments after the first point
                if (i > 0)
                {
                    vh.AddTriangle(vertexCount - 4, vertexCount - 3, vertexCount - 2);
                    vh.AddTriangle(vertexCount - 3, vertexCount - 1, vertexCount - 2);
                }
            }
        }
    }
}