using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace FriendshipOverMod.Cards;

[Pool(typeof(RegentCardPool))]
public class FOCastling : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyAlly;
    private const bool shouldShowInCardLibrary = true;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override string PortraitPath => $"res://StS2-FriendshipOverMod/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public FOCastling() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var ally = cardPlay.Target;
        var self = Owner.Creature;

        decimal selfBlock = self.Block;
        decimal allyBlock = ally.Block;

        await SetBlock(choiceContext, self, allyBlock, cardPlay);
        await SetBlock(choiceContext, ally, selfBlock, cardPlay);

        if (IsUpgraded)
        {
            decimal refund = Math.Floor(allyBlock / 2m);
            if (refund > 0)
            {
                await CreatureCmd.GainBlock(
                    ally,
                    new BlockVar(refund, ValueProp.Move),
                    cardPlay
                );
            }
        }
    }

    private async Task SetBlock(PlayerChoiceContext choiceContext, dynamic creature, decimal targetBlock, CardPlay cardPlay)
    {
        decimal currentBlock = creature.Block;

        if (currentBlock > targetBlock)
        {
            await CreatureCmd.LoseBlock(
                creature,
                currentBlock - targetBlock
            );
        }
        else if (currentBlock < targetBlock)
        {
            await CreatureCmd.GainBlock(
                creature,
                new BlockVar(targetBlock - currentBlock, ValueProp.Move),
                cardPlay
            );
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}