import React, { createContext, useContext, useEffect, useState } from "react";
import type { User } from "../types";
import { getUsers } from "../api/api";

type UserContextType = {
  user: User | null;
  setUser: (u: User | null) => void;
  users: User[];
};

const UserContext = createContext<UserContextType | undefined>(undefined);

export const UserProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(() => {
    const raw = localStorage.getItem("splitmate_user");
    return raw ? JSON.parse(raw) : null;
  });
  const [users, setUsers] = useState<User[]>([]);

  useEffect(() => {
    getUsers().then(setUsers).catch(() => {
      // fallback sample users if backend not ready
      setUsers([
        { id: 1, name: "Kamil" },
        { id: 2, name: "Anna" },
        { id: 3, name: "Tomek" },
      ]);
    });
  }, []);

  useEffect(() => {
    if (user) localStorage.setItem("splitmate_user", JSON.stringify(user));
    else localStorage.removeItem("splitmate_user");
  }, [user]);

  return <UserContext.Provider value={{ user, setUser, users }}>{children}</UserContext.Provider>;
};

export function useUser() {
  const ctx = useContext(UserContext);
  if (!ctx) throw new Error("useUser must be used within UserProvider");
  return ctx;
}
