using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathParticlesController : MonoBehaviour
{
    public enum DeathType
    {
        Player,
        Fire,
        Water,
        Grass,
    };

    [SerializeField] private float destroyTime = 1f;

    private ParticleSystem deathParticles;

    private void Awake()
    {
        deathParticles = GetComponent<ParticleSystem>();
    }

    public void Init(DeathType deathType)
    {
        Material material = null;

        foreach (GameAssets.DeathTypeMaterial deathTypeMaterial in GameAssets.Instance.deathTypeMaterialArray)
        {
            if (deathTypeMaterial.deathType == deathType)
            {
                material = deathTypeMaterial.material;
                break;
            }
        }

        deathParticles.GetComponent<Renderer>().material = material;
        Destroy(gameObject, destroyTime);
    }
}
