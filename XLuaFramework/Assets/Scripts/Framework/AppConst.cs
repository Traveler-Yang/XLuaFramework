using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum GameMode
{
    EditorMode,
    PackageBundle,
    UpdateMode,
}

public class AppConst
{
    public const string BundleExtension = ".ab";
    public const string FileListName = "filelist.txt";

    public static GameMode gameMode = GameMode.EditorMode;
    public static bool OpenLog = true;
    /// <summary>
    /// 热更资源地址
    /// </summary>
    public const string ResoucesUrl = "http://192.168.1.63/AssetBundles";
}
