using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Audio : Node {

    private class MusicInstance {
        public string Id;
        public AudioStreamPlayer2D Player;
        public float Volume;
        public bool Active;
    }

    public static Audio Me;

    private static RandomNumberGenerator s_Random = new();

    [Export] public AudioStream[] Sounds = new AudioStream[0];
    [Export] public string[] SoundIds = new string[0];
    [Export] public float[] SoundVolumes = new float[0];
    [Export] public AudioStream[] MusicTracks = new AudioStream[0];
    [Export] public string[] MusicTrackIds = new string[0];
    [Export] public float[] MusicTrackVolumes = new float[0];

    private List<MusicInstance> _musicInstances = new List<MusicInstance>();

    public override void _Ready() {
        Me = this;
    }

    public override void _Process(double delta) {
        foreach (MusicInstance musicInstance in Me._musicInstances) {
            if (musicInstance.Active) {
                musicInstance.Player.VolumeDb = Mathf.Min(musicInstance.Player.VolumeDb + (float)delta * 10f, musicInstance.Volume);
            } else {
                musicInstance.Player.VolumeDb = Mathf.Max(musicInstance.Player.VolumeDb - (float)delta * 10f, -20f);
            }
        }

        for (int index = 0; index < Me._musicInstances.Count; index++) {
            MusicInstance musicInstance = Me._musicInstances[index];

            if (musicInstance.Player.VolumeDb > -20f && musicInstance.Player.Playing) continue;

            musicInstance.Player.QueueFree();

            Me._musicInstances.RemoveAt(index);

            index--;
        }

        foreach (Node child in GetChildren()) {
            if (!(child is AudioStreamPlayer2D player)) continue;

            if (player.Playing) continue;

            player.QueueFree();
        }
    }

    public static void PlayMusic(string id) {
        foreach (MusicInstance musicInstance in Me._musicInstances) {
            musicInstance.Active = false;
        }

        foreach (MusicInstance musicInstance in Me._musicInstances) {
            if (musicInstance.Id != id) continue;

            musicInstance.Active = true;

            return;
        }

        AudioStreamPlayer2D audio = new AudioStreamPlayer2D() {
            Stream = Me.MusicTracks[Me.MusicTrackIds.ToList().IndexOf(id)],
            VolumeDb = -20f,
            Attenuation = 0f,
            MaxDistance = 9999999999999999f,
            PanningStrength = 0f,
            Bus = "Music"
        };

        Me._musicInstances.Add(new MusicInstance() {
            Id = id,
            Player = audio,
            Volume = Me.MusicTrackVolumes[Me.MusicTrackIds.ToList().IndexOf(id)],
            Active = true
        });

        Me.AddChild(audio);

        audio.Play();
    }

    public static void Play(string id) {
        var ids = Me.SoundIds.Select((soundId, index) => (soundId, index)).Where((data, index) => data.soundId == id).ToList();

        int index = ids[s_Random.RandiRange(0, ids.Count - 1)].index;

        AudioStreamPlayer2D audio = new AudioStreamPlayer2D() {
            Stream = Me.Sounds[index],
            VolumeDb = Me.SoundVolumes[index] + new RandomNumberGenerator().RandfRange(-1f, 1f),
            PitchScale = new RandomNumberGenerator().RandfRange(0.9f, 1.1f),
            Attenuation = 0f,
            MaxDistance = 9999999999999999f,
            PanningStrength = 0f,
        };

        Me.AddChild(audio);

        audio.Play();
    }
}
