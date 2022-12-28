using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants {

	public class AnimationParameters
    {
        public static string Walk = "Walking";
        public static string Stagger = "Stagger";
        public static string Throw = "Throw";
        public static string Pull = "Pull";
        public static string Dash = "Dash";
        public static string Charge = "Charging";
        public static string HookIdle = "HookIdle";
        public static string AnchorIdle = "AnchorIdle";
        public static string HarpoonIdle = "HarpoonIdle";
        public static string TravelByOwnHook = "TravelByOwnHook";
        public static string LoadScreenTrigger = "TriggerLoadingScene";
        public static string ScoreBoardChange = "ScoreChange";
        public static string PlatformShake = "Shake";
        public static string PlatformSink = "Sink";
        public static string PlatformReset = "Reset";
        public static string IsOpen = "IsOpen";
        public static string IsActive = "IsActive";
        public static string CatapultIdle = "Castle_Catapult_Idle";
        public static string Standing = "Standing";
        public static string Play = "Play";
    }

    public class SkinNames
    {
        public static string Base = "Base";
        public static string Pirate = "Pirate";
        public static string Lifeguard = "Lifeguard";
        public static string Knight = "Knight";
        public static string Astronaut = "Astronaut";
        public static string Tiki = "Tiki";
        public static string Archaeologist = "Archaeologist";
    }

    public class FmodParameters
    {
        public static string SeaMine = "SeaMine";
        public static string SFXMixer = "bus:/SFX";
        public static string MusicMixer = "bus:/Music";
        public static string GameplaySFXMixer = "bus:/SFX/GameplaySFX";
        public static string DeathIndex = "DeathIndex";
        public static string AirlockIndex = "Spacializer";
        public static string PauseSnapshot = "Pause";
    }

    public class AudioMixerChannels
    {
        public static string SFX = "SFXMixer";
        public static string Music = "MusicMixer";
    }
}
