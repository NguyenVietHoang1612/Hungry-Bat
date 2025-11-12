using UnityEngine;

namespace CandyProject
{
    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        public float VolumnMusic { get; private set; }
        public float VolumnSFX { get; private set; }


        public void PlayOneShotSfx(AudioClip clip)
        {
            if (clip != null)
                sfxSource.PlayOneShot(clip);
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void SetMusicVolume(float value)
        {
            VolumnMusic = value;
            musicSource.volume = VolumnMusic;
        }

        public void SetSFXVolume(float value)
        {
            VolumnSFX = value;
            sfxSource.volume = VolumnSFX;
        }
    }
}
