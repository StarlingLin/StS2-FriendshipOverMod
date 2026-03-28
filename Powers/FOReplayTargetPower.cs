using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Combat;

namespace FriendshipOverMod.Powers;

public class FORelayTargetPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override string? CustomPackedIconPath => "res://StS2-FriendshipOverMod/images/powers/friendshipovermod-f_o_relay_target_power.png";
    public override string? CustomBigIconPath => "res://StS2-FriendshipOverMod/images/powers/friendshipovermod-f_o_relay_target_power.png";

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player)
        {
            return;
        }

        if (cardPlay.Card.Type != CardType.Power)
        {
            return;
        }

        if (Applier?.Player == null)
        {
            return;
        }

        Flash();

        var relayOwner = Applier.Player;
        CardModel? copy = CreateCardForPlayer(cardPlay.Card, relayOwner);

        if (copy == null)
        {
            await PowerCmd.Remove(this);
            return;
        }

        CopyUpgradeState(cardPlay.Card, copy);

        CardCmd.PreviewCardPileAdd(
            await CardPileCmd.AddGeneratedCardToCombat(
                copy,
                PileType.Draw,
                addedByPlayer: true,
                CardPilePosition.Random
            )
        );

        await PowerCmd.Remove(this);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side)
        {
            await PowerCmd.Remove(this);
        }
    }

    private CardModel? CreateCardForPlayer(CardModel sourceCard, Player newOwner)
    {
        var combatState = newOwner.Creature?.CombatState;
        if (combatState == null)
        {
            return null;
        }

        var method = combatState.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m =>
                m.Name == "CreateCard" &&
                m.IsGenericMethodDefinition &&
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType == typeof(Player));

        if (method == null)
        {
            return null;
        }

        var generic = method.MakeGenericMethod(sourceCard.GetType());
        return generic.Invoke(combatState, new object[] { newOwner }) as CardModel;
    }

    private void CopyUpgradeState(CardModel source, CardModel copy)
    {
        bool upgraded = false;

        var sourceProp =
            source.GetType().GetProperty("IsUpgraded") ??
            source.GetType().GetProperty("Upgraded");

        if (sourceProp != null && sourceProp.PropertyType == typeof(bool))
        {
            upgraded = (bool)(sourceProp.GetValue(source) ?? false);
        }

        if (!upgraded)
        {
            return;
        }

        var upgradeMethod =
            copy.GetType().GetMethod("Upgrade", BindingFlags.Public | BindingFlags.Instance) ??
            copy.GetType().GetMethod("UpgradeCard", BindingFlags.Public | BindingFlags.Instance);

        upgradeMethod?.Invoke(copy, null);
    }
}