using UnityEngine;

public class UI_ShowGameInfo : MonoBehaviour
{
    [SerializeField] GameObject ClassicMode;
    [SerializeField] GameObject CastleMode;

    [SerializeField] GameObject EasyMode;
    [SerializeField] GameObject NormalMode;
    [SerializeField] GameObject HardMode;
    [SerializeField] GameObject NightmareMode;

    public void ShowGameInfo(Difficult difficult, bool isCastleMode)
    {
        SelectDifficultUI(difficult);
        SelectGameModeUI(isCastleMode);
    }

    void SelectGameModeUI(bool isCastleMode)
    {
        if (!isCastleMode)
        {
            CastleMode.gameObject.SetActive(false);
            ClassicMode.gameObject.SetActive(true);
        }
        else
        {
            CastleMode.gameObject.SetActive(true);
            CastleMode.gameObject.SetActive(false);
        }
    }

    void SelectDifficultUI(Difficult difficult)
    {
        switch (difficult)
        {
            case Difficult.Easy:
                ResetDifficultUI();
                EasyMode.gameObject.SetActive(true);
                break;

            case Difficult.Normal:
                ResetDifficultUI();
                NormalMode.gameObject.SetActive(true);
                break;

            case Difficult.Hard:
                ResetDifficultUI();
                HardMode.gameObject.SetActive(true);
                break;

            case Difficult.Nightmare:
                ResetDifficultUI();
                NightmareMode.gameObject.SetActive(true);
                break;

            default:
                ResetDifficultUI();
                break;
        }
    }

    void ResetDifficultUI()
    {
        EasyMode.gameObject.SetActive(false);
        NormalMode.gameObject.SetActive(false);
        HardMode.gameObject.SetActive(false);
        NightmareMode.gameObject.SetActive(false);
    }

}
