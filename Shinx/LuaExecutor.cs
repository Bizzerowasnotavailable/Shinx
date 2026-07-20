using System;
using System.Collections.Generic;
using System.Threading;
using UniLua;

namespace Shinx
{
    public struct LuaResult
    {
        public ThreadStatus Status;
        public string Error;
    }

    public static class LuaExecutor
    {
        public static ILuaState State;

        private static Thread _mainThread;
        private static readonly Queue<Action> _queue = new Queue<Action>();
        private static readonly object _lock = new object();
        private static bool _initialized = false;

        public static void Init()
        {
            _mainThread = Thread.CurrentThread;
            _initialized = true;

            State = LuaAPI.NewState();
            State.L_OpenLibs();
            LuaBridge.Setup(State);
        }

        public static bool IsMainThread => _initialized && Thread.CurrentThread == _mainThread;

        public static LuaResult DoString(string code)
        {
            if (!_initialized || Thread.CurrentThread == _mainThread || DesktopManager.Running)
            {
                return ExecuteDoString(code);
            }

            LuaResult result = default;
            int completed = 0;

            lock (_lock)
            {
                _queue.Enqueue(() =>
                {
                    result = ExecuteDoString(code);
                    Interlocked.Exchange(ref completed, 1);
                });
            }

            while (Interlocked.CompareExchange(ref completed, 0, 0) == 0)
                Thread.Sleep(1);

            return result;
        }

        public static void Run(Action action)
        {
            if (!_initialized || Thread.CurrentThread == _mainThread || DesktopManager.Running)
            {
                action();
                return;
            }

            int completed = 0;

            lock (_lock)
            {
                _queue.Enqueue(() =>
                {
                    action();
                    Interlocked.Exchange(ref completed, 1);
                });
            }

            while (Interlocked.CompareExchange(ref completed, 0, 0) == 0)
                Thread.Sleep(1);
        }

        public static void ProcessPending()
        {
            if (!_initialized) return;

            while (true)
            {
                Action work = null;
                lock (_lock)
                {
                    if (_queue.Count > 0)
                        work = _queue.Dequeue();
                }
                if (work == null) break;
                work();
            }
        }

        private static LuaResult ExecuteDoString(string code)
        {
            var status = State.L_DoString(code);
            string error = null;
            if (status != ThreadStatus.LUA_OK)
            {
                error = State.L_ToString(-1);
                State.Pop(1);
            }
            return new LuaResult { Status = status, Error = error };
        }
    }
}
