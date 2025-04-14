using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public static class IOHelper
{
    /// <summary>
    /// 创建文件夹，如果文件夹已存在则不做任何操作
    /// </summary>
    /// <param name="directoryPath">文件夹完整路径</param>
    public static void CreateDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    /// <summary>
    /// 快速创建空文件，如果文件已存在则不做任何操作
    /// </summary>
    /// <param name="filePath">文件完整路径</param>
    public static void CreateFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            using var fs = File.Create(filePath);
        }
    }

    /// <summary>
    /// 使用流式写入创建UTF-8文本文件（同步，4KB缓冲区）
    /// </summary>
    /// <param name="filePath">文件完整路径</param>
    /// <param name="content">文件内容</param>
    public static void CreateTextFileStream(string filePath, string content)
    {
        const int bufferSize = 4096; // 4KB缓冲区

        // 确保目标文件夹存在
        string directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        using var fs = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize);
        using var writer = new StreamWriter(fs, Encoding.UTF8);
        writer.Write(content);
        writer.Flush();
    }

    /// <summary>
    /// 使用流式写入创建UTF-8 文本文件（异步，128KB缓冲区，适用于大文件）
    /// </summary>
    /// <param name="filePath">文件完整路径</param>
    /// <param name="content">文件内容</param>
    public static async Task CreateTextFileStreamAsync(string filePath, string content)
    {
        const int bufferSize = 131072; // 128KB缓冲区

        // 确保目标文件夹存在
        string directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        await using var fs = new FileStream(filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize,
            FileOptions.Asynchronous);
        await using var writer = new StreamWriter(fs, Encoding.UTF8);
        await writer.WriteAsync(content);
        await writer.FlushAsync();
    }

    /// <summary>
    /// 使用流式读取UTF-8 文本文件（同步，4KB缓冲区）
    /// </summary>
    /// <param name="filePath">文件完整路径</param>
    /// <returns>文件内容</returns>
    public static string ReadTextFileStream(string filePath)
    {
        const int bufferSize = 4096; // 4KB缓冲区

        using var fs = new FileStream(filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize);
        using var reader = new StreamReader(fs, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// 使用流式读取UTF-8 文本文件（异步，128KB缓冲区，适用于大文件）
    /// </summary>
    /// <param name="filePath">文件完整路径</param>
    /// <returns>文件内容</returns>
    public static async Task<string> ReadTextFileStreamAsync(string filePath)
    {
        const int bufferSize = 131072; // 128KB缓冲区

        await using var fs = new FileStream(filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize,
            FileOptions.Asynchronous);
        using var reader = new StreamReader(fs, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    /// 从文件加载数据
    /// </summary>
    /// <param name="fileName">文件完整路径</param>
    /// <returns>加载的数据</returns>
    public static T LoadData<T>(string fileName)
    {
        string data = ReadTextFileStream(fileName);
        return JsonConvert.DeserializeObject<T>(data);
    }

    /// <summary>
    /// 保存数据到文件
    /// </summary>
    /// <param name="fileName">文件完整路径</param>
    /// <param name="data">要保存的数据</param>
    public static void SaveData<T>(string fileName, T data)
    {
        string json = JsonConvert.SerializeObject(data);
        CreateTextFileStream(fileName, json);
    }

    /// <summary>
    /// 从文件异步加载数据
    /// </summary>
    /// <param name="fileName">文件完整路径</param>
    /// <returns>加载的数据</returns>
    public static async Task<T> LoadDataAsync<T>(string fileName)
    {
        string data = await ReadTextFileStreamAsync(fileName);
        return JsonConvert.DeserializeObject<T>(data);
    }

    /// <summary>
    /// 异步保存数据到文件
    /// </summary>
    /// <param name="fileName">文件完整路径</param>
    /// <param name="data">要保存的数据</param>
    public static async Task SaveDataAsync<T>(string fileName, T data)
    {
        string json = JsonConvert.SerializeObject(data);
        await CreateTextFileStreamAsync(fileName, json);
    }
}
