using UnityEngine;
using Utils;

namespace Systems
{
    public class MusicManager : SingletonBehaviour<MusicManager>
    {
        public bool Mute;
        public AudioClip[] MusicClips;
        public AudioSource AudioSource;

        public float minPitch = 0.3f;
        public float maxPitch = 1.0f;
        public float pitchReductionSpeed = 1.0f;
        public float pitchIncreaseSpeed = 0.5f;

        private bool reducing = false;

        public void ReducePitch()
        {
            reducing = true;
        }

        public void IncreasePitch()
        {
            reducing = false;
        }

        private void Start()
        {
            PlayBackgroundMusic();
        }

        public void PlayBackgroundMusic()
        {
            if (!Mute && MusicClips.Length > 0)
            {
                AudioSource.clip = MusicClips[0];
                AudioSource.Play();
            }
        }

        public void PlayGameOverJingle()
        {
            if (!Mute && MusicClips.Length > 1)
            {
                AudioSource.clip = MusicClips[1];
                AudioSource.Play();
            }
        }

        void AdjustPitch()
        {
            if (reducing && AudioSource.pitch>minPitch)
            {
                AudioSource.pitch -= (pitchReductionSpeed * Time.deltaTime);
                if (AudioSource.pitch < minPitch)
                {
                    AudioSource.pitch = minPitch;
                }
            }
            else if (!reducing && AudioSource.pitch<maxPitch)
            {
                AudioSource.pitch += (pitchIncreaseSpeed * Time.deltaTime);
                if (AudioSource.pitch > maxPitch)
                {
                    AudioSource.pitch = maxPitch;
                }
            }
        }

        private void Update()
        {
            AdjustPitch();
        }
    }
}