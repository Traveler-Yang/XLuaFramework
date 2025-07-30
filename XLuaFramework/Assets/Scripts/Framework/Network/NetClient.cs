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
    private const int BufferSize = 1024 * 64;//�������ݵĴ�С
    private byte[] m_Buffer = new byte[BufferSize];//���ݻ���
    private MemoryStream m_MemStream;
    private BinaryReader m_BinaryReader;

    public NetClient()
    {
        m_MemStream = new MemoryStream();
        m_BinaryReader = new BinaryReader(m_MemStream);
    }

    /// <summary>
    /// ���ӷ�����
    /// </summary>
    /// <param name="host">IP��ַ</param>
    /// <param name="port">�˿ں�</param>
    public void OnConnectServer(string host, int port)
    {
        try
        {
            //��ȡ����IP��ַ
            IPAddress[] addresses = Dns.GetHostAddresses(host);
            if (addresses.Length == 0)
            {
                Debug.LogError("host invalid");
                return;
            }
            //�ж����绷��
            if (addresses[0].AddressFamily == AddressFamily.InterNetworkV6)
                //������绷����IPV6���򴴽�IPV6��TCPClient
                m_Client = new TcpClient(AddressFamily.InterNetworkV6);
            else
                //���򴴽�IPV4��TCPClient
                m_Client = new TcpClient(AddressFamily.InterNetwork);
            //���ò���
            m_Client.SendTimeout = 1000;
            m_Client.ReceiveTimeout = 1000;
            m_Client.NoDelay = true;
            //��ʼ����
            m_Client.BeginConnect(host, port, OnConnect, null);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    /// <summary>
    /// ���������ӳɹ��ص�
    /// </summary>
    /// <param name="asyncResult"></param>
    private void OnConnect(IAsyncResult asyncResult)
    {
        //�ж������Ƿ����ӳɹ�
        if (m_Client == null || !m_Client.Connected)
        {
            Debug.LogError("connect server error!!!");
            return;
        }
        Manager.Net.OnNetConnected();
        m_TcpStream = m_Client.GetStream();
        //�õ���������ʼ��ȡ
        m_TcpStream.BeginRead(m_Buffer, 0, BufferSize, OnRead, null);
    }

    /// <summary>
    /// ��ȡ���ݻص�
    /// </summary>
    /// <param name="asyncResult"></param>
    private void OnRead(IAsyncResult asyncResult)
    {
        try
        {
            if (m_Client == null || m_TcpStream == null)
                return;

            //�յ�����Ϣ����
            int length = m_TcpStream.EndRead(asyncResult);

            //�ж���Ϣ���ݴ�С
            if (length < 1)
            {
                //����Ǹ�����Ϣ����Ͽ�����
                OnDisConnected();
                return;
            }
            //��������
            ReceiveData(length);
            lock (m_TcpStream)
            {
                //�������ݺ󣬻�Ҫ������һ�ε����ݣ�������Ҫ���Buffer���ٶ�ȡ
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
    /// ��������
    /// </summary>
    private void ReceiveData(int length)
    {
        //��ָ��ָ��ĩβ����Ϊ������ܻ�����������
        m_MemStream.Seek(0, SeekOrigin.End);
        //ָ��ĩβ����׷���յ�������
        m_MemStream.Write(m_Buffer, 0, length);
        //�����ָ��ͷ
        m_MemStream.Seek(0, SeekOrigin.Begin);

        while (RemainingBytesLength() > 8)
        {
            int msgId = m_BinaryReader.ReadInt32();
            int msgLen = m_BinaryReader.ReadInt32();
            if (RemainingBytesLength() >= msgLen)
            {
                //��ȡ��Ϣ���ȣ�ת��Ϊstring
                byte[] data = m_BinaryReader.ReadBytes(msgLen);
                string message = System.Text.Encoding.UTF8.GetString(data);

                //ת��lua
                Manager.Net.Receive(msgId, message);
            }
            else
            {
                m_MemStream.Position = m_MemStream.Position - 8;
                break;
            }
        }
        //ʣ���ֽ�
        byte[] leftover = m_BinaryReader.ReadBytes(RemainingBytesLength());
        m_MemStream.SetLength(0);
        m_MemStream.Write(leftover, 0, leftover.Length);
    }

    /// <summary>
    /// ������Ϣ
    /// </summary>
    /// <param name="msgID">��ϢID</param>
    /// <param name="message">��Ϣ��Ϣ</param>
    public void SendMessage(int msgID, string message)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            ms.Position = 0;
            BinaryWriter bw = new BinaryWriter(ms);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            //Э��id
            bw.Write(msgID);
            //��Ϣ����
            bw.Write(data.Length);
            //��Ϣ����
            bw.Write(data);
            bw.Flush();
            if (m_Client != null && m_Client.Connected)
            {
                byte[] sendData = ms.ToArray();
                m_TcpStream.BeginWrite(sendData, 0, sendData.Length, OnEndSend, null);
            }
            else
            {
                Debug.LogError("������δ����!");
            }
        }
    }

    /// <summary>
    /// ���ͳɹ��ص�
    /// </summary>
    /// <param name="ar"></param>
    private void OnEndSend(IAsyncResult ar)
    {
        try
        {
            //��������
            m_TcpStream.EndRead(ar);
        }
        catch (Exception e)
        {
            OnDisConnected();
            Debug.LogError(e.Message);
        }
    }

    /// <summary>
    /// ʣ���ֽ���
    /// </summary>
    /// <returns></returns>
    private int RemainingBytesLength()
    {
        //�ֽ����ĳ��ȼ�ȥ��ȡ�����õĵ�ǰλ��
        return (int)(m_MemStream.Length - m_MemStream.Position);
    }

    /// <summary>
    /// �Ͽ�����
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
