using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public GameMode gameMode;
    public bool OpenLog;
    void Start()
    {
        Manager.Event.Subscribe(10000, OnLuaInit);

        AppConst.gameMode = this.gameMode;
        AppConst.OpenLog = this.OpenLog;
        DontDestroyOnLoad(this);

        Manager.Resource.ParseVersionFile();
        Manager.Lua.Init();

    }

    void OnLuaInit(object args)
    {
        Manager.Lua.StartLua("Main");
        XLua.LuaFunction func = Manager.Lua.LuaEnv.Global.Get<XLua.LuaFunction>("Main");
        func.Call();

        Manager.Pool.CreatGameObjectPool("UI", 10);
        Manager.Pool.CreatGameObjectPool("Monster", 120);
        Manager.Pool.CreatGameObjectPool("Effect", 120);
        Manager.Pool.CreatAssetPool("AssetBundle", 10);
    }

    private void OnApplicationQuit()
    {
        Manager.Event.UnSubscribe(10000, OnLuaInit);
    }
}
