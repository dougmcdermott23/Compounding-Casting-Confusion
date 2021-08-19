using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour, ISpell
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxLifeTime;

    private int damage;
    private SpellController.SpellType spellType;

    private Vector3 moveDirection;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Init(GameController gameController, Vector3 startPosition, Vector3 direction, int damage, SpellController.SpellType spellType)
    {
        this.moveDirection = direction;
        this.damage = damage;
        this.spellType = spellType;
        Destroy(gameObject, maxLifeTime);

        InitializeParticleSystem();
        animator.Play(spellType.ToString());
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void InitializeParticleSystem()
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

        Material material = null;

        foreach (GameAssets.DeathTypeMaterial deathTypeMaterial in GameAssets.Instance.deathTypeMaterialArray)
        {
            if (deathTypeMaterial.deathType == deathType)
            {
                material = deathTypeMaterial.material;
                break;
            }
        }

        ParticleSystem projectileParticles = transform.GetChild(0).GetComponent<ParticleSystem>();
        projectileParticles.GetComponent<Renderer>().material = material;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(GameAssets.ENEMY_TAG))
        {
            EnemyController enemy = collision.GetComponent<EnemyController>();
            enemy.Damage(damage, spellType);
            Destroy(gameObject);
        }
        else if (collision.CompareTag(GameAssets.TERRAIN_TAG))
        {
            InstantiateDeathParticles(spellType, transform.position);
            Destroy(gameObject);
        }
    }
}
