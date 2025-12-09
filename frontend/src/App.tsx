import React from "react";
import { UserProvider } from "./context/UserContext";
import { Dashboard } from "./pages/Dashboard";

export const App: React.FC = () => {
  return (
    <UserProvider>
      <div style={{ padding: 20, fontFamily: "Segoe UI, Roboto, sans-serif" }}>
        <h1>SplitMate — demo</h1>
        <Dashboard />
      </div>
    </UserProvider>
  );
};

export default App;
