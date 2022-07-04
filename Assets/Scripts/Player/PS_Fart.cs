using UnityEngine;

namespace Player
{
    public class PS_Fart : MonoBehaviour
    {
        public PlatformerMotor2D Motor;
        public ParticleSystem LargeFartEffect;
        public ParticleSystem collectedBean;
        public ParticleSystem SmallFartEffect;
        public ParticleSystem DeathFartEffect;
        public ParticleSystem DeathBloodEffect;

        // Use this for initialization
        void Start()
        {
            //Motor.onAirJump += () =>
            //{
            //    LargeFartEffect.Play();
            //};
            //Motor.onNormalJump += () =>
            //{
            //    SmallFartEffect.Play();
            //};

            //Motor.onCornerJump += () =>
            //{
            //    SmallFartEffect.Play();
            //};

            //Motor.onWallJump += (Vector2 wallNormal) =>
            //{
            //    SmallFartEffect.Play();
            //};
        }

        public void PlayDeathFartEffect()
        {
            DeathFartEffect.Play();
        }

        public void PlayDeathBloodEffect()
        {
            DeathBloodEffect.Play();
        }

        public void CollectBeansEffect(Transform _position)
        {
            ParticleSystem beansEffect = Instantiate(collectedBean, _position.position, Quaternion.identity);
        }
    }
}