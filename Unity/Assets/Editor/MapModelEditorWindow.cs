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
using UnityEngine.Experimental.TerrainAPI;

namespace Game.Editor
{
    public class MapModelEditorWindow : EditorWindow
    {
        private string m_MapModelPath;
        private MapModel m_MapModel;

        private Vector2 m_ScrollPosition;

        [MenuItem("Tools/Map Model Edit")]
        public static MapModelEditorWindow OpenWindow()
        {
            for (int i = 0; i < GameDefined.JSONSerializedRegisterTypes.Length; i++)
                JSONMap.RegisterType(GameDefined.JSONSerializedRegisterTypes[i]);

            MapModelEditorWindow editorWindow = GetWindow<MapModelEditorWindow>();
            var rect = editorWindow.position;
            rect.width = 830f;
            editorWindow.position = rect;
            return editorWindow;
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Map Model Edit");
            minSize = new Vector2(550f, 400f);
            m_ScrollPosition = Vector2.zero;
        }

        private void OnInspectorUpdate()
        {
            Repaint();
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
            GUILayout.Label(nameof(AttackWaveEventModel));
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
            attackWaveEventModel.Hide = EditorGUILayout.Toggle(attackWaveEventModel.Hide, string.Empty, GUILayout.Width(20f));
            GUILayout.Label("Technology", GUILayout.Width(120f));
            attackWaveEventModel.Technology = EditorGUILayout.IntField(attackWaveEventModel.Technology, GUILayout.Width(50f));
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);
        }
        private void PrintContent(MapTriggerEventModel mapTriggerEventModel)
        {
            GUILayout.Label(nameof(MapTriggerEventModel));
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