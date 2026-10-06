export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
}

export interface Composition {
  id?: string;
  chemicalElementId: string;
  minValue?: number | null;
  maxValue?: number | null;
  exactValue?: number | null;
}

export interface HeatTreatment {
  id: string;
  heatTreatmentTypeId: string;
  heatTreatmentTypeName: string;
  temperatureMin?: number | null;
  temperatureMax?: number | null;
  temperatureExact?: number | null;
  holdingTimeMin?: number | null;
  holdingTimeMax?: number | null;
  holdingTimeExact?: number | null;
  coolingMedium?: string | null;
  description?: string | null;
  stepOrder?: number | null;
  isDefault: boolean;
}

export interface MechanicalProperty {
  id: string;
  propertyTypeId: string;
  propertyTypeName: string;
  propertyTypeUnit: string;
  propertyTypeSymbol?: string | null;
  valueMin?: number | null;
  valueMax?: number | null;
  valueExact?: number | null;
  condition?: string | null;
  source?: string | null;
  heatTreatmentId?: string | null;
}

export interface AlloyGrade {
  id: string;
  name: string;
  description?: string | null;
  alloySystemId: string;
  chemicalCompositions: Composition[];
  heatTreatments: HeatTreatment[];
  mechanicalProperties: MechanicalProperty[];
}

export interface CreateHeatTreatmentRequest {
  heatTreatmentTypeId: string;
  temperatureMin?: number | null;
  temperatureMax?: number | null;
  temperatureExact?: number | null;
  holdingTimeMin?: number | null;
  holdingTimeMax?: number | null;
  holdingTimeExact?: number | null;
  coolingMedium?: string | null;
  description?: string | null;
  stepOrder?: number | null;
  isDefault: boolean;
}

export interface CreateMechanicalPropertyRequest {
  propertyTypeId: string;
  heatTreatmentId?: string | null;
  valueMin?: number | null;
  valueMax?: number | null;
  valueExact?: number | null;
  condition?: string | null;
  source?: string | null;
  notes?: string | null;
}

export interface CreateAlloyGradeRequest {
  name: string;
  description?: string | null;
  alloySystemId: string;
  chemicalCompositions: Omit<Composition, "id">[];
  heatTreatments?: CreateHeatTreatmentRequest[];
  defaultMechanicalProperties?: CreateMechanicalPropertyRequest[];
}

export interface AlloySystem {
  id: string;
  baseElementId: string;
  name: string;
  description?: string | null;
}

export interface ChemicalElement {
  id: string;
  name: string;
  symbol: string;
  description?: string | null;
  isBaseForAlloySystem: boolean;
  atomicNumber: number;
  atomicWeight: number;
  group: number;
  period: number;
  density: number;
}

export interface InfoAlloyGradeByMain {
  id: string;
  name: string;
  description?: string | null;
  chemicalCompositions: Composition[];
}

export interface User {
  id: string;
  login: string;
  firstName: string;
  lastName: string;
  secondName: string;
  age: number;
  roleIds: string[];
}

export interface CreateUserRequest {
  login: string;
  firstName: string;
  lastName: string;
  secondName: string;
  age: number;
  password: string;
  roleIds: string[];
}

export interface EditUserRequest {
  id: string;
  firstName: string;
  lastName: string;
  secondName: string;
  age: number;
  roleIds: string[];
}

export interface UpdateUserResponse {
  login: string;
  firstName: string;
  lastName: string;
  secondName: string;
  roleIds: string[];
}
