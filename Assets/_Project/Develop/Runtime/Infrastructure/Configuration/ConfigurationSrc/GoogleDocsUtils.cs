#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Infrastructure.Utilities;
using UnityEditor;
using UnityEngine;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.GetRequest;

namespace Infrastructure.Configuration
{
    public static class GoogleDocsUtils
    {
        private const string LOG_TAG = "[GOOGLE_DOC]";

        #region Nested Type

        public enum MajorDimension
        {
            ROWS,
            COLUMNS
        }

        #endregion

        #region Members

        private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private const string GoogleCredentialsFileName = "Credentials/GoogleDocCredentials/project_gdoc.json";

        #endregion

        #region Public Methods

        public static async Task<List<Sheet>> ReadAllSpreadSheet(string fileName, List<string> localConfigsNames)
        {
            string spreadsheetId = GetSpreadSheetIdByName(fileName);

            var spreadsheets = GetSheetsService().Spreadsheets;

            var requestMetaData = spreadsheets.Get(spreadsheetId);
            requestMetaData.IncludeGridData = false;
            var responseMetaData = await requestMetaData.ExecuteAsync();
            var metaDatas = responseMetaData.Sheets;

            string localConfigsNotFoundLog = string.Empty;
            List<string> titles = new List<string>();
            foreach (var metaData in metaDatas)
            {
                if (localConfigsNames.Contains(metaData.Properties.Title))
                {
                    titles.Add(metaData.Properties.Title);
                }
                else if (metaData.Properties.Title != "Contents")
                {
                    localConfigsNotFoundLog += $"\n  {metaData.Properties.Title}";
                }
            }

            if (!localConfigsNotFoundLog.IsNullOrEmpty())
            {
                Debug.LogWarning($"{LOG_TAG} Local configs not found: {localConfigsNotFoundLog}\n");
            }

            List<Sheet> sheets = new List<Sheet>();
            int countInResponse = 25;
            for (int i = 0; i < titles.Count; i += countInResponse)
            {
                int configsCount = Math.Min(countInResponse, titles.Count - i);
                Debug.Log($"{LOG_TAG} Try to load {i + 1}-{i + configsCount} sheets");
                var requestSheets = spreadsheets.Get(spreadsheetId);
                requestSheets.Ranges = titles.GetRange(i, configsCount);
                requestSheets.IncludeGridData = true;

                var responseSheets = await requestSheets.ExecuteAsync();
                sheets.AddRange(responseSheets.Sheets);
                string stringLoadedTitles = string.Empty;
                foreach (Sheet loadedSheet in responseSheets.Sheets)
                {
                    stringLoadedTitles += $"\n  {loadedSheet.Properties.Title}";
                }

                Debug.Log($"{LOG_TAG} Sheets loaded: {stringLoadedTitles}\n");
            }

            return sheets;
        }

        public static async Task<IList<IList<object>>> ReadAsync(string fileName, string googleSheetName,
            MajorDimension majorDimension = MajorDimension.ROWS)
        {
            var serviceValues = GetSheetsService().Spreadsheets.Values;

            string spreadsheetId = GetSpreadSheetIdByName(fileName);

            var request = serviceValues.Get(spreadsheetId, googleSheetName);
            request.MajorDimension = (MajorDimensionEnum)Enum.Parse(typeof(MajorDimensionEnum), majorDimension.ToString());

            var response = await request.ExecuteAsync();

            var values = response.Values;
            if (values == null || !values.Any())
            {
                Debug.Log($"{LOG_TAG} No data found in {googleSheetName}");
                return null;
            }

            Debug.Log($"{LOG_TAG} {googleSheetName} successfully imported!");

            return values;
        }

        public static async void WriteAsync<T>(List<T> dataConfig, string dataName,
            string fileName, string googleSheetName, MajorDimension majorDimension = MajorDimension.ROWS)
        {
            var serviceValues = GetSheetsService().Spreadsheets.Values;
            string spreadsheetId = GetSpreadSheetIdByName(fileName);
            await ClearAsync(serviceValues, spreadsheetId, googleSheetName);

            var valueRange = new ValueRange();
            valueRange.MajorDimension = majorDimension.ToString();
            valueRange.Values = ConfigToDataRows(dataConfig, dataName);

            var updateRequest = serviceValues.Update(valueRange, spreadsheetId, googleSheetName);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

            await updateRequest.ExecuteAsync();

            Debug.Log($"{LOG_TAG} {dataName} write to {googleSheetName}");
        }

        public static async void WriteAsync(IList<IList<object>> data, string fileName, string googleSheetName,
            MajorDimension majorDimension = MajorDimension.ROWS)
        {
            var serviceValues = GetSheetsService().Spreadsheets.Values;
            string spreadsheetId = GetSpreadSheetIdByName(fileName);
            await ClearAsync(serviceValues, spreadsheetId, googleSheetName);

            var valueRange = new ValueRange();
            valueRange.MajorDimension = majorDimension.ToString();
            valueRange.Values = data;

            var updateRequest = serviceValues.Update(valueRange, spreadsheetId, googleSheetName);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

            await updateRequest.ExecuteAsync();

            Debug.Log($"{LOG_TAG} {googleSheetName} successfully exported!");
        }

        public static async void WriteAsync(IList<IList<object>> data, string fileName, string googleSheetName,
            int startRow, int numberOfRows, MajorDimension majorDimension = MajorDimension.ROWS)
        {
            var serviceValues = GetSheetsService().Spreadsheets.Values;
            string spreadsheetId = GetSpreadSheetIdByName(fileName);

            var valueRange = new ValueRange();
            valueRange.MajorDimension = majorDimension.ToString();

            if (data.Count > numberOfRows)
                data = data.Take(numberOfRows).ToList();

            valueRange.Values = data;

            var range = googleSheetName + $"!A{startRow}";
            var updateRequest = serviceValues.Update(valueRange, spreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

            await updateRequest.ExecuteAsync();

            Debug.Log($"{LOG_TAG} {ObjectNames.NicifyVariableName(googleSheetName)} successfully exported!");
        }

        public static string GetSpreadSheetIdByName(string name)
        {
            GoogleCredential credential = GoogleCredential.FromFile(GoogleCredentialsFileName)
                                                          .CreateScoped(DriveService.ScopeConstants.Drive);

            DriveService driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
            });

            try
            {
                if (driveService == null)
                {
                    throw new ArgumentNullException("service");
                }

                var request = driveService.Files.List();
                request.Q = "mimeType = 'application/vnd.google-apps.spreadsheet'";
                FileList result = request.Execute();

                List<Google.Apis.Drive.v3.Data.File> files = result.Files.Where(file => file.Name == name).ToList();
                if (files.Count != 1)
                {
                    throw new Exception($"{LOG_TAG} There are {files.Count} files with name {name}");
                }

                Debug.Log($"{LOG_TAG} File {name} Found!");
                return files[0].Id;
            }
            catch (Exception ex)
            {
                throw new Exception("Request Files.List failed.", ex);
            }
        }

        public static IList<IList<object>> ConfigToDataRows<T>(IEnumerable<T> objs, string dataName, bool isInheritOrdered = false)
        {
            FieldInfo[] fi = GetFields<T>();

            if (isInheritOrdered)
            {
                fi = fi.OrderBy(field => field.MetadataToken).ToArray();
            }

            List<IList<object>> data = new List<IList<object>>();

            IList<object> tableName = new List<object>();
            tableName.Add(ObjectNames.NicifyVariableName(dataName));
            for (int i = 1; i < fi.Length; ++i)
            {
                tableName.Add("");
            }

            data.Add(tableName);

            IList<object> headerRow = new List<object>();
            foreach (FieldInfo f in fi)
            {
                headerRow.Add(f.Name);
            }

            data.Add(headerRow);

            foreach (T obj in objs)
            {
                IList<object> row = new List<object>();
                foreach (FieldInfo f in fi)
                {
                    if (f.FieldType.IsEnum)
                    {
                        row.Add(f.GetValue(obj).ToString());
                    }
                    else if (f.ReflectedType.IsEnum)
                    {
                        row.Add(obj.ToString());
                    }
                    else
                    {
                        row.Add(f.GetValue(obj));
                    }

                }

                data.Add(row);
            }

            return data;
        }

        public static void AddConfigToDataRows<T>(IEnumerable<T> objs,
            string dataName, ref List<IList<object>> data, bool isAddEmptyColumn = true, bool isInheritOrdered = false)
        {
            data.AddRange(ConfigToDataRows(objs, dataName, isInheritOrdered));

            if (isAddEmptyColumn)
            {
                data.Add(new List<object>());
            }
        }

        public static void AddConfigToDataColumns<T>(IEnumerable<T> objs,
            string dataName, ref List<IList<object>> data, bool isAddEmptyColumn = true, bool isInheritOrdered = false)
        {
            data.AddRange(ConfigToDataColumns(objs, dataName, isInheritOrdered));

            if (isAddEmptyColumn)
            {
                data.Add(new List<object>());
            }
        }

        public static List<T> DataRowsToConfig<T>(IList<IList<object>> data) where T : new()
        {
            var ret = new List<T>();

            List<string> headers = new List<string>();

            if (data.Count > 1)
                foreach (var obj in data[1])
                {
                    headers.Add(obj.ToString());
                }

            FieldInfo[] fi = GetFields<T>();
            PropertyInfo[] pi = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            bool isValueType = typeof(T).IsValueType;

            foreach (var row in data.Skip(2))
            {
                var obj = new T();
                // box manually to avoid issues with structs
                object boxed = obj;
                if (RowToObject(row, headers, fi, pi, boxed))
                {
                    // unbox value types
                    if (isValueType)
                        obj = (T)boxed;
                    ret.Add(obj);
                }
            }

            return ret;
        }

        public static List<T> SnatchDataRowsToConfig<T>(IList<IList<object>> data, bool isRemoveEmptyColumn = true) where T : new()
        {
            List<IList<object>> configData = new List<IList<object>>();

            while (data.Count > 0
                   && data[0].Count > 0)
            {
                configData.Add(data[0]);
                data.RemoveAt(0);
            }

            if (isRemoveEmptyColumn
                && data.Count > 0)
            {
                data.RemoveAt(0);
            }

            return DataRowsToConfig<T>(configData);
        }

        public static List<T> SnatchDataColumnsToConfig<T>(IList<IList<object>> data, bool isRemoveEmptyColumn = true) where T : new()
        {
            FieldInfo[] fi = GetFields<T>();

            List<IList<object>> configData = new List<IList<object>>();

            for (int i = 0; i < fi.Length; ++i)
            {
                configData.Add(data[0]);
                data.RemoveAt(0);
            }

            if (isRemoveEmptyColumn
                && data.Count > 0)
            {
                data.RemoveAt(0);
            }

            return DataColumnsToConfig<T>(configData);
        }

        public static List<T> SnatchDataColumnsToConfigWithCheck<T>(IList<IList<object>> data, bool isRemoveEmptyColumn = true) where T : new()
        {
            FieldInfo[] fi = GetFields<T>();
            List<IList<object>> configData = new List<IList<object>>();

            for (int i = 0; i < fi.Length; ++i)
            {
                if (data.Count > 0
                    && data[0].Count > 1
                    && data[0][1] is string headerName
                    && headerName.Equals(fi[i].Name, StringComparison.OrdinalIgnoreCase))
                {
                    configData.Add(data[0]);
                    data.RemoveAt(0);
                }
                else
                {
                    configData.Add(new List<object>() { "", fi[i].Name });
                }
            }

            if (isRemoveEmptyColumn && data.Count > 0)
            {
                data.RemoveAt(0);
            }

            return DataColumnsToConfig<T>(configData);
        }

        public static FieldInfo[] GetFields<T>()
        {
            FieldInfo[] fi = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public);
            if (fi.IsNullOrEmpty())
            {
                fi = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            }

            return fi;
        }

        #endregion

        #region Private Methods

        private static SheetsService GetSheetsService()
        {
            using (var stream = new FileStream(GoogleCredentialsFileName, FileMode.Open, FileAccess.Read))
            {
                var serviceInitializer = new BaseClientService.Initializer
                {
                    HttpClientInitializer = GoogleCredential.FromStream(stream).CreateScoped(Scopes)
                };
                return new SheetsService(serviceInitializer);
            }
        }

        private static async Task ClearAsync(SpreadsheetsResource.ValuesResource valuesResource,
            string spreadsheetId, string googleSheetName)
        {
            var clearValueRequest = new ClearValuesRequest();

            var clearRequest = valuesResource.Clear(clearValueRequest, spreadsheetId, googleSheetName);
            await clearRequest.ExecuteAsync();
        }

        private static IList<IList<object>> ConfigToDataColumns<T>(IEnumerable<T> objs, string dataName, bool isInheritOrdered = true)
        {
            IList<IList<object>> data = ConfigToDataRows<T>(objs, dataName, isInheritOrdered);
            data = Transpose(data);
            return data;
        }

        private static List<T> DataColumnsToConfig<T>(IList<IList<object>> data) where T : new()
        {
            data = Transpose(data);
            List<T> ret = DataRowsToConfig<T>(data);

            return ret;
        }

        public static IList<IList<object>> Transpose(IList<IList<object>> lists)
        {
            var longest = lists.Any() ? lists.Max(l => l.Count) : 0;
            List<IList<object>> outer = new List<IList<object>>(longest);
            for (int i = 0; i < longest; i++)
                outer.Add(new List<object>(lists.Count));
            for (int j = 0; j < lists.Count; j++)
            for (int i = 0; i < longest; i++)
                outer[i].Add(lists[j].Count > i ? lists[j][i] : default(object));

            return outer;
        }

        private static bool RowToObject(IList<object> row, List<string> headers, FieldInfo[] fi, PropertyInfo[] pi, object destObject)
        {
            bool setAny = false;
            for (int i = 0; i < headers.Count; ++i)
            {
                if (i < row.Count)
                {
                    string val = row[i] != null ? row[i].ToString() : "";
                    setAny = SetField(headers[i], val, fi, pi, destObject) || setAny;
                }
            }

            return setAny;
        }

        private static bool SetField(string fieldName, string val, FieldInfo[] fi, PropertyInfo[] pi, object destObject)
        {
            bool result = false;
            foreach (PropertyInfo p in pi)
            {
                // Case insensitive comparison
                if (string.Compare(fieldName, p.Name, true) == 0)
                {
                    // Might need to parse the string into the property type
                    object typedVal = p.PropertyType == typeof(string) ? val : ParseString(val, p.PropertyType);
                    p.SetValue(destObject, typedVal, null);
                    result = true;
                    break;
                }
            }

            foreach (FieldInfo f in fi)
            {
                // Case insensitive comparison
                if (string.Compare(fieldName, f.Name, true) == 0)
                {
                    object typedVal = null;
                    if (f.FieldType.IsEnum)
                    {
                        typedVal = Enum.Parse(f.FieldType, val);
                    }
                    else if (f.ReflectedType.IsEnum)
                    {
                        typedVal = Enum.Parse(f.ReflectedType, val);
                    }
                    else if (f.FieldType == typeof(string))
                    {
                        typedVal = val;
                    }
                    else
                    {
                        typedVal = ParseString(val, f.FieldType);
                    }

                    f.SetValue(destObject, typedVal);
                    result = true;
                    break;
                }
            }

            return result;
        }

        private static object ParseString(string strValue, Type t)
        {
            if (strValue == string.Empty)
            {
                return default;
            }

            var cv = TypeDescriptor.GetConverter(t);
            try
            {
                return cv.ConvertFromInvariantString(strValue.Replace(",", "."));
            }
            catch
            {
                throw new FormatException("string: " + strValue + " | type " + t);
            }
        }

        private static string GetColumnId(int idx)
        {
            char column = Convert.ToChar('A' + idx);

            if (column - 'Z' > 0)
            {
                int charQuantity = ('Z' - 'A' + 1);
                char firstChar = Convert.ToChar('A' + idx / charQuantity - 1);
                char secondChar = Convert.ToChar('A' + idx % charQuantity);
                return $"{firstChar}{secondChar}";
            }

            return $"{column}";
        }

        #endregion

    }
}

#endif