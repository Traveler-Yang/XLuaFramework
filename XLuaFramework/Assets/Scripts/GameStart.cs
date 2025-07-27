using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public GameMode gameMode;

    void Start()
    {
        AppConst.gameMode = this.gameMode;
        DontDestroyOnLoad(this);

        Manager.Resource.ParseVersionFile();
        Manager.Lua.Init(() =>
        {
            Manager.Lua.StartLua("Main");
            XLua.LuaFunction func = Manager.Lua.LuaEnv.Global.Get<XLua.LuaFunction>("Main");
            func.Call();
        });

    }
}
