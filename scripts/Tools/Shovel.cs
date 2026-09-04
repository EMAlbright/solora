using Godot;
using System;

public partial class Shovel : ToolBase
{
	public override UseAnimationType UseAnimation => UseAnimationType.Shovel;

	public override void Use(UseContext context	) {
		base.Use(context);
	}
}
