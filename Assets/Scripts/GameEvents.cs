using System;

public enum DeathCause { RedZone, Crushed, Bricked }

public static class GameEvents
{
    public static event Action<DeathCause> OnPlayerDeath;
    public static event Action OnLevelWin;
    public static event Action<bool> OnPerspectiveSwitch;

    public static void TriggerDeath(DeathCause cause) => OnPlayerDeath?.Invoke(cause);
    public static void TriggerWin()                   => OnLevelWin?.Invoke();
    public static void TriggerPerspective(bool is2D)  => OnPerspectiveSwitch?.Invoke(is2D);
}
