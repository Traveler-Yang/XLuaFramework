using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

public class LuaManager : MonoBehaviour
{
    /// <summary>
    /// 所有的lua文件名
    /// </summary>
    public List<string> LuaNames = new List<string>();

    /// <summary>
    /// 缓存lua脚本内容
    /// </summary>
    private Dictionary<string, byte[]> m_LuaScripts;

    /// <summary>
    /// lua虚拟机
    /// </summary>
    public LuaEnv LuaEnv;

    Action InitOK;

    public void Init(Action init)
    {
        InitOK = init;
        LuaEnv = new LuaEnv();
        LuaEnv.AddLoader(Loader);
        m_LuaScripts = new Dictionary<string, byte[]>();
#if UNITY_EDITOR
        if (AppConst.gameMode == GameMode.EditorMode)
            EditorLoadLuaScript();
        else
#endif
            LoadLuaScript();
    }

    public void StartLua(string name)
    {
        LuaEnv.DoString(string.Format("require'{0}'", name));
    }

    byte[] Loader(ref string name)
    {
        return GetLuaScript(name);
    }

    public byte[] GetLuaScript(string name)
    {
        name = name.Replace(".", "/");
        string fileName = PathUtil.GetLuaPath(name);

        byte[] luaScript = null;
        if (!m_LuaScripts.TryGetValue(fileName, out luaScript))
            Debug.LogErrorFormat("lua script is not exist : {0}", fileName);
        return luaScript;
    }

    void LoadLuaScript()
    {
        foreach (string name in LuaNames)
        {
            Manager.Resource.LoadLua(name, (UnityEngine.Object obj) =>
            {
                AddLuaScript(name, (obj as TextAsset).bytes);
                if (m_LuaScripts.Count >= LuaNames.Count)
                {
                    //所有lua加载完成时
                    InitOK?.Invoke();
                    LuaNames.Clear();
                    LuaNames = null;
                }
            });
        }
    }

    private void AddLuaScript(string assetsName, byte[] luaScript)
    {
        m_LuaScripts[assetsName] = luaScript;
    }

#if UNITY_EDITOR
    void EditorLoadLuaScript()
    {
        string[] luaFiles = Directory.GetFiles(PathUtil.LuaPath, "*.bytes", SearchOption.AllDirectories);
        for (int i = 0; i < luaFiles.Length; i++)
        {
            string fileName = PathUtil.GetStandardPath(luaFiles[i]);
            byte[]  file = File.ReadAllBytes(fileName);
            AddLuaScript(PathUtil.GetUnityPath(fileName), file);
        }
    }
#endif

    private void Update()
    {
        if (LuaEnv != null)
        {
            //释放内存
            LuaEnv.Tick();
        }
    }

    private void OnDestroy()
    {
        if (LuaEnv != null)
        {
            //脚本关闭时，销毁lua虚拟机
            LuaEnv.Dispose();
            LuaEnv = null;
        }
    }
}
