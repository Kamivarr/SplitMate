import type { User, Group, Expense } from "../types";

/**
 * Adres bazowy API. W środowisku produkcyjnym powinien być pobierany z zmiennych środowiskowych.
 */
const BASE_URL = "http://localhost:5000/api";

/**
 * Odpowiedź serwera po poprawnym uwierzytelnieniu.
 */
export interface LoginResponse {
  token: string;
  username: string;
  userId: number;
}

/**
 * Generuje nagłówki HTTP zawierające token autoryzacyjny z localStorage
 * oraz definicję typu zawartości (JSON).
 */
const getHeaders = (): HeadersInit => {
  const token = localStorage.getItem("splitmate_token");
  const headers: HeadersInit = {
    "Content-Type": "application/json",
  };
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }
  return headers;
};

// =============================================================================
// MODUŁ: AUTENTYKACJA (AUTH)
// =============================================================================

/**
 * Loguje użytkownika do systemu.
 * @param payload Obiekt zawierający Login i Password.
 */
export async function login(payload: any): Promise<LoginResponse> {
  const response = await fetch(`${BASE_URL}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    throw new Error("Błąd logowania: Nieprawidłowe dane uwierzytelniające.");
  }

  return response.json();
}

// =============================================================================
// MODUŁ: UŻYTKOWNICY (USERS)
// =============================================================================

/**
 * Pobiera listę wszystkich zarejestrowanych użytkowników.
 */
export async function getUsers(): Promise<User[]> {
  const res = await fetch(`${BASE_URL}/users`, { headers: getHeaders() });
  if (!res.ok) throw new Error("Błąd podczas pobierania listy użytkowników.");
  return res.json();
}

// =============================================================================
// MODUŁ: GRUPY (GROUPS)
// =============================================================================

/**
 * Pobiera listę grup, do których należy zalogowany użytkownik.
 */
export async function getGroups(): Promise<Group[]> {
  const res = await fetch(`${BASE_URL}/groups`, { headers: getHeaders() });
  if (!res.ok) throw new Error("Błąd podczas pobierania grup.");
  return res.json();
}

/**
 * Tworzy nową grupę rozliczeniową.
 * @param payload Nazwa grupy oraz lista ID członków.
 */
export async function createGroup(payload: { name: string; memberIds: number[] }): Promise<Group> {
  const res = await fetch(`${BASE_URL}/groups`, {
    method: "POST",
    headers: getHeaders(),
    body: JSON.stringify(payload),
  });

  if (!res.ok) throw new Error("Błąd podczas tworzenia nowej grupy.");
  return res.json();
}

// =============================================================================
// MODUŁ: WYDATKI (EXPENSES)
// =============================================================================

/**
 * Pobiera listę wszystkich wydatków.
 */
export async function getExpenses(): Promise<Expense[]> {
  const res = await fetch(`${BASE_URL}/expenses`, { headers: getHeaders() });
  if (!res.ok) throw new Error("Błąd podczas pobierania wydatków.");
  return res.json();
}

/**
 * Dodaje nowy wydatek do grupy.
 */
export async function createExpense(payload: {
  description: string;
  amount: number;
  groupId: number;
  paidByUserId: number;
  sharedWithUserIds: number[];
}): Promise<Expense> {
  const res = await fetch(`${BASE_URL}/expenses`, {
    method: "POST",
    headers: getHeaders(),
    body: JSON.stringify(payload),
  });

  if (!res.ok) throw new Error(`Błąd tworzenia wydatku: Status ${res.status}`);
  return res.json();
}

// =============================================================================
// MODUŁ: PODSUMOWANIA I ROZLICZENIA (SUMMARY/SETTLE)
// =============================================================================

/**
 * Pobiera wyliczony bilans (kto komu ile jest winien) dla danej grupy.
 * @param groupId ID grupy docelowej.
 */
export async function getGroupSummary(groupId: number): Promise<any> {
  const res = await fetch(`${BASE_URL}/summary/group/${groupId}`, { headers: getHeaders() });
  if (!res.ok) throw new Error("Błąd podczas generowania podsumowania grupy.");
  return res.json();
}

/**
 * Rejestruje spłatę długu między użytkownikami.
 * Uwaga: Backend weryfikuje, czy akcję wykonuje odbiorca płatności.
 */
export async function settleDebt(payload: {
  groupId: number;
  fromUserId: number;
  fromUserName: string;
  toUserId: number;
  toUserName: string;
  amount: number;
}): Promise<any> {
  const res = await fetch(`${BASE_URL}/summary/settle`, {
    method: "POST",
    headers: getHeaders(),
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    const errorData = await res.json().catch(() => ({}));
    throw new Error(errorData?.message || "Błąd podczas zatwierdzania spłaty.");
  }
  
  return res.json();
}