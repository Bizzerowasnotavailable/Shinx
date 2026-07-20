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
        private static int _nextPid = 1;

        public static int Register(string name, Thread thread, ICancellable cancellable = null)
        {
            int pid = _nextPid++;
            _processes.Add(new ProcessEntry
            {
                Pid = pid,
                Name = name,
                Thread = thread,
                Cancellable = cancellable
            });
            return pid;
        }

        public static void Unregister(int pid)
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

        public static List<ProcessEntry> List()
        {
            var alive = new List<ProcessEntry>();
            for (int i = _processes.Count - 1; i >= 0; i--)
            {
                var e = _processes[i];
                if (e.Thread != null && e.Thread.IsAlive)
                    alive.Add(e);
                else
                    _processes.RemoveAt(i);
            }
            return alive;
        }

        public static bool Stop(int pid)
        {
            for (int i = 0; i < _processes.Count; i++)
            {
                if (_processes[i].Pid == pid)
                {
                    var e = _processes[i];
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
                    _processes.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public static void StopAll()
        {
            for (int i = _processes.Count - 1; i >= 0; i--)
            {
                var e = _processes[i];
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
            _processes.Clear();
        }
    }
}
