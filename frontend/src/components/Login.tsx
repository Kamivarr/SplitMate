import React from "react";
import { useUser } from "../context/UserContext";

export const Login: React.FC = () => {
  const { user, setUser, users } = useUser();

  return (
    <div style={{ border: "1px solid #ddd", padding: 12, borderRadius: 8 }}>
      <h3>Login (wybierz użytkownika)</h3>
      {user ? (
        <div>
          <div>Jesteś zalogowany jako: <strong>{user.name}</strong></div>
          <button onClick={() => setUser(null)} style={{ marginTop: 8 }}>Wyloguj</button>
        </div>
      ) : (
        <div>
          <select onChange={(e) => {
            const id = Number(e.target.value);
            const u = users.find(x => x.id === id) ?? null;
            setUser(u);
          }} defaultValue="">
            <option value="" disabled>Wybierz użytkownika...</option>
            {users.map(u => <option key={u.id} value={u.id}>{u.name}</option>)}
          </select>
        </div>
      )}
    </div>
  );
};
