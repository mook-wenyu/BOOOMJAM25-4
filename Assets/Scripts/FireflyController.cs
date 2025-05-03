using UnityEngine;
using UnityEngine.UI;

public class FireflyController : MonoBehaviour
{
    public Texture fireflyTexture;  // 美术提供的火光贴图
    public float scrollSpeed = 0.1f;  // 水平移动速度
    public float jitterAmount = 0.02f; // 垂直抖动幅度
    public float flashSpeed = 2f;     // 闪烁频率

    private RawImage rawImage;
    private Material fireflyMaterial;
    private Vector2 uvOffset;

    void Start()
    {
        rawImage = GetComponent<RawImage>();

        // 使用支持UV偏移的Shader（确保Shader中有_MainTex_ST）
        fireflyMaterial = new Material(Shader.Find("UI/Unlit/Transparent"));
        fireflyMaterial.SetTexture("_MainTex", fireflyTexture);
        rawImage.material = fireflyMaterial;

        // 初始随机偏移（避免所有火光同步）
        uvOffset.x = Random.Range(0f, 1f);
        uvOffset.y = 0;
    }

    void Update()
    {
        // 水平循环移动（使用取模运算确保UV在[0,1]范围内循环）
        uvOffset.x = (uvOffset.x + Time.deltaTime * scrollSpeed) % 1f;

        // 垂直轻微抖动（模拟自然飘动）
        uvOffset.y = Mathf.Sin(Time.time * 0.5f) * jitterAmount;

        // 应用UV偏移
        fireflyMaterial.SetTextureOffset("_MainTex", uvOffset);

        // 动态闪烁效果
        float flash = 0.7f + Mathf.Sin(Time.time * flashSpeed) * 0.3f;
        fireflyMaterial.SetColor("_Color", new Color(1, 0.8f, 0.8f) * flash);
    }
}