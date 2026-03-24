using UnityEngine;

namespace Infrastructure.AudioControl
{
    public class AudioSound : MonoBehaviour
    {
        [SerializeField] private AudioClip[] clip;
        [SerializeField] private float pinchModifier = 0.1f;
        [SerializeField, Range(0f,1f)] private float volume = 1;
        

        public void Play(AudioSource source, bool isRandom)
        {
            if (clip.Length == 0)
                return;

            source.pitch = Random.Range(1 - pinchModifier, 1 + pinchModifier);
            int clipNumber = isRandom ? Random.Range(0, clip.Length) : 0;
            source.clip = clip[clipNumber];
            source.volume = volume;
            source.Play();
        }


        public void PlayOneShot(AudioSource source, bool isRandom)
        {
            if (clip.Length == 0)
                return;
            
            source.pitch = Random.Range(1 - pinchModifier, 1 + pinchModifier);
            int clipNumber = isRandom ? Random.Range(0, clip.Length) : 0;
            source.volume = volume;
            source.PlayOneShot(clip[clipNumber]);
        }


        public void SetVolume(float volume)
        {
            this.volume = volume;
        }
        
        public float GetVolume()
        {
            return volume;
        }
    }
}