export interface User {
  id: number;
  name: string;
}

export interface Group {
  id: number;
  name: string;
  members: User[];
}

export interface Expense {
  id: number;
  description: string;
  amount: number;
  groupId: number;
  paidByUserId: number;
  paidByUserName: string;
  isSettlement: boolean; 
  sharedWithUsers: User[];
}