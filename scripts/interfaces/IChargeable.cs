
using System.Data;

public interface IChargeable
{
    void BeginCharge(AttackContext context);
    void Charge(AttackContext context, float delta);
    void Release(AttackContext context);
}