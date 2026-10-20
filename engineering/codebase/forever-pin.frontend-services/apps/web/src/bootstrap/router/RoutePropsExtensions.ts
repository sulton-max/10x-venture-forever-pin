/** Extends route declarations with the decoding of address values into typed page props. */
export const RoutePropsExtensions = {
  /** Reads one route parameter or query value as text; an empty string when absent. */
  toText(value: unknown): string {
    const text = Array.isArray(value) ? value[0] : value;
    return typeof text === 'string' ? text : '';
  },

  /** Reads one value as a member of `values`, or `undefined` when absent or unknown. */
  toMember<TValue extends string>(value: unknown, values: Readonly<Record<string, TValue>>): TValue | undefined {
    const text = RoutePropsExtensions.toText(value);
    return Object.values(values).find((member) => member === text);
  },
} as const;
