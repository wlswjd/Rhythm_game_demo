using UnityEngine;

public static class GameState
{
    public static bool BossCleared;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        BossCleared = false;
    }
}