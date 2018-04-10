using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleSystemAutoDestroy : MonoBehaviour 
{
    [SerializeField]
	private List<ParticleSystem> systems;

    private void Start()
    {
        if(systems.Count == 0)
        {
            systems.Add(this.GetComponent<ParticleSystem>());
        }
    }

    private void Update()
	{
        bool allFinished = true;
        for (int i = 0; i < systems.Count; i++)
        {
            if(systems[i] != null)
            {
                if(systems[i].IsAlive())
                {
                    allFinished = false;
                }
            }
        }

        if(allFinished)
        {
            Destroy(this.gameObject);
        }
	}
}
