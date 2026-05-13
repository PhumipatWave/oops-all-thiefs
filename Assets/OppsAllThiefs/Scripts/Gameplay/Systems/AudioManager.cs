using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource[] sfxSources;

    public static AudioManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else Destroy(Instance);
    }

    public void PlaySfx(int i)
    {
        sfxSources[i].Stop();
        sfxSources[i].Play();
    }
}
