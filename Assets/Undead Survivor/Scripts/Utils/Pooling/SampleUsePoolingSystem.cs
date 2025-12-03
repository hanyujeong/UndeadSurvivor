using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Simple example of using PoolingSystem.
/// </summary>
public class SampleUsePoolingSystem : PoolingSystem<SampleUsePoolingSystem>
{
    [FormerlySerializedAs("particlePoolings")] [FormerlySerializedAs("particleEffects")] [SerializeField] private PoolDictionary<ParticleSystem> particlePools;

    protected override void Awake()
    {
        base.Awake();
        
        // Keep manager alive across scenes
        transform.parent = null;
        DontDestroyOnLoad(gameObject);

        // Register pools
        PoolCategories[typeof(ParticleSystem)] = particlePools;
    }
}
