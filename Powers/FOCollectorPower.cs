using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace FriendshipOverMod.Powers;

public class FOCollectorPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string? CustomPackedIconPath => "res://StS2-FriendshipOverMod/images/powers/friendshipovermod-f_o_collector_power.png";
    public override string? CustomBigIconPath => "res://StS2-FriendshipOverMod/images/powers/friendshipovermod-f_o_collector_power.png";
}