# Farla Tweaks Architecture

## Product layers

```text
Farla Tweaks App
├── Presentation
│   ├── Dashboard
│   ├── Setup Wizard
│   ├── Diagnostics
│   └── Activity / Rollback
├── Application Services
│   ├── Recommendation Engine
│   ├── Compatibility Engine
│   ├── Optimization Engine
│   └── System Profile
├── Core
│   ├── Tweak Definitions
│   ├── Backups
│   ├── Results
│   └── Safety Policies
└── Windows Integration
    ├── Registry
    ├── Services / Processes
    ├── System Information
    └── Performance Counters
```

## Non-negotiable behavior

The optimization engine must not blindly execute a list of commands. It must resolve a proposed plan first, validate dependencies/conflicts, create a backup, apply changes transactionally where possible, verify the result, and record an auditable activity entry.

Rollback is a first-class operation, not an afterthought.

## Development order

1. Data model and safety primitives
2. System detection
3. Tweak loading and validation
4. Backup / rollback
5. Recommendation engine
6. Setup wizard
7. Dashboard
8. Diagnostics
9. Game integrations
10. Monitoring
11. Adaptive Copilot
12. Accounts and licensing
