using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour, ISpell
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float minMoveSpeed;
    [SerializeField] private float moveSpeedDropMultiplier;
    [SerializeField] private float timeToDetonate;
    [SerializeField] private float damageRange;
    private int damage;
    private SpellController.SpellType spellType;
    private float detonationTimer;
    private Vector3 moveDirection;
    private bool hitWall;

    [Header("Blast")]
    [SerializeField] private float blastTime;
    [SerializeField] private float cameraShakeDurationExplosion;
    [SerializeField] private float cameraShakeMagnitudeExplosion;
    private SpriteRenderer blastSprite;
    Color blastBaseColor;
    private bool isTriggered;
    private float blastTimer;
    private Vector3 maxBlastSize;
    private Vector3 startingBlastSize;
    private Animator animator;

    private GameController gameController;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        blastSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        blastBaseColor = blastSprite.material.color;
        startingBlastSize = Vector3.one;
        maxBlastSize = Vector3.one * damageRange * 2f;
    }

    public void Init(GameController gameController, Vector3 startPosition, Vector3 direction, int damage, SpellController.SpellType spellType)
    {
        this.moveDirection = direction;
        this.damage = damage;
        this.spellType = spellType;

        detonationTimer = timeToDetonate;

        animator.Play(spellType.ToString());

        this.gameController = gameController;
    }

    private void Update()
    {
        if (isTriggered)
        {
            if (blastTimer <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                float t = Mathf.InverseLerp(0, blastTime, blastTime - blastTimer);

                Vector3 blastSize = Vector3.Lerp(Vector3.one, maxBlastSize, t);
                blastSprite.transform.localScale = blastSize;

                Color blastUpdateColor = Color.Lerp(blastBaseColor, Color.clear, t);
                blastSprite.material.color = blastUpdateColor;

                blastTimer -= Time.deltaTime;
            }

            return;
        }

        if (detonationTimer <= 0)
        {
            DetonateBomb();
            isTriggered = true;
        }
        else
        {
            moveSpeed -= moveSpeed * moveSpeedDropMultiplier * Time.deltaTime;
            if (moveSpeed < minMoveSpeed)
                moveSpeed = 0f;

            if (!hitWall)
                transform.position += moveDirection * moveSpeed * Time.deltaTime;
            detonationTimer -= Time.deltaTime;
        }
    }

    private void DetonateBomb()
    {
        List<EnemyController> enemies = EnemyController.GetEnemiesInRange(transform.position, damageRange);

        foreach (EnemyController enemy in enemies)
        {
            enemy.Damage(damage, spellType);
        }

        GetComponent<SpriteRenderer>().enabled = false;
        blastSprite.gameObject.SetActive(true);
        blastTimer = blastTime;

        SoundController.PlaySound(SoundController.Sound.BombExplosion);
        StartCoroutine(gameController.CameraShake.Shake(cameraShakeDurationExplosion, cameraShakeMagnitudeExplosion));
        InstantiateDeathParticles();
    }

    private void InstantiateDeathParticles()
    {
        DeathParticlesController.DeathType deathType = DeathParticlesController.DeathType.Fire;

        switch (spellType)
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

        DeathParticlesController deathParticles = Instantiate(GameAssets.Instance.deathParticlesPrefab, transform.position, Quaternion.identity).GetComponent<DeathParticlesController>();
        deathParticles.Init(deathType);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(GameAssets.TERRAIN_TAG))
        {
            hitWall = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, damageRange);
    }
}
