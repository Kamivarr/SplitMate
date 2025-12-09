import React from "react";
import type { Group } from "../types";

type Props = {
  groups: Group[];
  selectedGroupId: number | null;
  onSelect: (group: Group) => void;
};

export const GroupList: React.FC<Props> = ({ groups, selectedGroupId, onSelect }) => {
  return (
    <div style={{ border: "1px solid #ddd", padding: 12, borderRadius: 8 }}>
      <h3>Grupy</h3>
      {groups.length === 0 ? (
        <div>Brak grup</div>
      ) : (
        <ul style={{ listStyle: "none", padding: 0, margin: 0 }}>
          {groups.map((g) => (
            <li key={g.id} style={{ marginBottom: 6 }}>
              <button
                onClick={() => onSelect(g)}
                style={{
                  width: "100%",
                  textAlign: "left",
                  padding: "6px 8px",
                  background: selectedGroupId === g.id ? "#e6f4ff" : "#fff",
                  border: "1px solid #ccc",
                  borderRadius: 6,
                  cursor: "pointer"
                }}
              >
                {g.name} <small style={{ float: "right", opacity: 0.7 }}>{g.members.length}</small>
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};
