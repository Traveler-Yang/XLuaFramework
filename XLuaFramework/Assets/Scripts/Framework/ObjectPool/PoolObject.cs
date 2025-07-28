using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject
{
    /// <summary>
    /// �������
    /// </summary>
    public Object Object;

    /// <summary>
    /// ��������
    /// </summary>
    public string Name;

    /// <summary>
    /// ���һ��ʹ�õ�ʱ��
    /// </summary>
    public System.DateTime LastUserTime;

    public PoolObject(string name, Object obj)
    {
        Name = name;
        Object = obj;
        LastUserTime = System.DateTime.Now;
    }
}
