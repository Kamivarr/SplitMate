/**
 * Reprezentuje użytkownika systemu.
 */
export interface User {
  id: number;
  name: string;
}

/**
 * Reprezentuje grupę wydatków (np. "Wyjazd w Tatry").
 */
export interface Group {
  id: number;
  name: string;
  members: User[];
}

/**
 * Reprezentuje pojedynczy wydatek lub wpis rozliczeniowy (spłatę).
 */
export interface Expense {
  id: number;
  description: string;
  amount: number;
  groupId: number;
  paidByUserId: number;
  /** * Flaga oznaczająca, czy wpis jest spłatą długu (true) 
   * czy zwykłym wydatkiem (false).
   */
  isSettlement: boolean; 
  sharedWithUsers: User[];
}