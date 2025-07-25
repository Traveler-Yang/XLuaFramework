using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class HotUpdate : MonoBehaviour
{

    byte[] m_ReadPathFileListData;

    byte[] m_SeverFileListData;

    internal class DownFileInfo
    {
        public string url;//URL地址
        public string fileName;//文件名
        public DownloadHandler fileData;
    }
    /// <summary>
    /// 下载单个文件
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    IEnumerator DownloadFile(DownFileInfo info, Action<DownFileInfo> Complete)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(info.url);//请求下载
        yield return webRequest.SendWebRequest();//等待下载完成
        if (webRequest.result == UnityWebRequest.Result.ProtocolError ||
            webRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogErrorFormat("下载文件出错：{0}", info.url);
            yield break;
            //重试：扩展内容
        }
        info.fileData = webRequest.downloadHandler;
        Complete?.Invoke(info);
        webRequest.Dispose();//下载完成释放
    }

    /// <summary>
    /// 下载多个文件
    /// </summary>
    /// <param name="info"></param>
    /// <param name="Complete"></param>
    /// <returns></returns>
    IEnumerator DownloadFile(List<DownFileInfo> infos, Action<DownFileInfo> Complete, Action DownloadAllCompelet)
    {
        foreach (DownFileInfo info in infos)
        {
            yield return DownloadFile(info, Complete);
        }
        DownloadAllCompelet?.Invoke();
    }

    /// <summary>
    /// 获取文件信息
    /// </summary>
    /// <param name="fileData"></param>
    /// <returns></returns>
    private List<DownFileInfo> GetFileList(string fileData, string path)
    {
        string content = fileData.Trim().Replace("\r", "");
        string[] files = content.Split("\n");
        List<DownFileInfo> downFileInfos = new List<DownFileInfo>(files.Length);
        for (int i = 0; i < files.Length; i++)
        {
            string[] info = files[i].Split('|');
            DownFileInfo fileInfo = new DownFileInfo();
            fileInfo.fileName = info[1];
            fileInfo.url = Path.Combine(path, info[1]);
            downFileInfos.Add(fileInfo);
        }
        return downFileInfos;
    }

    private void Start()
    {
        if (IsFirstInstall())
        {
            ReleaseResources();
        }
        else
        {
            CheckUpdata();
        }
    }

    /// <summary>
    /// 检测是否是初次安装
    /// </summary>
    /// <returns></returns>
    private bool IsFirstInstall()
    {
        //检测只读目录是否存在版本文件
        bool isExistsReadPath = FileUtil.IsExists(Path.Combine(PathUtil.ReadPath, AppConst.FileListName));

        //检测读写目录是否存在版本文件
        bool isExistsReadWritePath = FileUtil.IsExists(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName));

        //只读目录存在，读写目录不存在，就认为是初次安装
        return isExistsReadPath && !isExistsReadWritePath;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    private void ReleaseResources()
    {
        string url = Path.Combine(PathUtil.ReadPath, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        StartCoroutine(DownloadFile(info, OnDownloadReadPathFileListComplete));
    }

    /// <summary>
    /// 释放资源回调
    /// </summary>
    /// <param name="file"></param>
    private void OnDownloadReadPathFileListComplete(DownFileInfo file)
    {
        m_ReadPathFileListData = file.fileData.data;
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, PathUtil.ReadPath);
        StartCoroutine(DownloadFile(fileInfos, OnReleaseFileComplete, OnReleaseAllFileComplete));
    }

    /// <summary>
    /// 单个文件下载完成回调
    /// </summary>
    /// <param name="fileInfo"></param>
    private void OnReleaseFileComplete(DownFileInfo fileInfo)
    {
        Debug.LogFormat("OnReleaseFileComplete: {0}", fileInfo.url);
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
    }

    /// <summary>
    /// 全部文件下载完成回调
    /// </summary>
    private void OnReleaseAllFileComplete()
    {
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ReadPathFileListData);
        CheckUpdata();
    }

    /// <summary>
    /// 检测更新
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void CheckUpdata()
    {
        string url = Path.Combine(AppConst.ResoucesUrl, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        StartCoroutine(DownloadFile(info, OnDownloadSeverFileListComplete));
    }

    /// <summary>
    /// 下载服务器数据回调
    /// </summary>
    /// <param name="info"></param>
    private void OnDownloadSeverFileListComplete(DownFileInfo file)
    {
        m_SeverFileListData = file.fileData.data;
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, AppConst.ResoucesUrl);
        List<DownFileInfo> downListFiles = new List<DownFileInfo>();

        for (int i = 0; i < fileInfos.Count; i++)
        {
            string localFile = Path.Combine(PathUtil.ReadWritePath, fileInfos[i].fileName);
            if (!FileUtil.IsExists(localFile))
            {
                fileInfos[i].url = Path.Combine(AppConst.ResoucesUrl, fileInfos[i].fileName);
                downListFiles.Add(fileInfos[i]);
            }
        }
        if (downListFiles.Count > 0)
            StartCoroutine(DownloadFile(fileInfos, OnUpdateFileComplete, OnUpdateAllFileComplete));
        else
            EnterGame();
    }

    private void OnUpdateFileComplete(DownFileInfo fileInfo)
    {
        Debug.LogFormat("OnUpdateFileComplete: {0}", fileInfo.url);
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
    }

    private void OnUpdateAllFileComplete()
    {
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_SeverFileListData);
        EnterGame();
    }

    private void EnterGame()
    {
        Manager.Resource.ParseVersionFile();
        Manager.Resource.LoadUI("Login/LoginUI", OnComplete);
    }

    private void OnComplete(UnityEngine.Object obj)
    {
        GameObject go = Instantiate(obj) as GameObject;
        go.transform.SetParent(this.transform);//设置父节点
        go.SetActive(true);//启用
        go.transform.localPosition = Vector3.zero;//位置归零
    }
}
