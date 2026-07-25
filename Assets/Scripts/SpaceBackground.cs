using UnityEngine;

public class SpaceBackground : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    private SpriteRenderer spriteRenderer;
    private Material mat;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mat = spriteRenderer.material;
    }

    void Update()
    {
        float offset = Time.time * scrollSpeed;
        mat.mainTextureOffset = new Vector2(offset, 0);
    }
}
