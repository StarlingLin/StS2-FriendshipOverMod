using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace FriendshipOverMod.Powers;

public class FOPlunderedPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string? CustomPackedIconPath => "res://StS2-FriendshipOverMod/images/powers/friendshipovermod-f_o_plundered_power.png";
    public override string? CustomBigIconPath => "res://StS2-FriendshipOverMod/images/powers/friendshipovermod-f_o_plundered_power.png";
}