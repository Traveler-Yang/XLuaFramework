using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    /// <summary>
    /// UI缓存
    /// </summary>
    Dictionary<string, GameObject> m_UI = new Dictionary<string, GameObject>();

    /// <summary>
    /// UI层级分组
    /// </summary>
    Dictionary<string, Transform> m_UIGroups = new Dictionary<string, Transform>();

    private Transform m_UIParent;//UICanvas节点

    private void Awake()
    {
        m_UIParent = this.transform.parent.Find("UI");//通过父节点找父节点下的UICanvas
    }

    public void SetUIGroup(List<string> group)
    {
        for (int i = 0; i < group.Count; i++)
        {
            GameObject go = new GameObject("Group-" + group[i]);
            go.transform.SetParent(m_UIParent,false);
            m_UIGroups.Add(group[i], go.transform);
        }
    }

    Transform GetUIGroup(string group)
    {
        if (!m_UIGroups.ContainsKey(group))
            Debug.LogErrorFormat("group is not exist", group);
        return m_UIGroups[group];
    }

    public void OpenUI(string uiName, string group, string luaName)
    {
        GameObject ui = null;
        if (m_UI.TryGetValue(uiName, out ui))
        {
            UILogic uILogic = ui.GetComponent<UILogic>();
            uILogic.OnOpen();
            return;
        }

        Manager.Resource.LoadUI(uiName, (UnityEngine.Object obj) =>
        {
            ui = Instantiate(obj) as GameObject;
            m_UI.Add(uiName, ui);

            Transform parent = GetUIGroup(group);
            ui.transform.SetParent(parent, false);//false:不保持世界坐标，跟着父节点的位置而变化

            UILogic uILogic = ui.AddComponent<UILogic>();
            uILogic.Init(luaName);
            uILogic.OnOpen();
        });
    }
}
