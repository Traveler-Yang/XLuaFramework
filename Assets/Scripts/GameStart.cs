using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public GameMode gameMode;

    void Awake()
    {
        AppConst.gameMode = this.gameMode;
        DontDestroyOnLoad(this);
    }
}
