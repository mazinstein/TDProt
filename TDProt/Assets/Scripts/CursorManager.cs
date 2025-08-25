using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursorTexture; 
    public CursorMode cursorMode = CursorMode.Auto;

    void Start()
    {
        if (cursorTexture != null)
        {
            // Точный кончик стрелки
            Vector2 hotSpot = new Vector2(22f, 17f);

            Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
