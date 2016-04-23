using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    private static AudioSource audioSource;
    private static AudioManager reference;  //singleton

    public GameObject oneShotPrefab;


    void Awake()
    {
        reference = this;
        audioSource = gameObject.GetComponent<AudioSource>();
        if (!audioSource)
            Debug.LogWarning("Audio Manager has no Audio Source component attached!");
    }

    public static void Play(AudioClip clip)
    {
        if (clip == null || audioSource.clip == clip) return;
        audioSource.clip = clip;
        audioSource.Play();
    }

    public static void Play(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;
        GameObject audioObj = PoolManager.Pools["Audio"].Spawn(reference.oneShotPrefab, position, Quaternion.identity);
        AudioSource source = audioObj.GetComponent<AudioSource>();
        source.clip = clip;
        source.Play();
        PoolManager.Pools["Audio"].Despawn(audioObj, clip.length);
    }

    public static void Play(AudioClip clip, Vector3 position, float pitch)
    {
        if (clip == null) return;
        GameObject audioObj = PoolManager.Pools["Audio"].Spawn(reference.oneShotPrefab, position, Quaternion.identity);
        AudioSource source = audioObj.GetComponent<AudioSource>();
        source.clip = clip;
        source.pitch = pitch;
        source.Play();
        PoolManager.Pools["Audio"].Despawn(audioObj, clip.length);
    }

    public static void Play2D(AudioClip clip)
    {
        if (clip == null) return;
        reference.GetComponent<AudioSource>().PlayOneShot(clip);
    }

    public static void Pause()
    {
        audioSource.Pause();
    }
    
    
    public static void Continue()
    {
    	audioSource.Play();
    }

    public static IEnumerator PauseForSeconds(float seconds)
    {
        audioSource.Pause();
        yield return new WaitForSeconds(seconds);
        audioSource.Play();
    }

    public static void Stop()
    {
        reference.StopAllCoroutines();
        audioSource.Stop();
    }
}

[System.Serializable]
public class AudioClips
{
    public string poolName = "";
    public AudioClip[] clips;
}
