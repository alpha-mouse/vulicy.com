export enum AdministrativeType {
  Region = 10,
  District = 15,
  VillageCouncil = 20,
  Capital = 41,
  RegionCenterCity = 43,
  DistrictCenterCity = 45,
  OtherCity = 47,
  CenterTown = 51,
  OtherTown = 53,
  ResortTown = 55,
  WorkTown = 57,
  VillageHomestead = 61,
  VillageSettlement = 63,
  VillageHamlet = 65,
  VillageAgroTown = 67,
  SpecialEconomicZone = 71,
}

export const ADMINISTRATIVE_PREFIX: Record<number, string> = {
  [AdministrativeType.Region]: 'вобл.',
  [AdministrativeType.District]: 'раён',
  [AdministrativeType.VillageCouncil]: 'п.с.',
  [AdministrativeType.Capital]: 'г.',
  [AdministrativeType.RegionCenterCity]: 'г.',
  [AdministrativeType.DistrictCenterCity]: 'г.',
  [AdministrativeType.OtherCity]: 'г.',
  [AdministrativeType.CenterTown]: 'г.п.',
  [AdministrativeType.OtherTown]: 'г.п.',
  [AdministrativeType.ResortTown]: 'к.п.',
  [AdministrativeType.WorkTown]: 'р.п.',
  [AdministrativeType.VillageHomestead]: 'х.',
  [AdministrativeType.VillageSettlement]: 'п.',
  [AdministrativeType.VillageHamlet]: 'в.',
  [AdministrativeType.VillageAgroTown]: 'аг.',
  [AdministrativeType.SpecialEconomicZone]: 'АЭЗ',
};

export interface Administrative {
  id: number;
  nameBeTarask: string;
  type: AdministrativeType;
  childAdministratives: Administrative[] | null;
}
