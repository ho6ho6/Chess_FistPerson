using UnityEngine;

// Chessに必要な駒の色、種類、関係性を定義する列挙型と構造体

public enum PieceColor
{
    white,
    black,
    none
}

public enum PieceType
{
    none,
    pawn,
    rook,
    knight,
    bishop,
    queen,
    king
}

public enum CellRelation
{
    Empty,
    Mine,
    Enemy
}

public enum PlayerState
{
    GodView,            // 俯瞰視点
    FirstPersonAim,     // 一人称視点
    ConfirmMove,        // 神視点で移動先を確認
    PromotionSelect,    // Pawnの進化
}

public struct ChessPiece
{
    public PieceColor pieceColor;
    public PieceType pieceType;

    // pieceTypeがnoneの時にtrue,!=noneの時にfalseを返す
    public bool IsEmpty => pieceType == PieceType.none;
}

// 移動・捕獲 処理で使う 普通の移動ならmoved=true kingも捕獲したならisKing~ed=true
public struct MoveResult
{
    public bool moved;
    public bool isKingCaptured;
    public bool needPromotion;

    public Vector2Int promotionPos;
    public PieceColor promotionColor;
    public PieceType promotionType; // CPU用
}

// 難易度
public enum Difficult
{
    Easy,       // 誰でも簡単に       5~10分で勝てる
    Normal,     // 満足出来るくらいに  15~30分程度で
    Hard,       // 玄人向けに         30分程度を目標に
    Nightmare,  // ミスを許さない
}

// CPU難易度 - Nightmareで使用する返り値設定
public struct Move
{
    public Vector2Int from;
    public Vector2Int to;
}