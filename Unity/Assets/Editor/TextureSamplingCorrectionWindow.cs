using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Game.Editor
{
    public class TextureSamplingCorrectionWindow : EditorWindow
    {
        private Texture2D m_InputTexture;
        private Texture2D m_OutputTexture;
        private AnimationCurve m_SamplingCorrection;

        private void OnEnable()
        {
            m_InputTexture = null;
            m_OutputTexture = null;
            m_SamplingCorrection = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }

        private void OnDisable()
        {
            if (m_InputTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(m_InputTexture);
                m_InputTexture = null;
            }
            if (m_OutputTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(m_OutputTexture);
                m_OutputTexture = null;
            }
        }

        private void OnGUI()
        {
            Texture2D texture = EditorGUILayout.ObjectField(m_InputTexture, typeof(Texture2D), false) as Texture2D;
            if (texture != null && texture != m_InputTexture)
            {
                EditorApplication.delayCall += () =>
                {
                    if (m_OutputTexture)
                    {
                        if (m_InputTexture)
                            UnityEngine.Object.DestroyImmediate(m_InputTexture);
                        m_InputTexture = texture;
                        if (m_OutputTexture)
                            UnityEngine.Object.DestroyImmediate(m_OutputTexture);
                        RebuildOutputTexture();
                    }
                };
            }
            m_SamplingCorrection = EditorGUILayout.CurveField(m_SamplingCorrection, GUILayout.Width(200f));

            string path = AssetDatabase.GetAssetPath(m_InputTexture);
            GUILayout.Label(path);
            GUILayout.Space(20f);
            if (GUILayout.Button("Override") && m_OutputTexture != null)
            {
                EditorApplication.delayCall += () =>
                {
                    string path = AssetDatabase.GetAssetPath(m_InputTexture);
                    byte[] bytes = m_OutputTexture.EncodeToPNG();
                    File.WriteAllBytes(path, bytes);
                    AssetDatabase.Refresh();
                };
            }
        }

        private void RebuildOutputTexture()
        {
            if (!m_InputTexture)
                return;
            if (m_OutputTexture)
                UnityEngine.Object.DestroyImmediate(m_OutputTexture);
            m_OutputTexture = new Texture2D(m_InputTexture.width, m_InputTexture.height);
            for (int x = 0; x < m_InputTexture.width; x++)
            {
                for (int y = 0; y < m_InputTexture.height; y++)
                {
                    Color color = m_InputTexture.GetPixel(x, y);
                    Color.RGBToHSV(color, out float h, out float s, out float v);
                    v = m_SamplingCorrection.Evaluate(v);
                    if (v < 0.004f)
                        v = 0.004f;
                    color = Color.HSVToRGB(h, s, v);
                    m_OutputTexture.SetPixel(x, y, color);
                }
            }
        }
    }
}