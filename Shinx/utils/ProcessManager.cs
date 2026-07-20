using System;
using System.Collections.Generic;
using System.Threading;
using Shinx.Commands;

namespace Shinx
{
    public static class ProcessManager
    {
        public struct ProcessEntry
        {
            public int Pid;
            public string Name;
            public Thread Thread;
            public ICancellable Cancellable;
        }

        private static readonly List<ProcessEntry> _processes = new List<ProcessEntry>();
        private static readonly object _lock = new object();
        private static int _nextPid = 1;

        public static int Register(string name, Thread thread, ICancellable cancellable = null)
        {
            int pid;
            lock (_lock)
            {
                pid = _nextPid++;
                _processes.Add(new ProcessEntry
                {
                    Pid = pid,
                    Name = name,
                    Thread = thread,
                    Cancellable = cancellable
                });
            }
            return pid;
        }

        public static void Unregister(int pid)
        {
            lock (_lock)
            {
                for (int i = 0; i < _processes.Count; i++)
                {
                    if (_processes[i].Pid == pid)
                    {
                        _processes.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        public static List<ProcessEntry> List()
        {
            var alive = new List<ProcessEntry>();
            lock (_lock)
            {
                for (int i = _processes.Count - 1; i >= 0; i--)
                {
                    var e = _processes[i];
                    if (e.Thread != null && e.Thread.IsAlive)
                        alive.Add(e);
                    else
                        _processes.RemoveAt(i);
                }
            }
            return alive;
        }

        public static bool Stop(int pid)
        {
            ProcessEntry? found = null;
            lock (_lock)
            {
                for (int i = 0; i < _processes.Count; i++)
                {
                    if (_processes[i].Pid == pid)
                    {
                        found = _processes[i];
                        break;
                    }
                }
            }

            if (found == null) return false;
            var e = found.Value;

            try
            {
                if (e.Cancellable != null)
                    e.Cancellable.Cancel();
                else if (e.Thread != null && e.Thread.IsAlive)
                    e.Thread.Interrupt();
            }
            catch { }
            try
            {
                if (e.Thread != null && e.Thread.IsAlive)
                    e.Thread.Join(2000);
            }
            catch { }

            lock (_lock)
            {
                for (int i = 0; i < _processes.Count; i++)
                {
                    if (_processes[i].Pid == pid)
                    {
                        _processes.RemoveAt(i);
                        return true;
                    }
                }
            }
            return true;
        }

        public static void StopAll()
        {
            ProcessEntry[] snapshot;
            lock (_lock)
            {
                snapshot = _processes.ToArray();
            }
            foreach (var e in snapshot)
            {
                try
                {
                    if (e.Cancellable != null)
                        e.Cancellable.Cancel();
                    else if (e.Thread != null && e.Thread.IsAlive)
                        e.Thread.Interrupt();
                }
                catch { }
                try
                {
                    if (e.Thread != null && e.Thread.IsAlive)
                        e.Thread.Join(2000);
                }
                catch { }
            }
            lock (_lock)
            {
                _processes.Clear();
            }
        }
    }
}
