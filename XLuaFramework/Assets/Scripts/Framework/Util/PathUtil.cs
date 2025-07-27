using UnityEngine;

public class PathUtil
{
    /// <summary>
    /// ��·����Assets��·����
    /// </summary>
    public static readonly string AssetsPath = Application.dataPath;

    /// <summary>
    /// ��Ҫ��bundle��·��
    /// </summary>
    public static readonly string BuildResourcesPath = AssetsPath + "/BuildResources";

    /// <summary>
    /// bundle������·��
    /// </summary>
    public static readonly string BundledOutPath = Application.streamingAssetsPath;

    /// <summary>
    /// ֻ��Ŀ¼
    /// </summary>
    public static readonly string ReadPath = Application.streamingAssetsPath;

    /// <summary>
    /// �ɶ�дĿ¼
    /// </summary>
    public static readonly string ReadWritePath = Application.persistentDataPath;

    /// <summary>
    /// lua�ű�·��
    /// </summary>
    public static readonly string LuaPath = "Assets/BuildResources/LuaScripts";

    /// <summary>
    /// bundle��Դ�ļ�·��
    /// </summary>
    public static string BundleResourcePath
    {
        get 
        {
            if (AppConst.gameMode == GameMode.UpdateMode)
                return ReadWritePath;//����Ǹ���ģʽ�����ȡ·��Ϊ������·��
            return ReadPath;//����Ϊ����·��
        }
    }

    /// <summary>
    /// ��ȡUnity���·��
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
    /// ��ȡ��׼·��
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetStandardPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
        return path.Trim().Replace("\\", "/");//��·���еķ�б���滻Ϊ��б��
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
