#if UNITY_EDITOR && PR_SAYKIT_ENABLED
using Infrastructure.Utilities;
using Newtonsoft.Json;
using SayKitInternal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;



class TmpFontChecker : EditorWindow
{
    [Serializable]
    class FontData
    {
        public string missingSymbols;
        public string missingSymbolsHex;



        public FontData()
        {
            missingSymbols = string.Empty;
            missingSymbolsHex = string.Empty;
        }
    }


    enum Loacalization
    {
        text_en,
        text_ru,
        text_de,
        text_fr,
        text_es,
        text_it,
        text_pt,
        text_ja,
        text_zh,
        text_ko,
    }



    Dictionary<string /*Language*/, FontData> fontDatas = new Dictionary<string, FontData>();


    string fileName;


    [MenuItem("Tools/TMP Font Checker")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TmpFontChecker));
    }


    private void Awake()
    {
        ClearFontDatas();
    }


    private void OnGUI()
    {
        GUILayout.Space(20.0f);
        if (GUILayout.Button("Check Unused Messages", GUILayout.Height(40.0f)))
        {
            CheckUnusedMessages();
        }

        GUILayout.Space(40.0f);
        if (GUILayout.Button("Check Missing Symbols", GUILayout.Height(40.0f)))
        {
            CheckLocalizations();
        }
        GUILayout.Space(20.0f);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Lang:", GUILayout.Width(50.0f));
        GUILayout.Label("Missing Symbols:", GUILayout.Width(300.0f));
        GUILayout.Label("Missing Symbols (Hex):");
        GUILayout.EndHorizontal();

        foreach (var data in fontDatas)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(data.Key, GUILayout.Width(50.0f));
            data.Value.missingSymbols = GUILayout.TextField(data.Value.missingSymbols, GUILayout.Width(300));
            data.Value.missingSymbolsHex = GUILayout.TextField(data.Value.missingSymbolsHex);
            GUILayout.EndHorizontal();
        }

    }


    protected void CheckUnusedMessages()
    {
        SKLanguageLocalization[] gameMessagesArray = GetLocalizationsFromServer();
        if (gameMessagesArray == null)
        {
            return;
        }
        List<SKLanguageLocalization> localizations = new List<SKLanguageLocalization>(gameMessagesArray);
        SKLanguageLocalization enLocalization = localizations.Find(loc => loc.Language.Contains("en"));

        List<string> allAssetsPaths = new List<string>(AssetDatabase.GetAllAssetPaths());
        allAssetsPaths.RemoveAll(asset => !asset.Contains("Assets"));
        allAssetsPaths.RemoveAll(asset => asset.Contains(".meta"));

        RemoveGameMessagesUsedInScripts(allAssetsPaths, enLocalization);
        RemoveGameMessagesUsedInAssets(allAssetsPaths, enLocalization, ".asset");
        RemoveGameMessagesUsedInAssets(allAssetsPaths, enLocalization, ".prefab");

        new List<string>(enLocalization.Strings.Keys).ForEach(key => Debug.Log(key));
    }


    protected void CheckLocalizations()
    {
        ClearFontDatas();

        SKLanguageLocalization[] localizations = GetLocalizationsFromServer();
        if (localizations == null)
        {
            return;
        }

        foreach (SKLanguageLocalization localization in localizations)
        {
            AddMissingSymbols(localization);
        }
        CheckEmptyMissingSymbols();
    }


    private void RemoveGameMessagesUsedInScripts(List<string> allAssetsPaths, SKLanguageLocalization enLocalization)
    {
        List<string> assetsPath = allAssetsPaths.FindAll(asset => asset.Contains(".cs"));

        Regex regex = new Regex("\"(.*?)\"");
        foreach (string scriptPath in assetsPath)
        {
            string script = File.ReadAllText(scriptPath);
            MatchCollection matchCollection = regex.Matches(script);
            foreach (Match match in matchCollection)
            {
                string codeInAsset = match.Value.Replace("\"", "");
                if (codeInAsset.Length < 3
                    || !codeInAsset.Contains("_"))
                {
                    continue;
                }

                List<string> keys = new List<string>(enLocalization.Strings.Keys);
                keys.RemoveAll(key => !key.Contains(codeInAsset));
                keys.ForEach(key => enLocalization.Strings.Remove(key));
            }
        }
    }


    private void RemoveGameMessagesUsedInAssets(List<string> allAssetsPaths, SKLanguageLocalization enLocalization, string fileExtension)
    {
        List<string> assetsPath = allAssetsPaths.FindAll(asset => asset.Contains(fileExtension));
        foreach (string scriptPath in assetsPath)
        {
            string assetText = File.ReadAllText(scriptPath);
            List<string> keys = new List<string>(enLocalization.Strings.Keys);
            keys.RemoveAll(key => !assetText.Contains(key));
            keys.ForEach(key => enLocalization.Strings.Remove(key));
        }
    }


    private void ClearFontDatas()
    {
        fontDatas.Clear();
    }


    protected SKLanguageLocalization[] GetLocalizationsFromServer()
    {
        SKLanguageLocalization[] localizations = null;
        string appKey = SayKitApp.GetAppKey();
        int _attempts = 1;

        var url = " https://app.saygames.io/localization/" + appKey;
        var errorTemplate = "Can't load localization for " + appKey + " version " + Application.version;

        try
        {
            SKLocalizationService.Instance.DownloadLocalizationsWithAttempts(url, JsonConvert.SerializeObject(new
            {
                version = Application.version,
                saykit = SKManager.Instance.Version,
                v = 1,
                embedded = true
            }),
                _attempts,
                (data, error) =>
                {
                    if (string.IsNullOrEmpty(error))
                    {
                        if (!string.IsNullOrEmpty(data))
                        {
                            localizations = JsonConvert.DeserializeObject<SKLanguageLocalization[]>(data);
                        }
                        else
                        {
                            Debug.LogError("Empty localizations data");
                        }
                    }
                    else
                    {
                        Debug.LogError($"{errorTemplate} {error}");
                    }
                });
        }
        catch (Exception e)
        {
            Debug.LogError($"{errorTemplate} {e}");
        }

        return localizations;
    }


    private void AddMissingSymbols(SKLanguageLocalization localization)
    {
        List<TMP_FontAsset> fontAssets = new List<TMP_FontAsset>();
        fontAssets.Add(TMP_Settings.defaultFontAsset);
        fontAssets.AddRange(TMP_Settings.fallbackFontAssets);

        FontData fontData = new FontData();

        foreach(string gameMessage in localization.Strings.Values)
        {
            foreach (char symbol in gameMessage)
            {
                if (fontData.missingSymbols.Contains(symbol))
                {
                    continue;
                }

                uint unicode = Convert.ToUInt32(symbol);
                TMP_FontUtilities.SearchForCharacter(fontAssets, unicode, out TMP_Character tmp_character);
                if (tmp_character != null)
                {
                    continue;
                }

                fontData.missingSymbols += symbol + " ";
                fontData.missingSymbolsHex += unicode.ToString("X") + ",";
            }
        }

        fontDatas.Add(localization.Language, fontData);
    }


    private void CheckEmptyMissingSymbols()
    {
        foreach (var data in fontDatas)
        {
            if (data.Value.missingSymbols.IsNullOrEmpty())
            {
                data.Value.missingSymbols = "All Symbols in font!";
            }
        }
    }
}
#endif