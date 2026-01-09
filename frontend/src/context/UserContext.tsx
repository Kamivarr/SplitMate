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

export const UserProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(() => {
    const raw = localStorage.getItem("splitmate_user");
    return raw ? JSON.parse(raw) : null;
  });
  const [users, setUsers] = useState<User[]>([]);

  const loginUser = (userData: User, token: string) => {
    localStorage.setItem("splitmate_token", token);
    localStorage.setItem("splitmate_user", JSON.stringify(userData));
    setUser(userData);
  };

  const logoutUser = () => {
    localStorage.removeItem("splitmate_token");
    localStorage.removeItem("splitmate_user");
    setUser(null);
  };

  useEffect(() => {
    // Pobieramy użytkowników (wymaga teraz tokena, więc jeśli nie jesteśmy zalogowani, backend rzuci 401)
    getUsers().then(setUsers).catch(() => {
      console.log("Niezalogowany lub błąd pobierania użytkowników");
    });
  }, [user]); // Odśwież listę po zalogowaniu

  return (
    <UserContext.Provider value={{ user, users, loginUser, logoutUser }}>
      {children}
    </UserContext.Provider>
  );
};

export function useUser() {
  const ctx = useContext(UserContext);
  if (!ctx) throw new Error("useUser must be used within UserProvider");
  return ctx;
}