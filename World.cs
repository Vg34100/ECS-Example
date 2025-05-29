// World.cs
using System.Collections.Generic;
using ECS_Example.Entities;
using ECS_Example.Components;

namespace ECS_Example
{
    public class World
    {
        private List<Entity> _entities = new List<Entity>();
        private Dictionary<int, Position> _positions = new Dictionary<int, Position>();
        private Dictionary<int, Shape> _shapes = new Dictionary<int, Shape>();
        private Dictionary<int, Velocity> _velocities = new Dictionary<int, Velocity>();
        private Dictionary<int, Input> _inputs = new Dictionary<int, Input>();
        private Dictionary<int, Gravity> _gravities = new Dictionary<int, Gravity>();
        private Dictionary<int, Collider> _colliders = new Dictionary<int, Collider>();
        private Dictionary<int, Jump> _jumps = new Dictionary<int, Jump>();
        private Dictionary<int, PlayerController> _playerControllers = new Dictionary<int, PlayerController>();
        // v-patrol
        private Dictionary<int, Patrol> _patrols = new Dictionary<int, Patrol>();

        // v-health
        private Dictionary<int, Health> _healths = new Dictionary<int, Health>();
        private Dictionary<int, Damager> _damagers = new Dictionary<int, Damager>();
        private Dictionary<int, Bounceable> _bounceables = new Dictionary<int, Bounceable>();
        private Dictionary<int, Invulnerable> _invulnerables = new Dictionary<int, Invulnerable>();

        // v-feedback
        private Dictionary<int, FlashEffect> _flashEffects = new Dictionary<int, FlashEffect>();
        private Dictionary<int, Stunned> _stunnedStates = new Dictionary<int, Stunned>();
        private Dictionary<int, InvulnerableSettings> _invulnerableSettings = new Dictionary<int, InvulnerableSettings>();
        private Dictionary<int, StunSettings> _stunSettings = new Dictionary<int, StunSettings>();

        // attack
        private Dictionary<int, Attack> _attacks = new Dictionary<int, Attack>();

        // camera
        private Dictionary<int, Camera> _cameras = new Dictionary<int, Camera>();
        private Dictionary<int, CameraTarget> _cameraTargets = new Dictionary<int, CameraTarget>();

        // level system
        private Dictionary<int, LevelComponent> _levelComponents = new Dictionary<int, LevelComponent>();
        private Dictionary<int, PathComponent> _pathComponents = new Dictionary<int, PathComponent>();
        private Dictionary<int, WallCollision> _wallCollisions = new Dictionary<int, WallCollision>();

        public Entity CreateEntity()
        {
            var entity = new Entity();
            _entities.Add(entity);
            return entity;
        }

        public void RemoveEntity(Entity entity)
        {
            _entities.Remove(entity);
            // Remove all components for this entity
            _positions.Remove(entity.Id);
            _shapes.Remove(entity.Id);
            _velocities.Remove(entity.Id);
            _inputs.Remove(entity.Id);
            _gravities.Remove(entity.Id);
            _colliders.Remove(entity.Id);
            _jumps.Remove(entity.Id);
            _playerControllers.Remove(entity.Id);
            _patrols.Remove(entity.Id);
            _healths.Remove(entity.Id);
            _damagers.Remove(entity.Id);
            _bounceables.Remove(entity.Id);
            _invulnerables.Remove(entity.Id);
            _flashEffects.Remove(entity.Id);
            _stunnedStates.Remove(entity.Id);
            _invulnerableSettings.Remove(entity.Id);
            _stunSettings.Remove(entity.Id);
            _levelComponents.Remove(entity.Id);
            _pathComponents.Remove(entity.Id);
            _wallCollisions.Remove(entity.Id);
        }

        public void AddComponent<T>(Entity entity, T component)
        {
            if (typeof(T) == typeof(Position))
                _positions[entity.Id] = (Position)(object)component;
            else if (typeof(T) == typeof(Shape))
                _shapes[entity.Id] = (Shape)(object)component;
            else if (typeof(T) == typeof(Velocity))
                _velocities[entity.Id] = (Velocity)(object)component;
            else if (typeof(T) == typeof(Input))
                _inputs[entity.Id] = (Input)(object)component;
            else if (typeof(T) == typeof(Gravity))
                _gravities[entity.Id] = (Gravity)(object)component;
            else if (typeof(T) == typeof(Collider))
                _colliders[entity.Id] = (Collider)(object)component;
            else if (typeof(T) == typeof(Jump))
                _jumps[entity.Id] = (Jump)(object)component;
            else if (typeof(T) == typeof(PlayerController))
                _playerControllers[entity.Id] = (PlayerController)(object)component;
            else if (typeof(T) == typeof(Patrol))
                _patrols[entity.Id] = (Patrol)(object)component;
            else if (typeof(T) == typeof(Health))
                _healths[entity.Id] = (Health)(object)component;
            else if (typeof(T) == typeof(Damager))
                _damagers[entity.Id] = (Damager)(object)component;
            else if (typeof(T) == typeof(Bounceable))
                _bounceables[entity.Id] = (Bounceable)(object)component;
            else if (typeof(T) == typeof(Invulnerable))
                _invulnerables[entity.Id] = (Invulnerable)(object)component;
            else if (typeof(T) == typeof(FlashEffect))
                _flashEffects[entity.Id] = (FlashEffect)(object)component;
            else if (typeof(T) == typeof(Stunned))
                _stunnedStates[entity.Id] = (Stunned)(object)component;
            else if (typeof(T) == typeof(InvulnerableSettings))
                _invulnerableSettings[entity.Id] = (InvulnerableSettings)(object)component;
            else if (typeof(T) == typeof(StunSettings))
                _stunSettings[entity.Id] = (StunSettings)(object)component;
            else if (typeof(T) == typeof(Attack))
                _attacks[entity.Id] = (Attack)(object)component;
            else if (typeof(T) == typeof(Camera))
                _cameras[entity.Id] = (Camera)(object)component;
            else if (typeof(T) == typeof(CameraTarget))
                _cameraTargets[entity.Id] = (CameraTarget)(object)component;
            else if (typeof(T) == typeof(LevelComponent))
                _levelComponents[entity.Id] = (LevelComponent)(object)component;
            else if (typeof(T) == typeof(PathComponent))
                _pathComponents[entity.Id] = (PathComponent)(object)component;
            else if (typeof(T) == typeof(WallCollision))
                _wallCollisions[entity.Id] = (WallCollision)(object)component;

        }

        public void RemoveComponent<T>(Entity entity)
        {
            if (typeof(T) == typeof(Invulnerable))
                _invulnerables.Remove(entity.Id);
            else if (typeof(T) == typeof(FlashEffect))
                _flashEffects.Remove(entity.Id);
            else if (typeof(T) == typeof(Stunned))
                _stunnedStates.Remove(entity.Id);
            else if (typeof(T) == typeof(InvulnerableSettings))
                _invulnerableSettings.Remove(entity.Id);
            else if (typeof(T) == typeof(Attack))
                _attacks.Remove(entity.Id);
            else if (typeof(T) == typeof(Camera))
                _cameras.Remove(entity.Id);
            else if (typeof(T) == typeof(CameraTarget))
                _cameraTargets.Remove(entity.Id);
            else if (typeof(T) == typeof(LevelComponent))
                _levelComponents.Remove(entity.Id);
            else if (typeof(T) == typeof(PathComponent))
                _pathComponents.Remove(entity.Id);
            else if (typeof(T) == typeof(WallCollision))
                _wallCollisions.Remove(entity.Id);

        }

        public bool TryGetComponent<T>(Entity entity, out T component)
        {
            component = default(T);

            if (typeof(T) == typeof(Position) && _positions.TryGetValue(entity.Id, out var position))
            {
                component = (T)(object)position;
                return true;
            }
            else if (typeof(T) == typeof(Shape) && _shapes.TryGetValue(entity.Id, out var shape))
            {
                component = (T)(object)shape;
                return true;
            }
            else if (typeof(T) == typeof(Velocity) && _velocities.TryGetValue(entity.Id, out var velocity))
            {
                component = (T)(object)velocity;
                return true;
            }
            else if (typeof(T) == typeof(Input) && _inputs.TryGetValue(entity.Id, out var inputComp))
            {
                component = (T)(object)inputComp;
                return true;
            }
            else if (typeof(T) == typeof(Gravity) && _gravities.TryGetValue(entity.Id, out var gravity))
            {
                component = (T)(object)gravity;
                return true;
            }
            else if (typeof(T) == typeof(Collider) && _colliders.TryGetValue(entity.Id, out var collider))
            {
                component = (T)(object)collider;
                return true;
            }
            else if (typeof(T) == typeof(Jump) && _jumps.TryGetValue(entity.Id, out var jump))
            {
                component = (T)(object)jump;
                return true;
            }
            else if (typeof(T) == typeof(PlayerController) && _playerControllers.TryGetValue(entity.Id, out var controller))
            {
                component = (T)(object)controller;
                return true;
            }
            else if (typeof(T) == typeof(Patrol) && _patrols.TryGetValue(entity.Id, out var patrol))
            {
                component = (T)(object)patrol;
                return true;
            }
            else if (typeof(T) == typeof(Health) && _healths.TryGetValue(entity.Id, out var health))
            {
                component = (T)(object)health;
                return true;
            }
            else if (typeof(T) == typeof(Damager) && _damagers.TryGetValue(entity.Id, out var damager))
            {
                component = (T)(object)damager;
                return true;
            }
            else if (typeof(T) == typeof(Bounceable) && _bounceables.TryGetValue(entity.Id, out var bounceable))
            {
                component = (T)(object)bounceable;
                return true;
            }
            else if (typeof(T) == typeof(Invulnerable) && _invulnerables.TryGetValue(entity.Id, out var invulnerable))
            {
                component = (T)(object)invulnerable;
                return true;
            }
            else if (typeof(T) == typeof(FlashEffect) && _flashEffects.TryGetValue(entity.Id, out var flashEffect))
            {
                component = (T)(object)flashEffect;
                return true;
            }
            else if (typeof(T) == typeof(Stunned) && _stunnedStates.TryGetValue(entity.Id, out var stunned))
            {
                component = (T)(object)stunned;
                return true;
            }
            else if (typeof(T) == typeof(InvulnerableSettings) && _invulnerableSettings.TryGetValue(entity.Id, out var invulnerableSettings))
            {
                component = (T)(object)invulnerableSettings;
                return true;
            }
            else if (typeof(T) == typeof(StunSettings) && _stunSettings.TryGetValue(entity.Id, out var stunSettings))
            {
                component = (T)(object)stunSettings;
                return true;
            }
            else if (typeof(T) == typeof(Attack) && _attacks.TryGetValue(entity.Id, out var attack))
            {
                component = (T)(object)attack;
                return true;
            }
            else if (typeof(T) == typeof(Camera) && _cameras.TryGetValue(entity.Id, out var camera))
            {
                component = (T)(object)camera;
                return true;
            }
            else if (typeof(T) == typeof(CameraTarget) && _cameraTargets.TryGetValue(entity.Id, out var cameraTarget))
            {
                component = (T)(object)cameraTarget;
                return true;
            }
            else if (typeof(T) == typeof(LevelComponent) && _levelComponents.TryGetValue(entity.Id, out var levelComponent))
            {
                component = (T)(object)levelComponent;
                return true;
            }
            else if (typeof(T) == typeof(PathComponent) && _pathComponents.TryGetValue(entity.Id, out var pathComponent))
            {
                component = (T)(object)pathComponent;
                return true;
            }
            else if (typeof(T) == typeof(WallCollision) && _wallCollisions.TryGetValue(entity.Id, out var wallCollision))
            {
                component = (T)(object)wallCollision;
                return true;
            }

            return false;
        }

        public List<Entity> GetEntities() => _entities;
    }
}