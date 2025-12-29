import React, { useState } from "react";
import { getQuote } from "../api/quotes";
import type { QuoteResponse } from "../api/types";
import { QuoteCard } from "../components/QuoteCard";

export function QuotesPage() {
  const [symbol, setSymbol] = useState("");
  const [forceRefresh, setForceRefresh] = useState(false);

  const [quote, setQuote] = useState<QuoteResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    const trimmed = symbol.trim().toUpperCase();
    if (!trimmed) return;

    setLoading(true);
    setError(null);
    setQuote(null);

    try {
      const result = await getQuote(trimmed, forceRefresh);
      setQuote(result);
    } catch {
      setError("Failed to fetch quote");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div style={{ maxWidth: 520, margin: "40px auto" }}>
      <h1>Quotes</h1>

      <form onSubmit={handleSubmit} style={{ marginBottom: 16 }}>
        <div style={{ marginBottom: 12 }}>
          <label>
            Symbol
            <input
              value={symbol}
              onChange={(e) => setSymbol(e.target.value)}
              type="text"
              name="symbol"
              style={{ display: "block", width: "100%" }}
              placeholder="e.g. AAPL"
            />
          </label>
        </div>

        <label style={{ display: "block", marginBottom: 12 }}>
          <input
            type="checkbox"
            checked={forceRefresh}
            onChange={(e) => setForceRefresh(e.target.checked)}
          />{" "}
          Force refresh
        </label>

        <button type="submit" disabled={loading}>
          {loading ? "Loading..." : "Get quote"}
        </button>
      </form>

      {loading && <p>Loading...</p>}

      {error && <p role="alert">{error}</p>}

      {quote && <QuoteCard quote={quote} />}
    </div>
  );
}
