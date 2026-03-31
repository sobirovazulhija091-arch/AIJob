import type { Notification } from './api'

/** Backend `NotificationType.MessageReceived` */
const MESSAGE_RECEIVED = 3

export function isMessageNotification(n: Notification): boolean {
  if (n.type === MESSAGE_RECEIVED) return true
  if (n.type == null && n.title?.trim().toLowerCase() === 'new message') return true
  return false
}

export function hasUnreadMessageAlerts(notifications: Notification[]): boolean {
  return notifications.some((n) => !n.isRead && isMessageNotification(n))
}

/** Unread items that are not in-app message previews (those use the Messages nav pip). */
export function hasUnreadGeneralAlerts(notifications: Notification[]): boolean {
  return notifications.some((n) => !n.isRead && !isMessageNotification(n))
}

export const NAV_ALERTS_EVENT = 'aijob:nav-alerts'

export function notifyNavAlertsChanged() {
  window.dispatchEvent(new Event(NAV_ALERTS_EVENT))
}
