import globals from "globals";
import pluginJs from "@eslint/js";
import tseslint from "@typescript-eslint/eslint-plugin"; // Adjusted to match the package name
import pluginVue from "eslint-plugin-vue";

/** @type {import('eslint').Linter.FlatConfig[]} */
export default [
  // Global settings for all files
  {
    files: ["**/*.{js,mjs,cjs,ts,vue}"],
    languageOptions: {
      globals: {
        ...globals.browser,
        __dirname: "readonly", // Adding `__dirname` global from your original config
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
      "vue/multi-word-component-names": "off", // Retaining rules from original config
      "vue/valid-v-slot": ["error", { allowModifiers: true }],
      "no-console": process.env.NODE_ENV === "production" ? "warn" : "off",
      "no-debugger": process.env.NODE_ENV === "production" ? "warn" : "off",
    },
  },
];
