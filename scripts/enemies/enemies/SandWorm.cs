using Godot;
using System;

public partial class SandWorm : Enemy {
    [Export] public PackedScene ProjectileScene;
    [Export] public Node2D ProjectileOrigin;
    [Export] public PackedScene Particle;

    private Vector2 _direction;
    private Vector2 _smoothedDirection;
    private float _switchTimer;
    private Vector2 _switchInterval = new Vector2(1.5f, 2.5f);
    private RandomNumberGenerator _random = new RandomNumberGenerator();
    private Vector2 _lastParticlePosition;

    public override void _Ready() {
        base._Ready();

        _direction = Vector2.Right.Rotated(_random.RandfRange(0f, Mathf.Pi * 2f));
        _smoothedDirection = _direction;
        _switchTimer = _random.RandfRange(_switchInterval.X, _switchInterval.Y);
        _lastParticlePosition = GlobalPosition;
    }

    public override void AddStates() {
        base.AddStates();

        _stateMachine.Add(new Idle("idle", this) {
            Movement = delta => {
                if (!Activated) return;

                _smoothedDirection = _smoothedDirection.Lerp(_direction, 2f * delta);
                Velocity = _smoothedDirection * 60f + Knockback;

                MoveAndSlide();

                if (_lastParticlePosition.DistanceTo(GlobalPosition) > 4f) {
                    Node2D node = Particle.Instantiate<Node2D>();
                    GetParent().AddChild(node);

                    node.GlobalPosition = GlobalPosition;

                    _lastParticlePosition = GlobalPosition;
                }
            }
        });

        float shootIndex = 0;

        _stateMachine.Add(new TimedAttack("attack", this) {
            Duration = 1f,
            OnPrepare = shootQueue => {
                shootQueue.Add(0.5f);
                shootQueue.Add(0.5f);
                shootQueue.Add(0.5f);
                shootQueue.Add(0.5f);

                shootIndex = 0;
            },
            OnShoot = direction => {
                SquashAndStretch.Trigger(new Vector2(1.4f, 0.6f), 8f);

                direction = Vector2.Right.Rotated((float)-Math.PI / 4f + Mathf.Pi / 2f * shootIndex);

                Projectile _projectile = ProjectileScene.Instantiate<Projectile>();

                _projectile.Source = this;

                GetParent().AddChild(_projectile);

                _projectile.GlobalPosition = GlobalPosition;
                _projectile.Position += direction * 4f;

                _projectile.LookAt(_projectile.GlobalPosition + direction);

                shootIndex++;
            }
        });
    }

    public override void _Process(double delta) {
        base._Process(delta);

        _switchTimer -= (float)delta;

        if (_switchTimer <= 0f) {
            _direction = Vector2.Right.Rotated(_random.RandfRange(0f, Mathf.Pi * 2f));

            _switchTimer = _random.RandfRange(_switchInterval.X, _switchInterval.Y);

            Vector2 target = GetWeightedTargets()[0].Player.GlobalPosition;

            if (GlobalPosition.DistanceTo(target) >= 64f) _direction = (target - GlobalPosition).Normalized();
        }
    }

    public override bool CanDamage(Projectile projectile) {
        if (!base.CanDamage(projectile)) return false;

        if (_stateMachine.CurrentState == "idle") return false;

        return true;
    }
}
