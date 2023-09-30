using Butjok.CommandLine;
using UnityEngine;

public class TestRenderer2 : MonoBehaviour {

    public ParticleSystem particleSystem;

    [Command]
    public void MakeParticles() {
        var particles = new ParticleSystem.Particle[1024];
        for (var i=0;i<particles.Length;i++) {
            particles[i].position = Random.insideUnitSphere * 10;
            particles[i].startColor = Color.white;
            particles[i].startSize = 1;
        }
        particleSystem.SetParticles(particles, particles.Length);
    }
}