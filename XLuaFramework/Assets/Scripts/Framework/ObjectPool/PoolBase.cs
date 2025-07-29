using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolBase : MonoBehaviour
{
    /// <summary>
    /// �ͷ���Դ��ʱ��/��
    /// </summary>
    protected float m_ReleaseTime;

    /// <summary>
    /// �ϴ��ͷ���Դ��ʱ��/��΢��   1���룩 = 1000000000����΢�룩
    /// </summary>
    protected float m_LastReleaseTime = 0;

    /// <summary>
    /// �����
    /// </summary>
    protected List<PoolObject> m_Objects;

    private void Start()
    {
        m_LastReleaseTime = System.DateTime.Now.Ticks;
    }

    /// <summary>
    /// ��ʼ�������
    /// </summary>
    /// <param name="time"></param>
    public void Init(float time)
    {
        m_ReleaseTime = time;
        m_Objects = new List<PoolObject>();
    }

    /// <summary>
    /// ȡ������
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
    /// ���ն���
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
