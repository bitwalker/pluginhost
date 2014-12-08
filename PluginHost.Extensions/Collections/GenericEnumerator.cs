using System;
using System.Collections;
using System.Collections.Generic;

namespace PluginHost.Extensions.Collections
{
    /// <summary>
    /// Provides the ability to create a strongly typed enumerator from
    /// any enumerator, as well as provide custom conversion and stepping
    /// functions for controlling enumeration of the source dataset.
    /// </summary>
    /// <typeparam name="T">The type of this enumerator</typeparam>
    public class TypedEnumerator<T> : IEnumerator<T>
    {
        private IEnumerator _enumerator;
        private Func<IEnumerator, bool> _moveNext;
        private Func<object, T> _getCurrent;
 
        public TypedEnumerator(IEnumerator enumerator)
            : this(enumerator, (obj) => (T) obj) {}

        public TypedEnumerator(IEnumerator enumerator, Func<object, T> converter)
            : this(enumerator, converter, (e) => e.MoveNext()) {}

        public TypedEnumerator(IEnumerator enumerator, Func<object, T> converter, Func<IEnumerator, bool> moveNext)
        {
            if (enumerator == null)
                throw new ArgumentNullException("enumerator", "Enumerator cannot be null!");
            if (converter == null)
                throw new ArgumentNullException("converter", "Conversion function cannot be null!");
            if (moveNext == null)
                throw new ArgumentNullException("moveNext", "Step function cannot be null!");

            _enumerator = enumerator;
            _moveNext   = moveNext;
            _getCurrent = converter;
        }

        public T Current
        {
            get { return _getCurrent(_enumerator.Current); }
        }

        public void Dispose()
        {
            if (_enumerator != null)
                _enumerator = null;
            if (_getCurrent != null)
                _getCurrent = null;
            if (_moveNext != null)
                _moveNext = null;
        }

        object System.Collections.IEnumerator.Current
        {
            get
            {
                if (_enumerator != null && _getCurrent != null)
                    return _getCurrent(_enumerator.Current);
                else
                    return default(T);
            }
        }

        public bool MoveNext()
        {
            if (_enumerator != null && _getCurrent != null)
                return _moveNext(_enumerator);
            else
                return false;
        }

        public void Reset()
        {
            if (_enumerator != null)
                _enumerator.Reset();
        }
    }
}
