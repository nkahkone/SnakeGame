using System;
using System.IO;
using System.Windows.Media;

namespace Snake
{
    public class AudioManager : IDisposable
    {
        private readonly MediaPlayer musicPlayer = new MediaPlayer();

        // Helper: resolves path relative to the executable + Assets folder
        private static string ResolvePath(string file)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", file);
        }

        // Constructor: pass only the filenames, not full paths
        public AudioManager(string musicFile, string appleSfxFile, string gameOverSfxFile)
        {
            MusicPath = ResolvePath(musicFile);
            AppleSfxPath = ResolvePath(appleSfxFile);
            GameOverSfxPath = ResolvePath(gameOverSfxFile);

            // Loop the background music
            musicPlayer.MediaEnded += (s, e) =>
            {
                musicPlayer.Position = TimeSpan.Zero;
                musicPlayer.Play();
            };
        }

        public string MusicPath { get; }
        public string AppleSfxPath { get; }
        public string GameOverSfxPath { get; }

        // Play looping background music
        public void PlayMusic()
        {
            if (!File.Exists(MusicPath)) return; // optional safety check
            musicPlayer.Open(new Uri(MusicPath, UriKind.Absolute));
            musicPlayer.Volume = 0.5;
            musicPlayer.Play();
        }

        public void StopMusic()
        {
            musicPlayer.Stop();
        }

        // Play a one-shot sound effect
        private void PlaySfx(string path)
        {
            if (!File.Exists(path)) return; // optional safety check
            var player = new MediaPlayer();
            player.Open(new Uri(path, UriKind.Absolute));
            player.MediaEnded += (s, e) => player.Close();
            player.Play();
        }

        public void PlayApple() => PlaySfx(AppleSfxPath);
        public void PlayGameOver() => PlaySfx(GameOverSfxPath);

        public void Dispose()
        {
            musicPlayer.Close();
        }
    }
}
