using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectsHandler : MonoBehaviour
{
    [SerializeField] List<ParticleSystem> systems = new();

    public void PlayParticleSystem(int index)
    {
        systems[index].Play();
    }

    public void StopParticleSystem(int index)
    {
        systems[index].Stop();
    }
}
