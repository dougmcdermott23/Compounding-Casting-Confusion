using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    private delegate void Function();
    private const string maxLevelSpellString = "Spell at max level!";

    private bool isEndOfWave;
    private bool isGameOver;
    private bool canStartLevel = true;
    private float waitTimeAfterWave = 1f;

    private int levelCounter;

    [Header("Player")]
    [SerializeField] Vector3 playerSpawnPosition;
    private PlayerController playerController;

    [Header("UI")]
    [SerializeField] private GameObject startText;
    [SerializeField] private TextMeshProUGUI gameOverTextUI;
    [SerializeField] private TextMeshProUGUI nextLevelTextUI;
    [SerializeField] private TextMeshProUGUI levelTextUI;
    [SerializeField] private TextMeshProUGUI healthTextUI;
    [SerializeField] private TextMeshProUGUI spellBufferTextUI;
    [SerializeField] private TextMeshProUGUI fireSpellTextUI;
    [SerializeField] private TextMeshProUGUI waterSpellTextUI;
    [SerializeField] private TextMeshProUGUI grassSpellTextUI;
    [SerializeField] private TextMeshProUGUI fireUpgradeTextUI;
    [SerializeField] private TextMeshProUGUI waterUpgradeTextUI;
    [SerializeField] private TextMeshProUGUI grassUpgradeTextUI;
    [SerializeField] private GameObject gameUIGameObject;
    private UpgradeMenu upgradeMenu;

    [Header("Camera")]
    [SerializeField] private float cameraShakeDurationGameOver;
    [SerializeField] private float cameraShakeMagnitudeGameOver;
private CameraController cameraController;
    public CameraShake CameraShake { get; set; }

    private EnemySpawner enemySpawner;

    private void Start()
    {
        levelCounter = 0;
        levelTextUI.SetText("Level: 1");

        playerController = Instantiate(GameAssets.Instance.playerPrefab, playerSpawnPosition, Quaternion.identity).GetComponent<PlayerController>();
        playerController.OnPlayerHealthUpdate += PlayerController_OnPlayerHealthUpdate;
        playerController.Init(this);

        enemySpawner = GetComponent<EnemySpawner>();
        enemySpawner.Init(this, playerController);
        enemySpawner.OnEndOfWave += EnemySpawner_OnEndOfWave;

        upgradeMenu = GetComponent<UpgradeMenu>();

        cameraController = Camera.main.GetComponent<CameraController>();
        CameraShake = Camera.main.GetComponent<CameraShake>();
        cameraController.Init(this, playerController);

        SoundController.Initialize();
    }

    private void Update()
    {
        if (UpgradeMenu.IsPaused)
            return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (isGameOver)
            {
                EnemyController.enemyList.Clear();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else if (canStartLevel)
            {
                if (enemySpawner.CanSpawnWave())
                {
                    startText.SetActive(false);

                    levelCounter++;
                    levelTextUI.SetText(string.Format("Level: {0}", levelCounter));

                    nextLevelTextUI.gameObject.SetActive(false);
                    canStartLevel = false;
                }

                enemySpawner.SpawnWave();
            }
        }

        if (isEndOfWave && !isGameOver && enemySpawner.CanSpawnWave())
        {
            canStartLevel = false;

            StartCoroutine(ExecuteAfterTime(waitTimeAfterWave, upgradeMenu.PauseUnpause));
            StartCoroutine(ExecuteAfterTime(waitTimeAfterWave, () => { gameUIGameObject.SetActive(false); }));

            isEndOfWave = false;
        }
    }

    private void PlayerController_OnPlayerHealthUpdate(object sender, int playerHealth)
    {
        healthTextUI.SetText(playerHealth.ToString());

        if (playerHealth <= 0)
        {
            DeathParticlesController deathParticles = Instantiate(GameAssets.Instance.deathParticlesPrefab, playerController.transform.position, Quaternion.identity).GetComponent<DeathParticlesController>();
            deathParticles.Init(DeathParticlesController.DeathType.Player);

            gameOverTextUI.gameObject.SetActive(true);
            nextLevelTextUI.SetText("Press ENTER to play again");
            nextLevelTextUI.gameObject.SetActive(true);

            cameraController.enabled = false;
            playerController.gameObject.SetActive(false);
            isGameOver = true;

            StartCoroutine(CameraShake.Shake(cameraShakeDurationGameOver, cameraShakeMagnitudeGameOver));
            SoundController.PlaySound(SoundController.Sound.GameOver);
        }
    }

    private void EnemySpawner_OnEndOfWave(object sender, System.EventArgs e)
    {
        Debug.Log("END OF WAVE");
        isEndOfWave = true;
    }

    public void SpellController_OnSpellBufferUpdate(object sender, List<KeyCode> spellBuffer)
    {
        string spellBufferText = "";

        foreach (KeyCode spellCommand in spellBuffer)
            spellBufferText += spellCommand.ToString();

        spellBufferTextUI.SetText(spellBufferText);
    }

    public void SpellController_OnSpellUpdate(object sender, SpellController.OnSpellUpdateEventArgs spell)
    {
        switch (spell.spellType)
        {
            case SpellController.SpellType.Fire:
                fireSpellTextUI.SetText(spell.spellString);
                break;
            case SpellController.SpellType.Water:
                waterSpellTextUI.SetText(spell.spellString);
                break;
            case SpellController.SpellType.Grass:
                grassSpellTextUI.SetText(spell.spellString);
                break;
        }
    }

    public void SpellController_OnSpellCast(object sender, SpellController.OnSpellCastArgs e)
    {
        if (e.spellCast)
        {
            Debug.Log(string.Format("Cast {0}", e.spellType.ToString()));
        }
    }

    public void UpgradeSpell(string spellTypeString)
    {
        bool upgradeSuccess = true;

        SpellController.SpellType spellType;
        Enum.TryParse(spellTypeString, out spellType);

        if (spellTypeString == spellType.ToString())
        {
            upgradeSuccess = playerController.UpgradeSpell(spellType);

            if (!playerController.CanUpgradeSpell(spellType))
            {
                switch (spellType)
                {
                    case SpellController.SpellType.Fire:
                        fireUpgradeTextUI.text = maxLevelSpellString;
                        break;
                    case SpellController.SpellType.Water:
                        waterUpgradeTextUI.text = maxLevelSpellString;
                        break;
                    case SpellController.SpellType.Grass:
                        grassUpgradeTextUI.text = maxLevelSpellString;
                        break;
                }
            }
        }

        if (upgradeSuccess)
        {
            upgradeMenu.PauseUnpause();
            gameUIGameObject.SetActive(true);
            nextLevelTextUI.gameObject.SetActive(true);

            canStartLevel = true;

            SoundController.PlaySound(SoundController.Sound.UpgradeSpell);
        }
    }

    private IEnumerator ExecuteAfterTime(float time, Action func)
    {
        yield return new WaitForSeconds(time);
        func();
    }

    public static float GetAngleFromVector(Vector3 direction)
    {
        direction = direction.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;
        return angle;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        float size = 0.3f;

        Gizmos.DrawLine(playerSpawnPosition - Vector3.up * size, playerSpawnPosition + Vector3.up * size);
        Gizmos.DrawLine(playerSpawnPosition - Vector3.left * size, playerSpawnPosition + Vector3.left * size);
    }
}
