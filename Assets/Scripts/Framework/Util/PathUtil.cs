using UnityEngine;

public class PathUtil
{
    /// <summary>
    /// 根路径
    /// </summary>
    public static readonly string AssetsPath = Application.dataPath;

    /// <summary>
    /// 需要打bundle的路径
    /// </summary>
    public static readonly string BuildResourcesPath = AssetsPath + "/BuildResources";

    /// <summary>
    /// bundle打包输出路径
    /// </summary>
    public static readonly string BundledOutPath = Application.streamingAssetsPath;

    /// <summary>
    /// 获取Unity相对路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetUnityPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
        return path.Substring(path.IndexOf("Assets"));
    }

    /// <summary>
    /// 获取标准路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetStandardPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
        return path.Trim().Replace("\\", "/");//将路径中的反斜杠替换为正斜杠
    }
}
