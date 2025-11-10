using UnityEngine;

namespace CandyProject
{
    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

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
            musicSource.volume = value;
        }

        public void SetSfxVolume(float value)
        {
            sfxSource.volume = value;
        }
    }
}
