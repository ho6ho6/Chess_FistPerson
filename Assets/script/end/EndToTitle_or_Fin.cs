using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class EndToTitle_or_Fin : MonoBehaviour
{

    // 製品版はボタンで選択
    void Update()
    {

        // 左クリックでシーン遷移
        if (Mouse.current.leftButton.wasPressedThisFrame)    // Input.GetMouseButtonDown(0)
        {
            ChangeScene();
        }

        // 右クリックで終了
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Debug.Log("一人称チェスを終了します");    // Input.GetMouseButtonDown(1)
        }
    }

    // タイトル画面からゲーム画面へ
    void ChangeScene()
    {
        SceneManager.LoadScene("scene_title");
    }
}