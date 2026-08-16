
using Godot;

public sealed class PlayerInput
{
    public PlayerInputFrame Read(Node2D player)
    {
        Vector2 moveDirection = Input.GetVector(
            "move_left",
            "move_right",
            "move_up",
            "move_down"
        );

        Vector2 aimDirection = player.GetGlobalMousePosition() - player.GlobalPosition;

        if (!aimDirection.IsZeroApprox())
        {
            aimDirection = aimDirection.Normalized();
        }

        return new PlayerInputFrame (
            MoveDirection: moveDirection,
            AimDirection: aimDirection,
            RunHeld: Input.IsActionPressed("run"),
            JumpPressed: Input.IsActionJustPressed("jump"),
            InteractPressed: Input.IsActionJustPressed("Interact"),
            PrimaryPressed: Input.IsActionJustPressed("PrimaryAction"),
            PrimaryHeld: Input.IsActionPressed("PrimaryAction"),
            PrimaryReleased: Input.IsActionJustReleased("PrimaryAction")
        );
    }
}