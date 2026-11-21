import api from "./api";

export function obterStreak() {
  return api.get("/streak").then((r) => r.data);
}

export function registrarCheckIn() {
  return api.post("/streak/checkin").then((r) => r.data);
}
