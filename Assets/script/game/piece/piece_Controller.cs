using UnityEngine;
using UnityEngine.InputSystem;

public class piece_Controller : MonoBehaviour
{
    public ChessPiece pieceData;
    public Vector2Int boardPos { get; private set; }

    [Header("Camera")]
    public Camera pieceCam;

    [Header("Sensitive")]
    private float sensitivity = 80.0f;
    private float rotationX = 5f;
    private float minY = -60f;
    private float maxY = 60f;

    // 個体差が必要ならここに追加

    // 各駒の固有カメラをOff
    Camera isPieceCameraActive;


    void Update()
    {
        if (pieceCam != null && pieceCam.gameObject.activeSelf)
        {
            HandleMovement();
        }
    }

    // 駒の座標や色を初期化
    public void Initialize(PieceType type, PieceColor color, Vector2Int pos)
    {
        pieceData = new ChessPiece
        {
            pieceType = type,
            pieceColor = color
        };

        boardPos = pos;
    }

    // 自分の色を把握
    public void SetupMyColor(ChessPiece piece)
    {
        pieceData.pieceColor = piece.pieceColor;
    }

    // prefabで非アクティブにしているが、一応
    public void SetupCamera()
    {
        isPieceCameraActive = GetComponentInChildren<Camera>();
        if (isPieceCameraActive != null) isPieceCameraActive.enabled = false; 
    }

    // Boardの座標値の違いで個体を区別する
    public void SetBoardPos(Vector2Int pos)
    {
        boardPos = pos;
    }

    // 移動先
    public void MoveToBoardPos(Vector3 newPos)
    {
        Debug.Log("見た目更新");
        // 見た目の座標更新
        transform.position = newPos;
    }

    // 各駒のカメラをOn/Off
    public void ActiveCamera()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pieceCam.gameObject.SetActive(true);
    }

    public void notActiveCamera()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pieceCam.gameObject.SetActive(false);
    }

    // アクティブのカメラを渡す
    public Camera GetActiveCamera()
    {
        return pieceCam;
    }

    // その駒は私の？
    public bool IsMine(PieceColor currentTurn)
    {
        return pieceData.pieceColor == currentTurn;
    }

    // この駒の型を返す
    public PieceType GetPieceType()
    {
        return pieceData.pieceType; // boardPiece はこの駒の ChessPiece 情報
    }

    void HandleMovement()
    {
        // 新InputSystemでマウスDelta取得
        Vector2 moveInput = Mouse.current.delta.ReadValue();

        float mouseX = moveInput.x * sensitivity * Time.deltaTime;
        float mouseY = moveInput.y * sensitivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, minY, maxY);

        transform.Rotate(Vector3.up * mouseX);        // キャラクター（駒本体）回転
        pieceCam.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f); // カメラ上下回転
    }

}