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
        //private UnitTable m_UnitTable;

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
            titleContent = new GUIContent("Map Model Edit");
            minSize = new Vector2(550f, 400f);

            //string unitTablePath = $"{GameDefined.ResourceSubmoduleDirectory}/Tables/UnitTable.json";
            //string content = File.ReadAllText(unitTablePath);
            //m_UnitTable = JSONMap.ParseJSON<UnitTable>(JSONObject.Create(content));

            m_ScrollPosition = Vector2.zero;
        }

        private void OnGUI()
        {
            if (string.IsNullOrWhiteSpace(m_MapModelPath) &&
                m_MapModel != null)
            {
                PrintChooseMapData();
            }
            else
            {
                PrintInfo();
                GUILayout.Space(20f);
                m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition, GUILayout.MinHeight(300f), GUILayout.MaxHeight(800f));
                AttackWaveEventModel[] eventModels = m_MapModel.EventModels;
                for (int i = 0; i < eventModels.Length; i++)
                    PrintContent(eventModels[i]);
                GUILayout.EndScrollView();
                GUILayout.Space(20f);
                if (GUILayout.Button("Add"))
                    EditorApplication.delayCall += () =>
                    {
                        AttackWaveEventModel[] models = new AttackWaveEventModel[m_MapModel.EventModels.Length + 1];
                        Array.Copy(m_MapModel.EventModels, models, m_MapModel.EventModels.Length);
                        models[models.Length - 1] = new AttackWaveEventModel()
                        {
                            MapSubType = MapSubType.AorB,
                            Guid = new Guid(),
                        };
                    };
                GUILayout.FlexibleSpace();
            }
        }

        private void PrintChooseMapData()
        {
            MapName[] mapNames = Enum.GetValues(typeof(MapName)) as MapName[];
            for (int i = 0; i < mapNames.Length; i++)
            {
                MapName mapName = mapNames[i];
                if (GUILayout.Button(((MapNameWrapper)mapName).ToString(), GUILayout.Width(450f)))
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
            m_MapModel.MapName = (MapName)EditorGUILayout.EnumFlagsField((MapNameWrapper)m_MapModel.MapName);
            if (GUILayout.Button("Sort"))
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
            GUILayout.EndHorizontal();
        }
        private void PrintContent(AttackWaveEventModel attackWaveEventModel)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Technology", GUILayout.Width(200f));
            attackWaveEventModel.Technology = EditorGUILayout.IntField(attackWaveEventModel.Technology, GUILayout.Width(50f));
            GUILayout.Label("StartTime", GUILayout.Width(200f));
            attackWaveEventModel.StartTime = EditorGUILayout.FloatField(attackWaveEventModel.StartTime, GUILayout.Width(80f));
            GUILayout.Label("TriggerTime", GUILayout.Width(200f));
            attackWaveEventModel.TriggerTime = EditorGUILayout.FloatField(attackWaveEventModel.TriggerTime, GUILayout.Width(80f));
            GUILayout.Label("EndTime", GUILayout.Width(120f));
            attackWaveEventModel.EndTime = EditorGUILayout.FloatField(attackWaveEventModel.EndTime, GUILayout.Width(80f));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            attackWaveEventModel.Hide = EditorGUILayout.Toggle("Hide", attackWaveEventModel.Hide, GUILayout.Width(80f));
            GUILayout.Label("MapSubType", GUILayout.Width(120f));
            attackWaveEventModel.MapSubType = (MapSubType)EditorGUILayout.EnumFlagsField(m_MapModel.MapSubType, GUILayout.Width(80f));
            GUILayout.EndHorizontal();
        }
    }
}