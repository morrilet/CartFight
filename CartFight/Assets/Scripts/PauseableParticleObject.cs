using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PauseableParticleObject : PausableObject
{
    private List<ParticleSystem> particleSystems;

	void Start ()
    {
        particleSystems = new List<ParticleSystem>();
        if(this.GetComponent<ParticleSystem>() != null)
        {
            foreach(ParticleSystem ps in this.GetComponents<ParticleSystem>())
            {
                particleSystems.Add(ps);
            }
        }
        if(this.transform.childCount > 0)
        {
            if (this.GetComponentsInChildren<ParticleSystem>().Length != 0)
            {
                foreach(ParticleSystem ps in this.GetComponentsInChildren<ParticleSystem>())
                {
                    particleSystems.Add(ps);
                }
            }
        }
	}

    private void Update()
    {
        if(IsPaused && !IsPausedPrev)
        {
            PauseParticles();
        }
        else if (!IsPaused && IsPausedPrev)
        {
            ResumeParticles();
        }
    }

    private void PauseParticles()
    {
        foreach(ParticleSystem ps in particleSystems)
        {
            if(ps.isPaused == false)
            {
                ps.Pause();
            }
        }
    }

    private void ResumeParticles()
    {
        foreach(ParticleSystem ps in particleSystems)
        {
            if (ps.isPaused == true)
            {
                ps.Play();
            }
        }
    }
}
