
using Godot;

public static class ProjectileCreator{
    public static PackedScene arrowScene = GD.Load<PackedScene>("res://scenes/arrow");

    public static Projectile SpawnArrow(Vector2 position, Vector2 direction, float power, Player p)
    {
        var arrow = arrowScene.Instantiate<Projectile>();
        arrow.GlobalPosition = position;
        arrow.Init(
            direction.Normalized(), power, p
        );
        p.GetTree().CurrentScene.AddChild(arrow);
        return arrow;
    }
}