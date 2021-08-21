using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Sheed
{
    public static class TaskUtility
    {
#if UNITY_EDITOR
        // [UnityEditor.MenuItem("Sheed/TaskUtility test")]
        static void Test()
        {
            Task.Delay(TimeSpan.FromSeconds(0)).Then(() =>
            {
                Debug.Log("throw first");
                throw new Exception("first");
            })
            .Then(() => Debug.Log("second phase"))
            .Catch(e => Debug.LogError($"first exception: {e}"))
            .Then(() => Debug.Log("third phase"))
            .Catch(e => Debug.LogError($"second exception: {e}"))
            ;
        }
#endif
        // javascript Promise style
        public static Task<To> Then<From, To>(this Task<From> original, Func<From, To> convert) => original.ContinueWith(
            task =>
            {
                if (task.IsFaulted)
                    return Task.FromException<To>(task.Exception.InnerException);

                return Task.FromResult(convert(task.GetAwaiter().GetResult()));
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();

        public static Task<To> Then<From, To>(this Task<From> original, Func<From, Task<To>> convert) => original.ContinueWith(
            task =>
            {
                if (task.IsFaulted)
                    return Task.FromException<To>(task.Exception.InnerException);

                return convert(task.GetAwaiter().GetResult());
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();

        public static Task Then<From>(this Task<From> original, Func<From, Task> convert) => original.ContinueWith(
            task =>
            {
                if (task.IsFaulted)
                    return Task.FromException(task.Exception.InnerException);

                return convert(task.GetAwaiter().GetResult());
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();

        public static Task Then<From>(this Task<From> original, Action<From> convert) => original.ContinueWith(
            task =>
            {
                if (task.IsFaulted)
                    return Task.FromException(task.Exception.InnerException);

                convert(task.GetAwaiter().GetResult());
                return Task.CompletedTask;
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();

        public static Task<To> Then<To>(this Task original, Func<To> convert) => original.ContinueWith(
            task =>
            {
                if (task.IsFaulted)
                    return Task.FromException<To>(task.Exception.InnerException);

                return Task.FromResult(convert());
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();

        public static Task<To> Then<To>(this Task original, Func<Task<To>> convert) => original.ContinueWith(
            task =>
            {
                if (task.IsFaulted)
                    return Task.FromException<To>(task.Exception.InnerException);

                return convert();
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();

        public static Task Then(this Task original, Func<Task> convert) => original.ContinueWith(
            task =>
            {
                if (task.IsFaulted)
                    return Task.FromException(task.Exception.InnerException);

                return convert();
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();


        public static Task Then(this Task original, Action convert) => original.ContinueWith(
            task =>
            {
                if (task.IsFaulted)
                    return Task.FromException(task.Exception.InnerException);

                convert();
                return Task.CompletedTask;
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();


        public static Task<From> Catch<From>(this Task<From> original, Func<Exception, From> convert) => original.ContinueWith(
            task =>
            {
                if (task.IsFaulted)
                    return Task.FromResult(convert(task.Exception.InnerException));

                if (task.IsCanceled)
                    return Task.FromResult(convert(new TaskCanceledException()));

                return task;
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();

        public static Task Catch(this Task original, Action<Exception> convert) => original.ContinueWith(
            task =>
            {
                if (task.IsFaulted)
                {
                    convert(task.Exception.InnerException);
                    return Task.CompletedTask;
                }

                if (task.IsCanceled)
                {
                    convert(new TaskCanceledException());
                    return Task.CompletedTask;
                }

                return task;
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();

        public static Task<To> Finally<From, To>(this Task<From> original, Func<From, To> convert) => original.ContinueWith(
            task =>
            {
                return Task.FromResult(convert(task.GetAwaiter().GetResult()));
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();

        public static Task<To> Finally<From, To>(this Task<From> original, Func<From, Task<To>> convert) => original.ContinueWith(
            task =>
            {
                return convert(task.GetAwaiter().GetResult());
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();

        public static Task Finally<From>(this Task<From> original, Func<From, Task> convert) => original.ContinueWith(
            task =>
            {
                return convert(task.GetAwaiter().GetResult());
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();

        public static Task Finally<From>(this Task<From> original, Action<From> convert) => original.ContinueWith(
            task =>
            {
                convert(task.GetAwaiter().GetResult());
                return Task.CompletedTask;
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();

        public static Task<To> Finally<To>(this Task original, Func<To> convert) => original.ContinueWith(
            task =>
            {
                return Task.FromResult(convert());
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();

        public static Task<To> Finally<To>(this Task original, Func<Task<To>> convert) => original.ContinueWith(
            task =>
            {
                return convert();
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();

        public static Task Finally(this Task original, Func<Task> convert) => original.ContinueWith(
            task =>
            {
                return convert();
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();

        public static Task Finally(this Task original, Action convert) => original.ContinueWith(
            task =>
            {
                convert();
                return Task.CompletedTask;
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();
    }
}
