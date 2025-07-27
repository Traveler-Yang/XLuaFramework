using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileUtil
{
    /// <summary>
    /// 检测文件是否存在
    /// </summary>
    /// <param name="path">目标路径</param>
    /// <returns></returns>
    public static bool IsExists(string path)
    {
        FileInfo file = new FileInfo(path);
        return file.Exists;
    }

    /// <summary>
    /// 写入文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="data"></param>
    public static void WriteFile(string path, byte[] data)
    {
        //规范化路径
        path = PathUtil.GetStandardPath(path);
        //最后一个斜杠的前面的这一部分就是文件夹
        string dir = path.Substring(0, path.LastIndexOf("/"));
        //判断此文件夹是否存在
        if (!Directory.Exists(dir))
        {
            //不存在，则创建一个
            Directory.CreateDirectory(dir);
        }
        //判断文件存不存在
        FileInfo file = new FileInfo(path);
        if (file.Exists)
        {
            //存在，则删除
            file.Delete();
        }
        try
        {
            using (FileStream fs  = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
                fs.Close();
            }
        }
        catch(IOException e)
        {
            Debug.LogError(e.Message);
        }
    }
}
