using System;
using System.Collections.Generic;
using System.Text;

namespace HellHeimRPG {
    public abstract class Option<T> {
    };

    public class Some <T> : Option <T> {
        public Some(T value) { Value = value; }

        public T Value { get; private set; }

        public T Map(Func<T, T> func) {
            return func(Value);
        }
    }

    public class None <T> : Option <T> {
    }
}
