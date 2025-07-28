using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    AudioSource m_MusicAudio;
    AudioSource m_SoundAduio;

    private float SoundVolume
    {
        get { return PlayerPrefs.GetFloat("SoundVolume", 1.0f); }
        set
        {
            m_SoundAduio.volume = value;
            PlayerPrefs.SetFloat("SoundVolume", value);
        }
    }

    private float MusicVolume
    {
        get { return PlayerPrefs.GetFloat("MusicVolume", 1.0f); }
        set
        {
            m_MusicAudio.volume = value;
            PlayerPrefs.GetFloat("MusicVolume", value);
        }
    }

    private void Awake()
    {
        m_MusicAudio = this.gameObject.AddComponent<AudioSource>();
        m_MusicAudio.playOnAwake = false;
        m_MusicAudio.loop = true;

        m_SoundAduio = this.gameObject.AddComponent<AudioSource>();
        m_SoundAduio.loop = false;
    }

    public void PlayMusic(string name)
    {
        //����С��0.1��֮���л����ֶ���return��С��0.1��������
        if (this.MusicVolume < 0.1f)
            return;
        //��ȡ��ǰ�������ֵ�����
        string oldName = "";
        if (m_MusicAudio.clip != null)
            oldName = m_MusicAudio.clip.name;
        //��ͬ���ֲ��ظ�����
        if (oldName == name)
        {
            m_MusicAudio.Play();
            return;
        }

        Manager.Resource.LoadMusic(name, (UnityEngine.Object obj) =>
        {
            m_MusicAudio.clip = obj as AudioClip;
            m_MusicAudio.Play();
        });
    }

    /// <summary>
    /// ��ͣ����
    /// </summary>
    public void PauseMusic()
    {
        m_MusicAudio.Pause();
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void OnUnPauseMusic()
    {
        m_MusicAudio.UnPause();
    }

    /// <summary>
    /// ֹͣ����
    /// </summary>
    public void StopMusic()
    {
        m_MusicAudio.Stop();
    }

    /// <summary>
    /// ������Ч
    /// </summary>
    /// <param name="name"></param>
    public void PlaySound(string name)
    {
        if (this.SoundVolume < 0.1f)
            return;

        Manager.Resource.LoadSound(name, (UnityEngine.Object obj) =>
        {
            m_SoundAduio.PlayOneShot(obj as AudioClip);
        });
    }

    /// <summary>
    /// ����������С
    /// </summary>
    /// <param name="value"></param>
    public void SetMusicVolume(float value)
    {
        this.MusicVolume = value;
    }

    /// <summary>
    /// ������Ч��С
    /// </summary>
    /// <param name="value"></param>
    public void SetSoundVolume(float value)
    {
        this.SoundVolume = value;
    }
}
