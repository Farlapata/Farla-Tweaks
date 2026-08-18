# Farla Tweaks Architecture

## Layers

- **App**: WPF presentation layer.
- **Core**: product logic, models, compatibility, state and execution contracts.
- **Database**: structured tweak definitions and provenance.
- **Diagnostics**: Windows health and hardware/system discovery.
- **Games**: game-specific profiles and session integration.
- **Copilot**: monitoring and evidence-based intervention logic.

## Safety boundary

The Core execution layer must never execute arbitrary downloads or opaque payloads. Each supported operation type should have an explicit executor and rollback strategy.

## Tweak lifecycle

1. Load definition.
2. Validate schema.
3. Check OS/hardware/software dependencies.
4. Check conflicts.
5. Snapshot affected state.
6. Apply through a known executor.
7. Verify the resulting state.
8. Record the session.
9. Revert using the stored snapshot when requested.

## Runtime vs persistent changes

Persistent changes such as registry settings belong in the setup/optimization pipeline. Live Copilot behavior should use explicit runtime controls and only intervene when a measurable problem is detected.
