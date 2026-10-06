import { useQuery } from "@tanstack/react-query";
import { getElements, getGrades, getGradesForMainElement, getSystems } from "./index";

export const useGrades = () => useQuery({ queryKey: ["grades"], queryFn: getGrades });

export const useSystems = () => useQuery({ queryKey: ["systems"], queryFn: getSystems });

export const useElements = () => useQuery({ queryKey: ["elements"], queryFn: getElements });

export const useGradesForMainElement = (id: string | null) =>
  useQuery({
    queryKey: ["grades", "for-main-element", id],
    queryFn: () => getGradesForMainElement(id as string),
    enabled: id !== null,
  });
