using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

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
        List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();
        //GetFiles 获取所有的文件
        //GetDirectories 获取所有的目录
        //查找文件路径，文件搜索匹配字符串，搜索选项
        string[] files = Directory.GetFiles(PathUtil.BuildResourcesPath, "*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".meta"))
                continue;//meta文件不处理

            AssetBundleBuild assetBundle = new AssetBundleBuild();
            string fileName = PathUtil.GetStandardPath(files[i]);//将路径转换为标准路径（转换反斜杠）
            Debug.Log("file:" + fileName);

            string assetName = PathUtil.GetUnityPath(fileName);
            assetBundle.assetNames = new string[] { assetName };
            string bundleName = files[i].Replace(PathUtil.BuildResourcesPath, "").ToLower();
            assetBundle.assetBundleName = bundleName + ".ab";
            assetBundleBuilds.Add(assetBundle);
        }
        if (Directory.Exists(PathUtil.BundledOutPath))
            Directory.Delete(PathUtil.BundledOutPath, true);//删除所有子目录下的文件
        Directory.CreateDirectory(PathUtil.BundledOutPath);//创建一个目录

        BuildPipeline.BuildAssetBundles(PathUtil.BundledOutPath, assetBundleBuilds.ToArray(), BuildAssetBundleOptions.None, target);
    }
}
