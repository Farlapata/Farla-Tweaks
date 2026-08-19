# Farla Tweaks commercialization plan

## Product promise

Farla is a Windows performance companion that analyzes the user's PC, explains compatible changes, applies only explicitly approved changes, and keeps rollback state locally.

## Sellable product boundary

### Core product
- Personalized onboarding wizard
- Hardware and Windows profile
- Compatibility-aware recommendation engine
- Explainable tweak review
- Transactional registry execution
- Persistent rollback snapshots
- History and rollback UI
- Read-only diagnostics
- Live system monitoring
- Game process detection

### Pro product candidates
- Fortnite session monitoring
- Session history and frametime analysis
- Adaptive Copilot investigations
- Advanced audited tweak catalog
- Benchmark comparisons
- Exportable diagnostics
- Automatic update channel

### Experimental / never ship by default
- Obfuscated binaries
- Unreviewed third-party utilities
- Broad process-killing scripts
- Blind registry packs
- Driver replacement automation without a verified source
- Claims of guaranteed FPS gains

## Licensing architecture

The commercial application should separate licensing from optimization logic.

```text
Checkout
  -> payment provider
  -> license service
  -> signed license response
  -> local activation cache
  -> Farla feature gate
```

The client should never contain a master licensing secret. Development builds may use a local developer license provider only.

## Release channels

- `dev`: active development
- `alpha`: internal testing
- `beta`: invited users and creators
- `stable`: paid public release

## Release gate

A build should not be promoted to stable until:

1. Windows x64 build succeeds.
2. Startup and onboarding work from a clean profile.
3. Every shipped tweak has a source, explanation, and rollback path.
4. Rejected/quarantined content is excluded from the shipped database.
5. Diagnostics are read-only.
6. Rollback has been tested on a clean Windows profile.
7. The release is versioned and reproducible.
8. The executable is Authenticode signed for public distribution.
9. A clean uninstall path exists.
10. The support and refund policy match the actual product behavior.
