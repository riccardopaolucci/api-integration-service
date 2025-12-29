import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { QuotesPage } from "../pages/QuotesPage";
import type { QuoteResponse } from "../api/types";

// ✅ Mock the quotes API module
vi.mock("../api/quotes", () => {
  return {
    getQuote: vi.fn()
  };
});

import { getQuote } from "../api/quotes";

describe("QuotesPage", () => {
  it("fetches and displays a quote", async () => {
    const user = userEvent.setup();

    const fakeQuote: QuoteResponse = {
        id: 1,
        symbol: "AAPL",
        price: 150,
        currency: "USD",
        lastUpdatedUtc: "2025-12-29T00:00:00.000Z",
        source: "cache"
    };

    // Create a promise we can resolve later, so we can assert "Loading..."
    let resolvePromise: (v: QuoteResponse) => void;
    const pending = new Promise<QuoteResponse>((resolve) => {
        resolvePromise = resolve;
    });

    (getQuote as unknown as ReturnType<typeof vi.fn>).mockReturnValue(pending);

    render(<QuotesPage />);

    await user.type(screen.getByLabelText(/symbol/i), "AAPL");
    await user.click(screen.getByRole("button", { name: /get quote/i }));

    // Loading should be visible while promise is unresolved
    expect(
    screen.getByRole("button", { name: /loading/i })
    ).toBeDisabled();

    // Now resolve the API call
    resolvePromise!(fakeQuote);

    // Then shows quote
    expect(await screen.findByText(/AAPL/i)).toBeInTheDocument();
    expect(screen.getByText(/150/i)).toBeInTheDocument();
    expect(screen.getByText(/USD/i)).toBeInTheDocument();
    });


  it("shows error when API fails", async () => {
    const user = userEvent.setup();

    (getQuote as unknown as ReturnType<typeof vi.fn>).mockRejectedValue(
      new Error("Boom")
    );

    render(<QuotesPage />);

    await user.type(screen.getByLabelText(/symbol/i), "AAPL");
    await user.click(screen.getByRole("button", { name: /get quote/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent(
      /failed to fetch quote/i
    );
  });
});
