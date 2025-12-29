import { api } from "./client";
import type { QuoteResponse } from "./types";

export async function getQuote(
  symbol: string,
  forceRefresh: boolean = false
): Promise<QuoteResponse> {
  const response = await api.get<QuoteResponse>("/quotes", {
    params: { symbol, forceRefresh }
  });

  return response.data;
}
