// structuredClone throws on a Vue reactive proxy at any depth, and toRaw only unwraps the outermost one.
// What gets cloned here all came from the API as JSON, so a JSON round trip loses nothing.
export function deepClone<T>(value: T): T {
  return value === undefined ? value : JSON.parse(JSON.stringify(value));
}
