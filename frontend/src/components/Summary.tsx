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

/**
 * Komponent wyświetlający podsumowanie długów w grupie ("Kto komu wisi").
 * Umożliwia wierzycielom zatwierdzanie spłat.
 */
export const Summary: React.FC<Props> = ({ group, onSettled }) => {
  const [summary, setSummary] = useState<SummaryItem[]>([]);
  const [loading, setLoading] = useState(true);

  // Pobieramy ID zalogowanego użytkownika (rzutowanie na Number zapewnia bezpieczeństwo typów)
  const loggedInUserId = Number(localStorage.getItem("userId") || 0);

  const fetchSummary = () => {
    setLoading(true);
    getGroupSummary(group.id)
      .then((s) => setSummary(Array.isArray(s) ? s : []))
      .catch(() => setSummary([]))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    fetchSummary();
  }, [group]); // Odświeżamy, gdy zmienia się wybrana grupa

  /**
   * Obsługuje proces zatwierdzania spłaty długu.
   */
  const handleSettle = async (item: SummaryItem) => {
    const confirmMsg = `Czy potwierdzasz, że otrzymałeś ${item.amount.toFixed(2)} zł od użytkownika ${item.fromUserName}?`;
    if (!window.confirm(confirmMsg)) return;

    try {
      await settleDebt({
        groupId: group.id,
        fromUserId: item.fromUserId,
        fromUserName: item.fromUserName,
        toUserId: item.toUserId,
        toUserName: item.toUserName,
        amount: item.amount
      });
      
      // Odświeżamy widok po udanej operacji
      fetchSummary();
      if (onSettled) onSettled();
    } catch (err) {
      alert("Błąd: Nie udało się zatwierdzić spłaty. Sprawdź czy jesteś zalogowany jako odbiorca.");
      console.error(err);
    }
  };

  if (loading) return <div style={{ marginTop: 12, color: "#666" }}>⏳ Przeliczanie bilansu...</div>;
  
  if (summary.length === 0) {
    return (
      <div style={{ marginTop: 12, padding: 10, backgroundColor: "#f0fff4", borderRadius: 8, color: "#2e7d32", border: "1px solid #c3e6cb" }}>
        Wszyscy są rozliczeni! 🎉 Brak długów w tej grupie.
      </div>
    );
  }

  return (
    <div style={{ border: "1px solid #eee", padding: 15, borderRadius: 8, marginTop: 12, backgroundColor: "#fff", boxShadow: "0 2px 4px rgba(0,0,0,0.05)" }}>
      <h3 style={{ marginTop: 0, borderBottom: "1px solid #eee", paddingBottom: 10 }}>📊 Bilans grupy</h3>
      <ul style={{ listStyle: "none", padding: 0, margin: 0 }}>
        {summary.map((s, i) => {
          // Sprawdzamy, czy zalogowany użytkownik jest odbiorcą tego długu
          const isCreditor = loggedInUserId === s.toUserId;

          return (
            <li key={i} style={{ padding: "10px 0", borderBottom: i !== summary.length - 1 ? "1px solid #f9f9f9" : "none" }}>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                <span>
                  <span style={{ color: "#d32f2f", fontWeight: 500 }}>{s.fromUserName}</span>
                  {' → '}
                  <span style={{ color: "#388e3c", fontWeight: 500 }}>{s.toUserName}</span>
                  : <strong>{s.amount.toFixed(2)} zł</strong>
                </span>
                
                {isCreditor ? (
                  <button 
                    onClick={() => handleSettle(s)}
                    title="Kliknij, jeśli otrzymałeś pieniądze"
                    style={{ 
                      backgroundColor: "#4caf50", 
                      color: "white", 
                      border: "none", 
                      padding: "6px 12px", 
                      borderRadius: 4, 
                      cursor: "pointer",
                      fontSize: "13px",
                      fontWeight: "bold"
                    }}
                  >
                    Potwierdź odbiór ✅
                  </button>
                ) : (
                  <span style={{ fontSize: "12px", color: "#999", fontStyle: "italic", background: "#f5f5f5", padding: "2px 6px", borderRadius: 4 }}>
                    Oczekiwanie na {s.toUserName} ⏳
                  </span>
                )}
              </div>
            </li>
          );
        })}
      </ul>
    </div>
  );
};