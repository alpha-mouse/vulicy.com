namespace Vulicy.Domain;

public enum AdministrativeType : byte
{
    Unknown = 0,

    /// <summary> Вобласьць </summary>
    Region = 10,
    /// <summary> Раён </summary>
    District = 15,
    /// <summary> Сельсавет </summary>
    VillageCouncil = 20,

    /// <summary>  </summary>
    TerminalCutoff = 40,

    /// <summary> Сталіца Рэспублікі Беларусь </summary>
    Capital = 41,
    /// <summary> Горад абласнога падпарадкавання (АТА) </summary>
    RegionCenterCity = 43,
    /// <summary> Горад раённага падпарадкавання (АТА) </summary>
    DistrictCenterCity = 45,
    /// <summary> Горад раённага падпарадкавання (ТА) </summary>
    OtherCity = 47,

    /// <summary> Пасёлак гарадскога тыпу - гарадскі пасёлак (АТА) </summary>
    CenterTown = 51,
    /// <summary> Пасёлак гарадскога тыпу - гарадскі пасёлак (ТА) </summary>
    OtherTown = 53,
    /// <summary> Пасёлак гарадскога тыпу - курортны пасёлак (ТА) </summary>
    ResortTown = 55,
    /// <summary> Пасёлак гарадскога тыпу - рабочы пасёлак (ТА) </summary>
    WorkTown = 57,

    /// <summary> Сельскі населены пункт - хутар </summary>
    VillageHomestead = 61,
    /// <summary> Сельскі населены пункт - пасёлак </summary>
    VillageSettlement = 63,
    /// <summary> Сельскі населены пункт - вёска </summary>
    VillageHamlet = 65,
    /// <summary> Сельскі населены пункт - аграгарадок </summary>
    VillageAgroTown = 67,

    /// <summary> Асаблівая эканамічная зона </summary>
    SpecialEconomicZone = 71,
}