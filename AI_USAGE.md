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

So in short: the human supplied the idea, requirements, contracts and architecture decisions; AI turned function/interface/entity descriptions into working implementations.