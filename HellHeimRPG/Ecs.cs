using HellHeimRPG.Components;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HellHeimRPG {
    class EntId { public int Index; }

    class Ent {

        public bool Active { get; set; } = false;
        public EntId Id { get; set; } = new EntId { Index = -1 };

        string tag = $"entity";
        public string Tag { get => tag; set => tag = value; }

        Dictionary<Type, object> components = new Dictionary<Type, object>();

        public Dictionary<Type, object> Components { get => components; }

        public T Add<T> (T component) {
            components[typeof(T)] = component;
            return component;
        }

        public T Get<T>() => (T)components[typeof(T)];

        public T Get<T>(Type t) => (T)components[t];

        public bool Has<T>() => components.ContainsKey(typeof(T));
        public bool Has(Type t) => components.ContainsKey(t);

        public bool Match(params Type[] match) { 
            foreach (var t in match) {
                if (!Has(t)) { return false; }
            }
            return true;
        }
    }

    abstract class Filter {
        public abstract void OnLoad(Ent ent);
        public abstract void OnCleanup(Ent ent);
        public abstract void Update();
        internal abstract void Render();
    }

    class Ecs {
        private Ecs() { } 
        private static Ecs instance = null;

        public static Ecs It {
            get {
                if (instance is null) { instance = new Ecs(); }
                return instance;
            }
        }

        List<Ent> entities = new List<Ent>(1000);
        List<Filter> filters = new List<Filter>();

        public Ent Get(EntId id) {
            return entities[id.Index];
        }

        public Option<Ent> FirstWith(params Type[] match) {
            foreach (var ent in Each(match)) return new Some<Ent>(ent);
            return new None<Ent>();
        }

        public IEnumerable<Ent> Each() {
            foreach (var ent in entities)
                yield return ent;
        }

        public IEnumerable<Ent> Each(params Type[] match) {
            foreach(var ent in entities) {
                if (!ent.Active) continue; 
                if (ent.Match(match)) yield return ent; 
            } 
        }

        public T Register<T> () where T : Filter, new() {
            var t = new T();
            filters.Add(t);
            return t;
        }

        public Ent Create() {
            int index = -1;
            for (int i = 0; i < entities.Count; i++) {
                if (!entities[i].Active) { index = i; break; }
            } 

            if (index < 0) {
                index = entities.Count;
                var ent = new Ent() { 
                    Active = true,
                    Id = new EntId { Index = index }
                };
                entities.Add(ent);
                return ent;
            } else {
                entities[index].Active = true;
                entities[index].Id.Index = index;
                return entities[index];
            }
        }

        public void Update() {
            filters.ForEach(f => f.Update());
        }

        public void Render() {
            filters.ForEach(f => f.Render()); 
        }
    }
}
