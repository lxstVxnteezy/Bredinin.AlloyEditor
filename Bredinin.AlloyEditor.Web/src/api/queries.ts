import { keepPreviousData, useQuery } from "@tanstack/react-query";
import {
  getElements,
  getGrades,
  getGradesForMainElement,
  getSystems,
  type GradesParams,
} from "./index";

// Keeps the previous page on screen while the next one loads.
export const useGrades = (params: GradesParams) =>
  useQuery({
    queryKey: ["grades", "list", params],
    queryFn: () => getGrades(params),
    placeholderData: keepPreviousData,
  });

export const useSystems = () => useQuery({ queryKey: ["systems"], queryFn: getSystems });

export const useElements = () => useQuery({ queryKey: ["elements"], queryFn: getElements });

export const useGradesForMainElement = (id: string | null) =>
  useQuery({
    queryKey: ["grades", "for-main-element", id],
    queryFn: () => getGradesForMainElement(id as string),
    enabled: id !== null,
  });
