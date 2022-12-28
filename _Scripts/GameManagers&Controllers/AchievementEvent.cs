using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AchievementEvent<T> where T : AchievementEvent<T>
{
    public string Description;

    private bool hasFired;
    public delegate void EventListener(T info);
    private static event EventListener Listeners;

    public static void RegisterListener(EventListener listener)
    {
        Listeners += listener;
    }

    public static void UnregisterListener(EventListener listener)
    {
        Listeners -= listener;
    }

    public void FireEvent()
    {
        if (hasFired)
        {
            throw new Exception("This event has already fired, to prevent infinite loops you can't refire an event");
        }
        hasFired = true;
        Listeners?.Invoke(this as T);
    }
}

public class DebugEvent : AchievementEvent<DebugEvent>
{
    public int VerbosityLevel;
}

public class UsableHitEvent : AchievementEvent<UsableHitEvent>
{
    public int RewiredID; // if bot then id = 5
    public Usable.UsableType Usable;
    public GameObject UsableOwner; // Who Threw it
    public GameObject PlayerHit; // Who got hit

    /*

    Info about cause of death, our killer, etc...

    Could be a struct, readonly, etc...

    */
}

public class PowerupPickupEvent : AchievementEvent<PowerupPickupEvent>
{
    public int RewiredID; // if bot then id = 5
    public Usable.UsableType Usable;
}

public class GameEndEvent : AchievementEvent<GameEndEvent>
{
    public int RewiredID; // if bot then id = 5
    public string Skin;
    public int MapIndex; // Map Played
    public int PlayerScore;
    public PlayerDataModel Winner;
    public List<PlayerDataModel> PlayerDataModels;
    public bool TournamentOver;
    public List<TournamentManager.TournamentContender> Contenders;
    public int TournamentScoreNeeded;
    public int BotDifficulty; // 0 = Easy, 1 = Medium, 2 = Hard
}

public class HazardEvent : AchievementEvent<HazardEvent>
{
    public int RewiredID; // if bot then id = 5
    public AchHazardType HazardType;
    public GameObject PlayerHit;
    public GameObject Skin;
    public int HitCount;
    // player killed?
}

public class PlayerDeathEvent : AchievementEvent<PlayerDeathEvent>
{
    public GameObject Skin;
    public GameObject Killer; // what killed him
    public int DeathOrder; // First to die?
}

public enum AchHazardType : int
{
    // Beach
    Wave,
    SeaMine,
    BeachBall,
    // PirateShip
    Cannons,
    Grater,
    // Castle
    Barrage,
    // SpaceStation
    Airlock,
    // Temple
    Boulder,
    Pillars,
    Pressureplate,
    Lava,
    None
}

