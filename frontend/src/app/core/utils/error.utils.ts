import { HttpErrorResponse } from '@angular/common/http';
import { API_ERROR_MESSAGES, DEFAULT_ERROR } from '../constants/api-errors';

export function resolveApiError(err: HttpErrorResponse): string {
  if (!err.error) return DEFAULT_ERROR;

  // Format from middleware: { status: 400, error: "some message" }
  if (typeof err.error.error === 'string') {
    const msg = err.error.error as string;
    // Identity errors can be concatenated with "; " — map each part individually
    const parts = msg.split('; ');
    const mapped = parts.map(p => API_ERROR_MESSAGES[p] ?? p);
    return mapped.join(' ');
  }

  // Format from ASP.NET model validation: { errors: { Field: ["msg1"] } }
  if (err.error.errors && typeof err.error.errors === 'object') {
    const messages: string[] = Object.values(err.error.errors).flat() as string[];
    return messages.map(m => API_ERROR_MESSAGES[m] ?? m).join(' ');
  }

  return DEFAULT_ERROR;
}
