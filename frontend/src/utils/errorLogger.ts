const LOG_KEY = 'hrms_error_logs';
const MAX_LOGS = 100;

interface ErrorLogEntry {
  id: string;
  timestamp: string;
  level: 'error' | 'warn' | 'info';
  message: string;
  source: string;
  stack?: string;
  metadata?: Record<string, unknown>;
}

export function logError(
  message: string,
  source: string,
  error?: unknown,
  metadata?: Record<string, unknown>
) {
  const entry: ErrorLogEntry = {
    id: crypto.randomUUID(),
    timestamp: new Date().toISOString(),
    level: 'error',
    message,
    source,
    stack: error instanceof Error ? error.stack : undefined,
    metadata,
  };
  saveLog(entry);
  console.error(`[HRMS Error] ${source}: ${message}`, error ?? '');
}

export function logWarn(
  message: string,
  source: string,
  metadata?: Record<string, unknown>
) {
  const entry: ErrorLogEntry = {
    id: crypto.randomUUID(),
    timestamp: new Date().toISOString(),
    level: 'warn',
    message,
    source,
    metadata,
  };
  saveLog(entry);
  console.warn(`[HRMS Warn] ${source}: ${message}`);
}

export function logInfo(
  message: string,
  source: string,
  metadata?: Record<string, unknown>
) {
  const entry: ErrorLogEntry = {
    id: crypto.randomUUID(),
    timestamp: new Date().toISOString(),
    level: 'info',
    message,
    source,
    metadata,
  };
  saveLog(entry);
}

export function getLogs(): ErrorLogEntry[] {
  try {
    const raw = localStorage.getItem(LOG_KEY);
    return raw ? JSON.parse(raw) : [];
  } catch {
    return [];
  }
}

export function clearLogs() {
  localStorage.removeItem(LOG_KEY);
}

export function downloadLogs() {
  const logs = getLogs();
  const blob = new Blob([JSON.stringify(logs, null, 2)], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `hrms-error-logs-${new Date().toISOString().split('T')[0]}.json`;
  a.click();
  URL.revokeObjectURL(url);
}

function saveLog(entry: ErrorLogEntry) {
  try {
    const logs = getLogs();
    logs.push(entry);
    if (logs.length > MAX_LOGS) logs.splice(0, logs.length - MAX_LOGS);
    localStorage.setItem(LOG_KEY, JSON.stringify(logs));
  } catch {
  }
}
