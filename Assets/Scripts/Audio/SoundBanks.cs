using UnityEngine;

public static class SoundBanks
{
    private static SampleBank itemPickup;
    private static SampleBank itemDrop;
    private static SampleBank itemEquip;
    private static SampleBank itemCraft;
    private static SampleBank itemHotbar;

    public static SampleBank ItemPickup => itemPickup ??= new SampleBank("Sounds/Items/Pickup");
    public static SampleBank ItemDrop => itemDrop ??= new SampleBank("Sounds/Items/Drop");
    public static SampleBank ItemEquip => itemEquip ??= new SampleBank("Sounds/Items/Equip");
    public static SampleBank ItemCraft => itemCraft ??= new SampleBank("Sounds/Items/Craft");
    public static SampleBank ItemHotbar => itemHotbar ??= new SampleBank("Sounds/Items/HotbarSelect");

    private static SampleBank voiceAttack;
    private static SampleBank voiceCough;
    private static SampleBank voiceDeath;
    private static SampleBank voiceFrozen;
    private static SampleBank voiceGasp;
    private static SampleBank voiceHurt;
    private static SampleBank voiceJump;
    private static SampleBank voiceReflection;
    private static SampleBank voiceRun;
    private static SampleBank voiceShocked;
    private static SampleBank voiceSigh;

    public static SampleBank VoiceAttack => voiceAttack ??= new SampleBank("Sounds/Player/Voice/Attack");
    public static SampleBank VoiceCough => voiceCough ??= new SampleBank("Sounds/Player/Voice/Cough");
    public static SampleBank VoiceDeath => voiceDeath ??= new SampleBank("Sounds/Player/Voice/Death");
    public static SampleBank VoiceFrozen => voiceFrozen ??= new SampleBank("Sounds/Player/Voice/Frozen");
    public static SampleBank VoiceGasp => voiceGasp ??= new SampleBank("Sounds/Player/Voice/Gasp");
    public static SampleBank VoiceHurt => voiceHurt ??= new SampleBank("Sounds/Player/Voice/Hurt");
    public static SampleBank VoiceJump => voiceJump ??= new SampleBank("Sounds/Player/Voice/Jump");
    public static SampleBank VoiceReflection => voiceReflection ??= new SampleBank("Sounds/Player/Voice/Reflection");
    public static SampleBank VoiceRun => voiceRun ??= new SampleBank("Sounds/Player/Voice/Run");
    public static SampleBank VoiceShocked => voiceShocked ??= new SampleBank("Sounds/Player/Voice/Shocked");
    public static SampleBank VoiceSigh => voiceSigh ??= new SampleBank("Sounds/Player/Voice/Sigh");

    private static SampleBank bodyDrink;
    private static SampleBank bodyEat;

    public static SampleBank BodyDrink => bodyDrink ??= new SampleBank("Sounds/Player/Body/Drink");
    public static SampleBank BodyEat => bodyEat ??= new SampleBank("Sounds/Player/Body/Eat");

    private static SampleBank uiClick;
    private static SampleBank uiHover;
    private static SampleBank uiOpen;
    private static SampleBank uiClose;
    private static SampleBank uiNotification;

    public static SampleBank UIClick => uiClick ??= new SampleBank("Sounds/UI/Click");
    public static SampleBank UIHover => uiHover ??= new SampleBank("Sounds/UI/Hover");
    public static SampleBank UIOpen => uiOpen ??= new SampleBank("Sounds/UI/Open");
    public static SampleBank UIClose => uiClose ??= new SampleBank("Sounds/UI/Close");
    public static SampleBank UINotification => uiNotification ??= new SampleBank("Sounds/UI/Notification");

    private static SampleBank blockHitWood;
    public static SampleBank BlockHitWood => blockHitWood ??= new SampleBank("Sounds/BlockHit/Wood");

    private static SampleBank ambientWind;
    private static SampleBank ambientWindForest;
    private static SampleBank ambientBirds;
    private static SampleBank ambientNight;
    private static SampleBank ambientCave;
    private static SampleBank ambientCaveDeep;
    private static SampleBank ambientCaveDeepier;
    private static SampleBank ambientRainCalm;
    private static SampleBank ambientRainStrong;
    private static SampleBank ambientRiver;
    private static SampleBank ambientSea;
    private static SampleBank ambientStreamCalm;
    private static SampleBank ambientWaterfallCalm;
    private static SampleBank ambientWaterfallStrong;
    private static SampleBank ambientFireBig;
    private static SampleBank ambientFireMedium;
    private static SampleBank ambientFireSmall;

    public static SampleBank AmbientWind => ambientWind ??= new SampleBank("Sounds/Ambient/Wind");
    public static SampleBank AmbientBirds => ambientBirds ??= new SampleBank("Sounds/Ambient/Birds");
    public static SampleBank AmbientNight => ambientNight ??= new SampleBank("Sounds/Ambient/Night");
    public static SampleBank AmbientCave => ambientCave ??= new SampleBank("Sounds/Ambient/Cave");
    public static SampleBank AmbientRain => ambientRainCalm ??= new SampleBank("Sounds/Ambient/Rain");
    public static SampleBank AmbientWater => ambientRiver ??= new SampleBank("Sounds/Ambient/Water");
    public static SampleBank AmbientFire => ambientFireMedium ??= new SampleBank("Sounds/Ambient/Fire");
}