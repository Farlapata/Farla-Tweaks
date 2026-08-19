# Farla Tweaks

Farla Tweaks is a Windows performance companion built around **explainable, reversible optimization**.

Farla is not designed as a giant button panel that blindly applies a stack of registry values. It profiles the PC, learns the user's dependencies, filters incompatible or unaudited changes, explains recommendations, and keeps rollback state for changes it actually applies.

## Current product state

`0.2.0-alpha.1` commercialization candidate.

The current build contains:

- First-run setup wizard with persistent user preferences
- Windows hardware and system profiling
- Compatibility-aware audited tweak recommendations
- Explicit review before registry changes
- Transactional registry apply and rollback
- History with per-change and full rollback
- Read-only Windows health diagnostics
- Live CPU, memory, and optional NVIDIA GPU telemetry
- Rule-based Copilot observations
- Fortnite process detection and local session history
- Settings and local data controls
- Versioned Windows x64 build and installer pipeline
- SHA-256 executable checksum generation

## Product principles

- Never blindly apply every tweak.
- Every shipped system change must be explainable and reversible.
- Dependencies and conflicts are first-class data.
- Unaudited, quarantined, or rejected content never reaches the recommendation engine.
- Third-party utilities do not become part of the product just because they were found in an old tweak pack.
- No runtime downloading of hidden scripts or binaries.
- Performance claims must be measurable. Farla does not promise a guaranteed FPS increase.
- Developers should be able to read the tweak database and understand why every entry exists.

## Release channels

- `dev` for active development
- `alpha` for internal testing
- `beta` for invited testers and creators
- `stable` for paid public distribution

## Build

The project targets Windows x64 and is built on a Windows GitHub Actions runner. The release workflow produces a self-contained single-file EXE, SHA-256 checksum, and Inno Setup installer artifact.

## Commercial boundary

The client-side architecture already separates optimization logic from licensing. Development builds use a local development license provider. The eventual stable release will use a remote activation service and signed distribution artifacts.

See:

- `docs/ARCHITECTURE.md`
- `docs/TWEAK_DATABASE.md`
- `docs/COMMERCIALIZATION.md`
- `SECURITY.md`
- `installer/FarlaTweaks.iss`
