export const CURRENCY_CODE = 'INR';
export const CURRENCY_SYMBOL = '₹';

const inrFormatter = new Intl.NumberFormat('en-IN', {
  style: 'currency',
  currency: 'INR',
  minimumFractionDigits: 0,
  maximumFractionDigits: 2,
});

const inrWholeFormatter = new Intl.NumberFormat('en-IN', {
  style: 'currency',
  currency: 'INR',
  minimumFractionDigits: 0,
  maximumFractionDigits: 0,
});

export function formatINR(amount: number | null | undefined, whole = false): string {
  if (amount == null || Number.isNaN(amount)) return '-';
  return (whole ? inrWholeFormatter : inrFormatter).format(amount);
}

export function formatINRRange(low?: number | null, high?: number | null): string {
  if (low == null) return '-';
  if (high == null) return formatINR(low, true);
  return `${formatINR(low, true)} - ${formatINR(high, true)}`;
}

export function formatSalaryChange(current: number, proposed: number): string {
  return `${formatINR(current, true)} → ${formatINR(proposed, true)}`;
}

export const INR_PREFIX = CURRENCY_SYMBOL;
