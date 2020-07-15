using esper.parsing;
using System;

namespace esper.data {
    public class IntData<T> : DataContainer {
        public T data;

        public IntData(T data) {
            this.data = data;
        }

        public override string ToString() {
            return data.ToString();
        }
    }
}
