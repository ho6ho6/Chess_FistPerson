using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class TryMove
{

    // 駒を動かす
    public static MoveResult Execute(Vector2Int from, Vector2Int to, BoardCell boardCell, Board board, Dictionary<Vector2Int, piece_Controller> pieceCtrOnBoard)
    {

        // 結果はどっち？
        var result = new MoveResult { moved = false, isKingCaptured = false };

        ChessPiece piece = board.GetChessPiece(from);

        // 空マスを選択したら
        if (piece.IsEmpty) return result;

        // ルール判定
        if (!ruleMove.CanMove(piece, from, to, board)) return result;

        // 移動実行、King捕獲フラグも返す
        bool isKingCaptured = ExecuteMove(from, to, boardCell, board, pieceCtrOnBoard);

        // Pawn 昇格チェック
        if (piece.pieceType == PieceType.pawn &&
            IsPromotionRank(to, piece.pieceColor))
        {
            //PromotePawn(to, piece.pieceType, boardCell, board, pieceCtrOnBoard);
            result.needPromotion = true;
            result.promotionPos = to;
            result.promotionColor = piece.pieceColor;
        }

        // 移動は成功 CanMoveで評価はされている
        result.moved = true;
        result.isKingCaptured = isKingCaptured;

        return result;
    }

    // Pawnの昇格は？
    public static bool IsPromotionRank(Vector2Int pos, PieceColor color)
    {
        return (color == PieceColor.white && pos.y == 7)
            || (color == PieceColor.black && pos.y == 0);
    }

    // Pawnの昇格を
    public static void PromotePawn(
        Vector2Int pos, PieceType newType,
        BoardCell boardCell, Board board,
        Dictionary<Vector2Int, piece_Controller> pieceCtrOnBoard)
    {
        // 古い Pawn controller を取得
        if (!pieceCtrOnBoard.TryGetValue(pos, out var oldCtr))
        {
            Debug.LogError("PromotePawn: controller not found");
            return;
        }

        PieceColor color = oldCtr.pieceData.pieceColor;

        // Pawn GameObject を破壊
        Object.Destroy(oldCtr.gameObject);
        pieceCtrOnBoard.Remove(pos);

        // 昇格先（まずは Queen 固定）
        // newType = PieceType.queen;

        // 論理盤面を更新
        board.SetChessPiece(pos, new ChessPiece
        {
            pieceType = newType,
            pieceColor = color
        });

        // 新しい Queen を生成
        var newCtr = piece_Factory.Spawn(
            newType,
            color,
            pos,
            boardCell
        );

        newCtr.SetBoardPos(pos);
        pieceCtrOnBoard[pos] = newCtr;

        Debug.Log($"Pawn promoted to {newType}");
    }


    static bool ExecuteMove(Vector2Int from, Vector2Int to, BoardCell boardCell, Board board, Dictionary<Vector2Int, piece_Controller> pieceCtrOnBoard)
    {
        Debug.Log($"ContainsKey(from) = {pieceCtrOnBoard.ContainsKey(from)}");

        ChessPiece piece = board.GetChessPiece(from);
        bool isKingCaptured = false;

        // 捕獲処理
        if (pieceCtrOnBoard.TryGetValue(to, out var captured))
        {
            if (captured != null)
            {
                Debug.Log($"捕獲");

                // 捕まえたのがKingだったら終了フラグを立てる
                if (board.GetChessPiece(to).pieceType == PieceType.king)
                    isKingCaptured = true;

                Object.Destroy(captured.gameObject);
                pieceCtrOnBoard.Remove(to);
            }
        }

        // boardを更新
        board.SetChessPiece(to,piece);
        board.ClearCell(from);

        // 駒Controllerも更新
        if (pieceCtrOnBoard.TryGetValue(from, out var mover))
        {
            pieceCtrOnBoard.Remove(from);
            pieceCtrOnBoard[to] = mover;

            // 論理座標更新
            mover.SetBoardPos(to);

            // mover.transform.position = board.BoardToWorld(to.x, to.y);
            mover.MoveToBoardPos(boardCell.BoardToWorld(to.x, to.y));
        }

        return isKingCaptured;
    }

}