using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void EventHandler(object args);

    Dictionary<int, EventHandler> m_Events = new Dictionary<int, EventHandler>();

    /// <summary>
    /// 订阅事件
    /// </summary>
    /// <param name="id">事件ID</param>
    /// <param name="e">EventHandler</param>
    public void Subscribe(int id, EventHandler e)
    {
        //判断此事件是否已经订阅
        //如果已经订阅了，则将此事件+=
        if (m_Events.ContainsKey(id))
            m_Events[id] += e;
        else//如果没有，则添加到字典
            m_Events.Add(id, e);
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    /// <param name="id">事件ID</param>
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
    /// 执行事件
    /// </summary>
    /// <param name="id">事件ID</param>
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
