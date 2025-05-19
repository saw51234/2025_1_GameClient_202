using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DamageTextEffect : MonoBehaviour
{
    [SerializeField] private float movespeed = 100f;
    [SerializeField] private float lifeTime = 1.5f;

    private TextMeshProUGUI textMesh;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Color originalcolor;
    private Vector2 moveDirection;
    private float timer = 0f;

    private bool isCrit = false;
    private bool isStatus = false;
    private bool useGravity = true;
    private float verticalVelocity = 100f;

    public void Initialized(bool critical, bool statusEffect)
    {
        isCrit = critical;
        isStatus = statusEffect;

        if (isStatus)
        {
            useGravity = false;
        }

        Start();
    }

    public void SetVerticalMovement()
    {
        useGravity = false;
    }

    private IEnumerator PunchScale(float intentisy)
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * intentisy;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private IEnumerator FlashText()
    {
        if (textMesh == null) yield break;

        Color flashColor = Color.white;
        float flashDuration = 0.2f;

        Color startColor = textMesh.color;

        textMesh.color = flashColor;

        yield return new WaitForSeconds(flashDuration);
    }

    private IEnumerator CreateFlashEffect()
    {
        if(textMesh == null) yield break;

        float interval = 0.05f;
        int flashCount = 3;
        
        for(int i=0; i<flashCount; i++)
        {
            textMesh.alpha = 0.5f;
            yield return new WaitForSeconds(interval);
            textMesh.alpha = 1.0f;
            yield return new WaitForSeconds(interval);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if(canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if(textMesh != null)
        {
            originalcolor = textMesh.color;
            float randomX = Random.Range(-0.5f, 0.5f);
            float randomY = useGravity ? Random.Range(0.5f, 1.0f) : Random.Range(0.8f, 1.5f);
            moveDirection = new Vector2(randomX, randomY).normalized;

            if(rectTransform != null)
            {
                rectTransform.rotation = Quaternion.Euler(0, 0, Random.Range(-10.0f, 10.0f));
            }
            if (useGravity)
            {
                verticalVelocity = Random.Range(100.0f, 200f);
            }
            StartCoroutine(PunchScale(isCrit ? 1.5f : 1.2f));
            if (isCrit)
            {
                StartCoroutine(FlashText());
                StartCoroutine(CreateFlashEffect());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (rectTransform == null) return;

        if (useGravity)
        {
            verticalVelocity -= 300f * Time.deltaTime;
            rectTransform.anchoredPosition += new Vector2(0, verticalVelocity * Time.deltaTime);
            rectTransform.anchoredPosition += new Vector2(moveDirection.x * movespeed * Time.deltaTime, 0);
        }
        else
        {
            rectTransform.anchoredPosition += (Vector2)(moveDirection * movespeed * Time.deltaTime);
        }

        timer += Time.deltaTime;
        if(timer >= lifeTime * 0.5f)
        {
            if(canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, (timer - lifeTime * 0.5f) / (lifeTime * 0.5f));
            }
            else if (textMesh != null)
            {
                float alpha = Mathf.Lerp(originalcolor.a, 0f, (timer - lifeTime * 0.5f) / (lifeTime * 0.5f));
                textMesh.color = new Color(originalcolor.r, originalcolor.g, originalcolor.b, alpha);
            }
            movespeed = Mathf.Lerp(movespeed, 20f, Time.deltaTime * 2f);

            if((canvasGroup != null && canvasGroup.alpha <= 0.05f) || (textMesh != null && textMesh.color.a <= 0.05f))
            {
                Destroy(gameObject);
            }
        }
    }
}
