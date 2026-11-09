# AI Usage

The brief allows AI tools. This file records how AI was used for this submission.

## Tool used

- **opencode** (AI coding assistant) was used as an implementation and scaffolding assistant throughout the project.

## Division of work

The human set the direction and made the key decisions that shaped the solution:

- I gave the high‑level idea and constraints; AI implemented the detailed code.
- I defined the seat‑hold lifecycle, edge cases (expiry, duplicates, over‑booking) and the overall API contract.
- I chose the technology stack: .NET 10 Web API, PostgreSQL 18, React 19, Docker, pnpm and shadcn/ui.
- I specified the frontend behaviour: polling every 4 seconds, live countdown, persistent student identity, and friendly error messages.
- I decided on the concurrency approach (PostgreSQL row lock + partial unique index) and the need for a concurrency test.
- I added an admin verification password to protect class creation; the hash is seeded at startup and can only be changed with the current password.

AI implemented most of the code from that direction:

- Backend scaffolding, entities, DbContext, DTOs, validators, services, controllers, middleware, EF migrations and startup configuration.
- The xUnit test project and the concurrency test that fires 40 simultaneous applications.
- Frontend: Vite/TypeScript scaffold, API client, polling and countdown hooks, component structure, and shadcn/ui‑style component wrappers.
- CSS and visual styling was made entirely by AI (Tailwind classes, theme variables and layout).
- Documentation and Docker files were drafted with AI and reviewed by the human.

## AI mistakes and how I caught them

1. **Extra admin password header (API contract)** – AI initially added the `X-Admin-Password` header as a required field on `POST /classes`, but the official API spec does not include this header. The spec's own example request would fail with `400`. I caught this when reviewing the feedback, which stated the contract must be followed exactly because the API is tested automatically. I then made the header optional so a request without it still works.

2. **Wrong hold duration default** – AI set `HOLD_DURATION_SECONDS` to `60` in `appsettings.json`, `docker-compose.yml` and `.env.example`, but the spec says the default should be `600`. The `ASSUMPTIONS.md` correctly documented `600`, but the code configuration did not match. I caught this by comparing the documented default against the actual configuration files and fixing all three to `600`.

3. **Incorrect status code for hold creation** – AI had `return Ok(result)` for `POST /classes/{classId}/holds`, returning status `200`, but the spec requires `201 Created`. I caught this during the deliverables review because the tests call services directly over HTTP and would not have caught a mismatch between the spec and the actual HTTP status code.

4. **Admin password documented as required but not in spec** – AI documented the `X-Admin-Password` header as a required admin protection in `ASSUMPTIONS.md` and the README, but the API contract does not require it. I caught this by cross-referencing the API spec against the documentation and making the header optional while recording the change in `ASSUMPTIONS.md`.

So in short: the human supplied the idea, requirements, contracts and architecture decisions; AI turned function/interface/entity descriptions into working implementations.