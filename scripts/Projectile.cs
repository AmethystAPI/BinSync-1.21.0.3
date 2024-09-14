using Godot;
using System;

public partial class Projectile : Node2D {
	[Export] public float Damage = 1f;
	[Export] public float Speed = 200f;
	[Export] public float Resistance = 0f;
	[Export] public float Lifetime = 5f;
	[Export] public float Invincibilitytime = 0.1f;
	[Export] public float Knockback = 1f;
	[Export] public bool InheritBelocity = true;
	[Export] public bool Pierce = false;
	[Export] public bool DestroyOnTerrain = true;
	[Export] public float ScreenShake = 0.5f;
	[Export] public float TerrainScreenShake = 0.3f;
	[Export] public PackedScene HitEffectScene;
	[Export] public PackedScene HitFlashScene;

	public Action Destroyed;

	public Node2D Source;
	public Vector2 InheritedVelocity;
	public float Velocity;

	private Area2D _damageArea;
	private float _lifetimeTimer;
	private float _invincibilityTimer;
	private Node2D _persistentVisuals;

	public override void _Ready() {
		_damageArea = GetNode<Area2D>("DamageArea");
		_persistentVisuals = GetNodeOrNull<Node2D>("PersistentVisuals");

		_lifetimeTimer = Lifetime;
		_invincibilityTimer = Invincibilitytime;

		Velocity = Speed;
	}

	public override void _Process(double delta) {
		_lifetimeTimer -= (float)delta;
		_invincibilityTimer -= (float)delta;

		if (_lifetimeTimer > 0) return;

		Destroyed?.Invoke();

		HandlePersistentVisuals();

		QueueFree();
	}

	public override void _PhysicsProcess(double delta) {
		Movement((float)delta);

		foreach (Node2D body in _damageArea.GetOverlappingBodies()) {
			if ((body is TileMap || body is Barrier) && _invincibilityTimer <= 0) {
				OnHit(body);

				if (Source is Player) Camera.Shake(TerrainScreenShake);

				if (DestroyOnTerrain) {
					Destroyed?.Invoke();

					HandlePersistentVisuals();

					SpawnHitEffect(Rotation);

					QueueFree();
				}

				return;
			}

			if (!(body is Damageable)) continue;

			Damageable damageable = body as Damageable;

			if (!damageable.CanDamage(this)) continue;

			damageable.Damage(this);

			Camera.Shake(ScreenShake);

			OnHit(body);

			if (!Pierce) {
				Destroyed?.Invoke();

				HandlePersistentVisuals();

				SpawnHitEffect(Rotation);

				QueueFree();

				break;
			}
		}
	}

	public virtual void OnHit(Node2D body) {

	}

	public virtual void Movement(float delta) {
		if (InheritBelocity && IsInstanceValid(Source) && Source is CharacterBody2D) InheritedVelocity = (Source as CharacterBody2D).Velocity;

		if (InheritBelocity) {
			GlobalPosition += GlobalTransform.BasisXform(Vector2.Right) * Velocity * (float)delta + InheritedVelocity * (float)delta;
		} else {
			GlobalPosition += GlobalTransform.BasisXform(Vector2.Right) * Velocity * (float)delta;
		}

		Velocity = Mathf.Lerp(Velocity, 0f, Resistance * (float)delta);
	}

	public virtual float GetDamage() {
		return Damage;
	}

	private void HandlePersistentVisuals() {
		if (_persistentVisuals == null) return;

		Vector2 position = _persistentVisuals.GlobalPosition;

		RemoveChild(_persistentVisuals);
		GetParent().AddChild(_persistentVisuals);

		_persistentVisuals.GlobalPosition = position;
	}

	private void SpawnHitEffect(float rotation) {
		if (HitEffectScene != null) {
			Node2D hitEffect = HitEffectScene.Instantiate<Node2D>();
			Source.GetParent().AddChild(hitEffect);
			hitEffect.GlobalPosition = GlobalPosition;
		}

		if (HitFlashScene != null) {
			Node2D hitFlash = HitFlashScene.Instantiate<Node2D>();
			Source.GetParent().AddChild(hitFlash);
			hitFlash.GlobalPosition = GlobalPosition;
			hitFlash.Rotation = rotation;
		}
	}
}
