using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool : PoolBase
{
    public override Object Spwan(string name)
    {
        Object obj = base.Spwan(name);
        if (obj == null)
            return null;

        GameObject go = obj as GameObject;
        go.SetActive(true);
        return obj;
    }

    public override void UnSpwan(string name, Object obj)
    {
        GameObject go = obj as GameObject;
        go.SetActive(false);
        go.transform.SetParent(this.transform, false);
        base.UnSpwan(name, obj);
    }

    public override void Release()
    {
        base.Release();
        foreach (PoolObject item in m_Objects)
        {
            if (System.DateTime.Now.Ticks - item.LastUserTime.Ticks >= m_ReleaseTime * 10000000)
            {
                Debug.LogFormat("GameObjectPool release time: {0}", System.DateTime.Now);
                Destroy(item.Object);
                Manager.Resource.MinusBundleCount(item.Name);
                m_Objects.Remove(item);
                Release();
                return;
            }
        }
    }
}
