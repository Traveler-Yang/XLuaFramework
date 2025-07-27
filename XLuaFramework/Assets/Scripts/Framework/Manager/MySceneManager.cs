using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    private string m_logicName = "[SceneLogic]";

    /// <summary>
    /// 检测场景是否已经加载
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    private bool IsLoadedScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }

    IEnumerator StartLoadScene(string sceneName, string luaName, LoadSceneMode mode)
    {
        if (IsLoadedScene(sceneName))
            yield break;

        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, mode);
        async.allowSceneActivation = true;
        yield return async;

        Scene scene = SceneManager.GetSceneByName(sceneName);
        GameObject go = new GameObject(m_logicName);

        SceneManager.MoveGameObjectToScene(go, scene);//将对象移动到当前场景

        SceneLogic logic = go.AddComponent<SceneLogic>();
        //logic.sceneName = sceneName;
        logic.Init(luaName);
        logic.OnEnter();
    }

}
