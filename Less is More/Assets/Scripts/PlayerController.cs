using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public event EventHandler<int> OnPlayerHealthUpdate;

    private enum State
    {
        Normal,
        Casting,
        HitStun,
    }

    // Input Key Codes
    public const string HORIZONTAL = "Horizontal";
    public const string VERTICAL = "Vertical";
    public const KeyCode SPELL_ONE_COMMAND = KeyCode.J;
    public const KeyCode SPELL_TWO_COMMAND = KeyCode.K;
    public const KeyCode SPELL_THREE_COMMAND = KeyCode.L;
    public const KeyCode RELEASE_SPELL_COMMAND = KeyCode.Space;

    [Header("Moving")]
    [SerializeField] private float moveSpeed;
    private Vector3 moveDirection;
    private Vector3 lastMoveDirection = Vector3.right;

    [Header("Health")]
    [SerializeField] private int maxHealth;
    private HealthSystem healthSystem;

    [Header("Damaged")]
    [SerializeField] private float knockBackSpeed;
    [SerializeField] private float minKnockBackSpeed;
    [SerializeField] private float knockBackSpeedMultiplier;
    [SerializeField] private float invulnerableTime;
    private float currentKnockBackSpeed;
    private bool isInvulnerable;

    private State state;
    private Rigidbody2D rb;
    private Transform directionIndicator;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Animator directionIndicatorAnimator;
    private ParticleSystem castingParticles;
    private ParticleSystem moveParticles;
    private SpellController spellController;
    private GameController gameController;

    private void Awake()
    {
        spellController = GetComponent<SpellController>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        state = State.Normal;
        healthSystem = new HealthSystem(maxHealth);

        directionIndicator = transform.GetChild(1);
        castingParticles = transform.GetChild(2).GetComponent<ParticleSystem>();
        moveParticles = transform.GetChild(3).GetComponent<ParticleSystem>();

        facingRight = true;

        directionIndicatorAnimator = directionIndicator.GetComponent<Animator>();
        directionIndicatorAnimator.Play(DIRECTION_IDLE);
    }

    public void Init(GameController gameController)
    {
        this.gameController = gameController;
        spellController.OnSpellBufferUpdate += gameController.SpellController_OnSpellBufferUpdate;
        spellController.OnSpellUpdate += gameController.SpellController_OnSpellUpdate;
        spellController.OnSpellCast += gameController.SpellController_OnSpellCast;

        OnPlayerHealthUpdate?.Invoke(this, healthSystem.GetCurrentHealth());

        spellController.Init(gameController);
    }

    private void Update()
    {
        if (UpgradeMenu.IsPaused)
        {
            moveDirection = Vector2.zero;
            return;
        }

        if (Input.GetKeyDown(SPELL_ONE_COMMAND))
        {
            spellController.AppendSpellBuffer(SPELL_ONE_COMMAND);
        }
        if (Input.GetKeyDown(SPELL_TWO_COMMAND))
        {
            spellController.AppendSpellBuffer(SPELL_TWO_COMMAND);
        }
        if (Input.GetKeyDown(SPELL_THREE_COMMAND))
        {
            spellController.AppendSpellBuffer(SPELL_THREE_COMMAND);
        }

        switch (state)
        {
            case State.Normal:
                moveDirection = new Vector2(Input.GetAxisRaw(HORIZONTAL), Input.GetAxisRaw(VERTICAL)).normalized;
                if (moveDirection != Vector3.zero)
                    lastMoveDirection = moveDirection;

                if (Input.GetKeyDown(RELEASE_SPELL_COMMAND))
                {
                    state = State.Casting;
                    directionIndicatorAnimator.Play(DIRECTION_CASTING);
                }
                break;

            case State.Casting:
                moveDirection = new Vector2(Input.GetAxisRaw(HORIZONTAL), Input.GetAxisRaw(VERTICAL)).normalized;
                if (moveDirection != Vector3.zero)
                    lastMoveDirection = moveDirection;

                if (Input.GetKeyUp(RELEASE_SPELL_COMMAND))
                {
                    state = State.Normal;
                    spellController.CheckSpellBuffer(lastMoveDirection);
                    directionIndicatorAnimator.Play(DIRECTION_IDLE);
                }
                break;

            case State.HitStun:
                currentKnockBackSpeed -= currentKnockBackSpeed * knockBackSpeedMultiplier * Time.deltaTime;

                if (currentKnockBackSpeed <= minKnockBackSpeed)
                    state = State.Normal;
                break;
        }

        SetChildAnimations();
    }

    private void FixedUpdate()
    {
        if (UpgradeMenu.IsPaused)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        switch (state)
        {
            case State.Normal:
                rb.velocity = moveDirection * moveSpeed;
                break;

            case State.Casting:
                rb.velocity = Vector2.zero;
                break;

            case State.HitStun:
                rb.velocity = lastMoveDirection * currentKnockBackSpeed;
                break;
        }

        SetAnimationParameters();
    }

    public bool UpgradeSpell(SpellController.SpellType spellType)
    {
        return spellController.UpgradeSpell(spellType);
    }

    public bool CanUpgradeSpell(SpellController.SpellType spellType)
    {
        return spellController.CanUpgradeSpell(spellType);
    }

    public void Damage(int damage, Vector3 damageSourcePosition)
    {
        if (isInvulnerable || healthSystem.IsDead())
            return;

        healthSystem.Damage(damage);

        Vector3 damageDirection = (transform.position - damageSourcePosition).normalized;
        KnockBack(damageDirection);

        SoundController.PlaySound(SoundController.Sound.PlayerHit);

        OnPlayerHealthUpdate?.Invoke(this, healthSystem.GetCurrentHealth());
    }

    private void KnockBack(Vector3 direction)
    {
        if (state == State.Casting)
            spellController.CheckSpellBuffer(lastMoveDirection);

        state = State.HitStun;
        lastMoveDirection = direction;
        currentKnockBackSpeed = knockBackSpeed;
        directionIndicatorAnimator.Play(DIRECTION_IDLE);
        StartCoroutine(SetInvulnerable());
    }

    IEnumerator SetInvulnerable()
    {
        isInvulnerable = true;

        yield return new WaitForSeconds(invulnerableTime);

        isInvulnerable = false;
    }

    ////////////////////////////////////////////////
    /// Player Animation
    ////////////////////////////////////////////////

    private const string PLAYER_MOVING = "isMoving";
    private const string PLAYER_INVULNERABLE = "isInvulnerable";
    private const string DIRECTION_IDLE = "DirectionIdle";
    private const string DIRECTION_CASTING = "DirectionCasting";

    [Header("Animation")]
    [SerializeField] private float animationSpeed;
    [SerializeField] private float rotationAngle;
    private bool facingRight;

    private float dustTime = 0.2f;
    private float dustTimer;

    private void SetAnimationParameters()
    {
        PlayerMoveAnimation();

        animator.SetBool(PLAYER_MOVING, moveDirection != Vector3.zero || state == State.Casting);
        animator.SetBool(PLAYER_INVULNERABLE, isInvulnerable);

        if ((moveDirection.x > 0 && !facingRight) || (moveDirection.x < 0 && facingRight))
            Flip();
    }

    private void SetChildAnimations()
    {
        // Casting Particle System
        if (castingParticles)
        {
            var emmision = castingParticles.emission;
            emmision.enabled = state == State.Casting;
        }

        // Move Particle System
        if (moveParticles && moveDirection != Vector3.zero && state == State.Normal)
        {
            if (dustTimer <= 0)
            {
                CreateDust(moveParticles);
                dustTimer = dustTime;
            }
            else
            {
                dustTimer -= Time.deltaTime;
            }
        }
        else
        {
            dustTimer = 0;
        }

        // Arrow for player direction
        float directionIndicatorOffset = 1.5f;
        Vector3 spriteOffset = new Vector3(0, 0.5f, 0);
        directionIndicator.position = transform.position + lastMoveDirection.normalized * directionIndicatorOffset + spriteOffset;
        float angle = GameController.GetAngleFromVector(lastMoveDirection);
        directionIndicator.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = !facingRight;
    }

    private void PlayerMoveAnimation()
    {
        if (moveDirection == Vector3.zero || state == State.Casting)
        {
            transform.rotation = Quaternion.identity;
            return;
        }

        float rot = Mathf.SmoothStep(-1 * rotationAngle, rotationAngle, Mathf.PingPong(Time.time * animationSpeed, 1));
        transform.rotation = Quaternion.Euler(0, 0, rot);
    }

    private void CreateDust(ParticleSystem dust)
    {
        dust.Play();
    }
}
