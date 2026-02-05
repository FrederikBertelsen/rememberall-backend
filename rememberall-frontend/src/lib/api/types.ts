/**
 * All TypeScript interfaces for RememberAll backend DTOs
 * These mirror the backend exactly for type safety
 */

// ============================================================================
// USER DTOs
// ============================================================================

export interface UserDto {
  id: string;
  name: string;
  email: string;
}

export interface CreateUserDto {
  name: string;
  email: string;
  password: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

// ============================================================================
// TODO LIST DTOs
// ============================================================================

export interface TodoListDto {
  id: string;
  ownerId: string;
  name: string;
  updatedAt: string;
  items: TodoItemDto[];
  accessors?: ListAccessDto[];
  invites?: InviteDto[];
}

export interface CreateTodoListDto {
  name: string;
}

export interface UpdateTodoListDto {
  id: string;
  name: string;
}

// ============================================================================
// TODO ITEM DTOs
// ============================================================================

export interface TodoItemDto {
  id: string;
  text: string;
  isCompleted: boolean;
  completionCount: number;
}

export interface CreateTodoItemDto {
  todoListId: string;
  text: string;
}

export interface UpdateTodoItemDto {
  id: string;
  text: string;
}

// ============================================================================
// LIST ACCESS DTOs
// ============================================================================

export interface ListAccessDto {
  id: string;
  userId: string;
  userName: string;
  userEmail: string;
  listId: string;
  listName: string;
  createdAt: string;
}

// ============================================================================
// INVITE DTOs
// ============================================================================

export interface InviteDto {
  id: string;
  inviteSenderId: string;
  inviteSenderName: string;
  inviteRecieverId: string;
  inviteRecieverName: string;
  listId: string;
  listName: string;
}

export interface CreateInviteDto {
  inviteRecieverEmail: string;
  listId: string;
}
