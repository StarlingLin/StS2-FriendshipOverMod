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

namespace FriendshipOverMod.Scripts;

[Pool(typeof(ColorlessCardPool))]
public class FOSelfExplode : CustomCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    private const decimal allyDamage = 15m;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HpLossVar(5m),
        new DamageVar(40m, ValueProp.Move)
    ];

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override string PortraitPath => $"res://StS2-FriendshipOverMod/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public FOSelfExplode() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature,
            DynamicVars.HpLoss.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            this
        );

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);

        await DealDamageToAllAllies(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.HpLoss.UpgradeValueBy(-1m);
        DynamicVars.Damage.UpgradeValueBy(10m);
    }

    private async Task DealDamageToAllAllies(PlayerChoiceContext choiceContext)
    {
        foreach (var player in CombatState.Players)
        {
            if (player == Owner)
            {
                continue;
            }

            await CreatureCmd.Damage(
                choiceContext,
                player.Creature,
                allyDamage,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                this
            );
        }
    }
}