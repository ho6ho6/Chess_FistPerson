using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UI_Promotion : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] PieceType pieceType;

    // 4つの駒のボタン機能を呼べばよいだけ、そしてどれか一つクリックされたら
    // 非アクティブにして次呼ばれるのを待つ
    public void OnClick()
    {

        switch (pieceType)
        {
            case PieceType.queen:
                Debug.Log("Queenのボタンが押されました");
                break;

            case PieceType.bishop:
                Debug.Log("Bishopのボタンが押されました");
                break;

            case PieceType.knight:
                Debug.Log("Knightのボタンが押されました");
                break;

            case PieceType.rook:
                Debug.Log("Rookのボタンが押されました");
                break;

            default:
                Debug.Log("例外的なボタンが押されました");
                break;
        }

        manager_game.Instance.OnSelectPromotion(pieceType);
    }

}
