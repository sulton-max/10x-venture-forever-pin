/** Holds the cache keys of code reads; every key starts with `codes`, so one invalidation refreshes them all. */
export const CodeQueryKeys = {
  /** The key prefix of every code read. */
  all: ['codes'] as const,
  /** The key of the owner's list for one search. */
  list: (search: string) => ['codes', 'list', search.trim().toLowerCase()] as const,
  /** The key of one code. */
  detail: (id: string) => ['codes', 'detail', id] as const,
} as const;
