using UnityEngine;

public class PoolManager : PoolingSystem<PoolManager>
{
    [SerializeField] private PoolDictionary<Transform> poolPrefabs;
    
    protected override void Awake()
    {
        base.Awake();
        
        PoolCategories[typeof(Transform)] = poolPrefabs;
    }

    public GameObject Get(int index)
    {
        Transform poolObject = Instance.Dequeue<Transform>(index);
        
        return poolObject.gameObject;
    }
    
    public void Set(GameObject poolObject)
    {
        Instance.Enqueue(poolObject.transform);
    }

    public int GetPoolPrefabsCount()
    {
        return poolPrefabs.poolConfigs.Count;
    }
}
