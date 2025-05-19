using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextTarget : MonoBehaviour
{
    [SerializeField] private int minDamage = 5;
    [SerializeField] private int maxDamage = 50;
    [SerializeField] private int minHeal = 10;
    [SerializeField] private int maxHeal = 10;
    [SerializeField] private float criticalChance = 0.2f;
    [SerializeField] private float missChance = 0.1f;
    [SerializeField] private float statusEffectChance = 0.15f;

    private string[] statusEffects = {"Posion", "Burn", "Freeze", "Stun", "Blind", "Silence"};

    private void ShowDamage(int amount, bool isCrit)
    {
        if(DamageEffectManager.instance != null)
        {
            Vector3 position = transform.position;
            position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1f, 1.5f), 0);
            DamageEffectManager.instance.ShowDamage(position, amount, isCrit);
        }
    }

    private void ShowHeal(int amount, bool isCrit)
    {
        if (DamageEffectManager.instance != null)
        {
            Vector3 position = transform.position;
            position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1f, 1.5f), 0);
            DamageEffectManager.instance.ShowHeal(position, amount, isCrit);
        }
    }

    private void ShowMiss()
    {
        if (DamageEffectManager.instance != null)
        {
            Vector3 position = transform.position;
            position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1f, 1.5f), 0);
            DamageEffectManager.instance.ShowMiss(position);
        }
    }

    private void ShowStatusEffect(string effectName)
    {
        if (DamageEffectManager.instance != null)
        {
            Vector3 position = transform.position;
            position += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(1f, 1.5f), 0);
            DamageEffectManager.instance.ShowStatusEffect(position,effectName);
        }
    }

    private void OnMouseDown()
    {
        float randomValue = Random.value;

        if (randomValue < missChance)
        {
            ShowMiss();
        }
        else if (randomValue < 0.5f)
        {
            bool isCrit = Random.value < criticalChance;
            int damage = Random.Range(minDamage, maxDamage + 1);

            if ((isCrit)) damage *= 2;

            ShowDamage(damage, isCrit);

            if (Random.value < statusEffectChance)
            {
                string statusEffect = statusEffects[Random.Range(0, statusEffects.Length)];
                ShowStatusEffect(statusEffect);
            }
        }

        else
        {
            bool isCrit = Random.value < criticalChance;
            int heal = Random.Range(minHeal, maxHeal + 1);

            if (isCrit) heal = Mathf.RoundToInt(heal * 1.5f);
            ShowHeal(heal, isCrit);
        }
    }
}
