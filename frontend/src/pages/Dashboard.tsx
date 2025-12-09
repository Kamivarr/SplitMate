import React, { useEffect, useState } from "react";
import { getGroups, getExpenses } from "../api/api";
import type { Group, Expense } from "../types";
import { GroupList } from "../components/GroupList";
import { ExpenseForm } from "../components/ExpenseForm";
import { Summary } from "../components/Summary";
import { useUser } from "../context/UserContext";

export const Dashboard: React.FC = () => {
  const [groups, setGroups] = useState<Group[]>([]);
  const [selectedGroup, setSelectedGroup] = useState<Group | null>(null);
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const { users } = useUser();

  // Pobranie grup
  useEffect(() => {
    getGroups().then(setGroups).catch(() => setGroups([]));
  }, []);

  // Aktualizacja selectedGroup jeśli zmieniły się grupy
  useEffect(() => {
    if (selectedGroup) {
      const found = groups.find(g => g.id === selectedGroup.id);
      if (found) setSelectedGroup(found);
    }
  }, [groups]);

  // Pobranie wydatków dla wybranej grupy
  useEffect(() => {
    if (selectedGroup) {
      getExpenses()
        .then(all => setExpenses(all.filter(e => e.groupId === selectedGroup.id)))
        .catch(() => setExpenses([]));
    } else {
      setExpenses([]);
    }
  }, [selectedGroup, groups]);

  // Funkcja odświeżania wydatków (przekazywana do ExpenseForm)
  const refreshExpenses = () => {
    if (selectedGroup) {
      getExpenses()
        .then(all => setExpenses(all.filter(e => e.groupId === selectedGroup.id)))
        .catch(() => setExpenses([]));
    }
  };

  return (
    <div style={{ display: "grid", gridTemplateColumns: "300px 1fr", gap: 12 }}>
      <div>
        <GroupList 
          groups={groups} 
          onSelect={g => setSelectedGroup(g)} 
          selectedGroupId={selectedGroup?.id ?? null} 
        />
      </div>
      <div>
        {selectedGroup ? (
          <>
            <ExpenseForm
              group={selectedGroup}
              users={selectedGroup.members.length > 0 ? selectedGroup.members : users}
              onAdded={() => {
                getGroups().then(setGroups); // aktualizacja grup
                refreshExpenses();           // aktualizacja wydatków
              }}
            />

            {/* Lista wydatków */}
            <div style={{ border: "1px solid #eee", padding: 12, borderRadius: 8, marginTop: 12 }}>
              <h3>Wydatki — {selectedGroup.name}</h3>
              {expenses.length > 0 ? (
              <ul>
                {expenses.map(e => {
                  const paidByUser = selectedGroup.members.find(u => u.id === e.paidByUserId);
                  return (
                    <li key={e.id}>
                      {e.description} — {e.amount} zł (Zapłacił: {paidByUser?.name ?? "Nieznany"})
                      <br />
                      Uczestnicy: {e.sharedWithUsers.map(u => u.name).join(", ")}
                    </li>
                  );
                })}
              </ul>
              ) : (
                <div>Brak wydatków</div>
              )}
            </div>

            <Summary group={selectedGroup} />
          </>
        ) : (
          <div>Wybierz grupę po lewej, żeby zobaczyć szczegóły</div>
        )}
      </div>
    </div>
  );
};
