using Godot;
using System;

public partial class MaterialItem : BaseItem
{
	// this is so when we place a material item, we create a copy

	// otherwise when breaking/manipulating placed item, it will point to same objects
	public MaterialItem Clone() {
		return new MaterialItem {
			ItemId = this.ItemId,
			DisplayName = this.DisplayName,
			Icon = this.Icon,
			Type = this.Type,
			Health = this.Health,
			WorldScale = this.WorldScale,
			MaxStackSize = this.MaxStackSize,
		};
	}
}
