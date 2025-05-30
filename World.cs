// World.cs - Complete rewrite using reflection-based component storage
using System;
using System.Collections.Generic;
using System.Linq;
using ECS_Example.Entities;

namespace ECS_Example
{
    public class World
    {
        private List<Entity> _entities = new List<Entity>();
        private Dictionary<Type, Dictionary<int, object>> _componentStorage = new Dictionary<Type, Dictionary<int, object>>();

        public Entity CreateEntity()
        {
            var entity = new Entity();
            _entities.Add(entity);
            return entity;
        }

        public void RemoveEntity(Entity entity)
        {
            _entities.Remove(entity);

            // Remove all components for this entity across all component types
            foreach (var componentDict in _componentStorage.Values)
            {
                componentDict.Remove(entity.Id);
            }
        }

        public void AddComponent<T>(Entity entity, T component) where T : struct
        {
            var componentType = typeof(T);

            if (!_componentStorage.ContainsKey(componentType))
            {
                _componentStorage[componentType] = new Dictionary<int, object>();
            }

            _componentStorage[componentType][entity.Id] = component;
        }

        public void RemoveComponent<T>(Entity entity) where T : struct
        {
            var componentType = typeof(T);

            if (_componentStorage.ContainsKey(componentType))
            {
                _componentStorage[componentType].Remove(entity.Id);
            }
        }

        public bool TryGetComponent<T>(Entity entity, out T component) where T : struct
        {
            component = default(T);
            var componentType = typeof(T);

            if (_componentStorage.ContainsKey(componentType) &&
                _componentStorage[componentType].TryGetValue(entity.Id, out var obj))
            {
                component = (T)obj;
                return true;
            }

            return false;
        }

        public bool HasComponent<T>(Entity entity) where T : struct
        {
            var componentType = typeof(T);
            return _componentStorage.ContainsKey(componentType) &&
                   _componentStorage[componentType].ContainsKey(entity.Id);
        }

        public T GetComponent<T>(Entity entity) where T : struct
        {
            if (TryGetComponent<T>(entity, out var component))
                return component;
            throw new InvalidOperationException($"Entity {entity.Id} does not have component {typeof(T).Name}");
        }

        // Query methods for cleaner system code
        public IEnumerable<Entity> Query<T>() where T : struct
        {
            var componentType = typeof(T);
            if (!_componentStorage.ContainsKey(componentType))
                yield break;

            var componentDict = _componentStorage[componentType];
            foreach (var entity in _entities)
            {
                if (componentDict.ContainsKey(entity.Id))
                    yield return entity;
            }
        }

        public IEnumerable<Entity> Query<T1, T2>()
            where T1 : struct
            where T2 : struct
        {
            foreach (var entity in _entities)
            {
                if (HasComponent<T1>(entity) && HasComponent<T2>(entity))
                    yield return entity;
            }
        }

        public IEnumerable<Entity> Query<T1, T2, T3>()
            where T1 : struct
            where T2 : struct
            where T3 : struct
        {
            foreach (var entity in _entities)
            {
                if (HasComponent<T1>(entity) && HasComponent<T2>(entity) && HasComponent<T3>(entity))
                    yield return entity;
            }
        }

        public IEnumerable<Entity> Query<T1, T2, T3, T4>()
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
        {
            foreach (var entity in _entities)
            {
                if (HasComponent<T1>(entity) && HasComponent<T2>(entity) &&
                    HasComponent<T3>(entity) && HasComponent<T4>(entity))
                    yield return entity;
            }
        }

        // Query with exclusion - useful for "has X but not Y" scenarios
        public IEnumerable<Entity> QueryExcluding<TInclude, TExclude>()
            where TInclude : struct
            where TExclude : struct
        {
            foreach (var entity in _entities)
            {
                if (HasComponent<TInclude>(entity) && !HasComponent<TExclude>(entity))
                    yield return entity;
            }
        }

        // Get all entities and their components for a specific type
        public IEnumerable<(Entity entity, T component)> GetComponents<T>() where T : struct
        {
            var componentType = typeof(T);
            if (!_componentStorage.ContainsKey(componentType))
                yield break;

            var componentDict = _componentStorage[componentType];
            foreach (var entity in _entities)
            {
                if (componentDict.TryGetValue(entity.Id, out var component))
                    yield return (entity, (T)component);
            }
        }

        public List<Entity> GetEntities() => _entities;

        // Helper method to get component count
        public int GetComponentCount<T>() where T : struct
        {
            var componentType = typeof(T);
            return _componentStorage.ContainsKey(componentType) ? _componentStorage[componentType].Count : 0;
        }

        // Debug helpers
        public void PrintComponentStats()
        {
            System.Diagnostics.Debug.WriteLine("=== Component Storage Stats ===");
            foreach (var kvp in _componentStorage)
            {
                System.Diagnostics.Debug.WriteLine($"{kvp.Key.Name}: {kvp.Value.Count} entities");
            }
        }

        // Get all component types that exist in the world
        public IEnumerable<Type> GetAllComponentTypes()
        {
            return _componentStorage.Keys;
        }

        // Advanced: Remove all entities with a specific component
        public void RemoveAllEntitiesWith<T>() where T : struct
        {
            var entitiesToRemove = Query<T>().ToList();
            foreach (var entity in entitiesToRemove)
            {
                RemoveEntity(entity);
            }
        }
    }
}