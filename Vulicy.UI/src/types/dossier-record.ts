export interface DossierRecord {
  Id: number;
  NameBeTarask?: string;
  Classification: number;
  DescriptionBe?: string;
  DescriptionRu?: string;
  NamingCategoryId?: number;
}

export interface DossierRecordSearchResult {
  id: number;
  nameBeTarask?: string;
  nameBeNark?: string;
  nameRu?: string;
  descriptionBe?: string;
  descriptionRu?: string;
  classification: number;
  namingCategoryId?: number;
  numFeatures: number;
}

export interface DossierRecordEditRequest {
  nameBeTarask: string;
  nameBeNark: string;
  nameRu: string | null;
  descriptionBe: string | null;
  descriptionRu: string | null;
  classification: number;
}
