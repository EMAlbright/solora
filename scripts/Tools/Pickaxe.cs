using Godot;
using System;

public partial class Pickaxe : ToolBase
{
    public override UseAnimationType UseAnimation => UseAnimationType.Mine;
	public override void Use(UseContext context	) {
		base.Use(context);
	}

}
