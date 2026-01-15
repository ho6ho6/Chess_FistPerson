using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class title : MonoBehaviour
{
    protected static bool isWhite = true;       // 偶数:白 奇数:黒
    protected static bool isDebugMode = false;  // デバッグモード
    protected static bool isCastleMode = false;  // 一人称チェス独自の配置でチェス
    protected static Difficult difficult;

    [SerializeField] GameObject TextIsWhite;
    [SerializeField] GameObject TextIsBlack;
    [SerializeField] GameObject IsDebugMode;
    [SerializeField] GameObject ClassicMode;
    [SerializeField] GameObject CastleMode;

    [SerializeField] GameObject EasyMode;
    [SerializeField] GameObject NormalMode;
    [SerializeField] GameObject HardMode;
    [SerializeField] GameObject NightmareMode;

    int count;

    void Start()
    {
        count = 1;
    }

    void Update()
    {
        // デバッグモード起動
        if (Keyboard.current.tabKey.IsPressed() && Keyboard.current.spaceKey.IsPressed()
            && Keyboard.current.tKey.wasPressedThisFrame)
        {

            isDebugMode = !isDebugMode;

            if (isDebugMode)
            {
                IsDebugMode.gameObject.SetActive(true);
            }
            else
            {
                IsDebugMode.gameObject.SetActive(false);
            }

            if (isDebugMode)
                Debug.Log("Debug Mode 起動");
            else
                Debug.Log("Debug Mode 終了");
        }

        // Spaceキーで白か黒か
        if (Keyboard.current.spaceKey.wasPressedThisFrame)  // Input.GetKeyDown(KeyCode.Space)
        {
            isWhite = !isWhite;
            if (isWhite)
            {
                TextIsBlack.gameObject.SetActive(false);
                TextIsWhite.gameObject.SetActive(true);
            }
            else
            {
                TextIsBlack.gameObject.SetActive(true);
                TextIsWhite.gameObject.SetActive(false);
            }
                Debug.Log("isWhite: " + isWhite);
        }

        // 右クリックで難易度設定
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            count++;
            SelectDifficult(count);
            if (count == 4) count = 0;
        }

        // 左コントロールで別のモード
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            isCastleMode = !isCastleMode;

            if (isCastleMode)
            {
                CastleMode.gameObject.SetActive(false);
                ClassicMode.gameObject.SetActive(true);
            }
            else
            {
                CastleMode.gameObject.SetActive(true);
                ClassicMode.gameObject.SetActive(false);
            }

                Debug.Log("籠城戦モード: " + isCastleMode);
        }

        // 左クリックでシーン遷移
        if (Mouse.current.leftButton.wasPressedThisFrame) ChangeScene();    // Input.GetMouseButtonDown(0)
    }

    // タイトル画面からゲーム画面へ
    void ChangeScene()
    {
        SceneManager.LoadScene("scene_game");
    }

    void SelectDifficult(int count)
    {
        Debug.Log($"count={count}");
        switch(count)
        {
            case 1:
                difficult = Difficult.Easy;
                ReSetGameMode();
                EasyMode.gameObject.SetActive(true);
                Debug.Log($"難易度設定:{difficult}");
                break;

            case 2:
                difficult = Difficult.Normal;
                ReSetGameMode();
                NormalMode.gameObject.SetActive(true);
                Debug.Log($"難易度設定:{difficult}");
                break;

            case 3:
                difficult = Difficult.Hard;
                ReSetGameMode();
                HardMode.gameObject.SetActive(true);
                Debug.Log($"難易度設定:{difficult}");
                break;

            case 4:
                difficult = Difficult.Nightmare;
                ReSetGameMode();
                NightmareMode.gameObject.SetActive(true);
                Debug.Log($"難易度設定:{difficult}");
                break;

            default:
                difficult = Difficult.Easy;
                break;
        }
    }

    void ReSetGameMode()
    {
        EasyMode.gameObject.SetActive(false);
        NormalMode.gameObject.SetActive(false);
        HardMode.gameObject.SetActive(false);
        NightmareMode.gameObject.SetActive(false);
    }

    // 白か黒かを取得
    public static bool GetIsWhite() { return isWhite; }

    public static bool IsCastleMode() { return isCastleMode; }

    public static bool GetIsDebugMode() { return isDebugMode; }

    public static Difficult GetDifficult() { return difficult; }

}