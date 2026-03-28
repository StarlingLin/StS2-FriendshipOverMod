using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using FriendshipOverMod.Powers;
using FriendshipOverMod.Utils;

namespace FriendshipOverMod.Cards;

[Pool(typeof(ColorlessCardPool))]
public class FOCollector : CustomCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar("Damage", 15m, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Fatal)
    ];

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override string PortraitPath => $"res://StS2-FriendshipOverMod/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public FOCollector() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState?.RunState?.CurrentRoom is not CombatRoom)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        bool shouldTriggerFatal = cardPlay.Target.Powers.All(p => p.ShouldOwnerDeathTriggerFatal());

        AttackCommand attackCommand = await DamageCmd.Attack(DynamicVars["Damage"].BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        bool killedTarget = attackCommand.Results.Any(r => r.WasTargetKilled);

        if (shouldTriggerFatal && killedTarget)
        {
            await TransferCollectorState();
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Damage"].UpgradeValueBy(5m);
        AddKeyword(CardKeyword.Retain);
    }

    private async Task TransferCollectorState()
    {
        var runState = CombatState!.RunState;
        var myCreature = Owner.Creature;
        var myNetId = Owner.NetId;

        var players = CombatState.Players
            .Where(p => p?.Creature != null)
            .ToList();

        var creatures = players
            .Select(p => p.Creature)
            .ToList();

        var oldCollector = creatures.FirstOrDefault(c => c.HasPower<FOCollectorPower>());

        if (oldCollector == null)
        {
            await RemovePowerIfPresent<FOPlunderedPower>(myCreature);

            if (!myCreature.HasPower<FOCollectorPower>())
            {
                await PowerCmd.Apply<FOCollectorPower>(myCreature, 1m, myCreature, this);
            }

            foreach (var creature in creatures)
            {
                if (creature == myCreature)
                {
                    continue;
                }

                if (!creature.HasPower<FOPlunderedPower>())
                {
                    await PowerCmd.Apply<FOPlunderedPower>(creature, 1m, myCreature, this);
                }
            }

            FOCollectorTracker.SetCollector(runState, myNetId);
            FOCollectorTracker.SetPlundered(runState, players.Where(p => p.NetId != myNetId).Select(p => p.NetId));
            return;
        }

        if (oldCollector == myCreature)
        {
            FOCollectorTracker.SetCollector(runState, myNetId);
            FOCollectorTracker.SetPlundered(runState, players.Where(p => p.NetId != myNetId).Select(p => p.NetId));
            return;
        }

        await PowerCmd.Remove<FOCollectorPower>(oldCollector);
        await RemovePowerIfPresent<FOPlunderedPower>(myCreature);

        if (!oldCollector.HasPower<FOPlunderedPower>())
        {
            await PowerCmd.Apply<FOPlunderedPower>(oldCollector, 1m, myCreature, this);
        }

        if (!myCreature.HasPower<FOCollectorPower>())
        {
            await PowerCmd.Apply<FOCollectorPower>(myCreature, 1m, myCreature, this);
        }

        foreach (var creature in creatures)
        {
            if (creature == myCreature)
            {
                await RemovePowerIfPresent<FOPlunderedPower>(creature);
                continue;
            }

            if (creature != oldCollector && !creature.HasPower<FOPlunderedPower>())
            {
                await PowerCmd.Apply<FOPlunderedPower>(creature, 1m, myCreature, this);
            }
        }

        FOCollectorTracker.SetCollector(runState, myNetId);
        FOCollectorTracker.SetPlundered(runState, players.Where(p => p.NetId != myNetId).Select(p => p.NetId));
    }

    private static async Task RemovePowerIfPresent<TPower>(dynamic creature) where TPower : PowerModel
    {
        if (creature == null)
        {
            return;
        }

        if (creature.HasPower<TPower>())
        {
            await PowerCmd.Remove<TPower>(creature);
        }
    }
}