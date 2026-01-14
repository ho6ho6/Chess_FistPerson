using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ruleMove
{
    // 駒は動けるか？
    public static bool CanMove(ChessPiece piece, Vector2Int from, Vector2Int to, Board board)
    {

        // そもそも駒が無い 移動先が自分の駒ならダメ
        if (piece.IsEmpty) return false;

        if (board.GetCellRelation(to, piece.pieceColor) == CellRelation.Mine)
            return false;


        // 駒の種類ごとの移動ルールを実装する
        switch (piece.pieceType)
        {
            case PieceType.pawn:
                return CanMovePawn(  piece,   from, to, board);

            case PieceType.rook:
                return CanMoveRook  (piece, from, to, board);

            case PieceType.knight:
                return CanMoveKnight(piece, from, to, board);

            case PieceType.bishop:
                return CanMoveBishop(piece, from, to, board);

            case PieceType.queen:
                return CanMoveQueen (piece, from, to, board);

            case PieceType.king:
                return CanMoveKing  (piece, from, to, board);

            default:
                return false;
        }

    }

    static bool CanMovePawn(ChessPiece piece, Vector2Int from, Vector2Int to, Board board)
    {
        // 盤面外はダメ
        if (!board.IsInsideBoard(to.x,to.y)) return false;

        // ポーンの基本的な動き: 1マス前進、初回は2マス前進可能、敵駒の斜め前に移動して捕獲
        int direction = (piece.pieceColor == PieceColor.white) ? 1 : -1;

        int dx = (to.x - from.x);
        int dz = (to.y - from.y);

        // 1マス
        if (dx == 0 && dz == direction)
        {
            return board.IsCellEmpty(to.x, to.y);
        }

        // 2マス
        bool isFirstMove = (piece.pieceColor == PieceColor.white && from.y == 1) ||
                           (piece.pieceColor == PieceColor.black && from.y == 6);

        if (isFirstMove && dx == 0 && dz == direction * 2)
        {
            Vector2Int middle = new Vector2Int(from.x, from.y + direction);

            return board.IsCellEmpty(middle.x, middle.y) && board.IsCellEmpty(to.x, to.y);
        }

        // 斜め捕獲は可能？
        if (Mathf.Abs(dx) == 1 && dz == direction)
        {
            return board.GetCellRelation(to, piece.pieceColor) == CellRelation.Enemy;
        }

        return false;
    }

    static bool CanMoveRook(ChessPiece piece, Vector2Int from, Vector2Int to, Board board)
    {
        if (!board.IsInsideBoard(to.x, to.y)) return false;

        int dx = to.x - from.x;
        int dz = to.y - from.y;

        // 直線 でないなら対象外
        if (!(dx == 0 || dz == 0))
            return false;

        int stepX = Mathf.Clamp(dx, -1, 1);
        int stepZ = Mathf.Clamp(dz, -1, 1);

        int x = from.x + stepX;
        int z = from.y + stepZ;

        while (x != to.x || z != to.y)
        {
            if (!board.IsCellEmpty(x, z))
                return false;

            x += stepX;
            z += stepZ;
        }

        CellRelation relation = board.GetCellRelation(to, piece.pieceColor);

        return relation != CellRelation.Mine;
    }

    static bool CanMoveKnight(ChessPiece piece, Vector2Int from, Vector2Int to, Board board)
    {
        if (!board.IsInsideBoard(to.x, to.y)) return false;

        // Knightの移動量は？
        Vector2Int dir = to - from;
        int dirx = Mathf.Abs(dir.x);
        int diry = Mathf.Abs(dir.y);

        if(!((dirx == 1 && diry == 2) || (dirx == 2 && diry == 1))) return false;

        // 移動マス判定
        CellRelation targetPos = board.GetCellRelation(to, piece.pieceColor);

        // empty か Enemy OK
        if (targetPos == CellRelation.Empty || targetPos == CellRelation.Enemy) return true;

        // 自分の駒だった
        return false;
    }

    static bool CanMoveBishop(ChessPiece piece, Vector2Int from, Vector2Int to, Board board)
    {
        if (!board.IsInsideBoard(to.x, to.y)) return false;

        int dx = Mathf.Abs(to.x - from.x);
        int dz = Mathf.Abs(to.y - from.y);

        if (dx != dz) return false;

        if (!IsPathClear(from, to, board))
            return false;

        CellRelation relation =
            board.GetCellRelation(to, piece.pieceColor);

        return relation != CellRelation.Mine;   // 敵か空だったら良い
    }

    static bool CanMoveQueen(ChessPiece piece, Vector2Int from, Vector2Int to, Board board)
    {
        if (!board.IsInsideBoard(to.x, to.y)) return false;

        int dx = Mathf.Abs(to.x - from.x);
        int dz = Mathf.Abs(to.y - from.y);

        if (!((dx == dz) || dx == 0 || dz == 0)) return false;

        if (!IsPathClear(from, to, board))
            return false;

        CellRelation relation = board.GetCellRelation(to, piece.pieceColor);

        return relation != CellRelation.Mine;
    }

    static bool CanMoveKing(ChessPiece piece, Vector2Int from, Vector2Int to, Board board)
    {
        if (!board.IsInsideBoard(to.x, to.y)) return false;

        int dx = Mathf.Abs(to.x - from.x);
        int dy = Mathf.Abs(to.y - from.y);

        if (dx > 1 || dy > 1) return false;

        CellRelation relation =
            board.GetCellRelation(to, piece.pieceColor);

        return relation != CellRelation.Mine;
    }


    static bool IsPathClear(Vector2Int from, Vector2Int to, Board board)
    {
        int dx = to.x - from.x;
        int dz = to.y - from.y;

        // 直線 or 斜線でないなら対象外
        
        // 横軸dxが同じ == 0 Rook Queen的な動き
        // 縦軸dzが同じ == 0

        // 斜め移動は縦横の移動量が同じ == 0 bishop queen

        if (!(dx == 0 || dz == 0 || Mathf.Abs(dx) == Mathf.Abs(dz)))
            return false;

        int stepX = Mathf.Clamp(dx, -1, 1);
        int stepZ = Mathf.Clamp(dz, -1, 1);

        int x = from.x + stepX;
        int z = from.y + stepZ;

        while (x != to.x || z != to.y)
        {
            if (!board.IsCellEmpty(x,z))
                return false;

            x += stepX;
            z += stepZ;
        }

        return true;
    }

    // 駒の移動先は？
    public static List<Vector2Int> GetMovableCells(ChessPiece piece, Vector2Int from, Board board)
    {
        var result = new List<Vector2Int>();

        for (int x=0; x<8; x++)
        {
            for (int y=0; y<8; y++)
            {
                Vector2Int to = new Vector2Int(x, y);

                if (CanMove(piece, from, to, board)) result.Add(to);
            }
        }
        return result;
    }
}