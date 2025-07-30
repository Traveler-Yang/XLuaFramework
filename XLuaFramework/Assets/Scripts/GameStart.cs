using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public GameMode gameMode;
    public bool OpenLog;
    void Start()
    {
        Manager.Event.Subscribe((int)GameEvent.StartLua, StartLua);
        Manager.Event.Subscribe((int)GameEvent.GameInit, GameInit);

        AppConst.gameMode = this.gameMode;
        AppConst.OpenLog = this.OpenLog;
        DontDestroyOnLoad(this);

        if (AppConst.gameMode == GameMode.UpdateMode)
            this.gameObject.AddComponent<HotUpdate>();
        else
            Manager.Event.Fire((int)GameEvent.GameInit);
    }

    private void GameInit(object args)
    {
        if (AppConst.gameMode != GameMode.EditorMode)
            Manager.Resource.ParseVersionFile();
        Manager.Lua.Init();
    }

    void StartLua(object args)
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
        Manager.Event.UnSubscribe((int)GameEvent.StartLua, StartLua);
        Manager.Event.UnSubscribe((int)GameEvent.GameInit, GameInit);
    }
}
