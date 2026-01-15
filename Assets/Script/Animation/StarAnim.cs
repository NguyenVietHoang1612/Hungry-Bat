using UnityEngine;

namespace CandyProject
{
    public class StarAnim : MonoBehaviour
    {
        [SerializeField] private Transform particleTransform;

        private Animator animator;

        private void Start()
        {
            animator = GetComponent<Animator>();
            particleTransform.gameObject.SetActive(false);

        }

        public void StateEventPlayParticle()
        {
            particleTransform.gameObject.SetActive(true);
            ParticleSystem particle = particleTransform.GetComponent<ParticleSystem>();
            particle.Play();
        }

        public void PlayAnimStar()
        {
            animator.SetBool("WinState", true);
        }

    }
}
