using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class HotUpdate : MonoBehaviour
{

    /// <summary>
    /// fileList文件信息    
    /// </summary>
    byte[] m_ReadPathFileListData;

    /// <summary>
    /// 资源服务的fileList文件信息
    /// </summary>
    byte[] m_SeverFileListData;

    internal class DownFileInfo
    {
        public string url;//URL地址
        public string fileName;//文件名
        public DownloadHandler fileData;//文件内容
    }

    //下载文件数量
    int m_DownloadCount;

    /// <summary>
    /// 下载单个文件
    /// </summary>
    /// <param name="info"></param>
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
        yield return new WaitForSeconds(0.02f);
        info.fileData = webRequest.downloadHandler;
        Complete?.Invoke(info);
        webRequest.Dispose();//下载完成释放
    }

    /// <summary>
    /// 下载多个文件
    /// </summary>
    /// <param name="infos"></param>
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
        string content = fileData.Trim().Replace("\r", "");//删除\r符号
        string[] files = content.Split("\n");
        List<DownFileInfo> downFileInfos = new List<DownFileInfo>(files.Length);
        for (int i = 0; i < files.Length; i++)
        {
            string[] info = files[i].Split('|');//通过|来分割成数组
            DownFileInfo fileInfo = new DownFileInfo();
            fileInfo.fileName = info[1];
            fileInfo.url = Path.Combine(path, info[1]);
            downFileInfos.Add(fileInfo);
        }
        return downFileInfos;
    }

    GameObject loadingObj;
    LoadingUI loadingUI;

    private void Start()
    {
        GameObject go = Resources.Load<GameObject>("LoadingUI");
        loadingObj = Instantiate(go);
        loadingObj.transform.SetParent(this.transform);
        loadingUI = loadingObj.GetComponent<LoadingUI>();
        //检查是否是首次安装
        if (IsFirstInstall())
        {
            //首次安装，释放资源
            ReleaseResources();
        }
        else
        {
            //否则，检查更新
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
        m_DownloadCount = 0;
        string url = Path.Combine(PathUtil.ReadPath, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        StartCoroutine(DownloadFile(info, OnDownloadReadPathFileListComplete));
    }

    /// <summary>
    /// 释放资源回调（下载只读目录文件列表完成）
    /// </summary>
    /// <param name="file"></param>
    private void OnDownloadReadPathFileListComplete(DownFileInfo file)
    {
        m_ReadPathFileListData = file.fileData.data;
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, PathUtil.ReadPath);
        StartCoroutine(DownloadFile(fileInfos, OnReleaseFileComplete, OnReleaseAllFileComplete));
        loadingUI.InitProgress(fileInfos.Count, "正在释放资源，不消耗流量。。。");
    }

    /// <summary>
    /// 单个文件下载完成回调
    /// </summary>
    /// <param name="fileInfo"></param>
    private void OnReleaseFileComplete(DownFileInfo fileInfo)
    {
        Debug.LogFormat("OnReleaseFileComplete: {0}", fileInfo.url);
        //下载完成bundle文件后进行写入
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
        m_DownloadCount++;
        loadingUI.UpdateProgress(m_DownloadCount);
    }

    /// <summary>
    /// 全部文件下载完成回调
    /// </summary>
    private void OnReleaseAllFileComplete()
    {
        //全部bundle文件下载完成后写入fileLIst
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ReadPathFileListData);
        CheckUpdata();
    }

    /// <summary>
    /// 检测更新
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void CheckUpdata()
    {
        //获取到服务器上的fileList文件，并下载
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
        m_DownloadCount = 0;
        m_SeverFileListData = file.fileData.data;
        //获取到服务器fileList文件信息
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, AppConst.ResoucesUrl);
        //需要下载的file列表
        List<DownFileInfo> downListFiles = new List<DownFileInfo>();

        for (int i = 0; i < fileInfos.Count; i++)
        {
            //获取本地的fileList文件的文件名（文件名是获取的服务端fileList文件列表的文件名）
            string localFile = Path.Combine(PathUtil.ReadWritePath, fileInfos[i].fileName);
            //来判断是否存在（因为是服务端的文件名），如果不存在，则代表需要更新
            if (!FileUtil.IsExists(localFile))
            {
                //如果不存在，就需要进行下载，得到资源服务器的文件路径
                fileInfos[i].url = Path.Combine(AppConst.ResoucesUrl, fileInfos[i].fileName);
                //存储到我们的需要下载的列表中
                downListFiles.Add(fileInfos[i]);
            }
        }
        //需要下载的文件列表如果大于0，则需要更新
        if (downListFiles.Count > 0)
        {
            StartCoroutine(DownloadFile(fileInfos, OnUpdateFileComplete, OnUpdateAllFileComplete));
            loadingUI.InitProgress(downListFiles.Count, "正在更新资源。。。");
        }
        else//否则不需要更新，则直接进入游戏
            EnterGame();
    }

    /// <summary>
    /// 更新资源服务器的单个文件回调
    /// </summary>
    /// <param name="fileInfo"></param>
    private void OnUpdateFileComplete(DownFileInfo fileInfo)
    {
        Debug.LogFormat("OnUpdateFileComplete: {0}", fileInfo.url);
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        //将最新的bundle文件写入
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
        m_DownloadCount++;
        loadingUI.UpdateProgress(m_DownloadCount);
    }

    /// <summary>
    /// 更新资源服务器的多个文件回调
    /// </summary>
    private void OnUpdateAllFileComplete()
    {
        //将最新的fileList文件写入
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_SeverFileListData);
        EnterGame();
        loadingUI.InitProgress(0, "正在载入。。。");
    }

    /// <summary>
    /// 进入游戏
    /// </summary>
    private void EnterGame()
    {
        Manager.Event.Fire((int)GameEvent.GameInit);
        Destroy(loadingObj);
    }
}
