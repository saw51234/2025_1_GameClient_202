using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public CardData cardData;
    public int cardIndex;

    public MeshRenderer cardRenderer;
    public TextMeshPro nameText;
    public TextMeshPro costText;
    public TextMeshPro attackText;
    public TextMeshPro descriptionText;

    public bool isDragging = false;
    private Vector3 originalPosition;

    public LayerMask enemyLayer;
    public LayerMask playerLayer;

    private CardManager cardManager;

    void Start()
    {
        playerLayer = LayerMask.GetMask("Player");
        enemyLayer = LayerMask.GetMask("Enemy");

        cardManager = FindObjectOfType<CardManager>();

        SetupCard(cardData);
    }

    public void SetupCard(CardData Data)
    {
        cardData = Data;

        if (nameText != null) nameText.text = Data.cardName;
        if (costText != null) costText.text = Data.manaCost.ToString();
        if (attackText != null) attackText.text = Data.effectAmount.ToString();
        if (descriptionText != null) descriptionText.text = Data.description;

        if(cardRenderer != null && Data.artwork != null)
        {
            Material cardMaterial = cardRenderer.material;
            cardMaterial.mainTexture = Data.artwork.texture;
        }

        if (descriptionText != null)
        {
            descriptionText.text = Data.description + Data.GetAdditionalEffectsDescription();
        }
    }

    private void OnMouseDown()
    {
        originalPosition = transform.position;
        isDragging = true;
    }


    private void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            transform.position = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;

        if(cardManager != null)
        {
            float distToDiscard = Vector3.Distance(transform.position, cardManager.discardPosition.position);
            if(distToDiscard < 2.0f)
            {
                cardManager.DiscardCard(cardIndex);
                return;
            }
        }

        CharacterStats playerStats = null;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if(playerObj != null)
        {
            playerStats = playerObj.GetComponent<CharacterStats>();
        }

        if (playerStats == null || playerStats.currentMana < cardData.manaCost)
        {
            Debug.Log($"������ �����մϴ�! (�ʿ�: {cardData.manaCost}, ���� : {playerStats?.currentMana ?? 0})");
            transform.position = originalPosition;
            return;
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        bool cardUsed = false;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, enemyLayer))
        {
            CharacterStats enemyStats = hit.collider.GetComponent<CharacterStats>();

            if(cardData.cardType == CardData.CardType.Attack)
            {
                enemyStats.TakeDamage(cardData.effectAmount);
                Debug.Log($"{cardData.cardName} ī��� ������ {cardData.effectAmount} �������� �������ϴ�. ");
                cardUsed = true;
            }
            else
            {
                Debug.Log("�� ī��� ������ ����� �� �����ϴ�.");
            }
        }
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerLayer))
        {
            if(playerStats != null)
            {
                if(cardData.cardType == CardData.CardType.Heal)
                {
                    playerStats.Heal(cardData.effectAmount);
                    Debug.Log($"{cardData.cardName} ī��� �÷��̾��� ü���� {cardData.effectAmount} ȸ�� �߽��ϴ�. ");
                    cardUsed = true;
                }
            }
            else
            {
                Debug.Log("�� ī��� �÷��̾�� ����� �� �����ϴ�.");
            }
        }

        if (!cardUsed)
        {
            transform.position = originalPosition;
            if (cardManager != null)
                cardManager.ArrangeHand();
            return;
        }

        playerStats.UseMana(cardData.manaCost);
        Debug.Log($"������ {cardData.manaCost} ��� �߽��ϴ�. (���� ���� : {playerStats.currentMana})");

        if(cardData.additionalEffects != null && cardData.additionalEffects.Count > 0)
        {
            ProcessAdditionalEffectsAndDiscard();
        }
        else
        {
            if(cardManager != null)
                cardManager.DiscardCard(cardIndex);
        }
    }

    private void ProcessAdditionalEffectsAndDiscard()
    {
        CardData cardDataCopy = cardData;
        int cardIndexCopy = cardIndex;

        foreach (var effect in cardDataCopy.additionalEffects)
        {
            switch (effect.effectType)
            {
                case CardData.AdditionalEffectType.DrawCard:

                    for (int i=0; i<effect.effectAmount; i++)
                    {
                        if(cardManager != null)
                        {
                            cardManager.DrawCard();
                        }
                    }
                    Debug.Log($"{effect.effectAmount} ���� ī�带 ��ο� �߽��ϴ�.");
                    break;

                case CardData.AdditionalEffectType.DiscardCard:
                    for (int i = 0; i < effect.effectAmount; i++)
                    {
                        if(cardManager != null && cardManager.handCards.Count > 0)
                        {
                            int randomIndex = Random.Range(0, cardManager.handCards.Count);

                            Debug.Log($"���� ī�� ������ : ���õ� �ε��� {randomIndex}, ���� ���� ũŰ : {cardManager.handCards.Count}");

                            if(cardIndexCopy < cardManager.handCards.Count)
                            {
                                if(randomIndex != cardManager.handCards.Count)
                                {
                                    cardManager.DiscardCard(randomIndex);

                                    if(randomIndex < cardIndexCopy)
                                    {
                                        cardIndexCopy--;
                                    }
                                }
                                else if (cardManager.handCards.Count > 1)
                                {
                                    int newIndex = (randomIndex + 1)% cardManager.handCards.Count;
                                    cardManager.DiscardCard(newIndex);

                                    if (randomIndex < cardIndexCopy)
                                    {
                                        cardIndexCopy--;
                                    }
                                }
                            }
                            else
                            {
                                cardManager.DiscardCard(randomIndex );
                            }
                        }
                    }
                    Debug.Log($"�������� {effect.effectAmount} ���� ī�带 ���Ƚ��ϴ�.");
                    break;

                case CardData.AdditionalEffectType.GainMana:
                    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                    if(playerObj != null)
                    {
                        CharacterStats playerStats = playerObj.GetComponent<CharacterStats>();
                        if(playerStats != null)
                        {
                            playerStats.GainMana(effect.effectAmount);
                            Debug.Log($"������ {effect.effectAmount} ȹ�� �߽��ϴ�! (���� ���� : {playerStats.currentMana}");
                        }
                    }
                break;

                case CardData.AdditionalEffectType.ReduceEnemyMana:
                    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Ebentg");
                    foreach (var enemy in enemies)
                    {
                        CharacterStats enemyStats = enemy.GetComponent<CharacterStats>();
                        if (enemyStats != null)
                        {
                            enemyStats.UseMana(effect.effectAmount);
                            Debug.Log($"������ {enemyStats.characterName} �� ������ {effect.effectAmount} ���� ���׽��ϴ�. ");
                        }
                    }
                    break;

                case CardData.AdditionalEffectType.ReduceCardCost:
                    for(int i=0; i<cardManager.cardObjects.Count; i++)
                    {
                        CardDisplay display = cardManager.cardObjects[i].GetComponent<CardDisplay>();
                        if (display != null && display != this)
                        {
                            TextMeshPro costText = display.costText;
                            if(costText != null)
                            {
                                int originalCost = display.cardData.manaCost;
                                int newCost = Mathf.Max(0, originalCost - effect.effectAmount);
                                costText.text = newCost.ToString();
                                costText.color = Color.green;
                            }
                        }
                    }
                    break;
            }
        }

        if (cardManager != null)
            cardManager.DiscardCard(cardIndexCopy);

    }

}
