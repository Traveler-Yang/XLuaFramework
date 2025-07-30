using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System;

public class NetClient
{
    private TcpClient m_Client;
    private NetworkStream m_TcpStream;
    private const int BufferSize = 1024 * 64;//接收数据的大小
    private byte[] m_Buffer = new byte[BufferSize];//数据缓存
    private MemoryStream m_MemStream;
    private BinaryReader m_BinaryReader;

    public NetClient()
    {
        m_MemStream = new MemoryStream();
        m_BinaryReader = new BinaryReader(m_MemStream);
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    /// <param name="host">IP地址</param>
    /// <param name="port">端口号</param>
    public void OnConnectServer(string host, int port)
    {
        try
        {
            //获取本机IP地址
            IPAddress[] addresses = Dns.GetHostAddresses(host);
            if (addresses.Length == 0)
            {
                Debug.LogError("host invalid");
                return;
            }
            //判断网络环境
            if (addresses[0].AddressFamily == AddressFamily.InterNetworkV6)
                //如果网络环境是IPV6，则创建IPV6的TCPClient
                m_Client = new TcpClient(AddressFamily.InterNetworkV6);
            else
                //否则创建IPV4的TCPClient
                m_Client = new TcpClient(AddressFamily.InterNetwork);
            //设置参数
            m_Client.SendTimeout = 1000;
            m_Client.ReceiveTimeout = 1000;
            m_Client.NoDelay = true;
            //开始连接
            m_Client.BeginConnect(host, port, OnConnect, null);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    /// <summary>
    /// 服务器连接成功回调
    /// </summary>
    /// <param name="asyncResult"></param>
    private void OnConnect(IAsyncResult asyncResult)
    {
        //判断网络是否连接成功
        if (m_Client == null || !m_Client.Connected)
        {
            Debug.LogError("connect server error!!!");
            return;
        }
        Manager.Net.OnNetConnected();
        m_TcpStream = m_Client.GetStream();
        //得到数据流后开始读取
        m_TcpStream.BeginRead(m_Buffer, 0, BufferSize, OnRead, null);
    }

    /// <summary>
    /// 读取数据回调
    /// </summary>
    /// <param name="asyncResult"></param>
    private void OnRead(IAsyncResult asyncResult)
    {
        try
        {
            if (m_Client == null || m_TcpStream == null)
                return;

            //收到的消息长度
            int length = m_TcpStream.EndRead(asyncResult);

            //判断消息数据大小
            if (length < 1)
            {
                //如果是个空消息，则断开连接
                OnDisConnected();
                return;
            }
            //解析数据
            ReceiveData(length);
            lock (m_TcpStream)
            {
                //解析数据后，还要接收下一次的数据，所以需要清空Buffer后再读取
                Array.Clear(m_Buffer, 0, m_Buffer.Length);
                m_TcpStream.BeginRead(m_Buffer, 0, BufferSize, OnRead, null);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            OnDisConnected();
        }
    }

    /// <summary>
    /// 解析数据
    /// </summary>
    private void ReceiveData(int length)
    {
        //将指针指向末尾，因为里面可能还有其他数据
        m_MemStream.Seek(0, SeekOrigin.End);
        //指向末尾后，再追加收到的数据
        m_MemStream.Write(m_Buffer, 0, length);
        //最后再指向开头
        m_MemStream.Seek(0, SeekOrigin.Begin);

        while (RemainingBytesLength() > 8)
        {
            int msgId = m_BinaryReader.ReadInt32();
            int msgLen = m_BinaryReader.ReadInt32();
            if (RemainingBytesLength() >= msgLen)
            {
                //读取消息长度，转换为string
                byte[] data = m_BinaryReader.ReadBytes(msgLen);
                string message = System.Text.Encoding.UTF8.GetString(data);

                //转到lua
                Manager.Net.Receive(msgId, message);
            }
            else
            {
                m_MemStream.Position = m_MemStream.Position - 8;
                break;
            }
        }
        //剩余字节
        byte[] leftover = m_BinaryReader.ReadBytes(RemainingBytesLength());
        m_MemStream.SetLength(0);
        m_MemStream.Write(leftover, 0, leftover.Length);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="msgID">消息ID</param>
    /// <param name="message">消息信息</param>
    public void SendMessage(int msgID, string message)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            ms.Position = 0;
            BinaryWriter bw = new BinaryWriter(ms);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            //协议id
            bw.Write(msgID);
            //消息长度
            bw.Write(data.Length);
            //消息内容
            bw.Write(data);
            bw.Flush();
            if (m_Client != null && m_Client.Connected)
            {
                byte[] sendData = ms.ToArray();
                m_TcpStream.BeginWrite(sendData, 0, sendData.Length, OnEndSend, null);
            }
            else
            {
                Debug.LogError("服务器未连接!");
            }
        }
    }

    /// <summary>
    /// 发送成功回调
    /// </summary>
    /// <param name="ar"></param>
    private void OnEndSend(IAsyncResult ar)
    {
        try
        {
            //结束发送
            m_TcpStream.EndRead(ar);
        }
        catch (Exception e)
        {
            OnDisConnected();
            Debug.LogError(e.Message);
        }
    }

    /// <summary>
    /// 剩余字节数
    /// </summary>
    /// <returns></returns>
    private int RemainingBytesLength()
    {
        //字节流的长度减去获取或设置的当前位置
        return (int)(m_MemStream.Length - m_MemStream.Position);
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    private void OnDisConnected()
    {
        if (m_Client != null && m_Client.Connected)
        {
            m_Client.Close();
            m_Client = null;

            m_TcpStream.Close();
            m_TcpStream = null;
        }
        Manager.Net.OnDisConnected();
    }
}
