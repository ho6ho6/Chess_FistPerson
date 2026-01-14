using UnityEngine;

public class CanvasDebugger : MonoBehaviour
{
    void Start()
    {
        var canvases = FindObjectsOfType<Canvas>(true); // 非アクティブ含む
        Debug.Log($"Canvas count = {canvases.Length}");

        foreach (var c in canvases)
        {
            Debug.Log(
                $"Canvas: {c.name}, " +
                $"Mode={c.renderMode}, " +
                $"Camera={(c.worldCamera ? c.worldCamera.name : "NULL")}, " +
                $"Active={c.gameObject.activeInHierarchy}"
            );
        }
    }
}
