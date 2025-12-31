namespace Vulicy.Domain;

public enum FeatureType
{
    Unknown = 0,
    /// <summary> вуліца </summary>
    Street = 11,
    /// <summary> праспэкт </summary>
    Avenue = 12,
    /// <summary> плошча </summary>
    Square = 14,
    /// <summary> бульвар </summary>
    Boulevard = 15,
    /// <summary> тракт </summary>
    HighRoad = 16,
    /// <summary> набярэжная </summary>
    Riverside = 17,
    /// <summary> шаша </summary>
    Highway = 18,
    /// <summary> кальцо </summary>
    Roundabout = 19,
    /// <summary> завулак </summary>
    Alley = 21,
    /// <summary> праезд </summary>
    Driveway = 22,
    /// <summary> тупік </summary>
    DeadEnd = 23,
    /// <summary> спуск </summary>
    Descent = 24,
    /// <summary> заезд </summary>
    Entryway = 25,
    /// <summary> парк </summary>
    Park = 34,
    /// <summary> сквэр </summary>
    PublicGarden = 39,
    /// <summary> станцыя </summary>
    Station = 35,
}