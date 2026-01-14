using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class UI_ReticleController : MonoBehaviour
{
    public GameObject reticleUI; // Canvas->Reticle->image*4を参照
    public GameObject PromoteUI; // Canvas->PromotionUI->各Buttonを参照

    public void SetReticleActive(bool active)
    {
        if (reticleUI != null)
            reticleUI.SetActive(active);
    }

    // UIの表示/非表示のみ
    public void ShowPromotionUI()
    {
        if (PromoteUI == null)
        {
            Debug.LogError("PromoteUI is NULL");
            return;
        }

        // レティクルは一旦削除
        SetReticleActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 昇格UI表示
        PromoteUI.SetActive(true);

    }

    public void HidePromotionUI()
    {
        SetReticleActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PromoteUI.SetActive(false);
    }


}