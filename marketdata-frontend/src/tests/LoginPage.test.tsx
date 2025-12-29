import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Routes, Route } from "react-router-dom";

import { AuthContext, type AuthContextValue } from "../context/authContext";
import { LoginPage } from "../pages/LoginPage";

function renderWithAuth(authValue: AuthContextValue) {
  return render(
    <AuthContext.Provider value={authValue}>
      <MemoryRouter initialEntries={["/login"]}>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/quotes" element={<div>Quotes Page</div>} />
        </Routes>
      </MemoryRouter>
    </AuthContext.Provider>
  );
}

describe("LoginPage", () => {
  it("logs in and navigates to /quotes", async () => {
    const user = userEvent.setup();

    const loginMock = vi.fn().mockResolvedValue(undefined);
    const authValue: AuthContextValue = {
      state: { token: null, isAuthenticated: false },
      login: loginMock,
      logout: vi.fn(),
      setTokenForTest: vi.fn()
    };

    renderWithAuth(authValue);

    await user.type(screen.getByLabelText(/username/i), "riccardo");
    await user.type(screen.getByLabelText(/password/i), "password");
    await user.click(screen.getByRole("button", { name: /log in/i }));

    expect(loginMock).toHaveBeenCalledWith("riccardo", "password");
    expect(await screen.findByText("Quotes Page")).toBeInTheDocument();
  });

  it("shows error on invalid credentials", async () => {
    const user = userEvent.setup();

    const loginMock = vi.fn().mockRejectedValue(new Error("Invalid credentials"));
    const authValue: AuthContextValue = {
      state: { token: null, isAuthenticated: false },
      login: loginMock,
      logout: vi.fn(),
      setTokenForTest: vi.fn()
    };

    renderWithAuth(authValue);

    await user.type(screen.getByLabelText(/username/i), "bad");
    await user.type(screen.getByLabelText(/password/i), "wrong");
    await user.click(screen.getByRole("button", { name: /log in/i }));

    expect(await screen.findByRole("alert")).toHaveTextContent(/invalid credentials/i);
  });
});
