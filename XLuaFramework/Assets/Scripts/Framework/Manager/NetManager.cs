using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetManager : MonoBehaviour
{
    NetClient m_NetClient;
    /// <summary>
    /// 接收消息的队列
    /// </summary>
    Queue<KeyValuePair<int, string>> m_MessageQueue = new Queue<KeyValuePair<int, string>>();
    XLua.LuaFunction ReceiveMessage;

    public void Init()
    {
        m_NetClient = new NetClient();
        ReceiveMessage = Manager.Lua.LuaEnv.Global.Get<XLua.LuaFunction>("ReceiveMessage");
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="message"></param>
    public void SendMessage(int messageId, string message)
    {
        m_NetClient.SendMessage(messageId, message);
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="post"></param>
    /// <param name="port"></param>
    public void ConnectedServer(string post, int port)
    {
        m_NetClient.OnConnectServer(post, port);
    }

    /// <summary>
    /// 网络连接成功
    /// </summary>
    public void OnNetConnected()
    {

    }

    /// <summary>
    /// 被服务器断开连接
    /// </summary>
    public void OnDisConnected()
    {

    }

    /// <summary>
    /// 接收数据
    /// </summary>
    /// <param name="msgId"></param>
    /// <param name="message"></param>
    public void Receive(int msgId, string message)
    {
        m_MessageQueue.Enqueue(new KeyValuePair<int, string>(msgId, message));
    }

    private void Update()
    {
        if (m_MessageQueue.Count > 0)
        {
            KeyValuePair<int, string> msg = m_MessageQueue.Dequeue();
            ReceiveMessage?.Call(msg.Key, msg.Value);
        }
    }
}
