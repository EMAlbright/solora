
using System;
using Godot;

public partial class Projectile: Area2D
{
    private Vector2 direction;
    private int damage;
    private float speed = 10;
    private Vector2 _velocity;
    private Player _owner;

    public void Init(Vector2 direction, float charge, Player p)
    {
        _owner = p;
        _velocity = speed * direction * Mathf.Lerp(0.5f, 1.5f, charge);
    }

    public override void _PhysicsProcess(double delta)
    {
        _velocity.Y += Gravity * (float)delta;
		GlobalPosition += _velocity * (float)delta;
    }
}