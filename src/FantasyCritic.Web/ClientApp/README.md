# FantasyCritic Web Client (ClientApp)

This directory contains the frontend client application for FantasyCritic, built with Vue.js and designed to work with the FantasyCritic web platform.

## Overview

- **Framework:** Vue.js (Single Page Application)
- **Build Tools:** Vite, Babel, Prettier, ESLint, Jest
- **State Management:** Vuex
- **Testing:** Jest, Vue Test Utils
- **Styling:** CSS, SCSS

## Directory Structure

- `src/` - Main source code for the Vue app
  - `components/` - Vue components (tables, widgets, modals, etc.)
  - `assets/` - Images and static assets
  - `css/`, `scss/` - Stylesheets
  - `mixins/` - Vue mixins
  - `router/` - Vue Router configuration
  - `store/` - Vuex store modules
  - `views/` - Page-level Vue components
  - `tests/` - Unit tests for components
- `public/` - Static files served directly (favicon, manifest, images, etc.)
- `index.html` - Main HTML entry point
- `package.json` - Project dependencies and scripts
- `vite.config.js` - Vite configuration
- `.prettierrc`, `.prettierignore` - Prettier formatting config
- `babel.config.js` - Babel configuration
- `jest.config.js` - Jest configuration
- `tsconfig.json`, `jsconfig.json` - TypeScript/JavaScript config

## Getting Started

### Prerequisites

- [Node.js](https://nodejs.org/) (v16 or higher recommended)
- [npm](https://www.npmjs.com/) or [yarn](https://yarnpkg.com/)

### Install Dependencies

```bash
npm install
```

### Development Server

```bash
npm run client
```

This will start the Vite development server. Open your browser to the provided local URL.

### Build for Production

```bash
npm run build
```

The production-ready files will be output to the `dist/` directory.

### Run Tests

```bash
npm run test
```

Runs unit tests using Jest.

## Linting & Formatting

- **Lint:**
  ```bash
  npm run lint
  ```
- **Format:**
  ```bash
  npm run format
  ```

## Notes

- The client app is designed to be served by the FantasyCritic ASP.NET backend, but can be run independently for development.
- Environment variables and API endpoints may need to be configured for local development.

## License

See the root `LICENSE.md` file for license information.
