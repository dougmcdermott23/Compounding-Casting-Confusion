using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SoundController
{
    public enum Sound
    {
        BombExplosion,
        EnemyExplosion,
        EnemyHit,
        EnemyJump,
        FailedSpell,
        GameOver,
        PlayerHit,
        ShootProjectile,
        UpgradeSpell,
    }

    private static GameObject oneShotGameObject;
    private static AudioSource oneShotAudioSource;
    private static bool soundEnabled;

    public static void Initialize()
    {
        soundEnabled = true;
    }

    public static void PlaySound(Sound sound)
    {
        if (soundEnabled)
        {
            if (oneShotGameObject == null)
            {
                oneShotGameObject = new GameObject("Sound");
                oneShotAudioSource = oneShotGameObject.AddComponent<AudioSource>();
            }
            oneShotAudioSource.PlayOneShot(GetAudioClip(sound));
        }
    }

    public static void SetSoundEnabled(bool enabled)
    {
        soundEnabled = enabled;
    }

    private static AudioClip GetAudioClip(Sound sound)
    {
        foreach (GameAssets.SoundAudioClip soundAudioClip in GameAssets.Instance.soundAudioClipArray)
        {
            if (soundAudioClip.sound == sound)
            {
                return soundAudioClip.audioClip;
            }
        }
        return null;
    }
}
