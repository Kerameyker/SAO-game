using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Створює текстову бирку з іменем і рівнем моба над його головою,
/// що завжди повернута до камери (класичний "billboard").
/// </summary>
public class MobNameTag : MonoBehaviour
{
    public string mobName = "Моб";
    public int mobLevel = 1;
    public Color nameColor = Color.white;

    private Transform tagTransform;
    private Transform cam;

    void Start()
    {
        if (Camera.main != null) cam = Camera.main.transform;
        BuildTag();
    }

    void BuildTag()
    {
        GameObject canvasObj = new GameObject("NameTag");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        canvasObj.transform.localScale = Vector3.one * 0.02f;
        tagTransform = canvasObj.transform;

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        RectTransform rt = canvasObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(220f, 40f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(canvasObj.transform, false);
        Text label = textObj.AddComponent<Text>();
        label.text = mobName + " (Рів. " + mobLevel + ")";
        label.alignment = TextAnchor.MiddleCenter;
        label.color = nameColor;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 28;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            if (Camera.main != null) cam = Camera.main.transform;
            else return;
        }
        if (tagTransform == null) return;
        tagTransform.rotation = cam.rotation;
    }
}
