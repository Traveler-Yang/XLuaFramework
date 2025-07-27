using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class BuildTool : Editor
{

    [MenuItem("Tools/Build Windows Bundle")]
    static void BuildWindowsBundle()
    {
        Build(BuildTarget.StandaloneWindows);
    }

    [MenuItem("Tools/Build Android Bundle")]
    static void BuildAndroidBundle()
    {
        Build(BuildTarget.Android);
    }

    [MenuItem("Tools/Build IOS Bundle")]
    static void BuildIOSBundle()
    {
        Build(BuildTarget.iOS);
    }
    static void Build(BuildTarget target)
    {
        //AB���ļ�·���б�
        List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();

        //�����ļ���Ϣ�б�
        List<string> bundleInfos = new List<string>();
        //GetFiles ��ȡ���е��ļ�
        //GetDirectories ��ȡ���е�Ŀ¼
        //�����ļ�·�����ļ�����ƥ���ַ���������ѡ��
        string[] files = Directory.GetFiles(PathUtil.BuildResourcesPath, "*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".meta"))
                continue;//meta�ļ�������

            AssetBundleBuild assetBundle = new AssetBundleBuild();
            string fileName = PathUtil.GetStandardPath(files[i]);//��·��ת��Ϊ��׼·����ת����б�ܣ�
            Debug.Log("file:" + fileName);

            //��ȡ��Դ�ļ�·��
            string assetName = PathUtil.GetUnityPath(fileName);
            //����ȡ���ĵ���Դ�ļ�·����ӵ�assetBundle.assetNames��
            assetBundle.assetNames = new string[] { assetName };
            //��ȡAB����AssetBundle���ļ�·��
            string bundleName = fileName.Replace(PathUtil.BuildResourcesPath, "").ToLower().TrimStart('/');
            //����ȡ����AB���ļ�·����ӵ�assetBundle.assetNames�в����.ab��׺
            assetBundle.assetBundleName = bundleName + ".ab";
            assetBundleBuilds.Add(assetBundle);//Add��List��

            //����ļ�������Ϣ
            List<string> dependenceInfo = GetDependnce(assetName);
            string bundleInfo = assetName + "|" + bundleName + ".ab";

            if (dependenceInfo.Count > 0)
                bundleInfo = bundleInfo + "|" + string.Join("|", dependenceInfo);

            bundleInfos.Add(bundleInfo);
        }
        if (Directory.Exists(PathUtil.BundledOutPath))//�жϸ����Ŀ¼�Ƿ����
            Directory.Delete(PathUtil.BundledOutPath, true);//���������ɾ��������Ŀ¼�µ��ļ�
        Directory.CreateDirectory(PathUtil.BundledOutPath);//֮�󴴽�һ��Ŀ¼

        BuildPipeline.BuildAssetBundles(PathUtil.BundledOutPath, assetBundleBuilds.ToArray(), BuildAssetBundleOptions.None, target);

        //���������ļ�����д�������ļ�·��
        File.WriteAllLines(PathUtil.BundledOutPath + "/" + AppConst.FileListName, bundleInfos);

        //ˢ��
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// ��ȡ�����ļ��б�
    /// </summary>
    /// <param name="curFile"></param>
    /// <returns></returns>
    static List<string> GetDependnce(string curFile)
    {
        List<string> dependence = new List<string>();
        string[] files = AssetDatabase.GetDependencies(curFile);//�õ������ļ�·���б�
        dependence = files.Where(File => !File.EndsWith(".cs") && !File.Equals(curFile)).ToList();
        return dependence;
    }
}
