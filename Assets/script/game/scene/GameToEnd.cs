using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class GameToEnd : MonoBehaviour
{
    // 製品版は白黒のキングがチェックメイトされたら自動で遷移
    public void GameToEnd_Update()
    {

        // kingが捕まった
        SceneManager.LoadScene("scene_end");
        Debug.Log("End画面へ");

    }

}