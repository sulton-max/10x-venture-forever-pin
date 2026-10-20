import js from '@eslint/js';
import vue from 'eslint-plugin-vue';
import globals from 'globals';
import { dirname, resolve } from 'node:path';
import tseslint from 'typescript-eslint';

// Layers: presentation → application → domain; application → integration → domain; bootstrap composes all.
const allowedLayers = {
  bootstrap: ['bootstrap', 'presentation', 'application', 'domain', 'integration'],
  presentation: ['presentation', 'application', 'domain'],
  application: ['application', 'domain', 'integration'],
  integration: ['integration', 'domain'],
  domain: ['domain'],
};

/**
 * Checks resolved internal imports, so relative paths cannot bypass a boundary: a layer reaches only
 * its allowed layers, and a domain slice only itself or that layer's `common/` slice.
 */
const boundariesRule = {
  meta: { type: 'problem', schema: [], messages: { edge: 'Disallowed layer or domain-slice dependency: {{target}}.' } },
  create(context) {
    const filename = context.filename.replaceAll('\\', '/');
    const rootIndex = filename.lastIndexOf('/src/');
    if (rootIndex < 0) return {};
    const root = filename.slice(0, rootIndex + 5);
    const [layer, slice] = filename.slice(root.length).split('/');
    const allowed = allowedLayers[layer];
    if (!allowed) return {};
    const check = (node) => {
      const source = node.source?.value;
      if (typeof source !== 'string') return;
      const target = source.startsWith('@/')
        ? resolve(root, source.slice(2))
        : source.startsWith('.')
          ? resolve(dirname(filename), source)
          : null;
      if (!target?.startsWith(root)) return;
      const [targetLayer, targetSlice] = target.slice(root.length).split('/');
      const crossesSlice =
        layer !== 'bootstrap' && targetSlice !== undefined && targetSlice !== slice && targetSlice !== 'common';
      if (!allowed.includes(targetLayer) || crossesSlice)
        context.report({ node, messageId: 'edge', data: { target: target.slice(root.length) } });
    };
    return {
      ImportDeclaration: check,
      ExportNamedDeclaration: check,
      ExportAllDeclaration: check,
      ImportExpression: check,
    };
  },
};

/** Bans packages a layer must not run on. */
function runtimeBan(packages, message) {
  return ['error', { patterns: [{ group: packages, message }] }];
}

export default [
  { ignores: ['dist/**', 'node_modules/**', 'coverage/**'] },
  js.configs.recommended,
  ...tseslint.configs.recommended,
  ...vue.configs['flat/recommended'],
  {
    files: ['**/*.{ts,vue}'],
    // __APP_VERSION__ is the build's version, defined by vite.config.ts.
    languageOptions: {
      globals: { ...globals.browser, __APP_VERSION__: 'readonly' },
      parserOptions: { parser: tseslint.parser },
    },
    // Prettier owns template formatting; ESLint keeps correctness and ordering rules.
    rules: {
      'vue/max-attributes-per-line': 'off',
      'vue/singleline-html-element-content-newline': 'off',
      'vue/multiline-html-element-content-newline': 'off',
      'vue/html-closing-bracket-newline': 'off',
      'vue/html-indent': 'off',
      'vue/html-self-closing': 'off',
      'vue/multi-word-component-names': 'off',
    },
  },
  { files: ['*.mjs', 'scripts/**/*.mjs', '*config.ts'], languageOptions: { globals: globals.node } },
  {
    files: ['src/**/*.{ts,vue}'],
    plugins: { architecture: { rules: { boundaries: boundariesRule } } },
    rules: { 'architecture/boundaries': 'error' },
  },
  {
    files: ['src/integration/**/*.ts'],
    rules: {
      'no-restricted-imports': runtimeBan(
        ['vue', 'vue-router', '@tanstack/*'],
        'Keep transport independent of the component runtime.',
      ),
    },
  },
  {
    files: ['src/domain/**/*.ts'],
    rules: {
      'no-restricted-imports': runtimeBan(
        ['vue', 'vue-router', '@tanstack/*', '@wow-two-beta/*', '@microsoft/*', 'zod'],
        'Domain rules depend on nothing at runtime.',
      ),
    },
  },
];
