using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell
{
    public SpellController.SpellType SpellType { get; set; }
    public int Level { get; set; }
    public int Damage { get; set; }

    private Dictionary<int, int> spellLevelToDamage = new Dictionary<int, int>
    {
        { 1, 2 },
        { 2, 1 },
        { 3, 1 },
    };

    public Spell(SpellController.SpellType spellType, int level)
    {
        SpellType = spellType;
        Level = level;
        Damage = spellLevelToDamage[level];
    }
}
