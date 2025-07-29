using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    Transform m_PoolParent;
    
    /// <summary>
    /// ������ֵ�
    /// </summary>
    Dictionary<string, PoolBase> m_Pools = new Dictionary<string, PoolBase>();

    private void Awake()
    {
        m_PoolParent = this.transform.parent.Find("Pool");
    }

    /// <summary>
    /// ���������
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="poolName"></param>
    /// <param name="releaseTime"></param>
    private void CreatPool<T>(string poolName, float releaseTime) where T : PoolBase
    {
        if (!m_Pools.TryGetValue(poolName, out PoolBase pool))
        {
            GameObject go = new GameObject(poolName);
            go.transform.SetParent(m_PoolParent);
            pool = go.AddComponent<T>();
            pool.Init(releaseTime);
            m_Pools.Add(poolName, pool);
        }
    }

    /// <summary>
    /// ������������
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="releaseTime"></param>
    public void CreatGameObjectPool(string poolName, float releaseTime)
    {
        CreatPool<GameObjectPool>(poolName, releaseTime);
    }

    /// <summary>
    /// ������Դ�����
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="releaseTime"></param>
    public void CreatAssetPool(string poolName, float releaseTime)
    {
        CreatPool<AssetPool>(poolName, releaseTime);
    }

    /// <summary>
    /// ȡ������
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public Object Spawn(string poolName, string assetName)
    {
        if (m_Pools.TryGetValue(poolName, out PoolBase pool))
        {
            return pool.Spwan(assetName);
        }
        return null;
    }

    /// <summary>
    /// ���ն���
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="assetName"></param>
    /// <param name="asset"></param>
    public void UnSpawn(string poolName, string assetName, Object asset)
    {
        if (m_Pools.TryGetValue(poolName, out PoolBase pool))
        {
            pool.UnSpwan(assetName, asset);
        }
    }
}
