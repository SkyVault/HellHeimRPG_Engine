using HellHeimRPG.Components;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HellHeimRPG {
    public class EntId { public int Index; }

    public class Ent { 
        public bool Active { get; set; } = false;
        public EntId Id { get; set; } = new EntId { Index = -1 };

        public string Tag { get; set; } = $"entity";

        public Dictionary<Type, object> Components { get; } = new Dictionary<Type, object>();

        public T Add<T> (T component) {
            Components[typeof(T)] = component;
            return component;
        }

        public T Get<T>() => (T)Components[typeof(T)];

        public T Get<T>(Type t) => (T)Components[t];

        public bool Has<T>() => Components.ContainsKey(typeof(T));
        public bool Has(Type t) => Components.ContainsKey(t);

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
        private static Ecs _instance = null;

        public static Ecs It {
            get {
                if (_instance is null) { _instance = new Ecs(); }
                return _instance;
            }
        }

        List<Ent> _entities = new List<Ent>(1000);
        List<Filter> _filters = new List<Filter>();

        public Ent Get(EntId id) {
            return _entities[id.Index];
        }

        public Option<Ent> FirstWith(params Type[] match) {
            foreach (var ent in Each(match)) return new Some<Ent>(ent);
            return new None<Ent>();
        }

        public IEnumerable<Ent> Each() {
            foreach (var ent in _entities)
                yield return ent;
        }

        public IEnumerable<Ent> Each(params Type[] match) {
            foreach(var ent in _entities) {
                if (!ent.Active) continue; 
                if (ent.Match(match)) yield return ent; 
            } 
        }

        public T Register<T> () where T : Filter, new() {
            var t = new T();
            _filters.Add(t);
            return t;
        }

        public Ent Create() {
            int index = -1;
            for (int i = 0; i < _entities.Count; i++) {
                if (!_entities[i].Active) { index = i; break; }
            } 

            if (index < 0) {
                index = _entities.Count;
                var ent = new Ent() { 
                    Active = true,
                    Id = new EntId { Index = index }
                };
                _entities.Add(ent);
                return ent;
            } else {
                _entities[index].Active = true;
                _entities[index].Id.Index = index;
                return _entities[index];
            }
        }

        public void Update() {
            _filters.ForEach(f => f.Update());
        }

        public void Render() {
            _filters.ForEach(f => f.Render()); 
        }
    }
}
