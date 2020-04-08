using System;
using System.Collections.Generic;
using System.Text;

namespace Harp {
    class It<T> {
        readonly IList<T> ts;

        int index = 0;

        public It(IList<T> _ts, int off = 0) {
            index = off;
            ts = _ts;
        }

        public T Current { get => ts[index]; }
        public bool Eof { get => !(index < ts.Count); }

        public bool Next() {
            if (Eof) return false; 
            index++;
            return true;
        }

        public T Peek(int off = 1) {
            if (index + off < ts.Count) return ts[index + off];
            return default;
        }

        public override bool Equals(object obj) {
            return obj is It<T> it &&
                   EqualityComparer<IList<T>>.Default.Equals(ts, it.ts) &&
                   index == it.index &&
                   EqualityComparer<T>.Default.Equals(Current, it.Current) &&
                   Eof == it.Eof &&
                   EqualityComparer<It<T>>.Default.Equals(Ref, it.Ref);
        }

        public override int GetHashCode() {
            return HashCode.Combine(ts, index, Current, Eof, Ref);
        }

        public It<T> Ref => new It<T>(ts, index);

        public static bool operator ==(It<T> a, It<T> b) { 
            return a.index == b.index && a.ts == b.ts;
        }

        public static bool operator !=(It<T> a, It<T> b) { 
            return a.index != b.index || a.ts != b.ts;
        }
    }
}
