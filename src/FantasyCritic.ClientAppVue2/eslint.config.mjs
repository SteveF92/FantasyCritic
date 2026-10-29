import globals from "globals";
import pluginJs from "@eslint/js";
import tseslint from "typescript-eslint";
import pluginVue from "eslint-plugin-vue";

/** @type {import('eslint').Linter.FlatConfig[]} */
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
        __dirname: "readonly", // Adding `__dirname` global from your original config
        process: "readonly", // Vite statically replaces `process.env.NODE_ENV` at build time
      },
    },
  },
  // Root-level Node scripts (dev server setup, Vite config) run outside the browser
  {
    files: ["aspnetcore-https.js", "vite.config.js", "vite.client.config.js", "vite.build.config.js", "eslint.config.mjs"],
    languageOptions: {
      globals: {
        ...globals.node,
      },
    },
  },
  // JavaScript and TypeScript specific configurations
  pluginJs.configs.recommended,
  ...tseslint.configs.recommended,
  // Vue-specific configurations (vue2-* presets: this app runs Vue 2.7, and the
  // default "flat/essential" preset targets Vue 3, flagging valid Vue 2 syntax
  // like .sync/.native modifiers and filters as errors)
  ...pluginVue.configs["flat/vue2-essential"],
  {
    files: ["**/*.vue"],
    languageOptions: {
      parserOptions: {
        parser: tseslint.parser, // Set TypeScript parser for Vue files
      },
    },
    rules: {
      "vue/multi-word-component-names": "off", // Retaining rules from original config
      "vue/valid-v-slot": ["error", { allowModifiers: true }],
      "no-console": process.env.NODE_ENV === "production" ? "warn" : "off",
      "no-debugger": process.env.NODE_ENV === "production" ? "warn" : "off",
    },
  },
  // Vue 3 deprecations already cleared from the codebase, kept out ahead of the upgrade. Some of
  // them (the events API, $set/$delete, Vue.config.keyCodes) are about script, not templates.
  {
    files: ["src/**/*.{js,ts,vue}"],
    rules: {
      "vue/no-deprecated-data-object-declaration": "error",
      "vue/no-deprecated-delete-set": "error",
      "vue/no-deprecated-dollar-listeners-api": "error",
      "vue/no-deprecated-dollar-scopedslots-api": "error",
      "vue/no-deprecated-events-api": "error",
      "vue/no-deprecated-functional-template": "error",
      "vue/no-deprecated-html-element-is": "error",
      "vue/no-deprecated-inline-template": "error",
      "vue/no-deprecated-model-definition": "error",
      "vue/no-deprecated-props-default-this": "error",
      "vue/no-deprecated-router-link-tag-prop": "error",
      "vue/no-deprecated-scope-attribute": "error",
      "vue/no-deprecated-slot-attribute": "error",
      "vue/no-deprecated-slot-scope-attribute": "error",
      "vue/no-deprecated-v-is": "error",
      "vue/no-deprecated-v-on-number-modifiers": "error",
      "vue/no-deprecated-vue-config-keycodes": "error",
    },
  },
];
