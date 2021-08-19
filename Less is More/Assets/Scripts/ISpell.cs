using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpell
{
    void Init(GameController gameController, Vector3 startPosition, Vector3 direction, int damage, SpellController.SpellType spellType);
}
