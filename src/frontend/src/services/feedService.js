import api from "./api";

export function obterFeed() {
  return api.get("/feed").then((r) => r.data);
}
