using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolBase : MonoBehaviour
{
    /// <summary>
    /// 释放资源的时间/秒
    /// </summary>
    protected float m_ReleaseTime;

    /// <summary>
    /// 上次释放资源的时间/毫微秒   1（秒） = 1000000000（毫微秒）
    /// </summary>
    protected float m_LastReleaseTime = 0;

    /// <summary>
    /// 对象池
    /// </summary>
    protected List<PoolObject> m_Objects;

    private void Start()
    {
        m_LastReleaseTime = System.DateTime.Now.Ticks;
    }

    /// <summary>
    /// 初始化对象池
    /// </summary>
    /// <param name="time"></param>
    public void Init(float time)
    {
        m_ReleaseTime = time;
        m_Objects = new List<PoolObject>();
    }

    /// <summary>
    /// 取出对象
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual Object Spwan(string name)
    {
        foreach (PoolObject po in m_Objects)
        {
            if (po.Name == name)
            {
                m_Objects.Remove(po);
                return po.Object;
            }
        }
        return null;
    }

    /// <summary>
    /// 回收对象
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    public virtual void UnSpwan(string name, Object obj)
    {
        PoolObject po = new PoolObject(name, obj);
        m_Objects.Add(po);
    }

    public virtual void Release()
    {

    }

    private void Update()
    {
        if (System.DateTime.Now.Ticks - m_LastReleaseTime <= m_ReleaseTime * 1000000000)
        {
            m_LastReleaseTime = System.DateTime.Now.Ticks;
            Release();
        }
    }
}
