/**
 * Invites service - handles sending and accepting invites for shared lists
 */

import { apiGet, apiPost, apiPatch, apiDelete } from '$lib/api/client';
import type { InviteDto, CreateInviteDto } from '$lib/api/types';

/**
 * Send an invite to share a list with another user
 */
export async function sendInvite(data: CreateInviteDto): Promise<InviteDto> {
  return apiPost<InviteDto>('/invites', data);
}

/**
 * Get invites sent by the current user
 */
export async function getSentInvites(): Promise<InviteDto[]> {
  return apiGet<InviteDto[]>('/invites/sent');
}

/**
 * Get invites received by the current user
 */
export async function getReceivedInvites(): Promise<InviteDto[]> {
  return apiGet<InviteDto[]>('/invites/received');
}

/**
 * Accept a received invite
 */
export async function acceptInvite(inviteId: string): Promise<void> {
  return apiPatch<void>(`/invites/${inviteId}/accept`, {});
}

/**
 * Decline/delete an invite
 */
export async function declineInvite(inviteId: string): Promise<void> {
  return apiDelete(`/invites/${inviteId}`);
}
