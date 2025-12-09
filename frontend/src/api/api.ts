import type { User, Group, Expense } from "../types";

const BASE = (import.meta.env.VITE_API_URL ?? "http://localhost:5000").replace(/\/+$/, "");

// ========================
// USERS
// ========================
export async function getUsers(): Promise<User[]> {
  const res = await fetch(`${BASE}/users`);
  if (!res.ok) throw new Error("Failed to fetch users");
  return res.json();
}

// ========================
// GROUPS
// ========================
export async function getGroups(): Promise<Group[]> {
  const res = await fetch(`${BASE}/groups`);
  if (!res.ok) throw new Error("Failed to fetch groups");
  return res.json();
}

// ========================
// EXPENSES
// ========================
export async function getExpenses(): Promise<Expense[]> {
  const res = await fetch(`${BASE}/expenses`);
  if (!res.ok) throw new Error("Failed to fetch expenses");
  return res.json();
}

// Tworzenie wydatku przez DTO
export async function createExpenseFromDto(payload: {
  description: string;
  amount: number;
  groupId: number;
  paidByUserId: number;
  sharedWithUserIds: number[];
}): Promise<Expense> {
  const res = await fetch(`${BASE}/expenses/from-dto`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  const text = await res.text();
  console.log("Backend response:", res.status, text);

  if (!res.ok) {
    throw new Error(`Failed to create expense: ${res.status}`);
  }

  return JSON.parse(text);
}


export async function getGroupSummary(groupId: number): Promise<any> {
  const res = await fetch(`${BASE}/summary/group/${groupId}`);
  const text = await res.text();
  console.log("Summary response:", res.status, text);
  if (!res.ok) throw new Error(`Failed to fetch summary: ${res.status}`);
  return JSON.parse(text);
}

