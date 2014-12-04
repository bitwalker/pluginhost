using System;

namespace PluginHost.Extensions.Functional
{
    public static class Option
    {
        public static Option<T> ToOption<T>(this T @this)
        {
            if (@this == null)
                return Option<T>.None;
            else
                return Option<T>.Some(@this);
        }
    }

    [Serializable]
    public sealed class Option<T> : IEquatable<Option<T>>
    {
        private readonly T _value;
        private readonly bool _hasValue;

        /// <summary>
        /// Default empty instance.
        /// </summary>
        public static readonly Option<T> None = new Option<T>(default(T), false);

        /// <summary>
        /// Returns a non-empty Option. If a null value is provided, 
        /// an argument exception is thrown.
        /// </summary>
        /// <param name="value">A value of type T</param>
        /// <returns></returns>
        public static Option<T> Some(T value)
        {
            return new Option<T>(value);
        }

        private Option(T item, bool hasValue)
        {
            _value = item;
            _hasValue = hasValue;
        }

        private Option(T value) : this(value, true)
        {
            if (value == null) throw new ArgumentNullException("value");
        }

        /// <summary> 
        /// Gets the underlying value, if it is available
        /// </summary>
        public T Value
        {
            get
            {
                if (_hasValue == false)
                    throw new InvalidOperationException("Accessed Option<T>.Value when HasValue is false. Use Option<T>.GetValueOrDefault instead of Option<T>.Value");
                return _value;
            }
        }

        /// <summary> 
        /// Gets a value indicating whether this instance has value. 
        /// </summary>
        public bool HasValue
        {
            get { return _hasValue; }
        }

        /// <summary>
        /// Get the stored value, or the default value for it's type
        /// </summary>
        /// <returns></returns>
        public T GetValueOrDefault()
        {
            return _hasValue ? _value : default(T);
        }

        /// <summary>
        /// Get the stored value, or return the provided default value
        /// </summary>
        public T GetValueOrDefault(T @default)
        {
            return _hasValue ? _value : @default;
        }

        /// <summary>
        /// Get the stored value, or the provided default if the Option[T] is None
        /// </summary>
        public T GetValueOrDefault(Func<T> @default)
        {
            return _hasValue ? _value : @default();
        }

        /// <summary>
        /// Apply an action to the value, if present
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Option<T> Apply(Action<T> action)
        {
            if (_hasValue)
                action(_value);
            return this;
        }

        /// <summary>
        /// Select a new value from the value of the Option, if it exists
        /// otherwise provides an instance of Option.None
        /// </summary>
        public Option<U> Select<U>(Func<T, U> selector)
        {
            if (_hasValue == false)
                return Option<U>.None;
            else
            {
                var selected = selector(_value);
                if (selected == null)
                    return Option<U>.None;
                else
                    return new Option<U>(selected);
            }
        }

        /// <summary>
        /// Determines whether the provided Option is equal to the current Option
        /// </summary>
        public bool Equals(Option<T> option)
        {
            if (ReferenceEquals(null, option)) return false;
            if (ReferenceEquals(this, option)) return true;

            if (_hasValue != option._hasValue) return false;
            if (_hasValue == false) return true;
            return _value.Equals(option._value);
        }

        /// <summary>
        /// Determines whether the provided object is equal to the current Option.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            var maybe = obj as Option<T>;
            if (maybe == null) return false;
            return Equals(maybe);
        }

        public override int GetHashCode()
        {
            if (_hasValue)
            {
                // 41 is just an odd prime, the likelihood of encountering it is not high in comparison to
                // 0 for example. We just want a good seed value, and 41 is my choice :)
                return 41*_value.GetHashCode();
            }
            else
            {
                return 0;
            }
        }

        public static bool operator ==(Option<T> left, Option<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Option<T> left, Option<T> right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Performs an implicit conversion from T to Option[T]
        /// </summary>
        public static implicit operator Option<T>(T item)
        {
            if (item == null)
                return Option<T>.None;
            else
                return new Option<T>(item);
        }

        /// <summary>
        /// Performs an explicit conversion from Option[T] to T
        /// </summary>
        public static explicit operator T(Option<T> item)
        {
            if (item == null) throw new ArgumentNullException("item");

            return item.HasValue ? item.Value : default(T);
        }

        /// <summary>
        /// Returns a string representing the Option's value
        /// </summary>
        public override string ToString()
        {
            if (_hasValue)
                return "Some<" + _value + ">";

            return "None";
        }
    }
}
