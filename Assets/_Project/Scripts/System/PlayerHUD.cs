using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset fontAsset;
    private Met_CharacterController2D player;
    private TextMeshProUGUI status;
    private int lastLife = -1;
    private int lastEnemies = -1;

    private void Awake()
    {
        GameObject panel = new GameObject("HUD", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(16f, -16f);
        rect.sizeDelta = new Vector2(420f, 85f);

        Image background = panel.GetComponent<Image>();
        background.color = new Color(0.04f, 0.09f, 0.17f, 0.85f);
        background.raycastTarget = false;

        status = CreateLabel(panel.transform, "Estado", new Vector2(12f, -8f), 25f);
        TextMeshProUGUI controls = CreateLabel(panel.transform, "Controles", new Vector2(12f, -46f), 17f);
        controls.text = "A/D: mover  Z: saltar  C: dash  X: atacar  V: lanzar";
    }

    private TextMeshProUGUI CreateLabel(Transform parent, string name, Vector2 position, float fontSize)
    {
        GameObject label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(parent, false);
        RectTransform rect = label.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(404f, 34f);

        TextMeshProUGUI text = label.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) text.font = fontAsset;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private void Update()
    {
        if (player == null)
        {
            GameObject character = GameObject.FindGameObjectWithTag("Player");
            if (character != null) player = character.GetComponent<Met_CharacterController2D>();
        }
        if (player == null) return;

        int life = Mathf.Max(0, Mathf.CeilToInt(player.life));
        int enemies = GameManager.Instance != null ? GameManager.Instance.RemainingEnemies : 0;
        if (life == lastLife && enemies == lastEnemies) return;

        status.text = "VIDA  " + life + "     ENEMIGOS  " + enemies;
        lastLife = life;
        lastEnemies = enemies;
    }
}
