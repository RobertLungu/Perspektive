using UnityEngine;
using UnityEngine.UI;

public class ScrollingBackground : MonoBehaviour
{
    [Header("Stars")]
    public int   starCount  = 80;
    public float minSize    = 3f;
    public float maxSize    = 8f;
    public float minSpeed   = 15f;
    public float maxSpeed   = 50f;
    public float spinSpeed  = 45f;
    [Range(0f, 1f)] public float brightness = 0.4f;

    private RectTransform rect;
    private RectTransform[] stars;
    private Vector2[]       velocities;
    private Sprite          dot;

    void Awake()
    {
        rect       = GetComponent<RectTransform>();
        dot        = CreateDot();
        stars      = new RectTransform[starCount];
        velocities = new Vector2[starCount];

        for (int i = 0; i < starCount; i++)
            stars[i] = SpawnStar(i, randomPosition: true);
    }

    void Update()
    {
        Vector2 bounds = rect.rect.size * 0.5f;

        for (int i = 0; i < starCount; i++)
        {
            var rt  = stars[i];
            var pos = rt.anchoredPosition + velocities[i] * Time.deltaTime;
            rt.anchoredPosition = pos;
            rt.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

            float half = rt.sizeDelta.x * 0.5f;
            if (pos.x > bounds.x + half || pos.x < -bounds.x - half ||
                pos.y > bounds.y + half || pos.y < -bounds.y - half)
                SpawnStar(i, randomPosition: false);
        }
    }

    RectTransform SpawnStar(int i, bool randomPosition)
    {
        GameObject go;
        if (stars[i] != null)
        {
            go = stars[i].gameObject;
        }
        else
        {
            go = new GameObject("Star", typeof(Image));
            go.transform.SetParent(transform, false);
        }

        var img = go.GetComponent<Image>();
        img.sprite = dot;
        img.color  = new Color(brightness, brightness, brightness, 1f);

        float size = Random.Range(minSize, maxSize);
        var   rt   = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(size, size);

        Vector2 bounds = rect.rect.size * 0.5f;

        if (randomPosition)
        {
            rt.anchoredPosition = new Vector2(
                Random.Range(-bounds.x, bounds.x),
                Random.Range(-bounds.y, bounds.y)
            );
        }
        else
        {
            bool horizontal = Random.value > 0.5f;
            rt.anchoredPosition = horizontal
                ? new Vector2(Random.Range(-bounds.x, bounds.x), -bounds.y - size)
                : new Vector2(-bounds.x - size, Random.Range(-bounds.y, bounds.y));
        }

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float speed = Random.Range(minSpeed, maxSpeed);
        velocities[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;

        stars[i] = rt;
        return rt;
    }

    Sprite CreateDot()
    {
        int   size = 32;
        float cx   = size * 0.5f;
        float r    = size * 0.38f;
        var   tex  = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dist  = Mathf.Sqrt((x - cx) * (x - cx) + (y - cx) * (y - cx));
            float alpha = Mathf.Clamp01(r - dist);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
