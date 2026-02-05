/**
 * Invites store - manages sent and received invites
 * Uses Svelte 5 runes for reactive state management
 */

import { type InviteDto, type CreateInviteDto } from '$lib/api/types';
import * as invitesService from '$lib/api/services/invites';
import { ApiError, handleApiError } from '$lib/utils/errors';

class InvitesStore {
  // ============================================================================
  // STATE (Svelte 5 runes)
  // ============================================================================

  sentInvites = $state<InviteDto[]>([]);
  receivedInvites = $state<InviteDto[]>([]);
  isLoading = $state(false);
  error = $state<string | null>(null);

  // ============================================================================
  // METHODS
  // ============================================================================

  /**
   * Fetch invites sent by the current user
   */
  async fetchSentInvites(): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      this.sentInvites = await invitesService.getSentInvites();
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Fetch invites received by the current user
   */
  async fetchReceivedInvites(): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      this.receivedInvites = await invitesService.getReceivedInvites();
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Send an invite to share a list
   */
  async sendInvite(data: CreateInviteDto): Promise<InviteDto> {
    this.isLoading = true;
    this.error = null;

    try {
      const invite = await invitesService.sendInvite(data);
      this.sentInvites.push(invite);
      return invite;
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Accept a received invite
   */
  async acceptInvite(inviteId: string): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      await invitesService.acceptInvite(inviteId);
      this.receivedInvites = this.receivedInvites.filter((i) => i.id !== inviteId);
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Decline a received invite
   */
  async declineInvite(inviteId: string): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      await invitesService.declineInvite(inviteId);
      this.receivedInvites = this.receivedInvites.filter((i) => i.id !== inviteId);
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Cancel/revoke a sent invite
   */
  async revokeInvite(inviteId: string): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      await invitesService.declineInvite(inviteId);
      this.sentInvites = this.sentInvites.filter((i) => i.id !== inviteId);
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Sync invites for a list - used for refresh polling
   * Silently updates invites without setting loading state
   */
  syncInvitesForList(listId: string, invites: InviteDto[]): void {
    // Filter to only invites for this list and merge with existing
    this.sentInvites = [
      ...this.sentInvites.filter((i) => i.listId !== listId),
      ...invites
    ];
  }
}

// Export singleton instance
export const invitesStore = new InvitesStore();
