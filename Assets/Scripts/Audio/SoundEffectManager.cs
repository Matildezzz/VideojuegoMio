using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour
{
    private const string SfxVolumeKey = "SFX_VOLUME";

    private static SoundEffectManager Instance;

    private static AudioSource audioSource;
    private static AudioSource randomPitchAudioSource;
    private static AudioSource voiceAudioSource;
    private static SoundEffectLibrary soundEffectLibrary;

    [SerializeField] private Slider sfxSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            AudioSource[] audioSources = GetComponents<AudioSource>();

            if (audioSources.Length > 0)
            {
                audioSource = audioSources[0];
            }

            if (audioSources.Length > 1)
            {
                randomPitchAudioSource = audioSources[1];
            }

            if (audioSources.Length > 2)
            {
                voiceAudioSource = audioSources[2];
            }

            soundEffectLibrary = GetComponent<SoundEffectLibrary>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(savedVolume);
            sfxSlider.onValueChanged.AddListener(HandleSliderChanged);
        }

        SetVolume(savedVolume);
    }

    private void OnDestroy()
    {
        if (Instance == this && sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(HandleSliderChanged);
        }
    }

    private void HandleSliderChanged(float value)
    {
        SetVolume(value);

        PlayerPrefs.SetFloat(SfxVolumeKey, value);
        PlayerPrefs.Save();
    }

    public static void Play(string soundName, bool randomPitch = false)
    {
        if (soundEffectLibrary == null)
        {
            return;
        }

        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName);

        if (audioClip == null)
        {
            return;
        }

        if (randomPitch)
        {
            if (randomPitchAudioSource == null)
            {
                return;
            }

            randomPitchAudioSource.pitch = Random.Range(0.9f, 1.1f);
            randomPitchAudioSource.PlayOneShot(audioClip);
        }
        else
        {
            if (audioSource == null)
            {
                return;
            }

            audioSource.PlayOneShot(audioClip);
        }
    }

    public static void PlayVoice(AudioClip audioClip, float pitch = 1f)
    {
        if (voiceAudioSource == null || audioClip == null)
        {
            return;
        }

        voiceAudioSource.pitch = pitch;
        voiceAudioSource.PlayOneShot(audioClip);
    }

    public static void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }

        if (randomPitchAudioSource != null)
        {
            randomPitchAudioSource.volume = volume;
        }

        if (voiceAudioSource != null)
        {
            voiceAudioSource.volume = volume;
        }

        AudioListener.volume = volume;
    }
}
