using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Data",menuName = "Character States/Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("States Info")]
    public int maxHealth;
    public int currentHealth;
    public int baseDefence;
    public int currentDefence;

    [Header("Kill")]
    public int killPoint;

    [Header("Level")]
    public int currentLevel;
    public int maxLevel;
    public int baseExp;//升级所需经验
    public int currentExp;
    public float levelBuff;
    
    public float LevelMultiplier
    {
        get { return 1 + (currentLevel - 1) * levelBuff; }
    }
    
    public void UpdateExp(int point)
    {
        currentExp += point;

        if (currentExp >= baseExp)
            LevelUp();
    }

    private void LevelUp()
    {
        //所有你想提升的属性
        currentLevel = Mathf.Clamp(currentLevel + 1,0,maxLevel);//限制最大值和最小值
        baseExp += (int)(baseExp * LevelMultiplier);//升级所需经验

        maxHealth = (int)(maxHealth * LevelMultiplier);
        currentHealth = maxHealth;
        Debug.Log("Level Up！" + currentLevel + "Max Health:" + maxHealth);

    }
}
