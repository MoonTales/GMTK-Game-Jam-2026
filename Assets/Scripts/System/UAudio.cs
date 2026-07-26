using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// This is a singleton used for the Unity Implementations of Audio within the project
    /// </summary>
    public class UAudio : Singleton<UAudio>
    {
        [SerializeField] private int _poolSize = 10;
        private List<AudioSource> _sources;
        
        // Now we will sewtup a matching system for Music, we will have up to 5 musics Sources
        [SerializeField] private int _musicPoolSize = 5;
        private List<AudioSource> _musicSources;

        //public float sfxvolumeslider = 1;
        
        [Header("Example Sounds")]
        public AudioClip ExampleAudioClip;
        
        [Header("RatP Sounds")]
        public AudioClip RATP_BacktrackSound;
        public AudioClip RATP_ButtonPressSound;
        public AudioClip RATP_ButtonFailSound;
        public AudioClip RATP_ButtonSuccessSound;
        public AudioClip RATP_SuccessSound;
        public AudioClip RATP_ElectricUpgradeSound;
        
        [Header("RatP Music")]
        public AudioClip RATP_BacktrackMusic;
        public AudioClip RATP_WarningAlarmMusic;
        public AudioClip RatP_IntenseMusic;
        
        [Header("Menu Music")]
        public AudioClip MenuMusic;
        
        
        
        [Header("Mutes")]
        public bool muteSFX = false;
        
        
        //variables for the soundtrack
        public float sfxValue = 1;

        private bool muted = false;

        protected override void Awake()
        {
            base.Awake();

            // Create a pool of AudioSources we can reuse
            _sources = new List<AudioSource>();
            _musicSources = new List<AudioSource>();
            for (int i = 0; i < _poolSize; i++)
            {
                AudioSource src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f; // 2D by default
                _sources.Add(src);
            }
            for (int i = 0; i < _musicPoolSize; i++)
            {
                AudioSource mus = gameObject.AddComponent<AudioSource>();
                mus.playOnAwake = false;
                mus.spatialBlend = 0f; // 2D by default
                mus.loop = true;
                _musicSources.Add(mus);
            }
            
        }
        
        private void Start()
        {
            sfxValue = 1;
        }
        
        private void PlaySFX(AudioClip clip, float volume = 1f, float deviation = 0f, GameObject fromObject = null)
        {
            if (muteSFX) return;
            if (clip == null) return;

            AudioSource src = GetFreeSource();
            if (src == null) return;

            src.transform.position = fromObject ? fromObject.transform.position : Camera.main ? Camera.main.transform.position : Vector3.zero;

            src.spatialBlend = fromObject ? 1f : 0f;
            src.volume = (volume * sfxValue);
            src.clip = clip;
            src.pitch = UnityEngine.Random.Range(1 - deviation, 1 + deviation);
            src.Play();
        }
        
        public void PlayMusic(AudioClip clip, float volume = 1f, float deviation = 0f, float playbackSpeed = 1f)
        {
            if (muteSFX) return;
            if (clip == null) return;
            // if its already playing in any other source, dont play it
            if (_musicSources.Exists(src => src.clip == clip && src.isPlaying)) return;

            AudioSource src = GetFreeMusicSource();
            if (src == null) return;

            src.spatialBlend = 0f; // Music is always 2D
            src.volume = (volume * sfxValue);
            src.clip = clip;
            src.pitch = UnityEngine.Random.Range(1 - deviation, 1 + deviation);
            src.loop = true;
            src.Play();
        }
        
        public void StopMusic(AudioClip clip)
        {
            foreach (var src in _musicSources)
            {
                if (src.clip == clip && src.isPlaying)
                {
                    src.Stop();
                    break;
                }
            }
        }
        
        public void StopAllMusic()
        {
            foreach (var src in _musicSources)
            {
                if (src.isPlaying)
                {
                    src.Stop();
                }
            }
        }
        
        public void PlayMenuMusic(float volume = 1f)
        {
            PlayMusic(MenuMusic, volume: 1f, deviation: 0.0f);
        }
        public void StopMenuMusic()
        {
            StopMusic(MenuMusic);
        }
        
        public void PlayRATP_SuccessSound()
        {
            PlaySFX(RATP_SuccessSound, volume: 1f, deviation: 0.1f);
        }
        public void PlayRATP_ElectricUpgradeSound(float volume = 1f)
        {
            PlaySFX(RATP_ElectricUpgradeSound, volume: 1f, deviation: 0.1f);
        }
        public void PlayRATP_ButtonSuccessSound()
        {
            PlaySFX(RATP_ButtonSuccessSound, volume: 1f, deviation: 0.1f);
        }
        public void PlayMusic_RATP_BacktrackMusic()
        {
            PlayMusic(RATP_BacktrackMusic, volume: 1f, deviation: 0.0f);
        }
        public void PlayMusic_RATP_WarningAlarmMusic()
        {
            PlayMusic(RATP_WarningAlarmMusic, volume: 1f, deviation: 0.0f);
        }
        public void PlayMusic_RatP_IntenseMusic()
        {
            PlayMusic(RatP_IntenseMusic, volume: 1f, deviation: 0.0f);
        }
        public void StopMusic_RATP_BacktrackMusic()
        {
            StopMusic(RATP_BacktrackMusic);
        }
        public void StopMusic_RATP_WarningAlarmMusic()
        {
            StopMusic(RATP_WarningAlarmMusic);
        }
        public void StopMusic_RatP_IntenseMusic()
        {
            StopMusic(RatP_IntenseMusic);
        }


        
        //This is called like:
        // UAudio.Instance.PlayExampleAudio(fromObject: someGameObject, volume: 0.5f, deviation: 0.2f);
        // where all the params a
        #region Example Sounds
        public void PlayExampleAudio(GameObject fromObject = null, float volume = 1f, float deviation = 0f)
        {
            PlaySFX(ExampleAudioClip, volume, deviation, fromObject);
        }
        #endregion

        public void PlayClip(AudioClip clip, GameObject fromObject = null, float volume = 1f, float deviation = 0f)
        {
            
            PlaySFX(clip, volume, deviation, fromObject);
        }
        
        public void PlayRATP_PlayGridBacktrackSound()
        {
            PlaySFX(RATP_BacktrackSound, volume: 1f, deviation: 0.1f);
        }
        
        public void PlayRATP_ButtonPressSound()
        {
            PlaySFX(RATP_ButtonPressSound, volume: 1f, deviation: 0.1f);
        }
        public void PlayRATP_ButtonFailSound()
        {
            PlaySFX(RATP_ButtonFailSound, volume: 1f, deviation: 0.1f);
        }
        

        private AudioSource GetFreeSource()
        {
            foreach (var src in _sources)
            {
                if (!src.isPlaying)
                    return src;
            }
            // If none are free, just reuse the first
            return _sources[0];
        }
        private AudioSource GetFreeMusicSource()
        {
            foreach (var src in _musicSources)
            {
                if (!src.isPlaying)
                    return src;
            }
            // If none are free, just reuse the first
            return _musicSources[0];
        }
    }
    
}
