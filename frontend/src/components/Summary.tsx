import React, { useEffect, useState } from "react";
import type { Group } from "../types";
import { getGroupSummary, settleDebt } from "../api/api";

type Props = { 
  group: Group;
  onSettled?: () => void; 
};

type SummaryItem = {
  fromUserId: number;
  fromUserName: string;
  toUserId: number;
  toUserName: string;
  amount: number;
};

export const Summary: React.FC<Props> = ({ group, onSettled }) => {
  const [summary, setSummary] = useState<SummaryItem[]>([]);
  const [loading, setLoading] = useState(true);

  // Pobieramy ID zalogowanego użytkownika z localStorage
  const loggedInUserId = Number(localStorage.getItem("userId"));

  const fetchSummary = () => {
    setLoading(true);
    getGroupSummary(group.id)
      .then((s) => setSummary(Array.isArray(s) ? s : []))
      .catch(() => setSummary([]))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    fetchSummary();
  }, [group]);

  const handleSettle = async (item: SummaryItem) => {
    if (!window.confirm(`Czy potwierdzasz odbiór spłaty ${item.amount} zł od ${item.fromUserName}?`)) return;

    try {
      await settleDebt({
        groupId: group.id,
        fromUserId: item.fromUserId,
        fromUserName: item.fromUserName,
        toUserId: item.toUserId,
        toUserName: item.toUserName,
        amount: item.amount
      });
      fetchSummary();
      if (onSettled) onSettled();
    } catch (err) {
      alert("Błąd podczas rozliczania. Tylko odbiorca może potwierdzić spłatę.");
    }
  };

  if (loading) return <div style={{ marginTop: 12 }}>Ładowanie podsumowania...</div>;
  if (summary.length === 0) return <div style={{ marginTop: 12 }}>Wszyscy są rozliczeni! 🎉</div>;

  return (
    <div style={{ border: "1px solid #eee", padding: 12, borderRadius: 8, marginTop: 12, backgroundColor: "#fff" }}>
      <h3 style={{ marginTop: 0 }}>Podsumowanie — {group.name}</h3>
      <ul style={{ listStyle: "none", padding: 0 }}>
        {summary.map((s, i) => (
          <li key={i} style={{ padding: "8px 0", borderBottom: i !== summary.length - 1 ? "1px solid #f0f0f0" : "none" }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <span>
                {s.fromUserName} → {s.toUserName}: <strong>{s.amount.toFixed(2)} zł</strong>
              </span>
              
              {loggedInUserId === s.toUserId ? (
                <button 
                  onClick={() => handleSettle(s)}
                  style={{ backgroundColor: "#4caf50", color: "white", border: "none", padding: "5px 10px", borderRadius: 4, cursor: "pointer" }}
                >
                  Potwierdź odbiór ✅
                </button>
              ) : (
                <span style={{ fontSize: "11px", color: "#888", fontStyle: "italic" }}>
                  Oczekiwanie na {s.toUserName}
                </span>
              )}
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
};