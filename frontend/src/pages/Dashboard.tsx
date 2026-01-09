import React, { useEffect, useState } from "react";
import { getGroups, getExpenses } from "../api/api";
import type { Group, Expense } from "../types";
import { GroupList } from "../components/GroupList";
import { ExpenseForm } from "../components/ExpenseForm";
import { Summary } from "../components/Summary";
import { CreateGroupForm } from "../components/CreateGroupForm"; // 1. Importujemy formularz tworzenia grup
import { useUser } from "../context/UserContext";

export const Dashboard: React.FC = () => {
  const [groups, setGroups] = useState<Group[]>([]);
  const [selectedGroup, setSelectedGroup] = useState<Group | null>(null);
  const [expenses, setExpenses] = useState<Expense[]>([]);
  
  const { user, users, logoutUser } = useUser();

  // Funkcja odświeżająca wszystko (grupy i wydatki)
  const refreshAll = () => {
    getGroups().then(newGroups => {
      setGroups(newGroups);
      // Jeśli mamy wybraną grupę, zaktualizujmy jej obiekt, aby Summary/ExpenseForm miały świeże dane
      if (selectedGroup) {
        const updated = newGroups.find(g => g.id === selectedGroup.id);
        if (updated) setSelectedGroup(updated);
      }
    }).catch(() => setGroups([]));
  };

  useEffect(() => {
    refreshAll();
  }, []);

  // Pobieranie wydatków po zmianie grupy lub odświeżeniu grup
  useEffect(() => {
    if (selectedGroup) {
      getExpenses()
        .then(all => {
          // Filtrujemy tylko wydatki dla danej grupy i pomijamy techniczne spłaty (opcjonalnie)
          // Jeśli chcesz widzieć spłaty na liście wydatków, usuń filtr !e.isSettlement
          setExpenses(all.filter(e => e.groupId === selectedGroup.id));
        })
        .catch(() => setExpenses([]));
    } else {
      setExpenses([]);
    }
  }, [selectedGroup]);

  return (
    <div style={{ padding: 12, maxWidth: "1200px", margin: "0 auto" }}>
      <header style={{ 
        display: "flex", 
        justifyContent: "space-between", 
        alignItems: "center", 
        padding: "10px 20px", 
        background: "#f8f9fa", 
        borderRadius: 8, 
        marginBottom: 20,
        border: "1px solid #ddd" 
      }}>
        <div>
          Witaj, <strong>{user?.name || "Użytkowniku"}</strong>! 👋
        </div>
        <button 
          onClick={logoutUser} 
          style={{ padding: "5px 15px", background: "#ff4d4f", color: "white", border: "none", borderRadius: 4, cursor: "pointer" }}
        >
          Wyloguj
        </button>
      </header>

      <div style={{ display: "grid", gridTemplateColumns: "300px 1fr", gap: 20 }}>
        {/* LEWA KOLUMNA */}
        <div>
          {/* 2. FORMULARZ TWORZENIA GRUPY */}
          <CreateGroupForm onGroupCreated={refreshAll} />
          
          <div style={{ marginTop: 20 }}>
            <GroupList 
              groups={groups} 
              onSelect={g => setSelectedGroup(g)} 
              selectedGroupId={selectedGroup?.id ?? null} 
            />
          </div>
        </div>

        {/* PRAWA KOLUMNA */}
        <div>
          {selectedGroup ? (
            <>
              <div style={{ marginBottom: 20 }}>
                <h2 style={{ marginTop: 0 }}>Grupa: {selectedGroup.name}</h2>
              </div>

              <ExpenseForm
                group={selectedGroup}
                // Używamy członków grupy, a jeśli grupa pusta (nowa), wszystkich dostępnych użytkowników
                users={selectedGroup.members && selectedGroup.members.length > 0 ? selectedGroup.members : users}
                onAdded={refreshAll}
              />

              <div style={{ border: "1px solid #eee", padding: 12, borderRadius: 8, marginTop: 12, background: "#fff" }}>
                <h3 style={{ marginTop: 0 }}>Historia Wydatków</h3>
                {expenses.length > 0 ? (
                  <ul style={{ paddingLeft: 20 }}>
                    {expenses.map(e => (
                      <li key={e.id} style={{ marginBottom: 10, color: e.isSettlement ? "#4caf50" : "#000" }}>
                        <strong>{e.description}</strong> — <strong>{e.amount} zł</strong>
                        <br />
                        <small style={{ color: "#666" }}>
                          {e.isSettlement ? "✅ Rozliczenie długu" : `Zapłacił(a): ${selectedGroup.members.find(u => u.id === e.paidByUserId)?.name || "Nieznany"}`}
                        </small>
                      </li>
                    ))}
                  </ul>
                ) : (
                  <div style={{ color: "#999", fontStyle: "italic" }}>Brak wpisów w tej grupie.</div>
                )}
              </div>

              {/* 3. PODSUMOWANIE I ROZLICZENIA */}
              <Summary group={selectedGroup} onSettled={refreshAll} />
            </>
          ) : (
            <div style={{ textAlign: "center", marginTop: 100, color: "#999", border: "2px dashed #ddd", padding: 40, borderRadius: 12 }}>
              <h3>Wybierz grupę z listy po lewej lub stwórz nową</h3>
              <p>Zarządzaj wspólnymi wydatkami w jednym miejscu.</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};