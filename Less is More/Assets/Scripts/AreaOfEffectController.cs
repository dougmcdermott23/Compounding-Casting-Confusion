using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaOfEffectController : MonoBehaviour, ISpell
{
    private struct AreaOfEffect
    {
        public float width;
        public float height;
        public float angle;

        public Vector3 center;
        public Vector3 v1, v2;
        public Vector3 topLeft, topRight, botLeft, botRight;

        public AreaOfEffect(float width, float height, float angle, Vector3 center)
        {
            this.width = width;
            this.height = height;
            this.angle = angle;
            this.center = center;

            v1 = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * height / 2f;
            v2 = new Vector2(-v1.y, v1.x).normalized * width / 2f;

            topRight = center + v1 - v2;
            topLeft = center + v1 + v2;
            botRight = center - v1 - v2;
            botLeft = center - v1 + v2;
        }

        // https://math.stackexchange.com/questions/190111/how-to-check-if-a-point-is-inside-a-rectangle/190373#190373
        public bool Contains(Vector3 point)
        {
            Vector2 AM = point - topLeft;
            Vector2 AB = topRight - topLeft;
            Vector2 AD = botLeft - topLeft;

            return ((Vector2.Dot(AM, AB) > 0 && Vector2.Dot(AB, AB) > Vector2.Dot(AM, AB)) && (Vector2.Dot(AM, AD) > 0 && Vector2.Dot(AD, AD) > Vector2.Dot(AM, AD)));
        }
    }

    [SerializeField] private float maxAliveTime;
    [SerializeField] private float damageTickTime;
    [SerializeField] private float distanceToCenter;
    [SerializeField] private float width;
    [SerializeField] private float height;

    private int damage;
    private SpellController.SpellType spellType;
    private float aliveTimer;
    private float damageTimer;
    AreaOfEffect areaOfEffect;

    private GameObject[] areaOfEffectSprites;

    private void Awake()
    {

    }

    public void Init(GameController gameController, Vector3 startPosition, Vector3 direction, int damage, SpellController.SpellType spellType)
    {
        this.damage = damage;
        this.spellType = spellType;

        aliveTimer = maxAliveTime;
        damageTimer = 0;

        CalculateArea(startPosition, direction);

        InitializeSpriteArray();
    }

    private void InitializeSpriteArray()
    {
        Sprite sprite = null;

        for (int i = 0; i < GameAssets.Instance.spellTypeSpriteArray.Length; i++)
        {
            if (GameAssets.Instance.spellTypeSpriteArray[i].spellType == spellType)
            {
                sprite = GameAssets.Instance.spellTypeSpriteArray[i].sprite;
                break;
            }
        }

        int numSprites = (int)(width * height);
        areaOfEffectSprites = new GameObject[numSprites];
        for (int i = numSprites - 1; i >= 0; i--)
        {
            int col = i % (int)width;
            int row = i / (int)width;

            Vector3 position = areaOfEffect.botRight + areaOfEffect.v2.normalized * (col + 0.5f) + areaOfEffect.v1.normalized * (row + 0.5f);

            if (InsideTerrain(position))
                continue;

            areaOfEffectSprites[i] = Instantiate(GameAssets.Instance.aoeSpritePrefab, position, Quaternion.identity);

            SpriteRenderer instanceSpriteRenderer = areaOfEffectSprites[i].GetComponent<SpriteRenderer>();
            instanceSpriteRenderer.sprite = sprite;
            instanceSpriteRenderer.sortingLayerName = "Spells";

            ObjectHover objectHover = areaOfEffectSprites[i].GetComponent<ObjectHover>();
            objectHover.Init((int)width - row);
        }
    }

    private bool InsideTerrain(Vector3 position)
    {
        foreach (BoxCollider2D collider in GameAssets.Instance.TerrainColliderArray)
        {
            Debug.Log(collider.bounds.extents);

            if (collider.bounds.Contains(position))
                return true;
        }

        return false;
    }

    private void Update()
    {
        if (aliveTimer <= 0)
        {
            for (int i = 0; i < areaOfEffectSprites.Length; i++)
                Destroy(areaOfEffectSprites[i]);

            Destroy(gameObject);
        }
        else
        {
            DealDamage();
            aliveTimer -= Time.deltaTime;
        }
    }

    private void DealDamage()
    {
        if (damageTimer <= 0)
        {
            for (int i = EnemyController.enemyList.Count - 1; i >= 0; i--)
            {
                if (areaOfEffect.Contains(EnemyController.enemyList[i].transform.position))
                    EnemyController.enemyList[i].Damage(damage, spellType);
            }

            damageTimer = damageTickTime;
        }
        else
        {
            damageTimer -= Time.deltaTime;
        }
    }

    private void CalculateArea(Vector3 startPosition, Vector3 direction)
    {
        Vector3 center = startPosition + direction.normalized * distanceToCenter;

        float angle = GameController.GetAngleFromVector(direction);
        areaOfEffect = new AreaOfEffect(width, height, angle, center);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(areaOfEffect.topLeft, areaOfEffect.topRight);
        Gizmos.DrawLine(areaOfEffect.topRight, areaOfEffect.botRight);
        Gizmos.DrawLine(areaOfEffect.botRight, areaOfEffect.botLeft);
        Gizmos.DrawLine(areaOfEffect.botLeft, areaOfEffect.topLeft);
    }
}
