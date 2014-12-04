using System;
using System.Threading.Tasks;

namespace PluginHost.Extensions.Functional
{
    /// <summary>
    /// Defines the result type of an operation that can either be an error or success value.
    /// This is intended for use in places where error handling should be deferred to a higher
    /// level. This can reduce the amount of boilerplate required at each level of the API, as
    /// only the top level (say a controller) needs to determine what to do with the failure case.
    /// 
    /// Example Usage (failable operation):
    /// 
    /// public static Either<SomeError, List<Stuff>> GetStuff()
    /// {
    ///     try
    ///     {
    ///         return _db.Stuff.Where(s => s.Things.Contains("foo")).ToList();
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         return new SomeError("Unable to get stuff: " + ex.Message);
    ///     }
    /// }
    /// 
    /// Example Usage (top level consumer):
    /// 
    /// [HttpGet]
    /// public HttpResponseMessage GetStuff()
    /// {
    ///     return _provider
    ///              .GetStuff()
    ///              .Map(ConvertToViewModel)
    ///              .Case(
    ///                error => BadResponse(error.Message),
    ///                model => OkResponse(model)
    ///              );
    /// } 
    /// 
    /// The above is a very simplistic example, but the point is that each level of your
    /// API can transform the potential success value on the way up (via Map), or return
    /// a value representing a higher-level error, and each level only has to be concerned
    /// with the errors that itself produces.
    /// </summary>
    /// <typeparam name="TError">Type of the Error value</typeparam>
    /// <typeparam name="TSuccess">Type of the Success value</typeparam>
    public sealed class Either<TError, TSuccess>
    {
        private readonly bool _isError = false;
        private readonly TError _error;
        private readonly TSuccess _success;

        public bool IsError { get { return _isError; } }
        /// <summary>
        /// Only for use when absolutely necessary. Use one of the accessor functions
        /// in all other cases.
        /// </summary>
        public TError Error { get { return _error; }}
        /// <summary>
        /// Only for use when absolutely necessary. Use one of the accessor functions
        /// in all other cases.
        /// </summary>
        public TSuccess Success { get { return _success; }}

        private Either(TError error)
        {
            _isError = true;
            _error   = error;
        }

        private Either(TSuccess value)
        {
            _isError = false;
            _success = value;
        }

        /// <summary>
        /// Check the type of the value held and invoke the matching handler function
        /// </summary>
        /// <typeparam name="U">The result type of the operation</typeparam>
        /// <param name="onError">When the Either contains an Error, this function is called.</param>
        /// <param name="onSuccess">When the Either contains Success, this function is called.</param>
        public U Case<U>(Func<TError, U> onError, Func<TSuccess, U> onSuccess)
        {
            if (_isError)
            {
                if (onError == null)
                    throw new ArgumentNullException("onError cannot be null");

                return onError(_error);
            }
            else
            {
                if (onSuccess == null)
                    throw new ArgumentNullException("onSuccess cannot be null");

                return onSuccess(_success);
            }
        }

        /// <summary>
        /// Check the type of the value held and invoke the matching handler function
        /// </summary>
        /// <param name="onError">When the Either contains an Error, this action is called.</param>
        /// <param name="onSuccess">When the Either contains Success, this action is called.</param>
        public void Case(Action<TError> onError, Action<TSuccess> onSuccess)
        {
            if (_isError)
            {
                if (onError == null)
                    throw new ArgumentNullException("onError cannot be null");
                onError(_error);
            }
            else
            {
                if (onSuccess == null)
                    throw new ArgumentNullException("onSuccess cannot be null");
                onSuccess(_success);
            }
        }

        /// <summary>
        /// Async version of Case, where both error and success handlers return a Task which
        /// should be executed to unwrap the result value.
        /// </summary>
        public async Task<U> CaseAsync<U>(Func<TError, Task<U>> onError, Func<TSuccess, Task<U>> onSuccess)
        {
            if (_isError)
            {
                if (onError == null)
                    throw new ArgumentNullException("onError cannot be null");

                return await onError(_error);
            }
            else
            {
                if (onSuccess == null)
                    throw new ArgumentNullException("onSuccess cannot be null");

                return await onSuccess(_success);
            }
        }

        /// <summary>
        /// A variation of CaseAsync, where the success case requires some expensive operation to obtain,
        /// which we wish to be delegated to another thread.
        /// </summary>
        public Task<U> CaseAsync<U>(Func<TError, U> onError, Func<TSuccess, Task<U>> onSuccess)
        {
            return CaseAsync(e => Task.FromResult(onError(e)), onSuccess);
        }

        /// <summary>
        /// A variation of CaseAsync, where the error case requires some expensive operation to be performed,
        /// such as logging to a database, which we wish to be delegated to another thread.
        /// </summary>
        public Task<U> CaseAsync<U>(Func<TError, Task<U>> onError, Func<TSuccess, U> onSuccess)
        {
            return CaseAsync(onError, s => Task.FromResult(onSuccess(s)));
        }

        /// <summary>
        /// Async version of Case, where both error and success cases do not return any value,
        /// but require some operation to be performed to handle each case.
        /// </summary>
        public Task CaseAsync(Action<TError> onError, Action<TSuccess> onSuccess)
        {
            if (_isError)
            {
                if (onError == null)
                    throw new ArgumentNullException("onError cannot be null");
                return Task.Run(() => onError(_error));
            }
            else
            {
                if (onSuccess == null)
                    throw new ArgumentNullException("onSuccess cannot be null");
                return Task.Run(() => onSuccess(_success));
            }
        }

        /// <summary>
        /// Map a transformation over the Either, which will be applied if the Either
        /// contains a Success value. If the Either contains an Error value, then the
        /// Error value will be propagated.
        /// </summary>
        /// <typeparam name="TError">The type of the Error value</typeparam>
        /// <typeparam name="TSuccess">The type of the current Success value</typeparam>
        /// <typeparam name="S">The type of the transformed Success value</typeparam>
        /// <param name="mapper">A function which takes a TSuccess and transforms it to a new value.</param>
        /// <returns>A new IEither, where the success type is the result type of the map operation.</returns>
        public Either<TError, S> Map<S>(Func<TSuccess, S> mapper)
        {
            if (_isError)
            {
                return new Either<TError, S>(_error);
            }
            else
            {
                return new Either<TError, S>(mapper(_success));
            }
        }

        /// <summary>
        /// Map an asynchronous transformation over the Either, which is only applied if the
        /// Either contains a success value. If the Either contains an Error, the Error value
        /// is propogated.
        /// </summary>
        public async Task<Either<TError, S>> MapAsync<S>(Func<TSuccess, Task<S>> mapper)
        {
            if (_isError)
            {
                return new Either<TError, S>(_error);
            }
            else
            {
                var result = await mapper(_success);
                return new Either<TError, S>(result);
            }
        }

        /// <summary>
        /// Map a transformation over the Either, which will be applied if the Either
        /// contains a Success value. If the Either contains an Error value, then the Error
        /// value will be propagated. This will flatten a nested Either, so that you do not
        /// have to do nested Map/Case calls.
        /// </summary>
        /// <typeparam name="S">The new Success type</typeparam>
        /// <param name="mapper">A function which takes a TSuccess and transforms it to a new value.</param>
        public Either<TError, S> FlatMap<S>(Func<TSuccess, Either<TError, S>> mapper)
        {
            if (_isError)
            {
                return new Either<TError, S>(_error);
            }
            else
            {
                return mapper(_success);
            }
        }

        /// <summary>
        /// Map an asynchronous transformation over the Either, which will be applied if the Either
        /// contains a Success value. If the Either contains an Error value, then the Error
        /// value will be propogated. This will flatten a nested Either, so that you do not
        /// have to do nested Map/Case calls.
        /// </summary>
        public async Task<Either<TError, S>> FlatMapAsync<S>(Func<TSuccess, Task<Either<TError, S>>> mapper)
        {
            if (_isError)
            {
                return new Either<TError, S>(_error);
            }
            else
            {
                return await mapper(_success);
            }
        }

        /// <summary>
        /// Allow implicit conversion from TError to Either<TError, TSuccess>
        /// </summary>
        /// <param name="error">An instance of TError</param>
        public static implicit operator Either<TError, TSuccess>(TError error)
        {
            return new Either<TError, TSuccess>(error);
        }

        /// <summary>
        /// Allow implicit conversion from TSuccess to Either<TError, TSuccess>
        /// </summary>
        /// <param name="error">An instance of TSuccess</param>
        public static implicit operator Either<TError, TSuccess>(TSuccess value)
        {
            return new Either<TError, TSuccess>(value);
        }
    }

    public static class AsyncEitherExtensions
    {
        /// <summary>
        /// Given a Task-wrapped Either, chain the result of awaiting that Task into a call
        /// to Map. The primary difference between this and MapAsync is that it will not execute
        /// the mapping function asynchronously. Best used when you want to map simple transformations
        /// over the result of an expensive operation which returns an Either.
        /// </summary>
        public static async Task<Either<TError, U>> Map<TError, TSuccess, U>(this Task<Either<TError, TSuccess>> awaitable, Func<TSuccess, U> mapper)
        {
            var result = await awaitable;
            return result.Map(mapper);
        }

        /// <summary>
        /// Given a Task-wrapped Either, chain the result of awaiting that Task into a call
        /// to MapAsync on that result. In this way you can use Task-wrapped Either's the same
        /// way you do the non-wrapped version.
        /// </summary>
        public static async Task<Either<TError, U>> MapAsync<TError, TSuccess, U>(this Task<Either<TError, TSuccess>> awaitable, Func<TSuccess, Task<U>> mapper)
        {
            var result = await awaitable;
            return await result.MapAsync(mapper);
        }

        /// <summary>
        /// Given a Task-wrapped Either, chain the result of awaiting that Task into a call
        /// to FlatMap. The primary difference between this and FlatMapAsync is that it will not
        /// execute the mapping function asynchronously. Best used when you want to map simple transformations
        /// over the result of an expensive operation which returns an Either.
        /// </summary>
        public static async Task<Either<TError, U>> FlatMap<TError, TSuccess, U>(this Task<Either<TError, TSuccess>> awaitable, Func<TSuccess, Either<TError, U>> mapper)
        {
            var result = await awaitable;
            return result.FlatMap(mapper);
        }

        /// <summary>
        /// Given a Task-wrapped Either, chain the result of awaiting that Task into a call
        /// to FlatMapAsync on that result. In this way you can use Task-wrapped Either's the same
        /// way you do the non-wrapped version.
        /// </summary>
        public static async Task<Either<TError, U>> FlatMapAsync<TError, TSuccess, U>(this Task<Either<TError, TSuccess>> awaitable, Func<TSuccess, Task<Either<TError, U>>> mapper)
        {
            var result = await awaitable;
            return await result.FlatMapAsync(mapper);
        }

        /// <summary>
        /// Given a Task-wrapped Either, chain the result of awaiting that Task into a call
        /// to Case. In this way you can use Task-wrapped Either's the same way you do the
        /// non-wrapped version.
        /// </summary>
        public static async Task<U> Case<TError, TSuccess, U>(this Task<Either<TError, TSuccess>> awaitable, Func<TError, U> onError, Func<TSuccess, U> onSuccess)
        {
            var result = await awaitable;
            return result.Case(onError, onSuccess);
        }

        /// <summary>
        /// Given a Task-wrapped Either, chain the result of awaiting that Task into a call
        /// to Case. In this way you can use Task-wrapped Either's the same way you do the
        /// non-wrapped version.
        /// </summary>
        public static async Task Case<TError, TSuccess>(this Task<Either<TError, TSuccess>> awaitable, Action<TError> onError, Action<TSuccess> onSuccess)
        {
            var result = await awaitable;
            result.Case(onError, onSuccess);
        }

        /// <summary>
        /// Given a Task-wrapped Either, chain the result of awaiting that Task into a call
        /// to CaseAsync on that result. In this way you can use Task-wrapped Either's the same
        /// way you do the non-wrapped version.
        /// </summary>
        public static async Task<U> CaseAsync<TError, TSuccess, U>(this Task<Either<TError, TSuccess>> awaitable, Func<TError, Task<U>> onError, Func<TSuccess, Task<U>> onSuccess)
        {
            var result = await awaitable;
            return await result.CaseAsync(onError, onSuccess);
        }

        /// <summary>
        /// Given a Task-wrapped Either, chain the result of awaiting that Task into a call
        /// to CaseAsync on that result. In this way you can use Task-wrapped Either's the same
        /// way you do the non-wrapped version.
        /// </summary>
        public static async Task<U> CaseAsync<TError, TSuccess, U>(this Task<Either<TError, TSuccess>> awaitable, Func<TError, Task<U>> onError, Func<TSuccess, U> onSuccess)
        {
            var result = await awaitable;
            return await result.CaseAsync(onError, onSuccess);
        }

        /// <summary>
        /// Given a Task-wrapped Either, chain the result of awaiting that Task into a call
        /// to CaseAsync on that result. In this way you can use Task-wrapped Either's the same
        /// way you do the non-wrapped version.
        /// </summary>
        public static async Task<U> CaseAsync<TError, TSuccess, U>(this Task<Either<TError, TSuccess>> awaitable, Func<TError, U> onError, Func<TSuccess, Task<U>> onSuccess)
        {
            var result = await awaitable;
            return await result.CaseAsync(onError, onSuccess);
        }

        /// <summary>
        /// Given a Task-wrapped Either, chain the result of awaiting that Task into a call
        /// to CaseAsync on that result. In this way you can use Task-wrapped Either's the same
        /// way you do the non-wrapped version.
        /// </summary>
        public static async Task CaseAsync<TError, TSuccess, U>(this Task<Either<TError, TSuccess>> awaitable, Action<TError> onError, Action<TSuccess> onSuccess)
        {
            var result = await awaitable;
            await result.CaseAsync(onError, onSuccess);
        }
    }
}
