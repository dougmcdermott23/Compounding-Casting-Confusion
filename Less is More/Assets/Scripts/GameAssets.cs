using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameAssets : MonoBehaviour
{
    private static GameAssets instance;

    public static GameAssets Instance
    {
        get
        {
            if (instance == null) instance = (Instantiate(Resources.Load("GameAssets")) as GameObject).GetComponent<GameAssets>();
            return instance;
        }
    }

    // Constants
    public const string PLAYER_TAG = "Player";
    public const string PROJECTILE_TAG = "Projectile";
    public const string ENEMY_TAG = "Enemy";
    public const string TERRAIN_TAG = "Terrain";

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject projectilePrefab;
    public GameObject bombPrefab;
    public GameObject areaOfEffectPrefab;
    public GameObject enemyPrefab;
    public GameObject aoeSpritePrefab;
    public GameObject deathParticlesPrefab;

    [Header("Enemy Sprites")]
    public Sprite enemyHitSprite;
    public EnemyType[] enemyTypeArray;

    [System.Serializable]
    public class EnemyType
    {
        public SpellController.SpellType spellType;
        public Sprite sprite;
    }

    [Header("Other Sprites")]
    public SpellTypeSprite[] spellTypeSpriteArray;

    [System.Serializable]
    public class SpellTypeSprite
    {
        public SpellController.SpellType spellType;
        public Sprite sprite;
    }

    [Header("Materials")]
    public DeathTypeMaterial[] deathTypeMaterialArray;

    [System.Serializable]
    public class DeathTypeMaterial
    {
        public DeathParticlesController.DeathType deathType;
        public Material material;
    }

    [Header("Sound")]
    public SoundAudioClip[] soundAudioClipArray;

    [System.Serializable]
    public class SoundAudioClip
    {
        public SoundController.Sound sound;
        public AudioClip audioClip;
    }

    private Transform terrainColliderTransform;
    private BoxCollider2D[] terrainColliderArray;
    public BoxCollider2D[] TerrainColliderArray
    {
        get
        {
            if (terrainColliderTransform == null)
                terrainColliderTransform = GameObject.Find("TerrainColliders").transform;

            if (terrainColliderArray == null)
            {
                List<BoxCollider2D> colliderList = new List<BoxCollider2D>();
                foreach (Transform child in terrainColliderTransform)
                {
                    colliderList.Add(child.GetComponent<BoxCollider2D>());
                }
                terrainColliderArray = colliderList.ToArray();
            }
            return terrainColliderArray;
        }
    }
}
