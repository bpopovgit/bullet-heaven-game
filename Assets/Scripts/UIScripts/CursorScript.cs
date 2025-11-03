using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorScript : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private Texture2D cursorSprite; // Assign your custom sprite here
    [SerializeField] private Vector2 hotSpot = Vector2.zero; // Set the cursor's "click" point
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    private void Start()
    {
        if (cursorSprite != null)
        {
            // Set the custom cursor
            Cursor.SetCursor(cursorSprite, hotSpot, cursorMode);
        }
        else
        {
            Debug.LogWarning("Cursor sprite not assigned in CursorScript.");
        }
    }

    private void OnDisable()
    {
        // Reset the cursor to the default when the script is disabled
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
