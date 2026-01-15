using UnityEngine;

public class SkyboxAudio : MonoBehaviour
{
    [SerializeField] Material skyboxDayNormal;
    [SerializeField] Material skyboxNightNormal;
    [SerializeField] Material skyboxDayRare;
    [SerializeField] Material skyboxNightRare;
    [SerializeField] Material skyboxNightmare;

    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioClip bgmDayN;
    [SerializeField] AudioClip bgmNightN;
    [SerializeField] AudioClip bgmDayR;
    [SerializeField] AudioClip bgmNightR;
    [SerializeField] AudioClip bgmNightmare;

    // 背景の状態を列挙
    public enum SkyType
    {
        DayNormal,
        NightNormal,
        DayRare,
        NightRare,
        Nightmare,
    }

    // どの背景にしよう？
    public SkyType DrawRandomSky(Difficult difficult)
    {
        int r = Random.Range(0, 100);
        Debug.Log($"抽選完了={r}");

        if (difficult != Difficult.Nightmare)
        {
            if (r < 25)  return SkyType.DayNormal;
            if (r < 50)  return SkyType.NightNormal;
            if (r < 75)  return SkyType.DayRare;
            if (r < 100) return SkyType.NightRare;
        }
        return SkyType.Nightmare;
    }

    // SkyBoxと曲をランダムに設定
    public void SetSky(SkyType type)
    {
        switch (type)
        {
            case SkyType.DayNormal:
                Debug.Log("普通の日(朝)が選択されました。");
                RenderSettings.skybox = skyboxDayNormal;
                bgmSource.clip = bgmDayN;
                break;

            case SkyType.NightNormal:
                Debug.Log("普通の日(夜)が選択されました。");
                RenderSettings.skybox = skyboxNightNormal;
                bgmSource.clip = bgmNightN;
                break;

            case SkyType.DayRare:
                Debug.Log("普通な日(朝)が選択されました。");
                RenderSettings.skybox = skyboxDayRare;
                bgmSource.clip = bgmDayR;
                break;

            case SkyType.NightRare:
                Debug.Log("レアな日(夜)が選択されました。");
                RenderSettings.skybox = skyboxNightRare;
                bgmSource.clip = bgmNightR;
                break;

            case SkyType.Nightmare:
                Debug.Log("最高難易度が選択されました。");
                RenderSettings.skybox = skyboxNightmare;
                bgmSource.clip = bgmNightmare;
                break;
        }

        bgmSource.Play();
    }

}
