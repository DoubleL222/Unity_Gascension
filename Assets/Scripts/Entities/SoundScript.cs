using UnityEngine;

namespace Entities
{
    public class SoundScript : MonoBehaviour
    {
        public AudioSource FartAudioSource;
        public AudioSource SmallFartAudioSource;
        public AudioSource JumpAudioSource;
        public AudioSource EatAudioSource;
        public PlatformerMotor2D Motor;
        public AudioClip[] FartSounds;
        public AudioClip SmallFartSound;
        public AudioClip DeathFartSound;
        public AudioClip GruntSound;
        public AudioClip PressingSound;
        public AudioClip EatSound;
        public AudioClip YumSound;
        private float _pressingCurrentPitch;
        private float _pressingStartPitch;
        private float _pressingLowestPitch = 1.2f;
        private float _pressingSoundPitchAlterationDelay = 0.02f;
        private float _pressingSoundPitchAlterationFullTime = 0.3f;
        private float _pressingSoundPitchAlterationTimer = 0f;
        private bool _doingPressingSound;

        // Use this for initialization
        void Start()
        {
            ////Large fart!
            //Motor.onAirJump += () =>
            //{
            //    FartAudioSource.pitch = Random.Range(0.9f, 1.1f);
            //    FartAudioSource.clip = FartSounds[Random.Range(0, FartSounds.Length)];
            //    FartAudioSource.Play();
            //};

            ////Audio for grunt
            Motor.onNormalJump += () =>
            {
                // Play grunt sound
                JumpAudioSource.pitch = Random.Range(1.3f, 1.5f);
                JumpAudioSource.clip = GruntSound;
                JumpAudioSource.Play();
            };

            Motor.onWallJump += (Vector2 wallNormal) =>
            {
                // Play grunt sound
                JumpAudioSource.pitch = Random.Range(1.3f, 1.5f);
                JumpAudioSource.clip = GruntSound;
                JumpAudioSource.Play();
            };

            Motor.onCornerJump += () =>
            {
                // Play grunt sound
                JumpAudioSource.pitch = Random.Range(1.3f, 1.5f);
                JumpAudioSource.clip = GruntSound;
                JumpAudioSource.Play();
            };
        }

        public void PlayEatSound()
        {
            EatAudioSource.volume = 0.7f;
            EatAudioSource.pitch = Random.Range(1.05f, 1.15f);
            EatAudioSource.clip = EatSound;
            EatAudioSource.Play();
        }

        public void PlayYumSound()
        {
            EatAudioSource.volume = 0.55f;
            EatAudioSource.pitch = Random.Range(1.4f, 1.5f);
            EatAudioSource.clip = YumSound;
            EatAudioSource.Play();
        }

        public void PlayFartSound()
        {
            FartAudioSource.pitch = Random.Range(0.9f, 1.1f);
            FartAudioSource.clip = FartSounds[Random.Range(0, FartSounds.Length)];
            FartAudioSource.Play();
        }

        public void PlayDeathFartSound()
        {
            FartAudioSource.pitch = 1.0f;
            FartAudioSource.clip = DeathFartSound;
            FartAudioSource.Play();
        }

        public void StartPressingSound()
        {
            if (_doingPressingSound)
                return;

            JumpAudioSource.Stop();
            JumpAudioSource.clip = PressingSound;
            _pressingStartPitch = Random.Range(1.5f, 1.7f);
            _pressingCurrentPitch = _pressingStartPitch;
            JumpAudioSource.pitch = _pressingCurrentPitch;
            JumpAudioSource.Play();
            _doingPressingSound = true;
            //Debug.Log("Start: " + _pressingStartPitch);
        }

        public void StopPressingSound()
        {
            if (!_doingPressingSound)
                return;

            JumpAudioSource.Stop();
            _pressingSoundPitchAlterationTimer = 0f;
            _doingPressingSound = false;
            //Debug.Log("Stop pressing sound");
        }

        void Update()
        {
            if (_doingPressingSound)
            {
                _pressingSoundPitchAlterationTimer += Time.unscaledDeltaTime;

                if (_pressingSoundPitchAlterationTimer > _pressingSoundPitchAlterationDelay && _pressingCurrentPitch > _pressingLowestPitch)
                {
                    _pressingCurrentPitch = Mathf.Lerp(_pressingStartPitch, _pressingLowestPitch, _pressingSoundPitchAlterationTimer - _pressingSoundPitchAlterationDelay / _pressingSoundPitchAlterationFullTime - _pressingSoundPitchAlterationDelay);
                    if (_pressingCurrentPitch < _pressingLowestPitch)
                        _pressingCurrentPitch = _pressingLowestPitch;
                    //Debug.Log("Pitch: " + _pressingCurrentPitch);
                    JumpAudioSource.pitch = _pressingCurrentPitch;
                }
            }
        }
    }
}