import globals from "globals";
import pluginJs from "@eslint/js";
import tseslint from "typescript-eslint";
import pluginVue from "eslint-plugin-vue";

/** @type {import('eslint').Linter.Config[]} */
export default [
  // Build output and NSwag-generated code are not hand-written; keep them out of lint
  // (mirrors the root .gitignore entries for these paths — `--ignore-path` was removed
  // in ESLint 9's flat config, so ignoring has to live here instead).
  {
    ignores: ["dist/**", "src/api/generated/**"],
  },
  // Global settings for all files
  {
    files: ["**/*.{js,mjs,cjs,ts,vue}"],
    languageOptions: {
      globals: {
        ...globals.browser,
      },
    },
  },
  // Root-level Node scripts (dev server setup, Vite config) run outside the browser
  {
    files: ["aspnetcore-https.js", "vite.config.js", "eslint.config.mjs"],
    languageOptions: {
      globals: {
        ...globals.node,
      },
    },
  },
  // JavaScript and TypeScript specific configurations
  pluginJs.configs.recommended,
  ...tseslint.configs.recommended,
  // Vue-specific configurations
  ...pluginVue.configs["flat/essential"],
  {
    files: ["**/*.vue"],
    languageOptions: {
      parserOptions: {
        parser: tseslint.parser, // Set TypeScript parser for Vue files
      },
    },
    rules: {
      // Views and components are named for what they are: league.vue, faq.vue
      "vue/multi-word-component-names": "off",
      "vue/valid-v-slot": ["error", { allowModifiers: true }],
      // Every component's script is TypeScript; a new one without lang="ts" would go unchecked.
      "vue/block-lang": ["error", { script: { lang: "ts" } }],
      // Typed emits are declared as validators whose parameters exist only for their types:
      // emits: { gameSelected: (_game: MasterGameYearViewModel) => true }
      "@typescript-eslint/no-unused-vars": ["error", { argsIgnorePattern: "^_" }],
      "no-console": process.env.NODE_ENV === "production" ? "warn" : "off",
      "no-debugger": process.env.NODE_ENV === "production" ? "warn" : "off",
    },
  },
];
