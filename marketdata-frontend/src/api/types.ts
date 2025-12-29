export type LoginRequest = {
  username: string;
  password: string;
};

export type LoginResponse = {
  token: string;
  expiresAtUtc?: string;
  username?: string;
  roles?: string[];
};

export type QuoteResponse = {
  id: number;
  symbol: string;
  price: number;
  currency: string;
  lastUpdatedUtc: string;
  source: string;
};
