using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class piece_Library : MonoBehaviour
{
    [Header("White")]
    [SerializeField] GameObject WPawnPrefab;
    [SerializeField] GameObject WBishopPrefab;
    [SerializeField] GameObject WKnightPrefab;
    [SerializeField] GameObject WKingPrefab;
    [SerializeField] GameObject WQueenPrefab;
    [SerializeField] GameObject WRookPrefab;

    [Header("Black")]
    [SerializeField] GameObject BPawnPrefab;
    [SerializeField] GameObject BBishopPrefab;
    [SerializeField] GameObject BKnightPrefab;
    [SerializeField] GameObject BKingPrefab;
    [SerializeField] GameObject BQueenPrefab;
    [SerializeField] GameObject BRookPrefab;

    // ゲームに使用される駒：抽象
    Dictionary<(PieceType, PieceColor), GameObject> prefabTable;

    // ゲームに使用される駒：座標参照
    // Dictionary<Vector2Int, GameObject> piecesOnBoard;

    // 駒の個性を持たせるPiece_Controller
    Dictionary<Vector2Int, piece_Controller> pieceCtrOnBoard
        = new Dictionary<Vector2Int, piece_Controller>();

    // Spawnで登録した pieceCtrOnBoard を渡す
    public Dictionary<Vector2Int, piece_Controller> GetPieceControllers()
    {
        return pieceCtrOnBoard;
    }

    // 駒生成
    public GameObject GetPrefab(PieceType type, PieceColor color)
    {
        if (color == PieceColor.white)
        {
            return type switch
            {
                PieceType.pawn   => WPawnPrefab,
                PieceType.knight => WKnightPrefab,
                PieceType.bishop => WBishopPrefab,
                PieceType.rook   => WRookPrefab,
                PieceType.queen  => WQueenPrefab,
                PieceType.king   => WKingPrefab,
                _ => null
            };
        }
        else
        {
            return type switch
            {
                PieceType.pawn   => BPawnPrefab,
                PieceType.knight => BKnightPrefab,
                PieceType.bishop => BBishopPrefab,
                PieceType.rook   => BRookPrefab,
                PieceType.queen  => BQueenPrefab,
                PieceType.king   => BKingPrefab,
                _ => null
            };
        }
    }

}