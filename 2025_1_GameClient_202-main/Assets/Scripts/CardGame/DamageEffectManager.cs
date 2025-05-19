using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageEffectManager : MonoBehaviour
{
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private Canvas uiCanvas;

    public static DamageEffectManager instance {  get; private set; }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if(uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
            if(uiCanvas == null)
            {
                Debug.Log("UI 컨버스를 찾을 수 없습니다. ");
            }
        }
    }

    public void ShowDamageText(Vector3 position, string text, Color color, bool isCrit = false, bool isStatus = false)
    {
        if (textPrefab == null || uiCanvas == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(position);

        if (screenPos.z < 0) return;

        GameObject damageText = Instantiate(textPrefab, uiCanvas.transform);

        RectTransform rectTransform = damageText.GetComponent<RectTransform>();
        if(rectTransform != null)
        {
            rectTransform.position = screenPos;
        }

        TextMeshProUGUI tmp = damageText.GetComponent<TextMeshProUGUI>();
        if(tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
            tmp.outlineColor = new Color(
                Mathf.Clamp01(color.r - 0.3f),
                Mathf.Clamp01(color.g - 0.3f),
                Mathf.Clamp01(color.b - 0.3f),
                color.a
            );

            float scale = 1.0f;

            int numvericValue;
            if(int.TryParse(text.Replace("+","").Replace("CRIT!", "").Replace("HEAL CRIT", ""), out numvericValue))
            {
                scale = Mathf.Clamp(numvericValue / 15f, 0.8f, 2.5f);
            }

            if (isCrit) scale = 1.4f;
            if (isStatus) scale *= 0.8f;

            damageText.transform.localScale = new Vector3(scale, scale, scale);
        }

        DamageTextEffect effect = damageText.AddComponent<DamageTextEffect>();
        if(effect != null)
        {
            effect.Initialized(isCrit, isStatus);
            if (isStatus)
            {
                effect.SetVerticalMovement();
            }
        }
    }

    public void ShowDamage(Vector3 position, int amount, bool isCrit = false)
    {
        string text = amount.ToString();
        Color color = isCrit ? new Color(1.0f, 0.8f, 0.0f) : new Color(1.0f, 0.3f, 0.3f);

        if(isCrit)
        {
            text = "CRIT!\n" + text;
        }

        ShowDamageText(position, text, color, isCrit);
    }

    public void ShowHeal(Vector3 position, int amount, bool isCrit = false)
    {
        string text = amount.ToString();
        Color color = isCrit ? new Color(0.4f, 1.0f, 0.4f) : new Color(0.3f, 0.9f, 0.3f);

        if (isCrit)
        {
            text = "HEAL CRIT!\n" + text;
        }

        ShowDamageText(position, text, color, isCrit);
    }

    public void ShowMiss(Vector3 position)
    {
        ShowDamageText(position, "MISS", Color.gray, false);
    }

    public void ShowStatusEffect(Vector3 position, string effectName)
    {
        Color color;

        switch (effectName.ToLower())
        {
            case "posion":
                color = new Color(0.5f, 0.1f, 0.5f);
                break;
            case "burn":
                color = new Color(1.0f, 0.4f, 0.0f);
                break;
            case "freeze":
                color = new Color(0.5f, 0.8f, 1.0f);
                break;
            case "stun":
                color = new Color(1.0f, 1.0f, 0.0f);
                break;
            default:
                color = new Color(1.0f, 1.0f, 1.0f);
                break;
        }

        ShowDamageText(position, effectName.ToLower(), color, false, true);
    }
}
