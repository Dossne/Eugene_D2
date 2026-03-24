using Google.Apis.Sheets.v4.Data;
using System;
using Infrastructure.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Infrastructure.Configuration;
using UnityEditor;
using UnityEngine;

public class EditorConfigWindow : EditorWindow
{
    private string fileName;
    private bool isClearConsole = false;

    private void Awake()
    {
        Init();
    }


    private void OnGUI()
    {
        GUILayout.Space(20.0f);

        GUILayout.BeginHorizontal();
        GUILayout.Label("File Name", GUILayout.MaxWidth(90.0f));
        fileName = GUILayout.TextField(fileName);
        GUILayout.EndHorizontal();

        isClearConsole = EditorGUILayout.Toggle("Clear Console", isClearConsole);

        if (GUILayout.Button("Load All Configs", GUILayout.Height(40.0f)))
        {
            LoadAllConfigs(string.Empty);
        }
        
        GUILayout.Space(30.0f);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Generate Say Game Config", GUILayout.Width(200.0f), GUILayout.Height(40.0f)))
        {
            GenerateSayGameConfig();
        }
        GUILayout.EndHorizontal();
    }


    [MenuItem("Tools/Config window")]
    public static void ShowWindow()
    {
        GetWindow(typeof(EditorConfigWindow));
    }


    public static void GenerateSayGameConfig()
    {
        List<string> names = new List<string>();
        List<JsonConvertableConfig> configs = LoadLocalConfigs(string.Empty);
        configs.ForEach(config => names.Add($"    public string {config.name} = string.Empty;"));

        string sayKitAppFile = Path.Combine(Application.dataPath, "SayKitApp.cs");
        List<string> lines = new List<string>(File.ReadAllLines(sayKitAppFile));

        int idx = lines.FindIndex(line => line.Contains("class SayKitGameConfig"));
        if(idx == -1)
        {
            return;
        }

        while(!lines[idx].Contains("{"))
        {
            ++idx;
        }
        ++idx;

        while (!lines[idx].Contains("}"))
        {
            if(lines[idx].Contains("public int"))
            {
                ++idx;
            }
            else
            {
                lines.RemoveAt(idx);
            }
        }

        lines.InsertRange(idx, names);

        File.WriteAllLines(sayKitAppFile, lines.ToArray());
        
        Debug.Log("SayGameConfig generated");

    }


    private void Init()
    {
        fileName = Application.version;
    }   


    private async void LoadAllConfigs(string filter)
    {
        EditorUtility.DisplayProgressBar("Loading Configs", "Loading...", 0.35f);

        if (isClearConsole)
        {
            ClearConsole();
        }

        try
        {
            List<JsonConvertableConfig> localConfigs = LoadLocalConfigs(filter);
            List<Sheet> sheets = await LoadSheets(localConfigs);
            Dictionary<string, IList<IList<object>>> tables = SheetsToTables(sheets);
            ApplyTablesToLocalConfigs(localConfigs, tables);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }

        EditorUtility.ClearProgressBar();
    }


    private void ClearConsole()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }


    private static List<JsonConvertableConfig> LoadLocalConfigs(string filter)
    {
        List<JsonConvertableConfig> localConfigs = new List<JsonConvertableConfig>();
        string[] guids = AssetDatabase.FindAssets("t: ScriptableObject" + " " + filter, new string[] { "Assets" });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if(!path.Contains("Config")
                && !path.Contains("Cfg"))
            {
                continue;
            }

            JsonConvertableConfig config = AssetDatabase.LoadAssetAtPath(path, typeof(JsonConvertableConfig)) as JsonConvertableConfig;
            if (config == null)
            {
                continue;
            }

            localConfigs.Add(config);
        }

        return localConfigs;
    }


    private async Task<List<Sheet>> LoadSheets(List<JsonConvertableConfig> localConfigs)
    {
        List<string> localConfigsNames = new List<string>();
        localConfigs.ForEach(localConfig => localConfigsNames.Add(localConfig.GoogleSheetName));

        return await GoogleDocsUtils.ReadAllSpreadSheet(fileName, localConfigsNames);
    }


    private Dictionary<string, IList<IList<object>>> SheetsToTables(List<Sheet> sheets)
    {
        Debug.Log("Start Convert sheets to tables");

        Dictionary<string, IList<IList<object>>> tables = new Dictionary<string, IList<IList<object>>>();
        foreach (Sheet sheet in sheets)
        {
            IList<IList<object>> values = new List<IList<object>>();
            foreach (var gridDatas in sheet.Data)
            {
                foreach (var rowData in gridDatas.RowData)
                {
                    IList<object> obj = new List<object>();
                    values.Add(obj);

                    if (rowData == null
                        || rowData.Values == null)
                    {
                        continue;
                    }

                    foreach (var cellData in rowData.Values)
                    {
                        obj.Add(cellData.FormattedValue);
                    }
                }
            }

            tables.Add(sheet.Properties.Title, values);
        }

        return tables;
    }


    private void ApplyTablesToLocalConfigs(List<JsonConvertableConfig> localConfigs, Dictionary<string, IList<IList<object>>> tables)
    {
        Debug.Log("Start Applying tables to local configs");
        string appliedConfigsLog = string.Empty;
        int appliedConfigsCount = 0;
        string erroredConfigsLog = string.Empty;
        int erroredConfigsCount = 0;
        List<object> exceptions = new List<object>();
        foreach (JsonConvertableConfig localConfig in localConfigs)
        {
            if(!tables.ContainsKey(localConfig.GoogleSheetName))
            {
                continue;
            }

            tables[localConfig.GoogleSheetName] = RemoveLastNullCells(tables[localConfig.GoogleSheetName], localConfig.SpreadSheetDimension);

            localConfig.fileName = fileName;

            try
            {
                localConfig.TableDataToConfigData_Editor(tables[localConfig.GoogleSheetName]);
                EditorUtility.SetDirty(localConfig);
                appliedConfigsLog += $"\n  {localConfig.GoogleSheetName} to {localConfig.name}";
                ++appliedConfigsCount;
            }
            catch(Exception ex)
            {
                exceptions.Add(ex);
                erroredConfigsLog += $"\n  {localConfig.GoogleSheetName}";
                ++erroredConfigsCount;
            }

        }

        if (!appliedConfigsLog.IsNullOrEmpty())
        {
            Debug.Log($"{appliedConfigsCount} applied configs: {appliedConfigsLog}\n");
        }

        if (!erroredConfigsLog.IsNullOrEmpty())
        {
            Debug.LogError($"{erroredConfigsCount} errored Configs: {erroredConfigsLog}\n");
        }

        exceptions.ForEach(ex => Debug.LogError(ex));
    }


    private IList<IList<object>> RemoveLastNullCells(IList<IList<object>> table, GoogleDocsUtils.MajorDimension dimension)
    {
        IList<IList<object>> resultTable;
        if(dimension == GoogleDocsUtils.MajorDimension.COLUMNS)
        {
            resultTable = GoogleDocsUtils.Transpose(table);
        }
        else
        {
            resultTable = new List<IList<object>>(table);
        }

        for(int i = resultTable.Count - 1; i >= 0; --i)
        {
            for(int j = resultTable[i].Count - 1; j >= 0; --j)
            {
                if (resultTable[i][j] != null)
                {
                    continue;
                }

                if(j + 1 == resultTable[i].Count)
                {
                    resultTable[i].RemoveAt(j);
                }
                else
                {
                    resultTable[i][j] = string.Empty;
                }
            }

            if(resultTable[i].Count == 0
                && resultTable.Count == i + 1)
            {
                resultTable.RemoveAt(i);
            }
        }

        

        return resultTable;
    }
}
