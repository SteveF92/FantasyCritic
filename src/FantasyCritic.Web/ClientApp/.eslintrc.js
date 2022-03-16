module.exports = {
  root: true,
  env: {
    node: true,
  },
  extends: [
    "plugin:vue/essential",
    "eslint:recommended",
    "plugin:prettier/recommended",
  ],
  parserOptions: {
    parser: "@babel/eslint-parser",
  },
  rules: {
    "no-console": process.env.NODE_ENV === "production" ? "warn" : "off",
    "no-debugger": process.env.NODE_ENV === "production" ? "warn" : "off",
    "vue/valid-v-slot": ["off"],
    "no-unused-vars": ["off"],
    "vue/no-unused-vars": ["off"],
    "vue/valid-v-for": ["off"],
    "vue/require-v-for-key": ["off"],
    "vue/no-unused-components": ["off"],
    "vue/multi-word-component-names": ["off"],
    "vue/no-dupe-keys": ["off"],
    "no-dupe-keys": ["off"],
    "no-unreachable": ["off"],
    "no-sparse-arrays": ["off"],
    "vue/no-side-effects-in-computed-properties": ["off"],
    "vue/return-in-computed-property": ["off"],
    "vue/no-mutating-props": ["off"],
    "vue/no-useless-template-attributes": ["off"],
  },
  globals: {
    "_": "writable"
  }
};
