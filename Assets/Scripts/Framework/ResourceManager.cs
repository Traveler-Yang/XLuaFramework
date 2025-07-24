using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    private void ParseVersionFile()
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

        action?.Invoke(bundleRequest?.asset);
    }

    public void LoadAsset(string assetName, Action<UObject> action)
    {
        StartCoroutine(LoadBundleAsync(assetName, action));
    }
    void Start()
    {
        this.ParseVersionFile();
        LoadAsset("Assets/BuildResources/UI/Prefabs/TestUI.prefab", OnComplete);
    }

    private void OnComplete(UObject obj)
    {
        GameObject go = Instantiate(obj) as GameObject;
        go.transform.SetParent(this.transform);//设置父节点
        go.SetActive(true);//启用
        go.transform.localPosition = Vector3.zero;//位置归零
    }
}
