using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using FriendshipOverMod.Utils;

namespace FriendshipOverMod.Patches;

[HarmonyPatch(typeof(RewardsSet), nameof(RewardsSet.GenerateWithoutOffering))]
public static class RewardsSetGeneratePatch
{
    private static bool _isGeneratingMirrorRewards;

    public static void Postfix(RewardsSet __instance, ref Task<List<Reward>> __result)
    {
        __result = HandleResult(__instance, __result);
    }

    private static async Task<List<Reward>> HandleResult(RewardsSet rewardsSet, Task<List<Reward>> originalTask)
    {
        var rewards = await originalTask;

        if (_isGeneratingMirrorRewards)
        {
            return rewards;
        }

        var player = rewardsSet.Player;
        var room = rewardsSet.Room;
        var runState = player?.RunState;

        if (player == null || room == null || runState == null)
        {
            return rewards;
        }

        if (!FOCollectorTracker.TryGetCollector(runState, out ulong collectorNetId))
        {
            return rewards;
        }

        bool isCollector = player.NetId == collectorNetId;
        bool isPlundered = FOCollectorTracker.IsPlundered(runState, player.NetId);

        if (isPlundered)
        {
            rewards.RemoveAll(r => r is GoldReward);
        }

        if (!isCollector)
        {
            return rewards;
        }

        try
        {
            _isGeneratingMirrorRewards = true;

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
        finally
        {
            _isGeneratingMirrorRewards = false;
        }

        return rewards;
    }
}