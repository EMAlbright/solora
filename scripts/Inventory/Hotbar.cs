using Godot;
using System;
using System.Collections.Generic;

public partial class Hotbar : Node
{
	private const int HotbarStartIndex = 16;
	private const int HotBarSlots = 4;

	private int _selectedSlot;

	private Inventory _inventory;
	private PlayerEquipment _equipment;

    [Signal]
    public delegate void HotbarSelectionChangedEventHandler(int selectedSlot);

    public override void _Ready()
    {
        _inventory = GetParent().GetNode<Inventory>("Inventory");
		_equipment = GetParent().GetNode<PlayerEquipment>("Equipment");
    }

    public override void _PhysicsProcess(double delta)
    {
       for(int i = 0; i < HotBarSlots; i++)
		{
			if (Input.IsActionJustPressed($"hotbar_slot_{i + 1}"))
			{
				SelectSlot(i);
				break;
			}
		}
    }

	public void SelectSlot(int index)
	{
		if (index < 0 || index >= HotBarSlots)
		{
			return;
		}

		_selectedSlot = index;
		int inventoryIndex = HotbarStartIndex + index;

		InventoryEntry entry = _inventory.GetItemAtIndex(inventoryIndex);

		if(entry == null)
		{
			_equipment.Unequip();
		} else
		{
			_equipment.Equip(entry);
		}
		
		EmitSignal(SignalName.HotbarSelectionChanged, _selectedSlot);
	}

	public int GetSelectedSlot()
	{
		return _selectedSlot;
	}

	public InventoryEntry GetSelectedItem()
	{
		return _inventory.GetItemAtIndex(HotbarStartIndex + _selectedSlot);
	}

}
