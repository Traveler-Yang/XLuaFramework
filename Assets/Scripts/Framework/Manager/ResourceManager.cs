using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;

public class ResourceManager : MonoBehaviour
{
    class BundleInfo
    {
        public string AssetsName;
        public string BundleName;
        public List<string> Dependences;
    }

    /// <summary>
    /// Bundle文件信息集合
    /// </summary>
    private Dictionary<string, BundleInfo> m_BundleInfos = new Dictionary<string, BundleInfo>();

    /// <summary>
    /// 解析版本文件
    /// </summary>
    public void ParseVersionFile()
    {
        //拿到版本文件路径
        string url = Path.Combine(PathUtil.BundleResourcePath, AppConst.FileListName);
        //读取文件
        string[] data = File.ReadAllLines(url);

        //解析文件
        for (int i = 0; i < data.Length; i++)
        {
            BundleInfo bundleInfo = new BundleInfo();
            string[] info = data[i].Split("|");//根据“|”字符进行分割并返回出来
            bundleInfo.AssetsName = info[0];
            bundleInfo.BundleName = info[1];
            //List特性：本质是数组，可动态扩容
            bundleInfo.Dependences = new List<string>(info.Length - 2);
            for (int j = 2; j < info.Length; j++)
            {
                bundleInfo.Dependences.Add(info[j]);
            }
            //资源名做Key，bundle文件和依赖文件作为信息
            m_BundleInfos.Add(bundleInfo.AssetsName, bundleInfo);
        }
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="assetName">资源文件名</param>
    /// <param name="action">完成回调</param>
    /// <returns></returns>
    IEnumerator LoadBundleAsync(string assetName, Action<UObject> action = null)
    {
        string bundleName = m_BundleInfos[assetName].BundleName;//获取bundle名
        string bundlePath = Path.Combine(PathUtil.BundleResourcePath, bundleName);//获取bundle路径
        List<string> dependences = m_BundleInfos[assetName].Dependences;//获取依赖资源列表
        //检查依赖
        if (dependences != null && dependences.Count > 0)
        {
            for (int i = 0; i < dependences.Count; i++)
            {
                yield return LoadBundleAsync(dependences[i]);
            }
        }
        //加载bundle
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return request;

        AssetBundleRequest bundleRequest = request.assetBundle.LoadAssetAsync(assetName);
        yield return bundleRequest;
        Debug.Log("this is LoadBundleAsync");
        action?.Invoke(bundleRequest?.asset);
    }

    /// <summary>
    /// 编辑器环境下加载资源
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    void EditorLoadAsset(string assetName, Action<UObject> action = null)
    {
        Debug.Log("this is EditorLoadAsset");
        UObject obj = AssetDatabase.LoadAssetAtPath(assetName, typeof(UObject));
        if (obj == null)
            Debug.LogError("assets name not exist:" + assetName);
        action?.Invoke(obj);
    }

    private void LoadAsset(string assetName, Action<UObject> action)
    {
        if (AppConst.gameMode == GameMode.EditorMode)
            EditorLoadAsset(assetName, action);
        else
            StartCoroutine(LoadBundleAsync(assetName, action));
    }

    /// <summary>
    /// 加载UI
    /// </summary>
    /// <param name="assetName">资源名</param>
    /// <param name="action"></param>
    public void LoadUI(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetUIPath(assetName), action);
    }

    /// <summary>
    /// 加载音乐
    /// </summary>
    /// <param name="assetName">资源名</param>
    /// <param name="action"></param>
    public void LoadMusic(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetMusicPath(assetName), action);
    }

    /// <summary>
    /// 加载音效
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    public void LoadSound(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetSoundPath(assetName), action);
    }

    /// <summary>
    /// 加载特效
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    public void LoadEffect(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetEffectPath(assetName), action);
    }

    /// <summary>
    /// 加载场景
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    public void LoadScene(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetScenePath(assetName), action);
    }

    //Tag:卸载暂时不做
}
