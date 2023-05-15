#if CHINESE_GUI
using CommanderWrapper = Game.Editor.CommanderChinese;
using AmonAINameWrapper = Game.Editor.AmonAINameChinese;
using MapNameWrapper = Game.Editor.MapNameChinese;
using UnitLabelWrapper = Game.Editor.UnitLabelChinese;
#else
using CommanderWrapper = Table.CommanderName;
using AmonAINameWrapper = Table.AmonAIName;
using MapNameWrapper = Table.MapName;
using UnitLabelWrapper = Table.UnitLabel;
#endif

using Game.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Table;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class MapModelEditorWindow : EditorWindow
    {
        private static Dictionary<string, Texture> m_TechTextureCache = new Dictionary<string, Texture>();
        private static List<string> m_TryLoadList = new List<string>();

        private string m_MapModelPath;
        private MapModel m_MapModel;

        private Vector2 m_ScrollPosition;

        [MenuItem("Tools/Map Model Edit")]
        public static MapModelEditorWindow OpenWindow()
        {
            MapModelEditorWindow editorWindow = GetWindow<MapModelEditorWindow>();
            var rect = editorWindow.position;
            rect.width = 830f;
            editorWindow.position = rect;
            return editorWindow;
        }

        private void OnEnable()
        {
            for (int i = 0; i < GameDefined.JSONSerializedRegisterTypes.Length; i++)
                JSONMap.RegisterType(GameDefined.JSONSerializedRegisterTypes[i]);

            titleContent = new GUIContent("Map Model Edit");
            minSize = new Vector2(550f, 400f);
            m_ScrollPosition = Vector2.zero;
            m_MapModelPath = null;
            m_MapModel = null;
        }

        private void OnInspectorUpdate()
        {
            Repaint();
            if (m_TryLoadList.Count > 0)
            {
                string textureName = m_TryLoadList[0];
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Textures/{textureName}.png");
                if (texture != null)
                    m_TechTextureCache[textureName] = texture;
                m_TryLoadList.RemoveAll((str) => str == textureName);
            }
        }
        private void OnGUI()
        {
            if (string.IsNullOrWhiteSpace(m_MapModelPath) ||
                m_MapModel == null)
            {
                PrintChooseMapData();
            }
            else
            {
                PrintInfo();
                GUILayout.Space(20f);
                m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition, GUILayout.MinHeight(300f), GUILayout.MaxHeight(800f));
                IEventModel[] eventModels = m_MapModel.EventModels;
                for (int i = 0; i < eventModels.Length; i++)
                {
                    switch (eventModels[i])
                    {
                        case AttackWaveEventModel attackWaveEventModel:
                            PrintContent(attackWaveEventModel);
                            break;
                        case MapTriggerEventModel mapTriggerEventModel:
                            PrintContent(mapTriggerEventModel);
                            break;
                        default:
                            break;
                    }
                }
                GUILayout.EndScrollView();
                GUILayout.Space(20f);
                GUILayout.FlexibleSpace();
            }
        }

        private void PrintChooseMapData()
        {
            MapName[] mapNames = Enum.GetValues(typeof(MapName)) as MapName[];
            for (int i = 0; i < mapNames.Length; i++)
            {
                MapName mapName = mapNames[i];
                if (GUILayout.Button(((MapNameWrapper)mapName).ToString(), GUILayout.Width(250f)))
                {
                    EditorApplication.delayCall += () =>
                    {
                        m_MapModelPath = $"{GameDefined.ResourceSubmoduleDirectory}/Models/{mapName}.json";
                        JSONObject @object = JSONObject.Create(File.ReadAllText(m_MapModelPath));
                        m_MapModel = JSONMap.ParseJSON<MapModel>(@object);
                    };
                }
            }
        }
        private void PrintInfo()
        {
            GUILayout.BeginHorizontal();
            m_MapModel.MapName = (MapName)EditorGUILayout.EnumFlagsField((MapNameWrapper)m_MapModel.MapName, GUILayout.Width(150f));
            GUILayout.Space(20f);
            if (GUILayout.Button("Sort", GUILayout.Width(80f)))
                EditorApplication.delayCall += () =>
                {
                    Array.Sort(m_MapModel.EventModels, (l, r) =>
                    {
                        int compare = l.TriggerTime.CompareTo(r.TriggerTime);
                        if (compare == 0)
                            compare = l.StartTime.CompareTo(r.StartTime);
                        return compare;
                    });
                };
            if (GUILayout.Button("Save", GUILayout.Width(80f)))
            {
                for (int i = 0; i < m_MapModel.EventModels.Length; i++)
                {
                    IEventModel eventModel = m_MapModel.EventModels[i];
                    if (eventModel is AttackWaveEventModel attackWaveEventModel)
                    {
                        if (attackWaveEventModel.TriggerTime != 0 && attackWaveEventModel.StartTime == 0)
                            attackWaveEventModel.StartTime = attackWaveEventModel.TriggerTime - 15;
                        if (attackWaveEventModel.TriggerTime != 0 && attackWaveEventModel.EndTime == 0)
                            attackWaveEventModel.EndTime = attackWaveEventModel.TriggerTime + 5;
                    }
                    else if (eventModel is MapTriggerEventModel mapTriggerEventModel)
                    {
                        if (mapTriggerEventModel.TriggerTime != 0 && mapTriggerEventModel.StartTime == 0)
                            mapTriggerEventModel.StartTime = mapTriggerEventModel.TriggerTime - 60;
                        if (mapTriggerEventModel.TriggerTime != 0 && mapTriggerEventModel.EndTime == 0)
                            mapTriggerEventModel.EndTime = mapTriggerEventModel.TriggerTime + 3;
                    }
                }
                string content = JSONMap.ToJSON(m_MapModel).ToString(true);
                File.WriteAllText(m_MapModelPath, content);
            }
            GUILayout.Space(20f);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Attack Wave", GUILayout.Width(130f)))
                EditorApplication.delayCall += () =>
                {
                    Append(new AttackWaveEventModel()
                    {
                        MapSubType = MapSubType.AorB,
                        Guid = Guid.NewGuid(),
                    });
                };
            if (GUILayout.Button("Add Map Trigger", GUILayout.Width(130f)))
                EditorApplication.delayCall += () =>
                {
                    Append(new MapTriggerEventModel()
                    {
                        MapSubType = MapSubType.AorB,
                        Guid = Guid.NewGuid(),
                    });
                };
            GUILayout.EndHorizontal();
        }
        private void PrintContent(AttackWaveEventModel attackWaveEventModel)
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(nameof(AttackWaveEventModel), GUILayout.Width(250f));
            attackWaveEventModel.Annotation = GUILayout.TextField(attackWaveEventModel.Annotation, GUILayout.MinWidth(100f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.Label("MapSubType", GUILayout.Width(100f));
            attackWaveEventModel.MapSubType = (MapSubType)EditorGUILayout.EnumPopup(attackWaveEventModel.MapSubType, GUILayout.Width(80f));
            GUILayout.Label("StartTime", GUILayout.Width(100f));
            attackWaveEventModel.StartTime = EditorGUILayout.FloatField(attackWaveEventModel.StartTime, GUILayout.Width(60f));
            GUILayout.Label("TriggerTime", GUILayout.Width(100f));
            attackWaveEventModel.TriggerTime = EditorGUILayout.FloatField(attackWaveEventModel.TriggerTime, GUILayout.Width(60f));
            GUILayout.Label("EndTime", GUILayout.Width(70f));
            attackWaveEventModel.EndTime = EditorGUILayout.FloatField(attackWaveEventModel.EndTime, GUILayout.Width(60f));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.Label("Hide", GUILayout.Width(60f));
            attackWaveEventModel.Hide = EditorGUILayout.Toggle(attackWaveEventModel.Hide, GUILayout.Width(20f));
            GUILayout.Label("Technology", GUILayout.Width(120f));
            attackWaveEventModel.Technology = EditorGUILayout.IntField(attackWaveEventModel.Technology, GUILayout.Width(50f));
            GUILayout.Label("IncreasedScale", GUILayout.Width(120f));
            attackWaveEventModel.IncreasedScale = EditorGUILayout.Toggle(attackWaveEventModel.IncreasedScale, GUILayout.Width(20f));
            GUILayout.Label("BigHybrid", GUILayout.Width(100f));
            attackWaveEventModel.BigHybrid = EditorGUILayout.IntField(attackWaveEventModel.BigHybrid, GUILayout.Width(60f));
            GUILayout.Label("SmallHybrid", GUILayout.Width(100f));
            attackWaveEventModel.SmallHybrid = EditorGUILayout.IntField(attackWaveEventModel.SmallHybrid, GUILayout.Width(60f));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            attackWaveEventModel.Texture = EditorGUILayout.TextField(attackWaveEventModel.Texture, GUILayout.Width(100f));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            if (!string.IsNullOrWhiteSpace(attackWaveEventModel.Texture))
            {
                m_TechTextureCache.TryGetValue(attackWaveEventModel.Texture, out Texture texture);
                if (texture == null)
                    m_TryLoadList.Add(attackWaveEventModel.Texture);
                GUILayout.Label(texture, GUILayout.Width(128f), GUILayout.Height(128f));
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(8f);
        }
        private void PrintContent(MapTriggerEventModel mapTriggerEventModel)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(nameof(MapTriggerEventModel), GUILayout.Width(250f));
            mapTriggerEventModel.Annotation = GUILayout.TextField(mapTriggerEventModel.Annotation, GUILayout.MinWidth(100f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.Label("MapSubType", GUILayout.Width(100f));
            mapTriggerEventModel.MapSubType = (MapSubType)EditorGUILayout.EnumPopup(mapTriggerEventModel.MapSubType, GUILayout.Width(80f));
            GUILayout.Label("StartTime", GUILayout.Width(100f));
            mapTriggerEventModel.StartTime = EditorGUILayout.FloatField(mapTriggerEventModel.StartTime, GUILayout.Width(60f));
            GUILayout.Label("TriggerTime", GUILayout.Width(100f));
            mapTriggerEventModel.TriggerTime = EditorGUILayout.FloatField(mapTriggerEventModel.TriggerTime, GUILayout.Width(60f));
            GUILayout.Label("EndTime", GUILayout.Width(70f));
            mapTriggerEventModel.EndTime = EditorGUILayout.FloatField(mapTriggerEventModel.EndTime, GUILayout.Width(60f));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.Label("Texture", GUILayout.Width(70f));
            mapTriggerEventModel.Texture = GUILayout.TextField(mapTriggerEventModel.Texture, GUILayout.Width(170f));
            GUILayout.Label("Desc", GUILayout.Width(60f));
            mapTriggerEventModel.Desc = GUILayout.TextField(mapTriggerEventModel.Desc, GUILayout.Width(280f));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.Label("BigHybrid", GUILayout.Width(70f));
            mapTriggerEventModel.BigHybrid = EditorGUILayout.IntField(mapTriggerEventModel.BigHybrid, GUILayout.Width(60f));
            GUILayout.Label("SmallHybrid", GUILayout.Width(70f));
            mapTriggerEventModel.SmallHybrid = EditorGUILayout.IntField(mapTriggerEventModel.SmallHybrid, GUILayout.Width(60f));
            GUILayout.Label("Technology", GUILayout.Width(70f));
            mapTriggerEventModel.Technology = EditorGUILayout.IntField(mapTriggerEventModel.Technology, GUILayout.Width(60f));
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);
        }

        private void Append(IEventModel eventModel)
        {
            IEventModel[] models = new IEventModel[m_MapModel.EventModels.Length + 1];
            Array.Copy(m_MapModel.EventModels, models, m_MapModel.EventModels.Length);
            models[models.Length - 1] = eventModel;
            m_MapModel.EventModels = models;
        }
    }
}