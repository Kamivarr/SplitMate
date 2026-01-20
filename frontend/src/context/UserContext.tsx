import React, { createContext, useContext, useEffect, useState } from "react";
import type { User } from "../types";
import { getUsers } from "../api/api";

type UserContextType = {
  user: User | null;
  users: User[];
  loginUser: (userData: User, token: string) => void;
  logoutUser: () => void;
};

const UserContext = createContext<UserContextType | undefined>(undefined);

/**
 * Dostarcza globalny stan użytkownika oraz listę wszystkich użytkowników w systemie.
 */
export const UserProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  // Inicjalizacja stanu użytkownika z localStorage z obsługą błędów parsowania
  const [user, setUser] = useState<User | null>(() => {
    try {
      const raw = localStorage.getItem("splitmate_user");
      return raw ? JSON.parse(raw) : null;
    } catch (e) {
      console.error("Błąd odczytu sesji użytkownika:", e);
      return null;
    }
  });

  const [users, setUsers] = useState<User[]>([]);

  /**
   * Loguje użytkownika, zapisując token i dane w pamięci przeglądarki.
   */
  const loginUser = (userData: User, token: string) => {
    localStorage.setItem("splitmate_token", token);
    localStorage.setItem("splitmate_user", JSON.stringify(userData));
    setUser(userData);
  };

  /**
   * Wylogowuje użytkownika i czyści dane sesji.
   */
  const logoutUser = () => {
    localStorage.removeItem("splitmate_token");
    localStorage.removeItem("splitmate_user");
    localStorage.removeItem("userId"); // Czyścimy też pomocnicze ID używane w Summary
    setUser(null);
  };

  // Pobieranie listy użytkowników przy starcie lub zmianie stanu logowania
  useEffect(() => {
    const token = localStorage.getItem("splitmate_token");
    if (token) {
      getUsers()
        .then(setUsers)
        .catch((err) => console.warn("Nie udało się pobrać listy użytkowników (możliwy brak autoryzacji):", err));
    }
  }, [user]);

  return (
    <UserContext.Provider value={{ user, users, loginUser, logoutUser }}>
      {children}
    </UserContext.Provider>
  );
};

/**
 * Hook ułatwiający dostęp do kontekstu użytkownika.
 */
export function useUser() {
  const ctx = useContext(UserContext);
  if (!ctx) throw new Error("useUser must be used within UserProvider");
  return ctx;
}