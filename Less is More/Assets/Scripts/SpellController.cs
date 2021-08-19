using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public class SpellController : MonoBehaviour
{
    public event EventHandler<List<KeyCode>> OnSpellBufferUpdate;
    public event EventHandler<OnSpellUpdateEventArgs> OnSpellUpdate;
    public event EventHandler<OnSpellCastArgs> OnSpellCast;

    public class OnSpellCastArgs : EventArgs
    {
        public SpellType spellType;
        public bool spellCast;
    }

    public class OnSpellUpdateEventArgs : EventArgs
    {
        public SpellType spellType;
        public string spellString;
    }

    public enum SpellType
    {
        Fire,
        Water,
        Grass,
    }

    private List<KeyCode> spellBuffer;
    private Dictionary<string, Spell> spellList;
    private Dictionary<int, GameObject> spellLevelDictionary;
    private Transform shootPosition;
    private int maxSpellBufferSize = 3;

    private GameController gameController;

    private void Awake()
    {
        shootPosition = transform.GetChild(0);

        spellLevelDictionary = new Dictionary<int, GameObject>
        {
            { 1, GameAssets.Instance.projectilePrefab},
            { 2, GameAssets.Instance.bombPrefab },
            { 3, GameAssets.Instance.areaOfEffectPrefab },
        };
    }

    public void Init(GameController gameController)
    {
        spellBuffer = new List<KeyCode>();
        spellList = new Dictionary<string, Spell>();

        // Initiate the spell combo list
        UpgradeSpell(SpellType.Fire);
        UpgradeSpell(SpellType.Water);
        UpgradeSpell(SpellType.Grass);

        this.gameController = gameController;
    }

    public void AppendSpellBuffer(KeyCode spellCommand)
    {
        spellBuffer.Add(spellCommand);

        if (spellBuffer.Count > maxSpellBufferSize)
            spellBuffer.RemoveAt(0);

        OnSpellBufferUpdate?.Invoke(this, spellBuffer);
    }

    public void CheckSpellBuffer(Vector3 shootDirection)
    {
        string spellCommandString = "";

        foreach (KeyCode spellCommand in spellBuffer)
            spellCommandString += spellCommand.ToString();

        bool success = false;
        if (spellList.ContainsKey(spellCommandString))
        {
            CastSpell(shootDirection, spellList[spellCommandString]);
            success = true;

            SoundController.PlaySound(SoundController.Sound.ShootProjectile);
        }
        else
        {
            SoundController.PlaySound(SoundController.Sound.FailedSpell);
        }

        spellBuffer.Clear();
        OnSpellBufferUpdate?.Invoke(this, spellBuffer);
        OnSpellCast?.Invoke(this, new OnSpellCastArgs { spellType = success ? spellList[spellCommandString].SpellType : default(SpellType) , spellCast = success });
    }

    private void CastSpell(Vector3 shootDirection, Spell spell)
    {
        ISpell instance = Instantiate(spellLevelDictionary[spell.Level], shootPosition.position, Quaternion.identity).GetComponent<ISpell>();
        instance.Init(gameController, shootPosition.position, shootDirection, spell.Damage, spell.SpellType);
    }

    public bool UpgradeSpell(SpellType upgradeSpellType)
    {
        int spellComplexity = CheckCurrentSpellComplexity(upgradeSpellType);
        if (spellComplexity >= maxSpellBufferSize)
            return false;

        string newSpellCommand = GenerateSpell(upgradeSpellType, spellComplexity + 1);
        AddSpellToList(newSpellCommand, new Spell(upgradeSpellType, newSpellCommand.Length));

        OnSpellUpdate?.Invoke(this, new OnSpellUpdateEventArgs { spellType = upgradeSpellType, spellString = newSpellCommand });

        return true;
    }

    public bool CanUpgradeSpell(SpellType upgradeSpellType)
    {
        int spellComplexity = CheckCurrentSpellComplexity(upgradeSpellType);
        if (spellComplexity >= maxSpellBufferSize)
            return false;
        else
            return true;
    }

    private int CheckCurrentSpellComplexity(SpellType spellType)
    {
        int spellComplexity = 0;

        KeyValuePair<string, Spell> spellCommand = GetSpellListEntryBySpellType(spellType);
        if (!spellCommand.Equals(default(KeyValuePair<string, Spell>)))
            spellComplexity = spellCommand.Value.Level;

        return spellComplexity;
    }

    private string GenerateSpell(SpellType spellType, int spellComplexity)
    {
        string spellCombo = "";
        bool spellGenerated = false;

        // Generate a new, unique spell combination string
        while (!spellGenerated)
        {
            spellCombo = RandomizeSpellCombo(spellComplexity);
            spellGenerated = true;

            foreach (KeyValuePair<string, Spell> spellCommand in spellList)
            {
                if (spellCombo == spellCommand.Key)
                {
                    spellGenerated = false;
                    break;
                }
            }
        }

        return spellCombo;
    }

    private void AddSpellToList(string spellCombo, Spell newSpell)
    {
        int numberOfSpells = 3;

        if (spellList.Count < numberOfSpells)
        {
            spellList.Add(spellCombo, newSpell);
        }
        else
        {
            KeyValuePair<string, Spell> SpellCommand = GetSpellListEntryBySpellType(newSpell.SpellType);
            if (!SpellCommand.Equals(default(KeyValuePair<string, Spell>)))
            {
                spellList.Remove(SpellCommand.Key);
                spellList.Add(spellCombo, newSpell);
            }
        }
    }

    private KeyValuePair<string, Spell> GetSpellListEntryBySpellType(SpellType spellType)
    {
        KeyValuePair<string, Spell> spellListEntry;

        foreach (KeyValuePair<string, Spell> SpellCommand in spellList)
        {
            if (SpellCommand.Value.SpellType == spellType)
            {
                spellListEntry = SpellCommand;
                break;
            }
        }

        return spellListEntry;
    }

    private string RandomizeSpellCombo(int spellComboLength)
    {
        string chars = PlayerController.SPELL_ONE_COMMAND.ToString() + PlayerController.SPELL_TWO_COMMAND.ToString() + PlayerController.SPELL_THREE_COMMAND.ToString();
        return new string(Enumerable.Repeat(chars, spellComboLength).Select(s => s[UnityEngine.Random.Range(0, chars.Length)]).ToArray());
    }
}
