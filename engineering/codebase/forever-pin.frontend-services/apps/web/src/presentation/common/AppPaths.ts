/** Builds the product's in-app addresses, shared by pages and the route table. */
export const AppPaths = {
  home: () => '/',
  pricing: () => '/pricing',
  blog: () => '/blog',
  blogPost: (slug: string) => `/blog/${encodeURIComponent(slug)}`,
  codes: () => '/app',
  newCode: () => '/app/new',
  copyCode: (id: string, mode: string) => `/app/new?copyOf=${encodeURIComponent(id)}&mode=${mode}`,
  editCode: (id: string) => `/app/${encodeURIComponent(id)}/edit`,
  billing: () => '/app/billing',
} as const;
