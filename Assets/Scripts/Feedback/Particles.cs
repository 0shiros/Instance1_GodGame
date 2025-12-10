using System.Collections;
using UnityEngine;

public class Particles : MonoBehaviour
{
    [SerializeField] ParticleSystem particles;

    public void playParticles()
    {
        particles.Play();
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        while (particles.isPlaying) 
            yield return null;
        Destroy(gameObject);
    }
}
