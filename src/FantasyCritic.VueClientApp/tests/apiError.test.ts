import { describe, expect, it } from 'vitest';
import { ApiException } from '@/api/clients';
import { getApiErrorMessage } from '@/utilities/apiError';

// Bodies and content types as the server sent them on 2026-11-07
function apiError(status: number, body: string, contentType?: string): ApiException {
  const headers: { [key: string]: string } = contentType ? { 'content-type': contentType } : {};
  return new ApiException('Bad Request', status, body, headers, null);
}

describe('getApiErrorMessage', () => {
  it("returns a plain text BadRequest message (operations that send no Accept header)", () => {
    const error = apiError(400, 'You cannot have a blank name.', 'text/plain; charset=utf-8');
    expect(getApiErrorMessage(error)).toBe('You cannot have a blank name.');
  });

  it('unquotes a JSON string BadRequest message (operations that send Accept: application/json)', () => {
    const error = apiError(400, '"You cannot have a blank name."', 'application/json; charset=utf-8');
    expect(getApiErrorMessage(error)).toBe('You cannot have a blank name.');
  });

  it('joins the messages of a validation failure', () => {
    const body = JSON.stringify({
      type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1',
      title: 'One or more validation errors occurred.',
      status: 400,
      errors: { PublisherName: ['The PublisherName field is required.'], BidAmount: ['The BidAmount field is required.'] }
    });
    const error = apiError(400, body, 'application/problem+json; charset=utf-8');
    expect(getApiErrorMessage(error)).toBe('The PublisherName field is required. The BidAmount field is required.');
  });

  it('falls back for a bare BadRequest(), which is ProblemDetails with no message', () => {
    const body = JSON.stringify({ type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1', title: 'Bad Request', status: 400 });
    const error = apiError(400, body, 'application/problem+json; charset=utf-8');
    expect(getApiErrorMessage(error, 'Could not decline the invite.')).toBe('Could not decline the invite.');
  });

  it('falls back for empty 401, 403 and 404 bodies', () => {
    expect(getApiErrorMessage(apiError(401, ''))).toBe('Something went wrong.');
    expect(getApiErrorMessage(apiError(403, ''))).toBe('Something went wrong.');
    expect(getApiErrorMessage(apiError(404, ''), 'Not found.')).toBe('Not found.');
  });

  it('falls back for a 500, whatever its body', () => {
    expect(getApiErrorMessage(apiError(500, '<!DOCTYPE html><html>Error</html>', 'text/html; charset=utf-8'))).toBe('Something went wrong.');
    expect(getApiErrorMessage(apiError(500, 'System.Exception: boom', 'text/plain; charset=utf-8'))).toBe('Something went wrong.');
  });

  it('falls back for an HTML 4xx body or JSON that does not parse', () => {
    expect(getApiErrorMessage(apiError(400, '<html></html>', 'text/html'))).toBe('Something went wrong.');
    expect(getApiErrorMessage(apiError(400, '{not json', 'application/json'))).toBe('Something went wrong.');
  });

  it('rethrows anything that is not an ApiException', () => {
    const networkFailure = new TypeError('Failed to fetch');
    expect(() => getApiErrorMessage(networkFailure)).toThrow(networkFailure);
    expect(() => getApiErrorMessage(undefined)).toThrow();
  });
});
