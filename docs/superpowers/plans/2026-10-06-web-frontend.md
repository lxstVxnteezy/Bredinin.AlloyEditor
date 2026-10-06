# Web Frontend Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Веб-приложение (каталог марок сплавов + админка пользователей) для AlloyEditor, работающее через Gateway.

**Architecture:** SPA в `Bredinin.AlloyEditor.Web/`. Один axios-клиент с интерцептором обновления токена; серверные данные через TanStack Query; авторизация через React Context. Продакшен: контейнер nginx со статикой, `/api/` проксируется на существующий nginx/Gateway.

**Tech Stack:** React 18, TypeScript, Vite, MUI, TanStack Query, React Router 6, axios, Vitest, React Testing Library, MSW, ESLint.

**Spec:** `docs/superpowers/specs/2026-10-06-web-frontend-design.md`

## Global Constraints

- Бэкенд не изменяется; в существующих `nginx.conf` и `docker-compose.yml` только добавляется (`location /`, сервис `web`).
- Все запросы относительными путями `/api/gateway/...`.
- Интерфейс на русском; без i18n-библиотеки, тёмной темы, Redux, SSR.
- Access-токен только в памяти, refresh-токен в `localStorage` (ключ `alloy.refreshToken`).
- Роль администратора проверяется по JWT-клейму `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` (строка или массив); имя роли взять из сидов миграции `2025_08_25_2116_InsertValuesInTables`.
- Недоступные функции (редактирование марки, сброс пароля) отключены с подсказкой.
- Типы DTO пишутся вручную по контрактам C#; поля camelCase.
- Готовность = `npm run lint` и `npm test` и `npm run build` проходят.

## Review Focus

- Параллельные 401: два запроса одновременно дают ровно один вызов `refresh-token`, оба повторяются.
- Refresh-токен недействителен при старте: приложение показывает логин, а не вечную загрузку.
- `/api/.../login` вернул 500 (текущее поведение бэкенда): форма показывает «Неверный логин или пароль».
- JWT с одной ролью строкой и с массивом ролей: роль админа определяется в обоих случаях.
- Состав сплава: дубликат элемента, отсутствие базового элемента, сумма больше 100%, смешение точного значения с диапазоном: форма не отправляет запрос.
- Пустой список марок/пользователей и ошибка загрузки: понятное состояние, не белый экран.

---

### Task 1: Каркас проекта

**Files:**
- Create: `Bredinin.AlloyEditor.Web/` (`package.json`, `tsconfig.json`, `vite.config.ts`, `index.html`, `eslint.config.js`, `.env.example`, `src/main.tsx`, `src/App.tsx`, `src/test/setup.ts`, `src/test/server.ts`)
- Test: `src/App.test.tsx`

**Interfaces:**
- Produces: скрипты `dev`, `build`, `lint` (`eslint . && tsc --noEmit`), `test` (`vitest run`); `src/test/server.ts` экспортирует `server` (MSW `setupServer`) для всех тестов; `vite.config.ts` проксирует `/api` на `process.env.VITE_PROXY_TARGET ?? "http://localhost"`.

- [ ] **Step 1:** Создать проект Vite React-TS и поставить зависимости из Tech Stack; Vitest с `jsdom`, `setupFiles: src/test/setup.ts` (jest-dom, старт/сброс/остановка `server`).
- [ ] **Step 2:** Написать тест `App.test.tsx`: `render(<App />)` показывает текст «AlloyEditor». Запустить `npm test`, убедиться, что падает.
- [ ] **Step 3:** Реализовать `App` минимально (заголовок) с `ThemeProvider` из MUI; `.env.example` с `VITE_PROXY_TARGET`.
- [ ] **Step 4:** Запустить `npm run lint && npm test && npm run build`; все проходят.
- [ ] **Step 5:** Коммит `feat(web): scaffold Vite React app`. В `.gitignore` убедиться, что `node_modules` и `dist` игнорируются.

### Task 2: API-типы, HTTP-клиент, нормализация ошибок

**Files:**
- Create: `src/api/types.ts`, `src/api/http.ts`, `src/api/errors.ts`, `src/api/index.ts` (функции запросов)
- Test: `src/api/errors.test.ts`

**Interfaces:**
- Produces:
  - `types.ts`: `AuthResponse {accessToken, refreshToken}`, `AlloyGrade {id,name,description?,alloySystemId,chemicalCompositions: Composition[], heatTreatments: HeatTreatment[], mechanicalProperties: MechanicalProperty[]}`, `Composition {id?,chemicalElementId,minValue?,maxValue?,exactValue?}`, `CreateAlloyGradeRequest {name,description?,alloySystemId,chemicalCompositions: Omit<Composition,"id">[], heatTreatments?: [], defaultMechanicalProperties?: []}`, `AlloySystem {id,baseElementId,name,description?}`, `ChemicalElement {id,name,symbol,description?,isBaseForAlloySystem,atomicNumber,atomicWeight,group,period,density}`, `InfoAlloyGradeByMain {id,name,description?,chemicalCompositions}`; типы пользователей по DTO `Identity.Service.Contracts` (прочитать `CreateUserCommand`, `EditUserCommand`, `SearchUserQuery`, `UpdateUserResponse`).
  - `errors.ts`: `class ApiError extends Error {status:number; constructor(status:number,message:string)}`, `toApiError(e: unknown): ApiError`.
  - `http.ts`: `http` (axios instance, `baseURL: ""`), `setAuthHandlers(h: {getAccessToken(): string|null; refresh(): Promise<string>; onAuthFailure(): void})`.
  - `index.ts`: `login(l,p)`, `refreshToken(t)`, `revokeAccess(userId)`, `getGrades()`, `createGrade(req): Promise<string>`, `deleteGrade(id)`, `getGradesForMainElement(id)`, `getSystems()`, `getElements()`, `getUsers()`, `createUser`, `updateUser`, `deleteUser(id)`; пути строго из Gateway-контроллеров (`/api/gateway/...`).

- [ ] **Step 1:** Тест `errors.test.ts`: `toApiError` для axios-ответа `{status:400,data:"Имя занято"}` даёт `status 400`, `message "Имя занято"`; для 500 с телом `{message:"x"}` берёт `x`; для сетевой ошибки `status 0`, сообщение «Нет связи с сервером»; 429 даёт «Слишком много запросов, повторите позже»; 403 даёт «Недостаточно прав». Запустить, увидеть падение.
- [ ] **Step 2:** Реализовать `errors.ts`, `types.ts`; запустить тест, пройти.
- [ ] **Step 3:** Реализовать `http.ts` (пока без refresh-логики, только Bearer и `toApiError` в response-интерцепторе) и `index.ts`.
- [ ] **Step 4:** `npm run lint && npm test`; коммит `feat(web): api client and types`.

### Task 3: Авторизация и обновление токена

**Files:**
- Create: `src/auth/jwt.ts`, `src/auth/tokenStorage.ts`, `src/auth/AuthProvider.tsx`, `src/auth/useAuth.ts`
- Modify: `src/api/http.ts` (refresh-интерцептор)
- Test: `src/auth/jwt.test.ts`, `src/api/refresh.test.ts`

**Interfaces:**
- Consumes: `login`, `refreshToken`, `setAuthHandlers` из Task 2.
- Produces:
  - `jwt.ts`: `parseJwt(token: string): {userId: string|null; roles: string[]}` (роли из клейма role: строка или массив; id из `sub` или `userId`).
  - `tokenStorage.ts`: `getRefreshToken(): string|null`, `setRefreshToken(t: string)`, `clearRefreshToken()`.
  - `useAuth(): {status: "loading"|"authenticated"|"anonymous"; roles: string[]; userId: string|null; login(l: string, p: string): Promise<void>; logout(): void; isAdmin: boolean}`.
  - `AuthProvider` при монтировании пытается обменять сохранённый refresh-токен; при ошибке `status = "anonymous"`.

- [ ] **Step 1:** `jwt.test.ts`: токен с `role: "Admin"` даёт `["Admin"]`; с массивом даёт массив; без клейма даёт `[]`. Увидеть падение, реализовать `parseJwt` (base64url-декод payload), пройти.
- [ ] **Step 2:** `refresh.test.ts` (MSW): (a) на 401 один вызов `/api/gateway/identity/refresh-token`, запрос повторён с новым токеном и успешен; (b) два параллельных запроса с 401 вызывают refresh ровно один раз; (c) если refresh вернул 401, вызывается `onAuthFailure` и промис отклоняется; (d) повторный 401 после успешного refresh не зацикливается. Увидеть падение.
- [ ] **Step 3:** Реализовать в `http.ts` response-интерцептор: общий `refreshPromise`, флаг `_retried` на конфиге запроса; запросы login/refresh-token сами не обновляются.
- [ ] **Step 4:** Реализовать `tokenStorage`, `AuthProvider`, `useAuth`; access-токен только в состоянии. `isAdmin` по имени роли из сидов миграции.
- [ ] **Step 5:** `npm run lint && npm test`; коммит `feat(web): auth with token refresh`.

### Task 4: Роутинг, каркас страниц, защита маршрутов

**Files:**
- Create: `src/router.tsx`, `src/components/Layout.tsx`, `src/components/ErrorBoundary.tsx`, `src/components/ConfirmDialog.tsx`, `src/components/ErrorAlert.tsx`, `src/auth/RequireAuth.tsx`, `src/auth/RequireRole.tsx`, `src/theme.ts`
- Modify: `src/main.tsx`, `src/App.tsx` (провайдеры: Theme, QueryClient с `retry` только для 5xx и один раз, Auth, Router, Snackbar)
- Test: `src/auth/guards.test.tsx`

**Interfaces:**
- Consumes: `useAuth` из Task 3.
- Produces: `RequireAuth` (при `anonymous` редирект на `/login`, при `loading` индикатор), `RequireRole` (props `role?: "admin"`; не админ получает «Недостаточно прав»); `ConfirmDialog` props `{open, title, text, onConfirm(), onCancel()}`; `ErrorAlert` props `{error: unknown}`; маршруты `/login`, `/grades`, `/systems`, `/elements`, `/users`; `/` редиректит на `/grades`.

- [ ] **Step 1:** `guards.test.tsx`: анонимный пользователь на `/grades` попадает на `/login`; обычный пользователь на `/users` видит «Недостаточно прав»; админ видит страницу; в меню пункта «Пользователи» нет у обычного пользователя. Увидеть падение.
- [ ] **Step 2:** Реализовать компоненты и маршруты (экраны пока заглушки); пройти тесты.
- [ ] **Step 3:** `npm run lint && npm test`; коммит `feat(web): routing, layout and guards`.

### Task 5: Экран входа

**Files:**
- Create: `src/features/login/LoginPage.tsx`
- Test: `src/features/login/LoginPage.test.tsx`

**Interfaces:**
- Consumes: `useAuth().login`.

- [ ] **Step 1:** Тесты (MSW): успешный вход переводит на `/grades`; ответ 401 показывает «Неверный логин или пароль»; ответ 500 на login показывает то же сообщение; пустые поля блокируют отправку; во время запроса кнопка отключена. Увидеть падение.
- [ ] **Step 2:** Реализовать форму (MUI), пройти тесты.
- [ ] **Step 3:** `npm run lint && npm test`; коммит `feat(web): login page`.

### Task 6: Системы сплавов и справочник элементов

**Files:**
- Create: `src/features/alloy-systems/SystemsPage.tsx`, `src/features/elements/ElementsPage.tsx`, `src/api/queries.ts` (хуки `useGrades`, `useSystems`, `useElements`, `useGradesForMainElement(id)`)
- Test: `src/features/elements/ElementsPage.test.tsx`, `src/features/alloy-systems/SystemsPage.test.tsx`

**Interfaces:**
- Consumes: `getSystems`, `getElements`, `getGradesForMainElement` из Task 2; `ErrorAlert`.
- Produces: хуки TanStack Query из `queries.ts` (ключи `["grades"]`, `["systems"]`, `["elements"]`), используемые в Task 7–8.

- [ ] **Step 1:** Тесты (MSW): таблица элементов показывает символ и название; поиск по тексту фильтрует строки; для `isBaseForAlloySystem` виден бейдж; пустой ответ показывает «Нет данных»; ошибка показывает `ErrorAlert`. Для систем: клик по строке запрашивает `for-main-element/{baseElementId}` и показывает марки. Увидеть падение.
- [ ] **Step 2:** Реализовать страницы и хуки; пройти тесты.
- [ ] **Step 3:** `npm run lint && npm test`; коммит `feat(web): alloy systems and elements pages`.

### Task 7: Каталог марок: список, карточка, удаление

**Files:**
- Create: `src/features/alloy-grades/GradesPage.tsx`, `src/features/alloy-grades/GradeDetails.tsx`
- Test: `src/features/alloy-grades/GradesPage.test.tsx`

**Interfaces:**
- Consumes: `useGrades`, `useSystems`, `useElements` (Task 6), `deleteGrade`, `ConfirmDialog`.
- Produces: `GradesPage` с кнопкой «Создать» (открывает форму из Task 8) и отключённой кнопкой «Редактировать» с подсказкой «Недоступно: нет эндпоинта в Gateway».

- [ ] **Step 1:** Тесты (MSW): таблица показывает название и название системы (сопоставление по `alloySystemId`); поиск по названию и фильтр по системе работают; клик открывает карточку с таблицей состава (символ элемента, мин, макс, точное); удаление требует подтверждения, после него список обновляется; пустой список показывает «Марок пока нет»; кнопка «Редактировать» disabled. Увидеть падение.
- [ ] **Step 2:** Реализовать страницу и карточку; блоки термообработки и механических свойств в карточке показываются только если массивы непустые.
- [ ] **Step 3:** `npm run lint && npm test`; коммит `feat(web): alloy grades catalog`.

### Task 8: Создание марки и валидация состава

**Files:**
- Create: `src/features/alloy-grades/compositionValidation.ts`, `src/features/alloy-grades/CreateGradeDialog.tsx`
- Modify: `src/features/alloy-grades/GradesPage.tsx` (подключить диалог)
- Test: `src/features/alloy-grades/compositionValidation.test.ts`, `src/features/alloy-grades/CreateGradeDialog.test.tsx`

**Interfaces:**
- Consumes: `createGrade`, `useSystems`, `useElements`, тип `Composition`.
- Produces: `validateComposition(items: Composition[], baseElementId: string): string[]` (список сообщений на русском; пустой список означает валидно).

- [ ] **Step 1:** `compositionValidation.test.ts`: пустой список даёт ошибку; нет базового элемента даёт ошибку; дубликат `chemicalElementId` даёт ошибку; `exactValue` вместе с `minValue` или `maxValue` даёт ошибку; только `minValue` или только `maxValue` даёт ошибку; `min > max` даёт ошибку; сумма (точные значения и максимумы) больше 100 даёт ошибку; корректный состав (Al 95 точно + Si мин 1 макс 5) даёт `[]`. Увидеть падение, реализовать, пройти.
- [ ] **Step 2:** `CreateGradeDialog.test.tsx` (MSW): некорректный состав блокирует отправку и показывает ошибки, POST не вызван; корректная форма отправляет `POST /api/gateway/alloy-grade` с `heatTreatments: []`, закрывается и обновляет список; ответ 400 с текстом показывается в диалоге. Увидеть падение.
- [ ] **Step 3:** Реализовать диалог: название (обязательно, `trim`), описание, выбор системы, динамические строки состава (элемент, режим «точное/диапазон»); базовый элемент выбранной системы берётся из `useSystems`.
- [ ] **Step 4:** `npm run lint && npm test`; коммит `feat(web): create alloy grade with composition validation`.

### Task 9: Админка пользователей

**Files:**
- Create: `src/features/users/UsersPage.tsx`, `src/features/users/UserDialog.tsx`
- Test: `src/features/users/UsersPage.test.tsx`

**Interfaces:**
- Consumes: `getUsers`, `createUser`, `updateUser`, `deleteUser`, `revokeAccess` (Task 2), `ConfirmDialog`, `RequireRole` (маршрут уже закрыт в Task 4).

- [ ] **Step 1:** Тесты (MSW): таблица показывает пользователей; создание и изменение отправляют соответствующие запросы и обновляют таблицу; удаление и «Отозвать доступ» требуют подтверждения; кнопка «Сбросить пароль» disabled с подсказкой; пустой список и ошибка показывают состояния. Увидеть падение.
- [ ] **Step 2:** Реализовать страницу и диалог по полям DTO из Task 2; пройти тесты.
- [ ] **Step 3:** `npm run lint && npm test`; коммит `feat(web): users admin page`.

### Task 10: Контейнер и nginx

**Files:**
- Create: `Bredinin.AlloyEditor.Web/Dockerfile`, `Bredinin.AlloyEditor.Web/nginx-web.conf`, `Bredinin.AlloyEditor.Web/.dockerignore`
- Modify: `docker-compose.yml` (сервис `web`), `nginx.conf` (`location /` на сервис `web`), `README.md` (раздел «Веб-интерфейс»: запуск dev и prod)

**Interfaces:**
- Consumes: готовая сборка `npm run build` (`dist/`).

- [ ] **Step 1:** Написать `Dockerfile`: стадия `node` (`npm ci && npm run build`), стадия `nginx` с копированием `dist/` и `nginx-web.conf` (SPA-фолбэк на `index.html`).
- [ ] **Step 2:** Прочитать текущие `docker-compose.yml` и `nginx.conf`; добавить сервис `web` в ту же сеть и `location /` с `proxy_pass` на него, не меняя существующие правила.
- [ ] **Step 3:** Проверка: `docker build Bredinin.AlloyEditor.Web` проходит; `docker compose config` валиден; при доступном Docker поднять стек и открыть `http://localhost/` (логин и каталог открываются). Если Docker недоступен, сообщить, что проверено только `docker build`/`config`.
- [ ] **Step 4:** Коммит `feat(web): docker image and nginx routing`.
