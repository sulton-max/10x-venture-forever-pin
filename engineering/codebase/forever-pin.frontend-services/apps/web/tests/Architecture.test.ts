import { ESLint } from 'eslint';
import { describe, expect, it } from 'vitest';

const eslint = new ESLint();

/** Lints one snippet as if it lived at `filePath` and returns the boundary findings. */
async function boundaryFindings(code: string, filePath: string) {
  const [result] = await eslint.lintText(code, { filePath });
  return (result?.messages ?? []).filter((message) => message.ruleId === 'architecture/boundaries');
}

describe('layer and slice boundaries', () => {
  it('rejects a presentation import from application orchestration', async () => {
    const findings = await boundaryFindings(
      "import Page from '../../presentation/codes/pages/CodesListPage.vue'; void Page;",
      'src/application/codes/Invalid.ts',
    );
    expect(findings).toHaveLength(1);
  });

  it('rejects an import across domain slices of one layer', async () => {
    const findings = await boundaryFindings(
      "import { billingApi } from '@/integration/billing'; void billingApi;",
      'src/integration/codes/Invalid.ts',
    );
    expect(findings).toHaveLength(1);
  });

  it('allows a slice to reach its integration seam and the common slice', async () => {
    const findings = await boundaryFindings(
      "import { codesApi } from '@/integration/codes'; import { apiClient } from '@/integration/common'; void codesApi; void apiClient;",
      'src/application/codes/Valid.ts',
    );
    expect(findings).toEqual([]);
  });
});
