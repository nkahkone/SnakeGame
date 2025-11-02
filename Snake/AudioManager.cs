using System.Windows.Media;


public class AudioManager : IDisposable
{
    private readonly MediaPlayer musicPlayer = new MediaPlayer();
    private readonly MediaPlayer sfxPlayer = new MediaPlayer(); // single SFX player; you can expand to pool if overlapping SFX needed

    // call with relative URIs to copied output files or pack URIs depending on how you include files
    public AudioManager(string musicPath, string appleSfxPath, string gameOverSfxPath)
    {
        MusicPath = musicPath;
        AppleSfxPath = appleSfxPath;
        GameOverSfxPath = gameOverSfxPath;

        // loop the music
        musicPlayer.MediaEnded += (s, e) =>
        {
            musicPlayer.Position = TimeSpan.Zero;
            musicPlayer.Play();
        };
    }

    public string MusicPath { get; }
    public string AppleSfxPath { get; }
    public string GameOverSfxPath { get; }

    public void PlayMusic()
    {
        try
        {
            musicPlayer.Open(new Uri(MusicPath, UriKind.Relative));
            musicPlayer.Volume = 0.5; // adjust
            musicPlayer.Play();
        }
        catch { /* handle/log errors as desired */ }
    }

    public void StopMusic()
    {
        musicPlayer.Stop();
    }

    // Play a quick sound effect (non-blocking)
    private void PlaySfx(string path)
    {
        try
        {
            // reuse sfxPlayer; for overlapping sfx consider creating new MediaPlayer for each play
            var player = new MediaPlayer();
            player.Open(new Uri(path, UriKind.Relative));
            player.MediaEnded += (s, e) =>
            {
                player.Close();
            };
            player.Play();
        }
        catch { /* handle/log errors */ }
    }

    public void PlayApple() => PlaySfx(AppleSfxPath);
    public void PlayGameOver() => PlaySfx(GameOverSfxPath);

    public void Dispose()
    {
        musicPlayer.Close();
        sfxPlayer.Close();
    }
}
