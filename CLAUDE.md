# Rolemaster Character Creation — Claude Instructions

## Session Start
At the start of every session, read the following files before doing anything else:
- `docs/project/memory.md` — project decisions and known issues
- `docs/project/agents.md` — role, priorities, constraints, and file conventions
- `docs/project/context.md` — Rolemaster ruleset background
- `docs/game-data/skills.md` — skill categories and profession DP costs
- `.claude/agents/Creation wizard.md` — specialist agent definition for CharacterWizard pages
- `.claude/agents/Character sheet.md` — specialist agent definition for CharacterSheet page

## Rules unclear

| Topic | Source file |
|-------|------------|
| Stats, skills, professions, cultures, races, combat, level advancement | `docs/rules/Rolemaster_Core_Law_(RMU).md` |
| Spells, spell lists, spell law mechanics (Channeling/Essence/Mentalism) | `docs/rules/Rolemaster_Spell_Law_(RMU).md` |
| Equipment, weapons, armor, starting wealth, magic items, alchemy | `docs/rules/Rolemaster_Treasure_Law_(RMU).md` |
| Creatures, creature talents, encounter design | `docs/rules/Rolemaster_Creature_Law_I.md` |

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).
