using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectsHandler : MonoBehaviour
{
    [SerializeField] List<ParticleSystem> systems = new();
    [SerializeField] List<BoxCollider> colliders = new();

    public void PlayParticleSystem(int index)
    {
        systems[index].Stop(); //makes sure particle system always plays from beginning.
        systems[index].Play();
    }

    public void StopParticleSystem(int index)
    {
        systems[index].Stop();
    }

    public void StartAttackBox(int index)
    {
        colliders[index].enabled = true;
    }

    public void StopAttackBox(int index)
    {
        colliders[index].enabled = false;
    }
}
