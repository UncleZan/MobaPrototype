using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Character
{
    int Level { get; set; }
    public int EXP { get; set; }
    public int SoulValue { get; set; }
    public float Speed { get; set; }
    public float AttackSpeed { get; set; }
    /// <summary>
    /// The attack range of the unit.
    /// Determines the distance from which the unit is able to perform an attack.
    /// </summary>
    public float AttackRange { get; set; }

    /// <summary>
    /// The class of the NPC. Skills and stats depends on this field.
    /// </summary>
    public NPCType type;

    

    public NPC(NPCType type, int stageLvl)
    {
        Level = stageLvl;
        this.type = type;
        switch (type)
        {
            case NPCType.MELEE_UNIT:
                health = 6 * Level;
                attack = 1 + Level;
                Speed = 0.8f + 0.1f + (Level * 0.1f);
                AttackRange = 0.01f;
                AttackSpeed = 2 + (Level * 0.1f);
                break;
            case NPCType.RANGED_UNIT:
                health = 4 * Level;
                attack = 1 + (int)Level / 2;
                Speed = 0.5f + 0.1f + (Level * 0.1f);
                AttackRange = 3.55f + (Level * 0.1f);
                AttackSpeed = 1 + (Level * 0.1f);
                break;
            default:
                break;
        }
        currentHealth = health;
        CalculaterReward();
    }

    public virtual void CalculaterReward()
    {
        EXP = (Level - 1) * 2 + 1;
        SoulValue = Level / 2 + 1;
    }
}

public enum NPCType
{
    MELEE_UNIT,
    RANGED_UNIT,
    MAGIC_UNIT
}