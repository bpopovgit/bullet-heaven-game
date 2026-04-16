using UnityEngine;

public static class PickupSpriteFactory
{
    private static Sprite _circleSprite;

    public static Sprite CircleSprite
    {
        get
        {
            if (_circleSprite == null)
                _circleSprite = CreateCircleSprite();

            return _circleSprite;
        }
    }

    public static void AddDefaultPhysics(GameObject go, float radius)
    {
        CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = radius;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public static SpriteRenderer AddDefaultRenderer(GameObject go, Color color, int sortingOrder)
    {
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = CircleSprite;
        renderer.color = color;
        renderer.sortingLayerName = "Actors";
        renderer.sortingOrder = sortingOrder;

        return renderer;
    }

    private static Sprite CreateCircleSprite()
    {
        const int size = 16;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false);
        texture.filterMode = FilterMode.Point;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.45f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                Color color = distance <= radius ? Color.white : Color.clear;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit: 16f);
    }
}
