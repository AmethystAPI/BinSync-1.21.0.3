using System;
using Godot;

[Tool]
[GlobalClass]
public partial class RandomVariantModifier : SmartTileModifier {
    [Export] public Vector2 Target;
    [Export] public Vector2[] Variants;

    private RandomNumberGenerator _random;

    public override Vector2I Modify(Vector2I center, Vector2I location) {
        if (_random == null) {
            _random = new RandomNumberGenerator();
            _random.Seed = Game.Seed;
        }

        if (location == center + Target) {
            int index = _random.RandiRange(0, Variants.Length);

            if (index == 0) {
                return location;
            } else {
                return center + (Vector2I)Variants[index - 1];
            }
        }

        return location;
    }
}