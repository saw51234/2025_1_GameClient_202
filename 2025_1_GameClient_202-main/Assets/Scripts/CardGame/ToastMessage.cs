using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.VersionControl;

public class ToastMessage : MonoBehaviour
{
    public static ToastMessage Instance { get; private set; }

    [SerializeField] private GameObject toastprefab;
    [SerializeField] private Transform messageContainer;
    [SerializeField] private float dispayTime = 2.5f;
    [SerializeField] private float faedTime = 0.5f;
    [SerializeField] private int maxMessage = 5;

    private Queue<GameObject> messageQueue = new Queue<GameObject>();
    private List<GameObject> activeMessage = new List<GameObject>();
    private bool isProcessinQueue = false;

    public enum MessageType
    {
        Normal,
        Success,
        Warning,
        Error,
        info
    }

    private IEnumerator ProcessMessageQueue()
    {
        isProcessinQueue = true;

        while(messageQueue.Count > 0)
        {
            GameObject toast = messageQueue.Dequeue();

            if(activeMessage.Count >= maxMessage && activeMessage.Count > 0)
            {
                Destroy(activeMessage[0]);
                activeMessage.RemoveAt(0);
            }

            toast.SetActive(true);
            activeMessage.Add(toast);

            CanvasGroup canvasgroup = toast.GetComponent<CanvasGroup>();
            if(canvasgroup == null)
            {
                canvasgroup = toast.AddComponent<CanvasGroup>();
            }

            canvasgroup.alpha = 0;
            float elapedTime = 0;
            while(elapedTime < faedTime)
            {
                canvasgroup.alpha = Mathf.Lerp(0,1,elapedTime / faedTime);
                elapedTime += Time.deltaTime;
                yield return null;
            }

            canvasgroup.alpha = 1;

            yield return new WaitForSeconds(dispayTime);

            elapedTime = 0;
            while (elapedTime < faedTime)
            {
                canvasgroup.alpha = Mathf.Lerp(1, 0, elapedTime / faedTime);
                elapedTime += Time.deltaTime;
                yield return null;
            }

            activeMessage.Remove(toast);
            Destroy(toast);

            yield return new WaitForSeconds(0.1f);
        }

        isProcessinQueue = false;
    }


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public  void ShowMessage(string message, MessageType type = MessageType.Normal)
    {
        if (toastprefab == null || messageContainer == null) return;

        GameObject toastInstance = Instantiate(toastprefab, messageContainer);
        toastInstance.SetActive(false);

        TextMeshProUGUI textComponent = toastInstance.GetComponentInChildren<TextMeshProUGUI>();
        Image backgroundImage = toastInstance.GetComponentInChildren<Image>();

        if(textComponent != null)
        {
            textComponent.text = message;

            Color textColor;
            Color backgroundColor;

            switch (type)
            {
                case MessageType.Success:
                    textColor = Color.green;
                    backgroundColor = new Color(0.2f, 0.6f, 0.2f, 0.8f);
                    break;
                case MessageType.Warning:
                    textColor = Color.yellow;
                    backgroundColor = new Color(0.8f, 0.6f, 0.2f, 0.8f);
                    break;
                case MessageType.Error:
                    textColor = Color.red;
                    backgroundColor = new Color(0.2f, 0.6f, 0.2f, 0.8f);
                    break;
                case MessageType.info:
                    textColor = Color.blue;
                    backgroundColor = new Color(0.2f, 0.6f, 0.2f, 0.8f);
                    break;
                default:
                    textColor = Color.white;
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                    break;
            }

            textComponent.color = textColor;
            if(backgroundColor != null)
            {
                backgroundImage.color = backgroundColor;
            }

            messageQueue.Enqueue(toastInstance);

            if (!isProcessinQueue)
            {
                StartCoroutine(ProcessMessageQueue());
            }
        }
    }
}
