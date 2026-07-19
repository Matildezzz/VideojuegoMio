using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("Cursor")]
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 hotspot = Vector2.zero;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    private void Start()
    {
        ApplyCursor();
    }

    public void ApplyCursor()
    {
        if (cursorTexture == null)
        {
            Debug.LogWarning("CursorManager: falta asignar la textura del cursor.");
            return;
        }

        Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
    }

    public void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }
}