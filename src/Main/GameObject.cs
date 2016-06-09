using System;
using System.Collections.Generic;
using System.Linq;

namespace Ge
{
    public class GameObject
    {
        private readonly MultiValueDictionary<Type, object> _components = new MultiValueDictionary<Type, object>();
        private SystemRegistry _registry;

        public string Name { get; set; }

        public Transform Transform { get; }

        internal static event Action<GameObject> GameObjectConstructed;

        public GameObject() : this(Guid.NewGuid().ToString())
        { }

        public GameObject(string name)
        {
            Transform t = new Transform();
            AddComponent(t);
            Transform = t;
            Name = name;
            GameObjectConstructed?.Invoke(this);
        }

        public void AddComponent<T>(T component) where T : Component
        {
            _components.Add(typeof(T), component);
            component.AttachToGameObject(this, _registry);
        }

        public void RemoveAll<T>() where T : Component
        {
            var components = _components[typeof(T)];
            foreach (Component c in components)
            {
                c.Removed(_registry);
            }

            _components.Remove(typeof(T));
        }

        public void RemoveComponent<T>(T component) where T : Component
        {
            _components.Remove(typeof(T), component);
            component.Removed(_registry);
        }

        public T GetComponent<T>() where T : Component
        {
            IReadOnlyCollection<object> components;
            if (!_components.TryGetValue(typeof(T), out components))
            {
                return null;
            }

            return (T)components.First();
        }

        internal void SetRegistry(SystemRegistry systemRegistry)
        {
            _registry = systemRegistry;
        }

        public IEnumerable<T> GetComponents<T>() where T : Component
        {
            IReadOnlyCollection<object> components;
            if (_components.TryGetValue(typeof(T), out components))
            {
                return (IReadOnlyCollection<T>)components;
            }
            else
            {
                return Array.Empty<T>();
            }
        }
    }
}