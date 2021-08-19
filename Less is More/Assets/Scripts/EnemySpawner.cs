using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public event EventHandler OnEndOfWave;

    public enum State
    {
        Waiting,
        InProgress,
    };

    [SerializeField] private Vector3[] spawnLocations;
    [SerializeField] private int enemiesInWave;
    [SerializeField] private float enemiesPerWaveModifier;
    [SerializeField] private float timeBetweenSpawns;

    [SerializeField] private float cameraShakeDurationEnemy;
    [SerializeField] private float cameraShakeMagnitudeEnemy;


    private GameController gameController;
    private PlayerController playerController;

    private State state;
    private int enemiesSpawned;

    private void Start()
    {
        state = State.Waiting;
    }

    public void Init(GameController gameController, PlayerController playerController)
    {
        this.gameController = gameController;
        this.playerController = playerController;
    }

    public bool CanSpawnWave()
    {
        return (state == State.Waiting && EnemyController.enemyList.Count <= 0);
    }

    public void SpawnWave()
    {
        if (!CanSpawnWave())
        {
            Debug.Log("WAVE STILL IN PROGRESS");
            return;
        }

        state = State.InProgress;
        StartCoroutine(SpawnEnemies());
    }

    private void SpawnEnemy(Vector3 spawnPosition, int maxHealth, SpellController.SpellType type)
    {
        EnemyController enemy = Instantiate(GameAssets.Instance.enemyPrefab, spawnPosition, Quaternion.identity).GetComponent<EnemyController>();
        enemy.Init(playerController, maxHealth, type);
        enemy.OnEnemyDead += Enemy_OnEnemyDead;
        EnemyController.enemyList.Add(enemy);
    }

    private void Enemy_OnEnemyDead(object sender, EnemyController.OnEnemyDeadEventArgs enemy)
    {
        InstantiateDeathParticles(enemy.spellType, enemy.position);

        if (!enemy.killed)
        {
            StartCoroutine(gameController.CameraShake.Shake(cameraShakeDurationEnemy, cameraShakeMagnitudeEnemy));
        }
    }

    private Vector3 RandomizeSpawnLocation()
    {
        return spawnLocations[UnityEngine.Random.Range(0, spawnLocations.Length)];
    }

    private SpellController.SpellType RandomizeEnemyType()
    {
        Array types = Enum.GetValues(typeof(SpellController.SpellType));
        System.Random random = new System.Random();
        return (SpellController.SpellType)types.GetValue(random.Next(types.Length));
    }

    private void InstantiateDeathParticles(SpellController.SpellType type, Vector3 position)
    {
        DeathParticlesController.DeathType deathType = DeathParticlesController.DeathType.Fire;

        switch (type)
        {
            case SpellController.SpellType.Fire:
                deathType = DeathParticlesController.DeathType.Fire;
                break;

            case SpellController.SpellType.Water:
                deathType = DeathParticlesController.DeathType.Water;
                break;

            case SpellController.SpellType.Grass:
                deathType = DeathParticlesController.DeathType.Grass;
                break;
        }

        DeathParticlesController deathParticles = Instantiate(GameAssets.Instance.deathParticlesPrefab, position, Quaternion.identity).GetComponent<DeathParticlesController>();
        deathParticles.Init(deathType);
    }

    private IEnumerator SpawnEnemies()
    {
        if (enemiesSpawned < enemiesInWave)
        {
            Vector3 spawnPosition = RandomizeSpawnLocation();
            int maxHealth = 2;
            SpellController.SpellType enemyType = RandomizeEnemyType();

            SpawnEnemy(spawnPosition, maxHealth, enemyType);
            enemiesSpawned++;

            yield return new WaitForSeconds(timeBetweenSpawns);

            StartCoroutine(SpawnEnemies());
        }
        else
        {
            state = State.Waiting;
            enemiesSpawned = 0;
            enemiesInWave = Mathf.FloorToInt((float)enemiesInWave * enemiesPerWaveModifier);
            OnEndOfWave?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnDrawGizmos()
    {
        if (spawnLocations != null)
        {
            Gizmos.color = Color.green;
            float size = 0.3f;

            for (int i = 0; i < spawnLocations.Length; i++)
            {
                Vector3 pos = spawnLocations[i] + transform.position;
                Gizmos.DrawLine(pos - Vector3.up * size, pos + Vector3.up * size);
                Gizmos.DrawLine(pos - Vector3.left * size, pos + Vector3.left * size);
            }
        }
    }
}
