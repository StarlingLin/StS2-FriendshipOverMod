using System.Collections.Generic;
using MegaCrit.Sts2.Core.Runs;

namespace FriendshipOverMod.Utils;

public static class FOCollectorTracker
{
    private static readonly Dictionary<IRunState, ulong> CollectorByRun = new();
    private static readonly Dictionary<IRunState, HashSet<ulong>> PlunderedByRun = new();

    public static void Clear(IRunState runState)
    {
        CollectorByRun.Remove(runState);
        PlunderedByRun.Remove(runState);
    }

    public static void SetCollector(IRunState runState, ulong collectorNetId)
    {
        CollectorByRun[runState] = collectorNetId;
        if (!PlunderedByRun.ContainsKey(runState))
        {
            PlunderedByRun[runState] = new HashSet<ulong>();
        }
    }

    public static bool TryGetCollector(IRunState runState, out ulong collectorNetId)
    {
        return CollectorByRun.TryGetValue(runState, out collectorNetId);
    }

    public static void SetPlundered(IRunState runState, IEnumerable<ulong> netIds)
    {
        if (!PlunderedByRun.ContainsKey(runState))
        {
            PlunderedByRun[runState] = new HashSet<ulong>();
        }

        PlunderedByRun[runState].Clear();
        foreach (ulong netId in netIds)
        {
            PlunderedByRun[runState].Add(netId);
        }
    }

    public static bool IsPlundered(IRunState runState, ulong netId)
    {
        return PlunderedByRun.TryGetValue(runState, out var set) && set.Contains(netId);
    }
}