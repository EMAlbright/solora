
using System.Data;

public interface IChargeable: IWeapon
{
    void DrawWeapon(Player player);
    void Charge(Player player, float delta);
    void Release(Player player);

}