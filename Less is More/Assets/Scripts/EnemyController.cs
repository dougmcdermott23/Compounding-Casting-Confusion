using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public event EventHandler<OnEnemyDeadEventArgs> OnEnemyDead;

    public class OnEnemyDeadEventArgs : EventArgs
    {
        public bool killed;
        public SpellController.SpellType spellType;
        public Vector3 position;
    };

    public static List<EnemyController> enemyList = new List<EnemyController>();

    public static List<EnemyController> GetEnemiesInRange(Vector3 position, float range)
    {
        List<EnemyController> enemiesInRange = new List<EnemyController>();

        foreach (EnemyController enemy in enemyList)
        {
            if (Vector3.Distance(position, enemy.transform.position) <= range)
            {
                enemiesInRange.Add(enemy);
            }
        }

        return enemiesInRange;
    }

    private enum State
    {
        Moving,
        Waiting,
        Attacking,
    }

    private static Dictionary<SpellController.SpellType, SpellController.SpellType> spellVulnerabilities =  new Dictionary<SpellController.SpellType, SpellController.SpellType>
        {
            { SpellController.SpellType.Fire, SpellController.SpellType.Water },
            { SpellController.SpellType.Water, SpellController.SpellType.Grass },
            { SpellController.SpellType.Grass, SpellController.SpellType.Fire },
        };

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float minMoveSpeed;
    [SerializeField] private float moveSpeedDropMultiplier;
    [SerializeField] private float hopWaitTime;
    private Vector3 moveDirection;
    private float currentMoveSpeed;
    private float waitTimer;

    [Header("Attack")]
    [SerializeField] private float detectionRange;
    [SerializeField] private float damageRange;
    [SerializeField] private float damageChargeTime;
    [SerializeField] private int damage;
    private float damageChargeTimer;
    private bool attackTriggered;

    [Header("Type")]
    [SerializeField] private SpellController.SpellType type;

    [Header("Blast Sprite")]
    [SerializeField] private float blastTime;
    private SpriteRenderer blastSprite;
    Color blastBaseColor;
    private bool isTriggered;
    private float blastTimer;
    private Vector3 maxBlastSize;
    private Vector3 startingBlastSize;

    private State state;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private ParticleSystem moveParticles;
    private PlayerController target;
    private HealthSystem healthSystem;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        state = State.Moving;
        healthSystem = new HealthSystem();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        moveParticles = transform.GetChild(0).GetComponent<ParticleSystem>();

        blastSprite = transform.GetChild(1).GetComponent<SpriteRenderer>();
        blastBaseColor = blastSprite.material.color;
        startingBlastSize = Vector3.one;
        maxBlastSize = Vector3.one * damageRange;
        facingRight = true;
    }

    public void Init(PlayerController target, int maxHealth, SpellController.SpellType type)
    {
        this.target = target;
        healthSystem.SetMaxHealth(maxHealth, true);
        this.type = type;

        moveDirection = (target.transform.position - transform.position).normalized;

        foreach (GameAssets.EnemyType enemyType in GameAssets.Instance.enemyTypeArray)
        {
            if (enemyType.spellType == type)
            {
                spriteRenderer.sprite = enemyType.sprite;
                break;
            }
        }
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

        switch (state)
        {
            case State.Moving:
                currentMoveSpeed -= currentMoveSpeed * moveSpeedDropMultiplier * Time.deltaTime;

                if (currentMoveSpeed <= minMoveSpeed)
                {
                    state = State.Waiting;
                    waitTimer = hopWaitTime;
                    CreateDust(moveParticles);
                }
                break;

            case State.Waiting:
                moveDirection = (target.transform.position - transform.position).normalized;

                if (waitTimer <= 0)
                {
                    state = State.Moving;
                    currentMoveSpeed = moveSpeed;
                    CreateDust(moveParticles);

                    SoundController.PlaySound(SoundController.Sound.EnemyJump);
                }
                else
                {
                    waitTimer -= Time.deltaTime;
                }

                if (CheckDistanceToTarget(detectionRange))
                {
                    state = State.Attacking;
                    damageChargeTimer = damageChargeTime;
                }
                break;

            case State.Attacking:
                if (damageChargeTimer <= 0)
                {
                    isTriggered = true;
                    Attack();
                }
                else
                {
                    damageChargeTimer -= Time.deltaTime;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        if (isTriggered)
            return;

        switch (state)
        {
            case State.Moving:
                rb.velocity = moveDirection * currentMoveSpeed;
                break;

            case State.Waiting:
            case State.Attacking:
                rb.velocity = Vector2.zero;
                break;
        }

        SetAnimationParameters();
    }

    private bool CheckDistanceToTarget(float range)
    {
        bool inRange = false;

        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance <= range)
        {
            inRange = true;
        }

        return inRange;
    }

    private void Attack()
    {
        if (CheckDistanceToTarget(damageRange))
            target.Damage(damage, transform.position);

        OnEnemyDead?.Invoke(this, new OnEnemyDeadEventArgs { killed = false, spellType = type, position = transform.position });
        enemyList.Remove(this);

        GetComponent<SpriteRenderer>().enabled = false;
        blastSprite.gameObject.SetActive(true);
        blastTimer = blastTime;

        SoundController.PlaySound(SoundController.Sound.EnemyExplosion);
    }

    public void Damage(int damage, SpellController.SpellType spellType)
    {
        if (spellVulnerabilities[type] == spellType)
        {
            healthSystem.Damage(damage);
            StartCoroutine(EnemyHitAnimation());
            SoundController.PlaySound(SoundController.Sound.EnemyHit);
        }
        
        if (healthSystem.IsDead())
        {
            OnEnemyDead?.Invoke(this, new OnEnemyDeadEventArgs { killed = true, spellType = type, position = transform.position });
            enemyList.Remove(this);
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRange);
    }

    ////////////////////////////////////////////////
    /// Enemy Animation
    ////////////////////////////////////////////////

    private const string ENEMY_MOVING = "isMoving";
    private const string ENEMY_ATTACK = "attack";

    [Header("Animation")]
    [SerializeField] private float hitAnimationTimer;
    private bool facingRight;

    private void SetAnimationParameters()
    {
        animator.SetBool(ENEMY_MOVING, state == State.Moving);
        if (state == State.Attacking && !attackTriggered)
        {
            animator.SetTrigger(ENEMY_ATTACK);
            attackTriggered = true;
        }

        if ((moveDirection.x > 0 && !facingRight) || (moveDirection.x < 0 && facingRight))
            Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = !facingRight;
    }

    private IEnumerator EnemyHitAnimation()
    {
        Sprite sprite = spriteRenderer.sprite;
        spriteRenderer.sprite = GameAssets.Instance.enemyHitSprite;

        yield return new WaitForSeconds(hitAnimationTimer);

        spriteRenderer.sprite = sprite;
    }

    private void CreateDust(ParticleSystem dust)
    {
        dust.Play();
    }
}
