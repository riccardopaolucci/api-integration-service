import { api } from "./client";
import type { LoginRequest, LoginResponse } from "./types";
import axios from "axios";

export async function login(
  req: LoginRequest
): Promise<LoginResponse> {
  try {
    const response = await api.post<LoginResponse>("/auth/login", req);
    return response.data;
  } catch (err: unknown) {
    if (axios.isAxiosError(err) && err.response?.status === 401) {
      throw new Error("Invalid credentials");
    }

    throw new Error("Login failed");
  }
}
