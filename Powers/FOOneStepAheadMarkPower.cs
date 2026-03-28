using System;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace FriendshipOverMod.Powers;

public class FOOneStepAheadMarkPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string? CustomPackedIconPath => "res://StS2-FriendshipOverMod/images/powers/friendshipovermod-f_o_one_step_ahead_mark_power.png";
    public override string? CustomBigIconPath => "res://StS2-FriendshipOverMod/images/powers/friendshipovermod-f_o_one_step_ahead_mark_power.png";

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        Player targetPlayer = Owner.Player ?? Owner.PetOwner;
        if (targetPlayer == null)
        {
            await PowerCmd.Remove(this);
            return;
        }

        int playedCount = CombatManager.Instance.History.CardPlaysStarted.Count();

        long selector =
            (long)playedCount +
            (long)(targetPlayer.NetId & 0xFFFF) +
            (long)((applier?.Player?.NetId ?? 0UL) & 0xFFFF);

        bool useSlimed = (selector & 1L) == 0L;

        CardModel statusCard = useSlimed
            ? CombatState.CreateCard<Slimed>(targetPlayer)
            : CombatState.CreateCard<Dazed>(targetPlayer);

        CardPileAddResult result = await CardPileCmd.AddGeneratedCardToCombat(
            statusCard,
            PileType.Draw,
            addedByPlayer: true,
            CardPilePosition.Random
        );

        if (LocalContext.IsMe(targetPlayer))
        {
            CardCmd.PreviewCardPileAdd(result);
            await Cmd.Wait(0.5f);
        }

        await PowerCmd.Remove(this);
    }
}