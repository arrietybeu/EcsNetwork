using System;
using System.Collections.Generic;
using System.Linq;

namespace arriety.ecs
{
    public class World
    {
        private List<Entity> entities = new();
        private List<ISystem> systems = new();
        private bool running = false;
        
        public void AddEntity(Entity entity)
        {
            entities.Add(entity);
        }
        
        public void RemoveEntity(Entity entity)
        {
            entities.Remove(entity);
        }
        
        public IEnumerable<Entity> GetEntities()
        {
            return entities;
        }
        
        public IEnumerable<Entity> GetEntitiesWith<T>() where T : class, IComponent
        {
            return entities.Where(e => e.HasComponent<T>());
        }
        
        public IEnumerable<Entity> GetEntitiesWith<T1, T2>() 
            where T1 : class, IComponent 
            where T2 : class, IComponent
        {
            return entities.Where(e => e.HasComponent<T1>() && e.HasComponent<T2>());
        }
        
        public IEnumerable<Entity> GetEntitiesWith<T1, T2, T3>() 
            where T1 : class, IComponent 
            where T2 : class, IComponent
            where T3 : class, IComponent
        {
            return entities.Where(e => e.HasComponent<T1>() && e.HasComponent<T2>() && e.HasComponent<T3>());
        }
        
        public void AddSystem(ISystem system)
        {
            systems.Add(system);
            if (running)
            {
                system.Initialize(this);
            }
        }
        
        public void RemoveSystem(ISystem system)
        {
            if (running)
            {
                system.Shutdown(this);
            }
            systems.Remove(system);
        }
        
        public T GetSystem<T>() where T : class, ISystem
        {
            return systems.OfType<T>().FirstOrDefault();
        }
        
        public void Initialize()
        {
            running = true;
            foreach (var system in systems)
            {
                system.Initialize(this);
            }
        }
        
        public void Update(float deltaTime)
        {
            if (!running) return;
            
            foreach (var system in systems)
            {
                system.Update(this, deltaTime);
            }
        }
        
        public void Shutdown()
        {
            running = false;
            foreach (var system in systems)
            {
                system.Shutdown(this);
            }
        }
        
        public Entity CreateEntity()
        {
            var entity = new Entity();
            AddEntity(entity);
            return entity;
        }
    }
} 