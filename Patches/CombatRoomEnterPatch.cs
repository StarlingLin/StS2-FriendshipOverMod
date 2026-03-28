using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using FriendshipOverMod.Utils;

namespace FriendshipOverMod.Patches;

[HarmonyPatch(typeof(CombatRoom), nameof(CombatRoom.Enter))]
public static class CombatRoomEnterPatch
{
    public static void Prefix(IRunState? runState, bool isRestoringRoomStackBase)
    {
        if (runState == null)
        {
            return;
        }

        if (isRestoringRoomStackBase)
        {
            return;
        }

        FOCollectorTracker.Clear(runState);
    }
}