using UnityEngine;

public class PathUtil
{
    /// <summary>
    /// 根路径（Assets的路径）
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
    /// 只读目录
    /// </summary>
    public static readonly string ReadPath = Application.streamingAssetsPath;

    /// <summary>
    /// 可读写目录
    /// </summary>
    public static readonly string ReadWritePath = Application.persistentDataPath;

    /// <summary>
    /// lua脚本路径
    /// </summary>
    public static readonly string LuaPath = "Assets/BuildResources/LuaScripts";

    /// <summary>
    /// bundle资源文件路径
    /// </summary>
    public static string BundleResourcePath
    {
        get 
        {
            if (AppConst.gameMode == GameMode.UpdateMode)
                return ReadWritePath;//如果是更新模式，则读取路径为持续化路径
            return ReadPath;//否则为流动路径
        }
    }

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

    public static string GetLuaPath(string name)
    {
        return string.Format("Assets/BuildResources/LuaScripts/{0}.bytes", name);
    }

    public static string GetUIPath(string name)
    {
        return string.Format("Assets/BuildResources/UI/Prefabs/{0}.prefab", name);
    }

    public static string GetMusicPath(string name)
    {
        return string.Format("Assets/BuildResources/Audio/Music/{0}", name);
    }
    public static string GetSoundPath(string name)
    {
        return string.Format("Assets/BuildResources/Audio/Sound/{0}", name);
    }
    public static string GetEffectPath(string name)
    {
        return string.Format("Assets/BuildResources/Effects/Prefabs/{0}.prefab", name);
    }
    public static string GetSpritePath(string name)
    {
        return string.Format("Assets/BuildResources/Sprites/{0}", name);
    }
    public static string GetScenePath(string name)
    {
        return string.Format("Assets/BuildResources/Scenes/{0}.unity", name);
    }
}
