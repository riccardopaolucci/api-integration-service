import { describe, it, expect, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { AuthProvider } from "../context/AuthProvider";
import { useAuth } from "../context/authContext";

function TestConsumer() {
  const { state, setTokenForTest, logout } = useAuth();

  return (
    <div>
      <div data-testid="token">{state.token ?? ""}</div>
      <button onClick={() => setTokenForTest("test-token")}>set</button>
      <button onClick={() => logout()}>logout</button>
    </div>
  );
}

describe("AuthContext", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("stores token and clears it on logout (minimal test)", async () => {
    const user = userEvent.setup();

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>
    );

    expect(screen.getByTestId("token").textContent).toBe("");

    await user.click(screen.getByRole("button", { name: /set/i }));
    expect(screen.getByTestId("token").textContent).toBe("test-token");

    await user.click(screen.getByRole("button", { name: /logout/i }));
    expect(screen.getByTestId("token").textContent).toBe("");
  });
});
