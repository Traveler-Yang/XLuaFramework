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
    /// Bundle�ļ���Ϣ����
    /// </summary>
    private Dictionary<string, BundleInfo> m_BundleInfos = new Dictionary<string, BundleInfo>();

    /// <summary>
    /// ���Bundle��Դ����
    /// </summary>
    private Dictionary<string, AssetBundle> m_AssetBundles = new Dictionary<string, AssetBundle>();

    /// <summary>
    /// �����汾�ļ�
    /// </summary>
    public void ParseVersionFile()
    {
        //�õ��汾�ļ�·��
        string url = Path.Combine(PathUtil.BundleResourcePath, AppConst.FileListName);
        //��ȡ�ļ�
        string[] data = File.ReadAllLines(url);

        //�����ļ�
        for (int i = 0; i < data.Length; i++)
        {
            BundleInfo bundleInfo = new BundleInfo();
            string[] info = data[i].Split("|");//���ݡ�|���ַ����зָ���س���
            bundleInfo.AssetsName = info[0];
            bundleInfo.BundleName = info[1];
            //List���ԣ����������飬�ɶ�̬����
            bundleInfo.Dependences = new List<string>(info.Length - 2);
            for (int j = 2; j < info.Length; j++)
            {
                bundleInfo.Dependences.Add(info[j]);
            }
            //��Դ����Key��bundle�ļ��������ļ���Ϊ��Ϣ
            m_BundleInfos.Add(bundleInfo.AssetsName, bundleInfo);

            //�����Դ�ļ���·���д���LuaScripts������˵������lua�ļ�������ӵ�luaManager�е�luaNames��
            if (info[0].IndexOf("LuaScripts") > 0)
                Manager.Lua.LuaNames.Add(info[0]);
        }
    }

    /// <summary>
    /// �첽������Դ
    /// </summary>
    /// <param name="assetName">��Դ�ļ���</param>
    /// <param name="action">��ɻص�</param>
    /// <returns></returns>
    IEnumerator LoadBundleAsync(string assetName, Action<UObject> action = null)
    {
        string bundleName = m_BundleInfos[assetName].BundleName;//��ȡbundle�� 
        string bundlePath = Path.Combine(PathUtil.BundleResourcePath, bundleName);//��ȡbundle·��
        List<string> dependences = m_BundleInfos[assetName].Dependences;//��ȡ������Դ�б�

        AssetBundle bundle = GetBundle(bundleName);
        if (bundle == null)
        {
            //�������
            if (dependences != null && dependences.Count > 0)
            {
                for (int i = 0; i < dependences.Count; i++)
                {
                    yield return LoadBundleAsync(dependences[i]);
                }
            }
            //����bundle
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
            yield return request;
            bundle = request.assetBundle;
            m_AssetBundles.Add(bundleName, bundle);
        }
        if (assetName.EndsWith(".unity"))
        {
            action?.Invoke(null);
            yield break;
        }
        AssetBundleRequest bundleRequest = bundle.LoadAssetAsync(assetName);
        yield return bundleRequest;
        Debug.Log("this is LoadBundleAsync");
        action?.Invoke(bundleRequest?.asset);
    }

    AssetBundle GetBundle(string name)
    {
        AssetBundle bundle = null;
        if (m_AssetBundles.TryGetValue(name, out bundle))
            return bundle;
        return null;
    }

#if UNITY_EDITOR
    /// <summary>
    /// �༭�������¼�����Դ
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
#endif

    private void LoadAsset(string assetName, Action<UObject> action)
    {
#if UNITY_EDITOR
        if (AppConst.gameMode == GameMode.EditorMode)
            EditorLoadAsset(assetName, action);
        else
#endif
            StartCoroutine(LoadBundleAsync(assetName, action));
    }

    /// <summary>
    /// ����UI
    /// </summary>
    /// <param name="assetName">��Դ��</param>
    /// <param name="action"></param>
    public void LoadUI(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetUIPath(assetName), action);
    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="assetName">��Դ��</param>
    /// <param name="action"></param>
    public void LoadMusic(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetMusicPath(assetName), action);
    }

    /// <summary>
    /// ������Ч
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    public void LoadSound(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetSoundPath(assetName), action);
    }

    /// <summary>
    /// ������Ч
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    public void LoadEffect(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetEffectPath(assetName), action);
    }

    /// <summary>
    /// ���س���
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    public void LoadScene(string assetName, Action<UObject> action = null)
    {
        LoadAsset(PathUtil.GetScenePath(assetName), action);
    }

    public void LoadLua(string assetName, Action<UObject> action = null)
    {
        //��������Ϊ����LuaNames�е�·���Ǵ�fileList��ȡ�����ģ��Ѿ���������·����
        //��ʹ��get�Ļ���·�����ظ�
        LoadAsset(assetName, action);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="action"></param>
    public void LoadPrefab(string assetName, Action<UObject> action = null)
    {
        LoadAsset(assetName, action);
    }

    //Tag:ж����ʱ����
}
