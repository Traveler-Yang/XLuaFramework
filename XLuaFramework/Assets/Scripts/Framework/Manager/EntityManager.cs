using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    /// <summary>
    /// entity����
    /// </summary>
    Dictionary<string, GameObject> m_Entities = new Dictionary<string, GameObject>();

    /// <summary>
    /// entity�㼶����
    /// </summary>
    Dictionary<string, Transform> m_Groups = new Dictionary<string, Transform>();

    private Transform m_EntityParent;//entity���ڵ�

    private void Awake()
    {
        m_EntityParent = this.transform.parent.Find("Entity");
    }

    public void SetEntityGroup(List<string> groups)
    {
        for (int i = 0; i < groups.Count; i++)
        {
            GameObject go = new GameObject("Group-" + groups[i]);
            go.transform.SetParent(m_EntityParent, false);
            m_Groups.Add(groups[i], go.transform);
        }
    }

    Transform GetUIGroup(string group)
    {
        if (!m_Groups.ContainsKey(group))
            Debug.LogErrorFormat("group is not exist", group);
        return m_Groups[group];
    }

    public void ShowEntity(string name, string group, string luaName)
    {
        GameObject entity = null;
        if (m_Entities.TryGetValue(name, out entity))
        {
            EntityLogic logic = entity.GetComponent<EntityLogic>();
            logic.OnShow();
            return;
        }

        Manager.Resource.LoadPrefab(name, (UnityEngine.Object obj) =>
        {
            entity = Instantiate(obj) as GameObject;
            Transform parent = GetUIGroup(group);
            entity.transform.SetParent(parent, false);//false:�������������꣬���Ÿ��ڵ��λ�ö��仯
            m_Entities.Add(name, entity);
            EntityLogic entityLogic = entity.AddComponent<EntityLogic>();
            entityLogic.Init(luaName);
            entityLogic.OnShow();
        });
    }
}
