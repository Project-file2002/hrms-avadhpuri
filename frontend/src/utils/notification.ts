import { App } from 'antd';

let messageApi: ReturnType<typeof App.useApp>['message'] | null = null;

export function initNotification(instance: ReturnType<typeof App.useApp>) {
  messageApi = instance.message;
}

function getMessage() {
  if (!messageApi) {
    console.warn('Notification not initialized. Call initNotification first.');
    return null;
  }
  return messageApi;
}

export function notifySuccess(content: string) {
  getMessage()?.success(content);
}

export function notifyError(content: string) {
  getMessage()?.error(content);
}

export function notifyInfo(content: string) {
  getMessage()?.info(content);
}

export function notifyWarning(content: string) {
  getMessage()?.warning(content);
}
