using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioSource ambientSource;
    public AudioSource sfxSource;

    public AudioClip sfxConstructionClip;

    private void Awake()
    {
        instance = this;
    }
    public void PlaySfx(float volume = 0.5f) {
        sfxSource.PlayOneShot(sfxConstructionClip,volume);
    }
}
