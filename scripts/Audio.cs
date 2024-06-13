using Godot;
using System;
using System.Linq;

public partial class Audio : Node
{
    public static Audio Me;

    [Export] public AudioStream[] Sounds = new AudioStream[0];
    [Export] public string[] SoundIds = new string[0];
    [Export] public float[] SoundVolumes = new float[0];

    public override void _Ready()
    {
        Me = this;
    }

    public override void _Process(double delta)
    {
        foreach (Node child in GetChildren())
        {
            if (!(child is AudioStreamPlayer2D player)) continue;

            if (player.Playing) continue;

            player.QueueFree();
        }
    }

    public static void Play(string id)
    {
        return;

        AudioStreamPlayer2D audio = new AudioStreamPlayer2D()
        {
            Stream = Me.Sounds[Me.SoundIds.ToList().IndexOf(id)],
            VolumeDb = Me.SoundVolumes[Me.SoundIds.ToList().IndexOf(id)] + new RandomNumberGenerator().RandfRange(-1f, 1f),
            PitchScale = new RandomNumberGenerator().RandfRange(0.9f, 1.1f),
            Attenuation = 0f,
            MaxDistance = 9999999999999999f,
            PanningStrength = 0f,
        };

        Me.AddChild(audio);

        audio.Play();
    }
}
