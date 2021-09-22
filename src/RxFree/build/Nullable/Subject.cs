// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.

// https://github.com/dotnet/reactive/blob/main/Rx.NET/Source/src/System.Reactive/Subjects/Subject.cs
// https://github.com/dotnet/reactive/blob/main/LICENSE

using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System
{
    /// <summary>
    /// Represents an object that is both an observable sequence as well as an observer.
    /// Each notification is broadcasted to all subscribed observers.
    /// </summary>
    /// <typeparam name="T">The type of the elements processed by the subject.</typeparam>
    [GeneratedCode("RxFree", "*")]
    [CompilerGenerated]
    [ExcludeFromCodeCoverage]
    internal class Subject<T> : IObserver<T>, IObservable<T>, IDisposable
    {
        static readonly SubjectDisposable[] Terminated = new SubjectDisposable[0];
        static readonly SubjectDisposable[] Disposed = new SubjectDisposable[0];

        SubjectDisposable[] observers;
        Exception? exception;

        /// <summary>
        /// Creates a subject.
        /// </summary>
        public Subject() => observers = Array.Empty<SubjectDisposable>();

        /// <summary>
        /// Indicates whether the subject has observers subscribed to it.
        /// </summary>
        public virtual bool HasObservers => Volatile.Read(ref observers).Length != 0;

        /// <summary>
        /// Indicates whether the subject has been disposed.
        /// </summary>
        public virtual bool IsDisposed => Volatile.Read(ref this.observers) == Disposed;

        static void ThrowDisposed() => throw new ObjectDisposedException(string.Empty);

        /// <summary>
        /// Notifies all subscribed observers about the end of the sequence.
        /// </summary>
        public virtual void OnCompleted()
        {
            for (; ; )
            {
                var observers = Volatile.Read(ref this.observers);
                if (observers == Disposed)
                {
                    exception = null;
                    ThrowDisposed();
                    break;
                }
                if (observers == Terminated)
                {
                    break;
                }
                if (Interlocked.CompareExchange(ref this.observers, Terminated, observers) == observers)
                {
                    foreach (var observer in observers)
                    {
                        observer.Observer?.OnCompleted();
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Notifies all subscribed observers about the specified exception.
        /// </summary>
        /// <param name="error">The exception to send to all currently subscribed observers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
        public virtual void OnError(Exception error)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            for (; ; )
            {
                var observers = Volatile.Read(ref this.observers);
                if (observers == Disposed)
                {
                    exception = null;
                    ThrowDisposed();
                    break;
                }
                if (observers == Terminated)
                {
                    break;
                }
                exception = error;
                if (Interlocked.CompareExchange(ref this.observers, Terminated, observers) == observers)
                {
                    foreach (var observer in observers)
                    {
                        observer.Observer?.OnError(error);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Notifies all subscribed observers about the arrival of the specified element in the sequence.
        /// </summary>
        /// <param name="value">The value to send to all currently subscribed observers.</param>
        public virtual void OnNext(T value)
        {
            var observers = Volatile.Read(ref this.observers);
            if (observers == Disposed)
            {
                exception = null;
                ThrowDisposed();
                return;
            }
            foreach (var observer in observers)
            {
                observer.Observer?.OnNext(value);
            }
        }

        /// <summary>
        /// Subscribes an observer to the subject.
        /// </summary>
        /// <param name="observer">Observer to subscribe to the subject.</param>
        /// <returns>Disposable object that can be used to unsubscribe the observer from the subject.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="observer"/> is null.</exception>
        public virtual IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            var disposable = default(SubjectDisposable);
            for (; ; )
            {
                var observers = Volatile.Read(ref this.observers);
                if (observers == Disposed)
                {
                    exception = null;
                    ThrowDisposed();
                    break;
                }
                if (observers == Terminated)
                {
                    var ex = exception;
                    if (ex != null)
                    {
                        observer.OnError(ex);
                    }
                    else
                    {
                        observer.OnCompleted();
                    }
                    break;
                }

                disposable ??= new SubjectDisposable(this, observer);

                var n = observers.Length;
                var b = new SubjectDisposable[n + 1];

                Array.Copy(observers, 0, b, 0, n);

                b[n] = disposable;
                if (Interlocked.CompareExchange(ref this.observers, b, observers) == observers)
                {
                    return disposable;
                }
            }

            return Disposable.Empty;
        }

        void Unsubscribe(SubjectDisposable observer)
        {
            for (; ; )
            {
                var a = Volatile.Read(ref observers);
                var n = a.Length;
                if (n == 0)
                {
                    break;
                }

                var j = Array.IndexOf(a, observer);

                if (j < 0)
                {
                    break;
                }

                SubjectDisposable[] b;

                if (n == 1)
                {
                    b = Array.Empty<SubjectDisposable>();
                }
                else
                {
                    b = new SubjectDisposable[n - 1];
                    Array.Copy(a, 0, b, 0, j);
                    Array.Copy(a, j + 1, b, j, n - j - 1);
                }
                if (Interlocked.CompareExchange(ref observers, b, a) == a)
                {
                    break;
                }
            }
        }

        class Disposable : IDisposable
        {
            public static IDisposable Empty { get; } = new Disposable();

            Disposable() { }

            public void Dispose() { }
        }

        sealed class SubjectDisposable : IDisposable
        {
            Subject<T> subject;
            volatile IObserver<T>? observer;

            public SubjectDisposable(Subject<T> subject, IObserver<T> observer)
            {
                this.subject = subject;
                this.observer = observer;
            }

            public IObserver<T>? Observer => observer;

            public void Dispose()
            {
                var observer = Interlocked.Exchange(ref this.observer, null);
                if (observer == null)
                {
                    return;
                }

                subject.Unsubscribe(this);
                subject = null!;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Subject{T}"/> class and unsubscribes all observers.
        /// </summary>
        public virtual void Dispose()
        {
            Interlocked.Exchange(ref observers, Disposed);
            exception = null;
        }
    }
}
