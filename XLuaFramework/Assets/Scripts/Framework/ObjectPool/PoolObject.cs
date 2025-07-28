using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject
{
    /// <summary>
    /// 具体对象
    /// </summary>
    public Object Object;

    /// <summary>
    /// 对象名字
    /// </summary>
    public string Name;

    /// <summary>
    /// 最后一次使用的时间
    /// </summary>
    public System.DateTime LastUserTime;

    public PoolObject(string name, Object obj)
    {
        Name = name;
        Object = obj;
        LastUserTime = System.DateTime.Now;
    }
}
