using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Infrastructure.SystemsLifeCycle;
using UnityEngine;

namespace Infrastructure.AudioControl
{
    public class AudioService : SingletonComponent<AudioService>
    {
        [Header("Music")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private SerializedDictionary<SfxType, AudioSound> musics;

        [Header("SFX")]
        [SerializeField] private AudioSource[] sfxSources;
        [SerializeField] private AudioSource[] sfxSourcesLoop;
        [SerializeField] private SerializedDictionary<SfxType, AudioSound> sounds;

        private SfxType currentMusicTheme;
        private bool isMusicEnabled = true;
        private bool isSoundEnabled = true;
        private readonly Dictionary<SfxType, AudioSource> activeSfxLoops = new();

        private bool isInit;


        public void Initialize()
        {
            if (isInit)
                return;

            activeSfxLoops.Clear();

            isInit = true;
        }


        public void Deinitialize()
        {
            if (!isInit)
                return;

            foreach (var entryPair in activeSfxLoops)
            {
                entryPair.Value.Stop();
            }

            activeSfxLoops.Clear();

            isInit = false;
        }


        public void SetMusicEnabled(bool value)
        {
            isMusicEnabled = value;
            if (value)
                PlayMusic(currentMusicTheme);
            else
                musicSource.Stop();
        }


        public void SetSoundEnabled(bool value)
        {
            if (!value)
            {
                foreach (var source in sfxSources)
                {
                    source.Stop();
                }
            }

            isSoundEnabled = value;
        }


        public void PlaySfx(SfxType effect, bool isRandom = false)
        {
            if (!isSoundEnabled || effect == SfxType.None)
                return;

            if (sounds.TryGetValue(effect, out AudioSound randomizer) && TryGetSfxSource(sfxSources, out AudioSource source))
            {
                randomizer.PlayOneShot(source, isRandom);
            }
        }


        public void PlaySfxLoop(SfxType effect, bool isRandom = false)
        {
            if (!isSoundEnabled || effect == SfxType.None)
                return;

            if (sounds.TryGetValue(effect, out AudioSound randomizer) && TryGetSfxSource(sfxSourcesLoop, out AudioSource source))
            {
                randomizer.Play(source, isRandom);
                activeSfxLoops.TryAdd(effect, source);
            }
        }


        public void StopSfxLoop(SfxType effect)
        {
            if (activeSfxLoops.TryGetValue(effect, out AudioSource source))
            {
                source.Stop();
                activeSfxLoops.Remove(effect);
            }
        }


        public void PlayMusic(SfxType music, bool isRandom = false)
        {
            currentMusicTheme = music;
            if (!isMusicEnabled || music == SfxType.None)
                return;

            if (musics.TryGetValue(music, out AudioSound randomizer))
            {
                randomizer.Play(musicSource, isRandom);
            }
        }


        public void StopMusic()
        {
            if (!isMusicEnabled)
                return;

            musicSource.Stop();
        }


        public void PauseMusic()
        {
            if (!isMusicEnabled)
                return;

            musicSource.Pause();
        }


        public void UnpauseMusic()
        {
            if (!isMusicEnabled)
                return;

            musicSource.UnPause();
        }


        private bool TryGetSfxSource(AudioSource[] sources, out AudioSource result)
        {
            result = null;

            for (var i = 0; i < sources.Length; i++)
            {
                var source = sources[i];
                if (!source.isPlaying)
                {
                    result = source;
                    return true;
                }
            }

            return false;
        }


        private static bool IsMusic(SfxType sfxType)
        {
            return (int)sfxType <= 999;
        }


#if PR_CHEAT || UNITY_EDITOR

        [TriInspector.Title("Debug")]
        [SerializeField] private SfxType sfxEditor;
        [SerializeField, Range(0, 1)] private float debugVolume;

        private SfxType lastSfxEditor;

        private void OnValidate()
        {
            if (lastSfxEditor != sfxEditor)
            {
                debugVolume = GetVolume(sfxEditor);
                lastSfxEditor = sfxEditor;
            }
            else
            {
                CheatSetVolume(sfxEditor, debugVolume);
            }
        }


        public void CheatPlay(SfxType sfxType, float volume)
        {
            CheatSetVolume(sfxType, volume);

            if (IsMusic(sfxType))
                PlayMusic(sfxType);
            else
                PlaySfx(sfxType);
        }


        public void CheatSetVolume(SfxType sfxType, float volume)
        {
            if (IsMusic(sfxType) && musics.TryGetValue(sfxType, out AudioSound musicSound))
            {
                musicSound.SetVolume(volume);
                musicSource.volume = volume;
            }
            else if (sounds.TryGetValue(sfxType, out AudioSound sfxSound))
            {
                sfxSound.SetVolume(volume);
            }
        }


        public float GetVolume(SfxType sfxType)
        {
            if (IsMusic(sfxType) && musics.TryGetValue(sfxType, out AudioSound musicSound))
            {
               return musicSound.GetVolume();
            }

            if (sounds.TryGetValue(sfxType, out AudioSound sfxSound))
            {
                return sfxSound.GetVolume();
            }

            return 1;
        }


        [TriInspector.Button]
        public void Play_Editor()
        {
            CheatPlay(sfxEditor, debugVolume);
        }


        [TriInspector.Button]
        public void StopMusic_Editor()
        {
            StopMusic();
        }


#endif
    }
}