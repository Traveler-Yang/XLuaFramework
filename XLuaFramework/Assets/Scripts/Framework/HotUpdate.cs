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
    /// fileList�ļ���Ϣ    
    /// </summary>
    byte[] m_ReadPathFileListData;

    /// <summary>
    /// ��Դ�����fileList�ļ���Ϣ
    /// </summary>
    byte[] m_SeverFileListData;

    internal class DownFileInfo
    {
        public string url;//URL��ַ
        public string fileName;//�ļ���
        public DownloadHandler fileData;//�ļ�����
    }

    //�����ļ�����
    int m_DownloadCount;

    /// <summary>
    /// ���ص����ļ�
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    IEnumerator DownloadFile(DownFileInfo info, Action<DownFileInfo> Complete)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(info.url);//��������
        yield return webRequest.SendWebRequest();//�ȴ��������
        if (webRequest.result == UnityWebRequest.Result.ProtocolError ||
            webRequest.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogErrorFormat("�����ļ�����{0}", info.url);
            yield break;
            //���ԣ���չ����
        }
        yield return new WaitForSeconds(0.02f);
        info.fileData = webRequest.downloadHandler;
        Complete?.Invoke(info);
        webRequest.Dispose();//��������ͷ�
    }

    /// <summary>
    /// ���ض���ļ�
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
    /// ��ȡ�ļ���Ϣ
    /// </summary>
    /// <param name="fileData"></param>
    /// <returns></returns>
    private List<DownFileInfo> GetFileList(string fileData, string path)
    {
        string content = fileData.Trim().Replace("\r", "");//ɾ��\r����
        string[] files = content.Split("\n");
        List<DownFileInfo> downFileInfos = new List<DownFileInfo>(files.Length);
        for (int i = 0; i < files.Length; i++)
        {
            string[] info = files[i].Split('|');//ͨ��|���ָ������
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
        //����Ƿ����״ΰ�װ
        if (IsFirstInstall())
        {
            //�״ΰ�װ���ͷ���Դ
            ReleaseResources();
        }
        else
        {
            //���򣬼�����
            CheckUpdata();
        }
    }

    /// <summary>
    /// ����Ƿ��ǳ��ΰ�װ
    /// </summary>
    /// <returns></returns>
    private bool IsFirstInstall()
    {
        //���ֻ��Ŀ¼�Ƿ���ڰ汾�ļ�
        bool isExistsReadPath = FileUtil.IsExists(Path.Combine(PathUtil.ReadPath, AppConst.FileListName));

        //����дĿ¼�Ƿ���ڰ汾�ļ�
        bool isExistsReadWritePath = FileUtil.IsExists(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName));

        //ֻ��Ŀ¼���ڣ���дĿ¼�����ڣ�����Ϊ�ǳ��ΰ�װ
        return isExistsReadPath && !isExistsReadWritePath;
    }

    /// <summary>
    /// �ͷ���Դ
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
    /// �ͷ���Դ�ص�������ֻ��Ŀ¼�ļ��б���ɣ�
    /// </summary>
    /// <param name="file"></param>
    private void OnDownloadReadPathFileListComplete(DownFileInfo file)
    {
        m_ReadPathFileListData = file.fileData.data;
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, PathUtil.ReadPath);
        StartCoroutine(DownloadFile(fileInfos, OnReleaseFileComplete, OnReleaseAllFileComplete));
        loadingUI.InitProgress(fileInfos.Count, "�����ͷ���Դ������������������");
    }

    /// <summary>
    /// �����ļ�������ɻص�
    /// </summary>
    /// <param name="fileInfo"></param>
    private void OnReleaseFileComplete(DownFileInfo fileInfo)
    {
        Debug.LogFormat("OnReleaseFileComplete: {0}", fileInfo.url);
        //�������bundle�ļ������д��
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
        m_DownloadCount++;
        loadingUI.UpdateProgress(m_DownloadCount);
    }

    /// <summary>
    /// ȫ���ļ�������ɻص�
    /// </summary>
    private void OnReleaseAllFileComplete()
    {
        //ȫ��bundle�ļ�������ɺ�д��fileLIst
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_ReadPathFileListData);
        CheckUpdata();
    }

    /// <summary>
    /// ������
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void CheckUpdata()
    {
        //��ȡ���������ϵ�fileList�ļ���������
        string url = Path.Combine(AppConst.ResoucesUrl, AppConst.FileListName);
        DownFileInfo info = new DownFileInfo();
        info.url = url;
        StartCoroutine(DownloadFile(info, OnDownloadSeverFileListComplete));
    }

    /// <summary>
    /// ���ط��������ݻص�
    /// </summary>
    /// <param name="info"></param>
    private void OnDownloadSeverFileListComplete(DownFileInfo file)
    {
        m_DownloadCount = 0;
        m_SeverFileListData = file.fileData.data;
        //��ȡ��������fileList�ļ���Ϣ
        List<DownFileInfo> fileInfos = GetFileList(file.fileData.text, AppConst.ResoucesUrl);
        //��Ҫ���ص�file�б�
        List<DownFileInfo> downListFiles = new List<DownFileInfo>();

        for (int i = 0; i < fileInfos.Count; i++)
        {
            //��ȡ���ص�fileList�ļ����ļ������ļ����ǻ�ȡ�ķ����fileList�ļ��б���ļ�����
            string localFile = Path.Combine(PathUtil.ReadWritePath, fileInfos[i].fileName);
            //���ж��Ƿ���ڣ���Ϊ�Ƿ���˵��ļ���������������ڣ��������Ҫ����
            if (!FileUtil.IsExists(localFile))
            {
                //��������ڣ�����Ҫ�������أ��õ���Դ���������ļ�·��
                fileInfos[i].url = Path.Combine(AppConst.ResoucesUrl, fileInfos[i].fileName);
                //�洢�����ǵ���Ҫ���ص��б���
                downListFiles.Add(fileInfos[i]);
            }
        }
        //��Ҫ���ص��ļ��б��������0������Ҫ����
        if (downListFiles.Count > 0)
        {
            StartCoroutine(DownloadFile(fileInfos, OnUpdateFileComplete, OnUpdateAllFileComplete));
            loadingUI.InitProgress(downListFiles.Count, "���ڸ�����Դ������");
        }
        else//������Ҫ���£���ֱ�ӽ�����Ϸ
            EnterGame();
    }

    /// <summary>
    /// ������Դ�������ĵ����ļ��ص�
    /// </summary>
    /// <param name="fileInfo"></param>
    private void OnUpdateFileComplete(DownFileInfo fileInfo)
    {
        Debug.LogFormat("OnUpdateFileComplete: {0}", fileInfo.url);
        string writeFile = Path.Combine(PathUtil.ReadWritePath, fileInfo.fileName);
        //�����µ�bundle�ļ�д��
        FileUtil.WriteFile(writeFile, fileInfo.fileData.data);
        m_DownloadCount++;
        loadingUI.UpdateProgress(m_DownloadCount);
    }

    /// <summary>
    /// ������Դ�������Ķ���ļ��ص�
    /// </summary>
    private void OnUpdateAllFileComplete()
    {
        //�����µ�fileList�ļ�д��
        FileUtil.WriteFile(Path.Combine(PathUtil.ReadWritePath, AppConst.FileListName), m_SeverFileListData);
        EnterGame();
        loadingUI.InitProgress(0, "�������롣����");
    }

    /// <summary>
    /// ������Ϸ
    /// </summary>
    private void EnterGame()
    {
        Manager.Event.Fire((int)GameEvent.GameInit);
        Destroy(loadingObj);
    }
}
