// SystemManager.cs - Manages all systems with reflection-based auto-discovery
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ECS_Example
{
    public class SystemManager
    {
        private List<object> _systems = new List<object>();
        private List<(object system, MethodInfo updateMethod)> _updateSystems = new List<(object, MethodInfo)>();
        private List<(object system, MethodInfo drawMethod)> _drawSystems = new List<(object, MethodInfo)>();

        public T AddSystem<T>(T system) where T : class
        {
            _systems.Add(system);

            // Check for Update method using reflection
            var updateMethod = system.GetType().GetMethod("Update");
            if (updateMethod != null)
            {
                _updateSystems.Add((system, updateMethod));
            }

            // Check for Draw method using reflection
            var drawMethod = system.GetType().GetMethod("Draw");
            if (drawMethod != null)
            {
                _drawSystems.Add((system, drawMethod));
            }

            return system;
        }

        public void Update(World world, float deltaTime)
        {
            foreach (var (system, updateMethod) in _updateSystems)
            {
                var parameters = updateMethod.GetParameters();

                try
                {
                    // Handle different Update method signatures automatically
                    if (parameters.Length == 1)
                    {
                        updateMethod.Invoke(system, new object[] { world });
                    }
                    else if (parameters.Length == 2)
                    {
                        updateMethod.Invoke(system, new object[] { world, deltaTime });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating system {system.GetType().Name}: {ex.Message}");
                }
            }
        }

        public void Draw(World world)
        {
            foreach (var (system, drawMethod) in _drawSystems)
            {
                try
                {
                    drawMethod.Invoke(system, new object[] { world });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error drawing system {system.GetType().Name}: {ex.Message}");
                }
            }
        }

        public T GetSystem<T>() where T : class
        {
            return _systems.OfType<T>().FirstOrDefault();
        }

        public bool HasSystem<T>() where T : class
        {
            return _systems.OfType<T>().Any();
        }

        public void RemoveSystem<T>() where T : class
        {
            var systemToRemove = _systems.OfType<T>().FirstOrDefault();
            if (systemToRemove != null)
            {
                _systems.Remove(systemToRemove);
                _updateSystems.RemoveAll(x => x.system == systemToRemove);
                _drawSystems.RemoveAll(x => x.system == systemToRemove);
            }
        }

        public void Clear()
        {
            _systems.Clear();
            _updateSystems.Clear();
            _drawSystems.Clear();
        }

        public void PrintSystemInfo()
        {
            System.Diagnostics.Debug.WriteLine("=== System Manager Info ===");
            System.Diagnostics.Debug.WriteLine($"Total Systems: {_systems.Count}");
            System.Diagnostics.Debug.WriteLine($"Update Systems: {_updateSystems.Count}");
            System.Diagnostics.Debug.WriteLine($"Draw Systems: {_drawSystems.Count}");

            foreach (var system in _systems)
            {
                System.Diagnostics.Debug.WriteLine($"- {system.GetType().Name}");
            }
        }
    }
}