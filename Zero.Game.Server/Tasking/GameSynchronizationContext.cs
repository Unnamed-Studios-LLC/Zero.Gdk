using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Zero.Game.Server
{
    internal sealed class GameSynchronizationContext : SynchronizationContext
    {
        public struct SyncScope : IDisposable
        {
            private readonly SynchronizationContext _context;

            public SyncScope(SynchronizationContext context)
            {
                _context = context;
            }

            public void Dispose()
            {
                SynchronizationContext.SetSynchronizationContext(_context);
            }
        }

        private struct ActionRequest
        {
            private readonly SendOrPostCallback _callback;
            private readonly object _state;
            private readonly ManualResetEvent _waitEvent;

            public ActionRequest(SendOrPostCallback callback, object state, ManualResetEvent waitEvent = null)
            {
                _callback = callback;
                _state = state;
                _waitEvent = waitEvent;
            }

            public void Invoke()
            {
                try
                {
                    _callback(_state);
                }
                finally
                {
                    _waitEvent?.Set();
                }
            }
        }

        private const int InitialCapacity = 20;

        private readonly List<ActionRequest> _pendingActions;
        private readonly List<ActionRequest> _actionsToExecute = new(InitialCapacity);
        private readonly int _mainThreadId;
        private int _count = 0;

        private GameSynchronizationContext(int mainThreadId)
        {
            _pendingActions = new List<ActionRequest>(InitialCapacity);
            _mainThreadId = mainThreadId;
        }

        private GameSynchronizationContext(List<ActionRequest> pendingActions, int mainThreadId)
        {
            _pendingActions = pendingActions;
            _mainThreadId = mainThreadId;
        }

        public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == Instance._mainThreadId;
        private static GameSynchronizationContext Instance { get; set; }

        public static void Close(int msTimeout)
        {
            WaitForPending(msTimeout);
            SetSynchronizationContext(null);
        }

        public static SyncScope CreateScope()
        {
            var current = Current;
            SetSynchronizationContext(Instance);
            return new SyncScope(current);
        }

        public static void InitializeOnCurrentThread()
        {
            Instance = new GameSynchronizationContext(Thread.CurrentThread.ManagedThreadId);
            SetSynchronizationContext(Instance);
        }

        public static void Run()
        {
            if (Current is not GameSynchronizationContext context)
            {
                return;
            }
            context.Execute();
        }

        public override SynchronizationContext CreateCopy()
        {
            return new GameSynchronizationContext(_pendingActions, _mainThreadId);
        }

        public override void OperationStarted()
        {
            Interlocked.Increment(ref _count);
        }
        public override void OperationCompleted()
        {
            Interlocked.Decrement(ref _count);
        }

        public override void Post(SendOrPostCallback callback, object state)
        {
            lock (_pendingActions)
            {
                _pendingActions.Add(new ActionRequest(callback, state));
            }
        }

        public override void Send(SendOrPostCallback callback, object state)
        {
            if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                callback(state);
            }
            else
            {
                using var waitHandle = new ManualResetEvent(false);
                lock (_pendingActions)
                {
                    _pendingActions.Add(new ActionRequest(callback, state, waitHandle));
                }
                waitHandle.WaitOne();
            }
        }

        private static bool WaitForPending(long msTimeout)
        {
            if (Current is not GameSynchronizationContext context)
            {
                return true;
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (context.HasPendingTasks())
            {
                if (stopwatch.ElapsedMilliseconds > msTimeout)
                {
                    break;
                }

                context.Execute();
                Thread.Sleep(1);
            }

            return !context.HasPendingTasks();
        }

        private void Execute()
        {
            lock (_pendingActions)
            {
                _actionsToExecute.AddRange(_pendingActions);
                _pendingActions.Clear();
            }

            while (_actionsToExecute.Count > 0)
            {
                var action = _actionsToExecute[0];
                _actionsToExecute.RemoveAt(0);
                action.Invoke();
            }
        }

        private bool HasPendingTasks()
        {
            return _pendingActions.Count != 0 ||
                _count != 0;
        }
    }
}
