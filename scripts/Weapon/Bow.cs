
using Godot;

public partial class Bow: WeaponBase, IChargeable
{
	private Player _player;
	private Inventory _inventory;
	private Projectile _projectile;
	private bool _isCharging;
	private double _dTime;
	private double _maxTime = 1.5f;

	public override void OnEquip(Player player, InventoryEntry item)
	{   
		base.OnEquip(player, item);
		_player = player;
		_inventory = player.Inventory;

	}

	public void DrawWeapon(Player p)
	{
		_isCharging = true;
		_dTime = 0;
		_player.PlayerAnimation("Bow", WeaponData.AttackDuration);
	}
	public void Charge(Player p, float delta)
	{
		if (!_isCharging)
		{
			return;
		}
		_dTime += delta;
		_dTime = Mathf.Min(_dTime, _maxTime);
	}

	public void Release(Player p)
	{
		if (!_isCharging)
		{
			return;
		}
		_isCharging = false;
		_player.PlayerAnimation("BowRelease");
	}

	public void FireArrow()
	{
		if (!_player.Inventory.HasItem("arrow"))
		{
			return;
		}
		float chargePower = (float)(_dTime/_maxTime);
		
		ProjectileCreator.SpawnArrow(
			position: _player.GlobalPosition,
			direction: _player.AimDirection,
			power: chargePower,
			p: _player
		);
	}
}
