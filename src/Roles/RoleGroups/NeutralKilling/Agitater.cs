using VentLib.Logging;

namespace TOHTOR.Roles.RoleGroups.NeutralKilling;

public class AgiTater : NeutralKillingBase
{
    protected override RoleModifier Modify(RoleModifier roleModifier)
    {
        base.Modify(roleModifier);
        VentLogger.Warn($"{this.RoleName} Not Implemented Yet", "RoleImplementation");
        return roleModifier;
    }
}