using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Проста шкала HP над головою моба (і боса) - завжди повернута до камери,
/// автоматично ховається, коли істота мертва.
/// </summary>
public class MobHealthBar : MonoBehaviour
{
    public Health targetHealth;
    public float heightOffset = 2.6f;
    public int mobLevel = 1;

    private Transform barTransform;
    private Slider slider;
    private Text numberLabel;
    private Transform cam;

    void Start()
    {
        if (targetHealth == null) targetHealth = GetComponent<Health>();
        if (Camera.main != null) cam = Camera.main.transform;
        BuildBar();
    }

    void BuildBar()
    {
        GameObject canvasObj = new GameObject("HealthBarWorld");
        canvasObj.transform.SetParent(transform, false);
        canvasObj.transform.position = transform.position + Vector3.up * heightOffset; // світова позиція - не залежить від повороту моба
        canvasObj.transform.localScale = Vector3.one * 0.018f;
        barTransform = canvasObj.transform;

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(200f, 26f);

        GameObject bg = new GameObject("Bg");
        bg.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.6f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero; bgRect.offsetMax = Vector2.zero;

        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(bg.transform, false);
        slider = sliderObj.AddComponent<Slider>();
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero; sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = new Vector2(2f, 2f); sliderRect.offsetMax = new Vector2(-2f, -2f);

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero; fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero; fillAreaRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.8f, 0.15f, 0.15f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero; fillRect.offsetMax = Vector2.zero;

        slider.fillRect = fillRect;
        slider.targetGraphic = fillImg;
        slider.minValue = 0f;
        slider.maxValue = targetHealth != null ? targetHealth.maxHealth : 100f;
        slider.value = targetHealth != null ? targetHealth.currentHealth : 100f;
        slider.handleRect = null;
        slider.interactable = false;

        // числовий підпис HP поверх шкали - однозначно видно, чи змінюється значення
        GameObject textObj = new GameObject("NumberLabel");
        textObj.transform.SetParent(bg.transform, false);
        numberLabel = textObj.AddComponent<Text>();
        numberLabel.alignment = TextAnchor.MiddleCenter;
        numberLabel.color = Color.white;
        numberLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        numberLabel.fontSize = 16;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero; textRect.offsetMax = Vector2.zero;
    }

    void LateUpdate()
    {
        if (targetHealth == null || barTransform == null) return;
        barTransform.position = transform.position + Vector3.up * heightOffset; // світова позиція щокадру - не "тягнеться" за поворотом моба
        slider.maxValue = targetHealth.maxHealth;
        slider.value = targetHealth.currentHealth;
        if (numberLabel != null)
            numberLabel.text = Mathf.CeilToInt(targetHealth.currentHealth) + " / " + Mathf.CeilToInt(targetHealth.maxHealth) + "  (Рів. " + mobLevel + ")";
        barTransform.gameObject.SetActive(!targetHealth.IsDead);

        if (cam == null)
        {
            if (Camera.main != null) cam = Camera.main.transform;
            else return;
        }
        barTransform.rotation = cam.rotation;
    }
}
