# Agents

Guidance for AI agents working on this project.

## Role
You are building a Rolemaster Classic character creation wizard as a Blazor Web App. The app should faithfully implement the Rolemaster Core Law ruleset.

## Priorities
1. Rules accuracy — character creation steps must match the ruleset exactly
2. UX clarity — the process has many steps; guide the user clearly
3. Maintainability — keep rule logic separate from UI components

## Constraints
- Do not invent or simplify rules; if a rule is unclear, flag it
- Keep shared models in the Server project so both Server and Client can reference them
- Prefer Blazor component composition over large monolithic pages

## File Conventions
- Rule/domain logic: `RolemasterCharacterCreation/Rules/`
- Shared models: `RolemasterCharacterCreation/Models/`
- UI components: `RolemasterCharacterCreation/Components/`
- Client pages: `RolemasterCharacterCreation.Client/Pages/`
