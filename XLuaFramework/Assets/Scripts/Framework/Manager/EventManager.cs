using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void EventHandler(object args);

    Dictionary<int, EventHandler> m_Events = new Dictionary<int, EventHandler>();

    /// <summary>
    /// �����¼�
    /// </summary>
    /// <param name="id">�¼�ID</param>
    /// <param name="e">EventHandler</param>
    public void Subscribe(int id, EventHandler e)
    {
        //�жϴ��¼��Ƿ��Ѿ�����
        //����Ѿ������ˣ��򽫴��¼�+=
        if (m_Events.ContainsKey(id))
            m_Events[id] += e;
        else//���û�У�����ӵ��ֵ�
            m_Events.Add(id, e);
    }

    /// <summary>
    /// ȡ�������¼�
    /// </summary>
    /// <param name="id">�¼�ID</param>
    /// <param name="e">EventHandler</param>
    public void UnSubscribe(int id, EventHandler e)
    {
        if (m_Events.ContainsKey(id))
        {
            if (m_Events[id] != null)
                m_Events[id] -= e;

            if (m_Events[id] == null)
                m_Events.Remove(id);
        }
    }

    /// <summary>
    /// ִ���¼�
    /// </summary>
    /// <param name="id">�¼�ID</param>
    /// <param name="args"></param>
    public void Fire(int id, object args = null)
    {
        EventHandler handler;
        if (m_Events.TryGetValue(id, out handler))
        {
            handler(args);
        }
    }
}
