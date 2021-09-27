﻿// <auto-generated />
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace System
{
    /// <summary>
    /// Provides a set of static methods for subscribing delegates to observables.
    /// </summary>
    [GeneratedCode("RxFree", "*")]
    [CompilerGenerated]
    [ExcludeFromCodeCoverage]
    static partial class ObservableExtensions
    {
        static readonly Action<Exception> rethrow = e => ExceptionDispatchInfo.Capture(e).Throw();
        static readonly Action nop = () => { };

        /// <summary>
        /// Subscribes to the observable providing just the <paramref name="onNext"/> delegate.
        /// </summary>
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext)
            => Subscribe(source, onNext, rethrow, nop);

        /// <summary>
        /// Subscribes to the observable providing both the <paramref name="onNext"/> and 
        /// <paramref name="onError"/> delegates.
        /// </summary>
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError)
            => Subscribe(source, onNext, onError, nop);

        /// <summary>
        /// Subscribes to the observable providing both the <paramref name="onNext"/> and 
        /// <paramref name="onCompleted"/> delegates.
        /// </summary>
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action onCompleted)
            => Subscribe(source, onNext, rethrow, onCompleted);

        /// <summary>
        /// Subscribes to the observable providing all three <paramref name="onNext"/>, 
        /// <paramref name="onError"/> and <paramref name="onCompleted"/> delegates.
        /// </summary>
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError, Action onCompleted)
            => source.Subscribe(new AnonymousObserver<T>(onNext, onError, onCompleted));
    }
}
