using System;
using System.Collections.Generic;

namespace arriety.ecs
{
    public class Entity
    {
        private static int nextId = 1;
        
        public int Id { get; }
        private Dictionary<Type, IComponent> components = new();
        
        public Entity()
        {
            Id = nextId++;
        }
        
        public T AddComponent<T>() where T : class, IComponent, new()
        {
            var component = new T();
            component.EntityId = Id;
            components[typeof(T)] = component;
            return component;
        }
        
        public T AddComponent<T>(T component) where T : class, IComponent
        {
            component.EntityId = Id;
            components[typeof(T)] = component;
            return component;
        }
        
        public T GetComponent<T>() where T : class, IComponent
        {
            return components.TryGetValue(typeof(T), out var component) ? component as T : null;
        }
        
        public bool HasComponent<T>() where T : class, IComponent
        {
            return components.ContainsKey(typeof(T));
        }
        
        public void RemoveComponent<T>() where T : class, IComponent
        {
            components.Remove(typeof(T));
        }
        
        public IEnumerable<IComponent> GetAllComponents()
        {
            return components.Values;
        }
    }
} 