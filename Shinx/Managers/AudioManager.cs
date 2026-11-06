using System;
using System.IO;
using System.Threading;
using Cosmos.Kernel.HAL.Devices.Audio;
using Cosmos.Kernel.System.Audio;
using Cosmos.Kernel.System.FileSystem;
using CosmosAudio = Cosmos.Kernel.System.Audio.AudioManager;

namespace Shinx.Audio
{
    public static class AudioManager
    {
        private const string VolumePath = "/etc/audio.volume";
        private static readonly object _gate = new object();

        private static volatile int _volume = 80;
        private static string _current = "";

        public static bool IsEnabled => CosmosAudio.IsEnabled;

        public static bool IsAvailable => CosmosAudio.IsEnabled && CosmosAudio.Primary != null;

        public static bool IsPlaying => CosmosAudio.Primary?.Player != null;

        public static string DeviceName => CosmosAudio.Primary?.Name ?? "";

        public static string DriverName => CosmosAudio.Primary?.DriverName ?? "";

        public static uint SampleRate => CosmosAudio.Primary?.SampleRate ?? 0;

        public static string CurrentName => _current;

        public static int DeviceCount => CosmosAudio.Count;

        public static int Volume
        {
            get { return _volume; }
            set
            {
                _volume = Math.Clamp(value, 0, 100);
                SaveVolume();
            }
        }

        public static bool TryGetDevice(int index, out string name, out string driver,
            out uint sampleRate, out int channels, out int bits)
        {
            name = "";
            driver = "";
            sampleRate = 0;
            channels = 0;
            bits = 0;

            if (!CosmosAudio.TryGet(index, out AudioDevice? device) || device == null)
                return false;

            name = device.Name;
            driver = device.DriverName;
            sampleRate = device.SampleRate;
            channels = device.Format.Channels;
            bits = device.Format.ChannelSize * 8;
            return true;
        }

        public static void Initialize()
        {
            LoadVolume();

            if (!CosmosAudio.IsEnabled)
            {
                Console.WriteLine("[audio] disabled (CosmosEnableAudio=false)");
                return;
            }

            AudioDevice? device = CosmosAudio.Primary;
            if (device == null)
            {
                Console.WriteLine("[audio] no output");
                return;
            }

            Console.WriteLine("[audio] " + device.Name + " (" + device.DriverName + "), "
                + device.SampleRate + " Hz, " + device.Format.Channels + " ch, "
                + (device.Format.ChannelSize * 8) + "-bit");
        }

        public static bool PlayFile(string path, bool wait, bool force, out string error)
        {
            error = "";
            path = Shell.ResolvePath(path);

            if (!File.Exists(path))
            {
                error = "no such file: " + path;
                return false;
            }

            if (!PermissionManager.CanAccess(path, UserManager.currentUser))
            {
                error = "permission denied: " + path;
                return false;
            }

            AudioStream? stream = null;
            IVfsFileHandle? file = null;

            try
            {
                if (!VfsManager.TryOpenFile(path, out file))
                {
                    error = "cannot open: " + path;
                    return false;
                }

                if (!WaveAudioStream.TryOpen(file, out WaveAudioStream? wave))
                {
                    file.Dispose();
                    error = "not a PCM WAV file";
                    return false;
                }

                stream = wave;

                if (!IsPlayable(stream, out error))
                {
                    file.Dispose();
                    return false;
                }
            }
            catch (Exception e)
            {
                if (file != null)
                    file.Dispose();

                error = "cannot play WAV: " + e.Message;
                return false;
            }

            return Start(stream, file, path, wait, force, out error);
        }

        public static bool Beep(int frequency, int milliseconds, out string error)
        {
            error = "";

            if (frequency < 37 || frequency > 32767)
            {
                error = "frequency must be 37..32767 Hz";
                return false;
            }

            if (milliseconds <= 0)
            {
                error = "duration must be greater than zero";
                return false;
            }

            try
            {
#pragma warning disable CA1416
                Console.Beep(frequency, milliseconds);
#pragma warning restore CA1416
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        public static int Stop()
        {
            lock (_gate)
            {
                _current = "";

                AudioPlayer? player = CosmosAudio.Primary?.Player;
                if (player == null)
                    return 0;

                player.RequestStop();
                return 1;
            }
        }

        public static void PrintStatus()
        {
            if (!CosmosAudio.IsEnabled)
            {
                Console.WriteLine("audio: disabled (CosmosEnableAudio=false)");
                return;
            }

            AudioDevice? device = CosmosAudio.Primary;
            if (device == null)
            {
                Console.WriteLine("audio: no output (run cosmos run --audio intel-hda)");
                return;
            }

            Console.WriteLine("output: " + device.Name + " (" + device.DriverName + ")");
            Console.WriteLine("format: " + device.SampleRate + " Hz, "
                + device.Format.Channels + " ch, "
                + (device.Format.ChannelSize * 8) + "-bit");
            Console.WriteLine("volume: " + Volume + "%");

            if (device.Player == null)
                Console.WriteLine("playing: (idle)");
            else
                Console.WriteLine("playing: " + (_current.Length > 0 ? _current : "(external)"));
        }

        public static void PrintDevices()
        {
            if (!CosmosAudio.IsEnabled)
            {
                Console.WriteLine("audio: disabled (CosmosEnableAudio=false)");
                return;
            }

            int count = CosmosAudio.Count;
            if (count == 0)
            {
                Console.WriteLine("audio: no output (run cosmos run --audio intel-hda)");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                if (!CosmosAudio.TryGet(i, out AudioDevice? device))
                    continue;

                Console.WriteLine("[" + i + "] " + device.Name + "  " + device.DriverName
                    + "  " + device.SampleRate + " Hz, "
                    + device.Format.Channels + " ch, "
                    + (device.Format.ChannelSize * 8) + "-bit");
            }
        }

        private static bool Start(AudioStream stream, IDisposable? owner, string name,
            bool wait, bool force, out string error)
        {
            AudioPlayback? playback;

            lock (_gate)
            {
                if (!EnsureIdle(force, out error))
                {
                    if (owner != null)
                        owner.Dispose();
                    return false;
                }

                if (!CosmosAudio.TryStartPlayback(new GainStream(stream), owner, out playback))
                {
                    if (owner != null)
                        owner.Dispose();

                    error = CosmosAudio.Primary?.Player != null
                        ? "output is busy; use audio stop or -f"
                        : "could not start playback";
                    return false;
                }

                _current = name;
            }

            if (wait && playback != null)
                WaitForPlayback(playback);

            return true;
        }

        private static bool EnsureIdle(bool force, out string error)
        {
            error = "";

            if (!CosmosAudio.IsEnabled)
            {
                error = "audio is not compiled in";
                return false;
            }

            AudioDevice? device = CosmosAudio.Primary;
            if (device == null)
            {
                error = "no audio output; run cosmos run --audio intel-hda";
                return false;
            }

            if (device.Player == null)
                return true;

            if (!force)
            {
                error = "output is busy; use audio stop or -f";
                return false;
            }

            Stop();
            WaitForIdle(500);

            if (device.Player != null)
            {
                error = "output is still stopping; try again";
                return false;
            }

            return true;
        }

        private static bool IsPlayable(AudioStream stream, out string error)
        {
            AudioFormat format = stream.Format;

            if (format.IsEmpty || format.Channels != 2 || format.ChannelSize != 2 || !format.Signed)
            {
                error = "only signed 16-bit stereo WAV files are supported";
                return false;
            }

            error = "";
            return true;
        }

        private static void LoadVolume()
        {
            try
            {
                if (File.Exists(VolumePath)
                    && int.TryParse(File.ReadAllText(VolumePath).Trim(), out int volume))
                    _volume = Math.Clamp(volume, 0, 100);
            }
            catch
            {
            }
        }

        private static void SaveVolume()
        {
            try
            {
                if (Directory.Exists("/etc"))
                    File.WriteAllText(VolumePath, _volume.ToString());
            }
            catch
            {
            }
        }

        private static void WaitForIdle(int timeoutMs)
        {
            int waited = 0;
            while (CosmosAudio.Primary?.Player != null && waited < timeoutMs)
            {
                Thread.Sleep(5);
                waited += 5;
            }
        }

        private static void WaitForPlayback(AudioPlayback playback)
        {
            while (playback.IsPlaying)
                Thread.Sleep(10);
        }

        private sealed class GainStream : AudioStream
        {
            private readonly AudioStream _inner;

            public GainStream(AudioStream inner)
            {
                _inner = inner;
            }

            public override AudioFormat Format => _inner.Format;

            public override uint SampleRate => _inner.SampleRate;

            public override bool Depleted => _inner.Depleted;

            public override int Read(Span<byte> destination)
            {
                int written = _inner.Read(destination);
                int volume = AudioManager.Volume;

                if (written <= 0 || volume >= 100)
                    return written;

                for (int i = 0; i + 1 < written; i += 2)
                {
                    short sample = (short)(destination[i] | (destination[i + 1] << 8));
                    int scaled = sample * volume / 100;
                    destination[i] = (byte)(scaled & 0xFF);
                    destination[i + 1] = (byte)((scaled >> 8) & 0xFF);
                }

                return written;
            }
        }
    }
}
