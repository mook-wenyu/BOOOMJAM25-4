using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using Newtonsoft.Json;

public class EditorUtils
{
    private class PropertyInfo
    {
        public string Name;
        public string Type;
    }

    private class PendingConfig
    {
        public string ConfigName;
        public List<PropertyInfo> Properties;
        public ISheet Sheet;
    }

    private static List<PendingConfig> pendingJsonConfigs = new List<PendingConfig>();

    [MenuItem("Tools/生成配置Json")]
    public static void GenerateConfigs()
    {
        DeleteAllOldFiles();

        var excelPath = $"{Application.dataPath}/../Configs";
        if (!Directory.Exists(excelPath))
        {
            Debug.LogError("Directory does not exist.");
            return;
        }

        string[] files = Directory.GetFiles(excelPath, "*.xlsx", SearchOption.TopDirectoryOnly);

        for (int i = 0; i < files?.Length; i++)
        {
            ReadExcel(files[i]);
        }

        AssetDatabase.Refresh();

        // 等待类型刷新完再导出 JSON
        EditorApplication.delayCall += () =>
        {
            foreach (var pending in pendingJsonConfigs)
            {
                GenerateConfigJson(pending.Properties, pending.ConfigName, pending.Sheet);
            }

            AssetDatabase.Refresh();
            Debug.Log("完成导出！");
            pendingJsonConfigs.Clear();
        };
    }

    private static void ReadExcel(string filePath)
    {
        IWorkbook wk = null;
        string extension = Path.GetExtension(filePath);
        string fileName = Path.GetFileNameWithoutExtension(filePath);

        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            if (extension.Equals(".xls"))
                wk = new HSSFWorkbook(fs);
            else
                wk = new XSSFWorkbook(fs);
        }

        ISheet sheet = wk.GetSheetAt(0);
        IRow row = sheet.GetRow(1); // 字段名称
        IRow rowType = sheet.GetRow(2); // 字段类型
        var firstCell = row.GetCell(0);
        if (firstCell.ToString() != "id")
        {
            Debug.LogError($"导出Configs错误！{fileName}表中第一列不是id！");
            return;
        }

        List<PropertyInfo> configProperties = new List<PropertyInfo>();
        for (int i = 1; i < row.LastCellNum; i++)
        {
            var cell = row.GetCell(i);
            var name = cell.ToString();
            if (string.IsNullOrEmpty(name))
                break;

            var cellType = rowType.GetCell(i);
            configProperties.Add(new PropertyInfo() { Name = name, Type = cellType.ToString() });
        }

        GenrateConfigClass(configProperties, fileName);
        pendingJsonConfigs.Add(new PendingConfig
        {
            ConfigName = fileName,
            Properties = configProperties,
            Sheet = sheet
        });
    }

    private static void GenrateConfigClass(List<PropertyInfo> propertyInfos, string configName)
    {
        var filePath = $"{Application.dataPath}/Scripts/Configs/{configName}Config.cs";
        var fileDir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(fileDir))
            Directory.CreateDirectory(fileDir);

        var sb = new StringBuilder();
        sb.AppendLine($"public class {configName}Config : BaseConfig");
        sb.AppendLine("{");

        foreach (var prop in propertyInfos)
        {
            sb.AppendLine($"    public {prop.Type} {prop.Name};");
        }

        sb.AppendLine("}");

        File.WriteAllText(filePath, sb.ToString());
    }

    private static void GenerateConfigJson(List<PropertyInfo> propertyInfos, string configName, ISheet sheet)
    {
        var filePath = $"{Application.dataPath}/Resources/Configs/{configName}Config.txt";
        var fileDir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(fileDir))
            Directory.CreateDirectory(fileDir);

        propertyInfos.Insert(0, new PropertyInfo() { Name = "id", Type = "string" });

        Dictionary<string, BaseConfig> rawDataDic = new Dictionary<string, BaseConfig>();

        for (int i = 3; i <= sheet.LastRowNum; i++)
        {
            var row = sheet.GetRow(i);
            if (row == null) break;

            StringBuilder sb = new StringBuilder();
            sb.Append("{");

            for (int j = 0; j <= row.LastCellNum && j < propertyInfos.Count; j++)
            {
                var cell = row.GetCell(j);
                if (cell == null) continue;

                string value;
                if (cell.CellType == CellType.Formula)
                {
                    value = cell.CachedFormulaResultType == CellType.Numeric
                        ? cell.NumericCellValue.ToString()
                        : cell.StringCellValue.ToString().Replace(@"\", @"\\");
                }
                else
                {
                    value = cell.ToString().Replace(@"\", @"\\");
                }

                //if (string.IsNullOrEmpty(value)) break;

                if (string.IsNullOrEmpty(value))
                {
                    Debug.LogWarning($"空值出现在第 {i + 1} 行，第 {j + 1} 列（字段名：{propertyInfos[j].Name}）");
                    break;
                }

                if (propertyInfos[j].Type.Contains("[]"))
                    value = "[" + value + "]";
                else
                    value = "\"" + value + "\"";

                sb.Append($"\"{propertyInfos[j].Name}\":{value}");
                if (j < row.LastCellNum - 1) sb.Append(",");
            }

            sb.Append("}");

            var type = Type.GetType($"{configName}Config, Assembly-CSharp");
            if (type == null)
            {
                Debug.LogError($"找不到类型: {configName}Config，可能没有编译完成");
                continue;
            }

            var config = JsonConvert.DeserializeObject(sb.ToString(), type) as BaseConfig;
            //if (config == null || config.id == "0") continue;

            if (config == null || string.IsNullOrEmpty(config.id)) continue;
            rawDataDic.Add(config.id, config);
        }

        var json = JsonConvert.SerializeObject(rawDataDic, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            Formatting = Formatting.Indented,
        });

        File.WriteAllText(filePath, json);
    }

    private static void DeleteAllOldFiles()
    {
        var csDir = $"{Application.dataPath}/Scripts/Configs";
        var jsonDir = $"{Application.dataPath}/Resources/Configs";

        if (Directory.Exists(csDir)) Directory.Delete(csDir, true);
        Directory.CreateDirectory(csDir);

        if (Directory.Exists(jsonDir)) Directory.Delete(jsonDir, true);
        Directory.CreateDirectory(jsonDir);

        AssetDatabase.Refresh();
    }
}