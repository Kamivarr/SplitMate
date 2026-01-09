import React from "react";
import { UserProvider, useUser } from "./context/UserContext";
import { Dashboard } from "./pages/Dashboard";
import { Login } from "./components/Login";

// Tworzymy pomocniczy komponent opakowujący logikę wyświetlania
const MainContent: React.FC = () => {
  const { user } = useUser();

  // Jeśli user jest null (niezalogowany), pokazujemy formularz logowania
  if (!user) {
    return (
      <div style={{ display: "flex", justifyContent: "center", marginTop: 50 }}>
        <Login />
      </div>
    );
  }

  // Jeśli user istnieje, pokazujemy właściwy Dashboard
  return <Dashboard />;
};

export const App: React.FC = () => {
  return (
    <UserProvider>
      <div style={{ padding: 20, fontFamily: "Segoe UI, Roboto, sans-serif" }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <h1>SplitMate — demo</h1>
        </div>
        <hr />
        <MainContent />
      </div>
    </UserProvider>
  );
};

export default App;