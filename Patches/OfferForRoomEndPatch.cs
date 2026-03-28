using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using FriendshipOverMod.Utils;

namespace FriendshipOverMod.Patches;

[HarmonyPatch(typeof(RewardsCmd), nameof(RewardsCmd.OfferForRoomEnd))]
public static class OfferForRoomEndPatch
{
    public static bool Prefix(Player player, AbstractRoom room, ref Task __result)
    {
        __result = RedirectOffer(player, room);
        return false;
    }

    private static async Task RedirectOffer(Player player, AbstractRoom room)
    {
        if (player?.RunState == null)
        {
            await RewardsCmd.OfferCustom(player, new List<Reward>());
            return;
        }

        var runState = player.RunState;

        if (!FOCollectorTracker.TryGetCollector(runState, out ulong collectorNetId))
        {
            var normalRewards = await new RewardsSet(player)
                .WithRewardsFromRoom(room)
                .GenerateWithoutOffering();

            await RewardsCmd.OfferCustom(player, normalRewards);
            return;
        }

        var rewards = await new RewardsSet(player)
            .WithRewardsFromRoom(room)
            .GenerateWithoutOffering();

        if (FOCollectorTracker.IsPlundered(runState, player.NetId))
        {
            rewards.RemoveAll(r => r is GoldReward);
        }

        if (player.NetId == collectorNetId)
        {
            foreach (var otherPlayer in runState.Players)
            {
                if (otherPlayer.NetId == collectorNetId)
                {
                    continue;
                }

                if (!FOCollectorTracker.IsPlundered(runState, otherPlayer.NetId))
                {
                    continue;
                }

                var otherRewards = await new RewardsSet(otherPlayer)
                    .WithRewardsFromRoom(room)
                    .GenerateWithoutOffering();

                foreach (var goldReward in otherRewards.OfType<GoldReward>())
                {
                    rewards.Add(new GoldReward(goldReward.Amount, player));
                }
            }
        }

        await RewardsCmd.OfferCustom(player, rewards);
    }
}