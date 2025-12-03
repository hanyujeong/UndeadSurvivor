using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic pooling system base class.
/// Inherit this class to create a manager that handles pooling of specific component types.
/// </summary>
/// <typeparam name="T">The type of the manager inheriting this pooling system.</typeparam>
public abstract class PoolingSystem<T> : Singleton<T> where T : PoolingSystem<T>
{
    /// <summary>
    /// Dictionary mapping component types to their component dictionary (pool).
    /// </summary>
    protected Dictionary<Type, IPoolDictionary> PoolCategories = new Dictionary<Type, IPoolDictionary>(3);

    /// <summary>
    /// Initialize all registered component dictionaries at Start.
    /// </summary>
    private void Start()
    {
        foreach (IPoolDictionary poolDictionary in PoolCategories.Values)
        {
            poolDictionary.Initialize(transform);
        }
    }

    /// <summary>
    /// Return an component back into its pool.
    /// </summary>
    /// <typeparam name="TComponent">Type of the component being pooled.</typeparam>
    /// <param name="component">The component instance to enqueue.</param>
    public void Enqueue<TComponent>(TComponent component) where TComponent : Component
    {
        if (PoolCategories.TryGetValue(typeof(TComponent), out IPoolDictionary poolDictionary))
        {
            component.gameObject.SetActive(false);
            poolDictionary.Enqueue(component);
        }
    }

    /// <summary>
    /// Retrieve an component from the pool by key.
    /// </summary>
    /// <typeparam name="TComponent">Type of the component being pooled.</typeparam>
    /// <param name="key">The key identifying the component prefab.</param>
    /// <returns>The pooled component instance.</returns>
    public TComponent Dequeue<TComponent>(string key) where TComponent : Component
    {
        if (PoolCategories.TryGetValue(typeof(TComponent), out IPoolDictionary poolDictionary))
        {
            TComponent component = poolDictionary.Dequeue(key) as TComponent;
            if (component) component.gameObject.SetActive(true);
            
            return component;
        }
        
        return null;
    }
    
    /// <summary>
    /// Retrieve an component from the pool by index.
    /// </summary>
    /// <typeparam name="TComponent">Type of the component being pooled.</typeparam>
    /// <param name="index">The index identifying the component prefab.</param>
    /// <returns>The pooled component instance.</returns>
    public TComponent Dequeue<TComponent>(int index) where TComponent : Component
    {
        if (PoolCategories.TryGetValue(typeof(TComponent), out IPoolDictionary poolDictionary))
        {
            TComponent component = poolDictionary.Dequeue(index) as TComponent;
            if (component) component.gameObject.SetActive(true);
            
            return component;
        }
        
        return null;
    }
    
    /// <summary>
    /// Interface for component dictionaries (pools).
    /// Provides basic enqueue/dequeue/init operations.
    /// </summary>
    protected interface IPoolDictionary
    {
        public Component Dequeue(string key);
        public Component Dequeue(int index);
        public void Enqueue(Component component);
        public void Initialize(Transform parent);
    }

    /// <summary>
    /// Generic component dictionary implementation for pooling a specific component type.
    /// </summary>
    /// <typeparam name="TComponent">Type of component being pooled.</typeparam>
    [Serializable]
    protected class PoolDictionary<TComponent> : IPoolDictionary where TComponent : Component
    {
        /// <summary>
        /// Dictionary mapping keys to component info (prefab, pool, init count).
        /// </summary>
        [SerializeField] private SerializeDictionary<string, PoolConfig<TComponent>> poolConfigs =
            new SerializeDictionary<string, PoolConfig<TComponent>>();

        /// <summary>
        /// Information about a specific pool config (prefab, pool, init count).
        /// </summary>
        [Serializable]
        public struct PoolConfig<UComponent> where UComponent : Component
        {
            public uint initCreateCount;            // Number of instances to pre-create
            public UComponent componentPrefab;      // Prefab reference
            public Queue<UComponent> pool;          // Pool queue
        }

        /// <summary>
        /// Initialize the pool by creating the initial set of components.
        /// </summary>
        public void Initialize(Transform parent)
        {
            SerializeDictionary<string, PoolConfig<TComponent>>.DataType[] data = poolConfigs.GetDataType();
            for (int i = 0; i < data.Length; ++i)
            {
                string key = data[i].key;
                PoolConfig<TComponent> info = data[i].value;

                info.pool = new Queue<TComponent>((int)info.initCreateCount);
                poolConfigs[key] = info;

                for (int j = 0; j < info.initCreateCount; ++j)
                {
                    Enqueue(CreateComponent(info.componentPrefab, key, parent));
                }
            }
        }

        /// <summary>
        /// Create a new component instance from prefab.
        /// </summary>
        private TComponent CreateComponent(TComponent component, string key, Transform parent)
        {
            TComponent newComponent = Instantiate(component, parent);
            newComponent.gameObject.SetActive(false);
            newComponent.name = key;
            
            return newComponent;
        }

        /// <summary>
        /// Enqueue a component back into the pool.
        /// </summary>
        public void Enqueue(Component component)
        {
            if (poolConfigs.TryGetValue(component.name, out PoolConfig<TComponent> componentInfo))
            {
                component.transform.SetParent(Instance.transform, true);
                componentInfo.pool.Enqueue(component as TComponent);
            }
        }

        /// <summary>
        /// Dequeue a component from the pool by key.
        /// Creates a new one if the pool is empty.
        /// </summary>
        public Component Dequeue(string key)
        {
            if (poolConfigs.TryGetValue(key, out PoolConfig<TComponent> componentInfo))
            {
                TComponent component = componentInfo.pool.Count > 0
                    ? componentInfo.pool.Dequeue()
                    : CreateComponent(componentInfo.componentPrefab, key, null);

                return component;
            }
            
            return null;
        }
        
        /// <summary>
        /// Dequeue a component from the pool by index.
        /// Creates a new one if the pool is empty.
        /// </summary>
        public Component Dequeue(int index)
        {
            SerializeDictionary<string, PoolConfig<TComponent>>.DataType[] data = poolConfigs.GetDataType();

            Debug.Assert(index >= 0 && index < data.Length, $"Index {index} is out of range (0 ~ {data.Length - 1}).");

            string key = data[index].key;
            PoolConfig<TComponent> componentInfo = data[index].value;

            Component component = componentInfo.pool is not null && componentInfo.pool.Count > 0
                ? componentInfo.pool.Dequeue()
                : CreateComponent(componentInfo.componentPrefab, key, null);

            return component;
        }
    }
}
