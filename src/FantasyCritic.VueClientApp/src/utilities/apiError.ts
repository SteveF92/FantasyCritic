import { ApiException } from '@/api/clients';

const defaultFallback = 'Something went wrong.';

type ValidationProblemDetails = { errors: Record<string, string[]> };

/**
 * The message to show for a failed API call: the server's own message for a 4xx, otherwise `fallback`.
 *
 * Anything that isn't an `ApiException` (a network failure, a bug in the calling code) is rethrown,
 * so it reaches the console instead of turning into an error message.
 */
export function getApiErrorMessage(error: unknown, fallback: string = defaultFallback): string {
  if (!(error instanceof ApiException)) {
    throw error;
  }

  // 5xx bodies are the error page or a stack trace, not a message
  if (error.status < 400 || error.status >= 500) {
    return fallback;
  }

  const body = error.response.trim();
  if (body === '') {
    return fallback;
  }

  // `BadRequest("message")` is plain text when the client sends no Accept header (operations
  // with no response type), a JSON string when it sends Accept: application/json, and model
  // validation failures are ValidationProblemDetails
  const contentType = String(error.headers['content-type'] ?? '');
  if (contentType.startsWith('text/plain')) {
    return body;
  }

  if (!contentType.includes('json')) {
    return fallback;
  }

  let parsed: unknown;
  try {
    parsed = JSON.parse(body);
  } catch {
    return fallback;
  }

  if (typeof parsed === 'string' && parsed.trim() !== '') {
    return parsed;
  }

  if (isValidationProblemDetails(parsed)) {
    const messages = Object.values(parsed.errors).flat();
    if (messages.length > 0) {
      return messages.join(' ');
    }
  }

  return fallback;
}

function isValidationProblemDetails(value: unknown): value is ValidationProblemDetails {
  if (typeof value !== 'object' || value === null || !('errors' in value)) {
    return false;
  }

  const errors = value.errors;
  return typeof errors === 'object' && errors !== null && Object.values(errors).every((x) => Array.isArray(x) && x.every((m) => typeof m === 'string'));
}
