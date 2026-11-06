using System;
using System.Collections.Generic;
using Shinx.Audio;

namespace Shinx.Commands
{
    public class core_audio : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (parameters.Contains('h'))
            {
                Usage();
                return;
            }

            if (args.Length == 0)
            {
                AudioManager.PrintStatus();
                return;
            }

            string command = args[0].ToLowerInvariant();

            switch (command)
            {
                case "help":
                    Usage();
                    return;

                case "status":
                    if (!CheckFlags(parameters, "")) return;
                    AudioManager.PrintStatus();
                    return;

                case "devices":
                    if (!CheckFlags(parameters, "")) return;
                    AudioManager.PrintDevices();
                    return;

                case "volume":
                    if (!CheckFlags(parameters, "")) return;
                    SetVolume(args);
                    return;

                case "play":
                    if (!CheckFlags(parameters, "wf")) return;
                    Play(args, parameters);
                    return;

                case "beep":
                    if (!CheckFlags(parameters, "")) return;
                    Beep(args);
                    return;

                case "stop":
                    if (!CheckFlags(parameters, "")) return;
                    int stopped = AudioManager.Stop();
                    Console.WriteLine(stopped == 0 ? "audio: nothing playing" : "audio: stopped " + stopped);
                    return;

                default:
                    Usage();
                    return;
            }
        }

        private static void SetVolume(string[] args)
        {
            if (args.Length == 1)
            {
                Console.WriteLine("volume: " + AudioManager.Volume + "%");
                return;
            }

            if (args.Length != 2 || !int.TryParse(args[1], out int volume))
            {
                Console.WriteLine("usage: audio volume [0-100]");
                return;
            }

            if (volume < 0 || volume > 100)
            {
                Console.WriteLine("audio: volume must be 0..100");
                return;
            }

            AudioManager.Volume = volume;
            Console.WriteLine("volume: " + AudioManager.Volume + "%");
        }

        private static void Play(string[] args, HashSet<char> parameters)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("usage: audio play [-w] [-f] <file.wav>");
                return;
            }

            bool wait = parameters.Contains('w');
            bool force = parameters.Contains('f');

            if (!AudioManager.PlayFile(args[1], wait, force, out string error))
            {
                Console.WriteLine("audio: " + error);
                return;
            }

            Console.WriteLine("audio: playing " + args[1]);
        }

        private static void Beep(string[] args)
        {
            int frequency = 800;
            int duration = 200;

            if (args.Length > 3
                || (args.Length > 1 && !int.TryParse(args[1], out frequency))
                || (args.Length > 2 && !int.TryParse(args[2], out duration)))
            {
                Console.WriteLine("usage: audio beep [hz] [ms]");
                return;
            }

            if (!AudioManager.Beep(frequency, duration, out string error))
            {
                Console.WriteLine("audio: " + error);
                return;
            }

            Console.WriteLine("audio: beep " + frequency + " Hz for " + duration + " ms");
        }

        private static bool CheckFlags(HashSet<char> parameters, string allowed)
        {
            foreach (char p in parameters)
            {
                if (allowed.IndexOf(p) < 0)
                {
                    Console.WriteLine("audio: unknown option: -" + p);
                    return false;
                }
            }

            return true;
        }

        private static void Usage()
        {
            Console.WriteLine("usage: audio [-h] [status|devices|volume|play|beep|stop]");
            Console.WriteLine("  audio status");
            Console.WriteLine("  audio devices");
            Console.WriteLine("  audio volume [0-100]");
            Console.WriteLine("  audio play [-w] [-f] <file.wav>");
            Console.WriteLine("  audio beep [hz] [ms]");
            Console.WriteLine("  audio stop");
        }
    }
}
