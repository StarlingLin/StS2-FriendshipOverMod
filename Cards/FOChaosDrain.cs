using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace FriendshipOverMod.Cards;

[Pool(typeof(SilentCardPool))]
public class FOChaosDrain : CustomCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>("GainStrength", 3m),
        new PowerVar<DexterityPower>("GainDexterity", 3m),
        new PowerVar<StrengthPower>("LoseStrength", 2m),
        new PowerVar<DexterityPower>("LoseDexterity", 2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public FOChaosDrain() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override string PortraitPath => $"res://StS2-FriendshipOverMod/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var teammate = GetRandomTeammate();
        if (teammate == null || teammate.Creature == null)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        await PowerCmd.Apply<StrengthPower>(
            teammate.Creature,
            -DynamicVars["LoseStrength"].BaseValue,
            Owner.Creature,
            this
        );

        await PowerCmd.Apply<DexterityPower>(
            teammate.Creature,
            -DynamicVars["LoseDexterity"].BaseValue,
            Owner.Creature,
            this
        );

        await PowerCmd.Apply<StrengthPower>(
            Owner.Creature,
            DynamicVars["GainStrength"].BaseValue,
            Owner.Creature,
            this
        );

        await PowerCmd.Apply<DexterityPower>(
            Owner.Creature,
            DynamicVars["GainDexterity"].BaseValue,
            Owner.Creature,
            this
        );
    }

    protected override void OnUpgrade()
    {
        DynamicVars["LoseStrength"].UpgradeValueBy(-1m);
        DynamicVars["LoseDexterity"].UpgradeValueBy(-1m);
    }

    private dynamic? GetRandomTeammate()
    {
        var candidates = new List<dynamic>();

        foreach (var player in CombatState!.Players)
        {
            if (player == Owner)
            {
                continue;
            }

            if (player.Creature == null)
            {
                continue;
            }

            candidates.Add(player);
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates[Random.Shared.Next(candidates.Count)];
    }
}