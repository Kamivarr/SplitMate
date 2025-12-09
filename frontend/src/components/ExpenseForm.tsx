import React, { useEffect, useState } from "react";
import type { Group, User } from "../types";
import { createExpenseFromDto } from "../api/api"; // <- nowa funkcja API

type Props = {
  group: Group;
  users: User[];
  onAdded?: () => void;
};

export const ExpenseForm: React.FC<Props> = ({ group, users, onAdded }) => {
  const [description, setDescription] = useState("");
  const [amount, setAmount] = useState<number | "">("");
  const [paidBy, setPaidBy] = useState<number | null>(users[0]?.id ?? null);
  const [sharedWith, setSharedWith] = useState<number[]>(group.members.map(m => m.id));

  useEffect(() => {
    setSharedWith(group.members.map(m => m.id));
    setPaidBy(users[0]?.id ?? null);
  }, [group, users]);

  function toggleUser(id: number) {
    setSharedWith(prev => prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]);
  }

  async function submit(e?: React.FormEvent) {
    e?.preventDefault();
    if (!description || amount === "" || amount === 0 || paidBy === null) {
      alert("Wypełnij opis, kwotę i wybierz kto zapłacił.");
      return;
    }

    const payload = {
      description,
      amount: Number(amount),
      groupId: group.id,
      paidByUserId: paidBy,
      sharedWithUserIds: sharedWith
    };

    try {
      await createExpenseFromDto(payload); // używamy endpointu /from-dto
      setDescription("");
      setAmount("");
      if (onAdded) onAdded();
    } catch (err) {
      console.error(err);
      alert("Błąd podczas tworzenia wydatku (zobacz konsolę).");
    }
  }

  return (
    <form onSubmit={submit} style={{ border: "1px solid #eee", padding: 12, borderRadius: 8, marginTop: 12 }}>
      <h3>Dodaj wydatek — {group.name}</h3>
      <div style={{ marginBottom: 8 }}>
        <input placeholder="Opis" value={description} onChange={e => setDescription(e.target.value)} style={{ width: "100%" }} />
      </div>
      <div style={{ marginBottom: 8 }}>
        <input type="number" placeholder="Kwota" value={amount} onChange={e => setAmount(e.target.value === "" ? "" : Number(e.target.value))} />
      </div>
      <div style={{ marginBottom: 8 }}>
        <label>Kto zapłacił: </label>
        <select value={paidBy ?? ""} onChange={e => setPaidBy(Number(e.target.value))}>
          <option value="">-- wybierz --</option>
          {users.map(u => <option key={u.id} value={u.id}>{u.name}</option>)}
        </select>
      </div>
      <div style={{ marginBottom: 8 }}>
        <div>Uczestnicy:</div>
        {group.members.map(m => (
          <label key={m.id} style={{ display: "block" }}>
            <input type="checkbox" checked={sharedWith.includes(m.id)} onChange={() => toggleUser(m.id)} /> {m.name}
          </label>
        ))}
      </div>
      <div>
        <button type="submit">Dodaj wydatek</button>
      </div>
    </form>
  );
};
