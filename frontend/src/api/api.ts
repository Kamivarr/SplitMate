import type { User, Group, Expense } from "../types";

const BASE = "http://localhost:5000/api";

export interface LoginResponse {
  token: string;
  username: string;
  userId: number;
}
// Pomocnicza funkcja do nagłówka z tokenem
const getHeaders = () => {
  const token = localStorage.getItem("splitmate_token");
  const headers: HeadersInit = {
    "Content-Type": "application/json"
  };
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }
  return headers;
};

// ========================
// AUTH
// ========================
export async function login(payload: any): Promise<LoginResponse> {
  const response = await fetch(`${BASE}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });

  if (!response.ok) {
    throw new Error("Błąd logowania");
  }

  // Zwracamy wynik bezpośrednio
  return response.json();
}
// ========================
// USERS
// ========================
export async function getUsers(): Promise<User[]> {
  const res = await fetch(`${BASE}/users`, { headers: getHeaders() });
  if (!res.ok) throw new Error("Failed to fetch users");
  return res.json();
}

// ========================
// GROUPS
// ========================
export async function getGroups(): Promise<Group[]> {
  const res = await fetch(`${BASE}/groups`, { headers: getHeaders() });
  if (!res.ok) throw new Error("Failed to fetch groups");
  return res.json();
}

// ========================
// EXPENSES
// ========================
export async function getExpenses(): Promise<Expense[]> {
  const res = await fetch(`${BASE}/expenses`, { headers: getHeaders() });
  if (!res.ok) throw new Error("Failed to fetch expenses");
  return res.json();
}

export async function createExpenseFromDto(payload: {
  description: string;
  amount: number;
  groupId: number;
  paidByUserId: number;
  sharedWithUserIds: number[];
}): Promise<Expense> {
  const res = await fetch(`${BASE}/expenses/from-dto`, {
    method: "POST",
    headers: getHeaders(),
    body: JSON.stringify(payload),
  });

  if (!res.ok) throw new Error(`Failed to create expense: ${res.status}`);
  return res.json();
}

export async function getGroupSummary(groupId: number): Promise<any> {
  const res = await fetch(`${BASE}/summary/group/${groupId}`, { headers: getHeaders() });
  if (!res.ok) throw new Error(`Failed to fetch summary: ${res.status}`);
  return res.json();
}