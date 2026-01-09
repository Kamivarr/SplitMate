import React, { useState } from "react";
import { useUser } from "../context/UserContext";
import * as api from "../api/api";

export const Login: React.FC = () => {
  const { user, users, loginUser, logoutUser } = useUser();
  
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!username || !password) {
      return alert("Wpisz login i hasło!");
    }
    
    setIsLoading(true);
    try {
      const data = await api.login({ login: username, password: password });
      
      // --- KLUCZOWA POPRAWKA ---
      // Musimy zapisać userId w localStorage, aby Summary.tsx mógł go odczytać
      // i porównać z s.toUserId
      localStorage.setItem("userId", data.userId.toString());
      // -------------------------

      // Zapisujemy użytkownika i token w kontekście (UserContext pewnie zapisuje token)
      loginUser({ id: data.userId, name: data.username }, data.token);
      
      setUsername("");
      setPassword("");
    } catch (err) {
      alert("Błąd logowania! Sprawdź login lub hasło.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div style={{ border: "1px solid #ddd", padding: 20, borderRadius: 8, maxWidth: 300, background: "#fff" }}>
      <h3>Logowanie</h3>
      
      {user ? (
        <div>
          <div style={{ marginBottom: 10 }}>
            Zalogowany jako: <strong>{user.name}</strong> (ID: {localStorage.getItem("userId")})
          </div>
          <button 
            onClick={() => {
              localStorage.removeItem("userId"); // Czyścimy przy wylogowaniu
              logoutUser();
            }} 
            style={{ width: "100%", padding: "8px", cursor: "pointer" }}
          >
            Wyloguj
          </button>
        </div>
      ) : (
        <form onSubmit={handleLogin}>
          <div style={{ marginBottom: 10 }}>
            <input 
              type="text" 
              placeholder="Login" 
              value={username} 
              onChange={(e) => setUsername(e.target.value)} 
              style={{ width: "100%", padding: "8px", boxSizing: "border-box" }}
            />
          </div>
          <div style={{ marginBottom: 10 }}>
            <input 
              type="password" 
              placeholder="Hasło" 
              value={password} 
              onChange={(e) => setPassword(e.target.value)} 
              style={{ width: "100%", padding: "8px", boxSizing: "border-box" }}
            />
          </div>
          <button 
            type="submit" 
            disabled={isLoading}
            style={{ 
              width: "100%", 
              padding: "10px", 
              backgroundColor: "#007bff", 
              color: "white", 
              border: "none", 
              borderRadius: 4,
              cursor: isLoading ? "not-allowed" : "pointer"
            }}
          >
            {isLoading ? "Logowanie..." : "Zaloguj"}
          </button>
          
          <div style={{ marginTop: 15, fontSize: "0.8em", color: "#666" }}>
            <strong>Dostępni użytkownicy:</strong><br />
            {users.length > 0 ? users.map(u => u.name).join(", ") : "Ładowanie..."}
          </div>
        </form>
      )}
    </div>
  );
};