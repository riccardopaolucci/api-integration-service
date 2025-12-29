import type { QuoteResponse } from "../api/types";

type Props = {
  quote: QuoteResponse;
};

export function QuoteCard({ quote }: Props) {
  const formattedTime = new Date(quote.lastUpdatedUtc).toLocaleString();

  return (
    <div
      style={{
        border: "1px solid #ddd",
        borderRadius: 8,
        padding: 16
      }}
    >
      <h2 style={{ marginTop: 0 }}>{quote.symbol}</h2>

      <p style={{ fontSize: 18, margin: "8px 0" }}>
        <strong>{quote.price}</strong> {quote.currency}
      </p>

      <p style={{ margin: "8px 0" }}>Last updated: {formattedTime}</p>
      <p style={{ margin: "8px 0" }}>Source: {quote.source}</p>
    </div>
  );
}
