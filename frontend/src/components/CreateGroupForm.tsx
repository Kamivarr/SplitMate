import React, { useState, useEffect } from "react";
import { getUsers, createGroup } from "../api/api";
import type { User } from "../types";

export const CreateGroupForm: React.FC<{ onGroupCreated: () => void }> = ({ onGroupCreated }) => {
  const [name, setName] = useState("");
  const [availableUsers, setAvailableUsers] = useState<User[]>([]);
  const [selectedUsers, setSelectedUsers] = useState<number[]>([]);
  
  // Pobieramy ID aktualnie zalogowanego użytkownika
  const currentUserId = Number(localStorage.getItem("userId"));

  useEffect(() => {
    getUsers()
      .then(users => {
        // Filtrujemy listę, aby nie wybierać samego siebie (dodamy się automatycznie w handleSubmit)
        const others = users.filter(u => u.id !== currentUserId);
        setAvailableUsers(others);
      })
      .catch(err => console.error("Błąd pobierania użytkowników:", err));
  }, [currentUserId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Logika: Grupa musi mieć nazwę. Członkowie to Ty + osoby wybrane.
    const allMembers = [...selectedUsers, currentUserId];

    if (!name) return alert("Podaj nazwę grupy!");
    if (allMembers.length < 2) return alert("Wybierz przynajmniej jednego dodatkowego członka!");

    try {
      await createGroup({ name, memberIds: allMembers });
      setName("");
      setSelectedUsers([]);
      onGroupCreated();
      alert("Grupa stworzona!");
    } catch (err) {
      alert("Błąd podczas tworzenia grupy.");
    }
  };

  return (
    <form onSubmit={handleSubmit} style={{ 
      marginBottom: 20, 
      padding: 15, 
      background: "#fff", 
      borderRadius: 8, 
      border: "1px solid #ddd",
      boxShadow: "0 2px 4px rgba(0,0,0,0.05)" 
    }}>
      <h4 style={{ marginTop: 0 }}>🆕 Stwórz nową grupę</h4>
      
      <input 
        placeholder="Np. Wyjazd do Krakowa" 
        value={name} 
        onChange={e => setName(e.target.value)} 
        style={{ 
          width: "100%", 
          padding: "8px", 
          marginBottom: "15px", 
          borderRadius: "4px", 
          border: "1px solid #ccc",
          boxSizing: "border-box" // Ważne dla szerokości inputa
        }}
      />

      <p style={{ fontSize: "14px", fontWeight: "bold", marginBottom: 5 }}>Wybierz znajomych do grupy:</p>
      
      <div style={{ 
        maxHeight: "150px", 
        overflowY: "auto", 
        border: "1px solid #eee", 
        padding: "10px", 
        borderRadius: "4px",
        background: "#fafafa"
      }}>
        {availableUsers.length > 0 ? (
          availableUsers.map(u => (
            <label key={u.id} style={{ display: "block", marginBottom: "5px", cursor: "pointer" }}>
              <input 
                type="checkbox" 
                checked={selectedUsers.includes(u.id)}
                onChange={(e) => {
                  if (e.target.checked) setSelectedUsers([...selectedUsers, u.id]);
                  else setSelectedUsers(selectedUsers.filter(id => id !== u.id));
                }}
              /> 
              <span style={{ marginLeft: 8 }}>{u.name}</span>
            </label>
          ))
        ) : (
          <span style={{ color: "#999", fontSize: "12px" }}>Brak innych użytkowników w systemie...</span>
        )}
      </div>

      <button 
        type="submit" 
        style={{ 
          width: "100%", 
          marginTop: 15, 
          padding: "10px", 
          background: "#007bff", 
          color: "white", 
          border: "none", 
          borderRadius: "4px", 
          cursor: "pointer",
          fontWeight: "bold"
        }}
      >
        Stwórz Grupę
      </button>
    </form>
  );
};