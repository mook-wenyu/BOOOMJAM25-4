using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Text;
using Newtonsoft.Json;

public class EditorUtils
{
    [MenuItem("Tools/生成配置Json")]
    public static void GenerateConfigs()
    {
        //EditorUtility.DisplayDialog("提示", "开始生成配置！", "OK");
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
        EditorApplication.delayCall += () => Debug.Log("完成导出！");
    }

    private class PropertyInfo 
    {
        public string Name;
        public string Type;
    }

    private static void ReadExcel(string filePath) 
    {
        IWorkbook wk = null;
        string extension = Path.GetExtension(filePath);
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        FileStream fs = File.OpenRead(filePath);
        if (extension.Equals(".xls"))
        {
            //把xls文件中的数据写入wk中
            wk = new HSSFWorkbook(fs);
        }
        else
        {
            //把xlsx文件中的数据写入wk中
            wk = new XSSFWorkbook(fs);
        }

        fs.Close();

        //只读第一个Sheet，其他Sheet忽略
        ISheet sheet = wk.GetSheetAt(0);

        IRow row = sheet.GetRow(1); // 第一行是字段名称
        IRow rowType = sheet.GetRow(2); // 第二行是字段类型
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
            {
                // 不允许中间有空列的，遇到空列认为后边就没数据了
                break;
            }

            var cellType = rowType.GetCell(i);
            configProperties.Add(new PropertyInfo() { Name = name, Type = cellType.ToString() });
        }

        GenrateConfigClass(configProperties, fileName);
        GenerateConfigJson(configProperties, fileName, sheet);
    }

    private static void GenrateConfigClass(List<PropertyInfo> propertyInfos, string configName) 
    {
        var filePath = $"{Application.dataPath}/Scripts/Configs/{configName}Config.cs";
        var fileDir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(fileDir)) 
        {
            Directory.CreateDirectory(fileDir);
        }

        var sb = new StringBuilder();

        sb.AppendLine($"public class {configName}Config : BaseConfig");
        sb.AppendLine("{");

        for (int i = 0; i < propertyInfos.Count; i++)
        {
            sb.AppendLine($"    public {propertyInfos[i].Type} {propertyInfos[i].Name};");
        }

        sb.AppendLine("}");

        File.WriteAllText(filePath, sb.ToString());
        //AssetDatabase.Refresh();
    }

    private static void GenerateConfigJson(List<PropertyInfo> propertyInfos, string configName, ISheet sheet) 
    {
        var filePath = $"{Application.dataPath}/Resources/Configs/{configName}Config.txt";

        var fileDir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(fileDir))
        {
            Directory.CreateDirectory(fileDir);
        }

        propertyInfos.Insert(0, new PropertyInfo() { Name = "id", Type = "string" });
        Dictionary<int, BaseConfig> rawDataDic = new Dictionary<int, BaseConfig>();

        for (int i = 3; i <= sheet.LastRowNum; i++)
        {
            var row = sheet.GetRow(i);

            if (row == null) 
            {
                break;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            for (int j = 0; j <= row.LastCellNum; j++)
            {
                var cell = row.GetCell(j);
                if (cell == null) continue;
                string value;
                if (cell.CellType == CellType.Formula)
                {
                    if (cell.CachedFormulaResultType == CellType.Numeric)
                    {
                        value = cell.NumericCellValue.ToString();
                    }
                    else 
                    {
                        value = cell.StringCellValue.ToString();
                        value = value.Replace(@"\", @"\\");
                    }
                }
                else
                {
                    // 直接读取值
                    value = cell.ToString().Replace(@"\", @"\\");
                }
                if (string.IsNullOrEmpty(value))
                {
                    // 不允许中间有空列的，遇到空列认为后边就没数据了
                    break;
                }

                if (propertyInfos[j].Type.Contains("[]"))
                {
                    value = "[" + value + "]";
                }
                else 
                {
                    value = "\"" + value + "\"";
                }

                sb.Append($"\"{propertyInfos[j].Name}\":{value}");
                if (j < row.LastCellNum - 1) 
                {
                    sb.Append(",");
                }
            }
            sb.Append("}");

            var type = Type.GetType($"{configName}Config, Assembly-CSharp");

            var config = JsonConvert.DeserializeObject(sb.ToString(), type) as BaseConfig;

            // 为0说明这一行的数据有问题，直接跳过
            if (config.id == 0)
            {
                continue;
            }

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

        if (Directory.Exists(csDir)) 
        {
            Directory.Delete(csDir, true);
        }
        Directory.CreateDirectory(csDir);

        if (Directory.Exists(jsonDir)) 
        {
            Directory.Delete(jsonDir, true);
        }
        Directory.CreateDirectory(jsonDir);

        AssetDatabase.Refresh();
    }
}
