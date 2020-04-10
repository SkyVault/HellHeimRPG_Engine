using System;
using System.Collections.Generic;
using System.Text;

namespace Harp {
    class It<T> {
        readonly IList<T> _ts;

        int _index = 0;

        public It(IList<T> ts, int off = 0) {
            _index = off;
            this._ts = ts;
        }

        public T Current
        {
            get
            {
                if (Eof) return default;
                return _ts[_index]; 
            }
        }

        public bool Eof { get => !(_index < _ts.Count); }

        public bool Next() {
            if (Eof) return false; 
            _index++;
            return true;
        }

        public T Peek(int off = 1) {
            if (_index + off < _ts.Count) return _ts[_index + off];
            return default;
        }

        public override bool Equals(object obj) {
            return obj is It<T> it &&
                   EqualityComparer<IList<T>>.Default.Equals(_ts, it._ts) &&
                   _index == it._index &&
                   EqualityComparer<T>.Default.Equals(Current, it.Current) &&
                   Eof == it.Eof &&
                   EqualityComparer<It<T>>.Default.Equals(Ref, it.Ref);
        }

        public override int GetHashCode() {
            return HashCode.Combine(_ts, _index, Current, Eof, Ref);
        }

        public It<T> Ref => new It<T>(_ts, _index);

        public static bool operator ==(It<T> a, It<T> b) { 
            return a._index == b._index && a._ts == b._ts;
        }

        public static bool operator !=(It<T> a, It<T> b) { 
            return a._index != b._index || a._ts != b._ts;
        }
    }
}
