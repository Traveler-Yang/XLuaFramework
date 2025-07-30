using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingUI : MonoBehaviour
{
    [SerializeField]
    Slider progressValue;

    [SerializeField]
    TextMeshProUGUI progressText;

    [SerializeField]
    GameObject progressBar;

    [SerializeField]
    Text progressDesc;


    float m_Max;

    public void InitProgress(float max, string desc)
    {
        m_Max = max;
        progressBar.SetActive(true);
        progressDesc.text = desc;
        progressValue.value = max > 0 ? 0 : 100;
        progressText.gameObject.SetActive(max > 0);
    }

    public void UpdateProgress(float progress)
    {
        progressValue.value = progress / m_Max;
        progressText.text = string.Format("{0:0}%", progressValue.value * 100);
    }
}
