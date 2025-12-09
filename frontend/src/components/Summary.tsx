import React, { useEffect, useState } from "react";
import type { Group } from "../types";
import { getGroupSummary } from "../api/api";

type Props = { group: Group };

type SummaryItem = {
  fromUserName: string;
  toUserName: string;
  amount: number;
};

export const Summary: React.FC<Props> = ({ group }) => {
  const [summary, setSummary] = useState<SummaryItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    getGroupSummary(group.id)
      .then((s) => setSummary(Array.isArray(s) ? s : []))
      .catch(() => setSummary([]))
      .finally(() => setLoading(false));
  }, [group]);

  if (loading) return <div style={{ marginTop: 12 }}>Ładowanie podsumowania...</div>;
  if (summary.length === 0) return <div style={{ marginTop: 12 }}>Brak rozliczeń (lub brak wydatków)</div>;

  return (
    <div style={{ border: "1px solid #eee", padding: 12, borderRadius: 8, marginTop: 12 }}>
      <h3>Podsumowanie — {group.name}</h3>
      <ul>
        {summary.map((s, i) => (
          <li key={i}>
            {s.fromUserName} → {s.toUserName}: <strong>{s.amount.toFixed(2)} zł</strong>
          </li>
        ))}
      </ul>
    </div>
  );
};
