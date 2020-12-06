using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tEngine.Helpers {
    public class IndexedObject<T> {
        private readonly int mIndex;
        private readonly T mRootObject;

        public int Index {
            get { return mIndex; }
        }

        public T Value {
            get { return mRootObject; }
        }

        public IndexedObject( T rootObject, int index ) {
            mRootObject = rootObject;
            mIndex = index;
        }
    }
}