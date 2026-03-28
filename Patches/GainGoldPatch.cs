using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using FriendshipOverMod.Utils;

namespace FriendshipOverMod.Patches;

[HarmonyPatch(typeof(PlayerCmd), nameof(PlayerCmd.GainGold))]
public static class GainGoldPatch
{
    private static bool _redirecting;

    public static bool Prefix(decimal amount, Player player, bool wasStolenBack, ref Task __result)
    {
        if (_redirecting)
        {
            return true;
        }

        if (player?.RunState == null)
        {
            return true;
        }

        var runState = player.RunState;

        if (!FOCollectorTracker.TryGetCollector(runState, out ulong collectorNetId))
        {
            return true;
        }

        if (player.NetId == collectorNetId)
        {
            return true;
        }

        if (!FOCollectorTracker.IsPlundered(runState, player.NetId))
        {
            return true;
        }

        var collector = runState.Players.FirstOrDefault(p => p.NetId == collectorNetId);
        if (collector == null)
        {
            return true;
        }

        __result = RedirectGold(amount, collector);
        return false;
    }

    private static async Task RedirectGold(decimal amount, Player collector)
    {
        try
        {
            _redirecting = true;
            await PlayerCmd.GainGold(amount, collector, false);
        }
        finally
        {
            _redirecting = false;
        }
    }
}