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
        //AB包文件路径列表
        List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();

        //依赖文件信息列表
        List<string> bundleInfos = new List<string>();
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

            //获取资源文件路径
            string assetName = PathUtil.GetUnityPath(fileName);
            //将获取到的的资源文件路径添加到assetBundle.assetNames中
            assetBundle.assetNames = new string[] { assetName };
            //获取AB包（AssetBundle）文件路径
            string bundleName = fileName.Replace(PathUtil.BuildResourcesPath, "").ToLower().TrimStart('/');
            //将获取到的AB包文件路径添加到assetBundle.assetNames中并添加.ab后缀
            assetBundle.assetBundleName = bundleName + ".ab";
            assetBundleBuilds.Add(assetBundle);//Add到List中

            //添加文件依赖信息
            List<string> dependenceInfo = GetDependnce(assetName);
            string bundleInfo = assetName + "|" + bundleName + ".ab";

            if (dependenceInfo.Count > 0)
                bundleInfo = bundleInfo + "|" + string.Join("|", dependenceInfo);

            bundleInfos.Add(bundleInfo);
        }
        if (Directory.Exists(PathUtil.BundledOutPath))//判断该输出目录是否存在
            Directory.Delete(PathUtil.BundledOutPath, true);//如果存在则删除所有子目录下的文件
        Directory.CreateDirectory(PathUtil.BundledOutPath);//之后创建一个目录

        BuildPipeline.BuildAssetBundles(PathUtil.BundledOutPath, assetBundleBuilds.ToArray(), BuildAssetBundleOptions.None, target);

        //创建依赖文件，并写入依赖文件路径
        File.WriteAllLines(PathUtil.BundledOutPath + "/" + AppConst.FileListName, bundleInfos);

        //刷新
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 获取依赖文件列表
    /// </summary>
    /// <param name="curFile"></param>
    /// <returns></returns>
    static List<string> GetDependnce(string curFile)
    {
        List<string> dependence = new List<string>();
        string[] files = AssetDatabase.GetDependencies(curFile);//得到依赖文件路径列表
        dependence = files.Where(File => !File.EndsWith(".cs") && !File.Equals(curFile)).ToList();
        return dependence;
    }
}
