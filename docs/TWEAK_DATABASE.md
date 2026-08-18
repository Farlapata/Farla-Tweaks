# Tweak Database

The database is an asset, not the user interface.

Each tweak should include:

- stable ID
- consistent name
- purpose and explanation
- category and tags
- risk level
- evidence status
- Windows compatibility
- hardware compatibility
- dependencies
- conflicts
- restart requirement
- source and credits
- exact changes
- rollback information

## Deduplication

Exact duplicate operations should be collapsed. Meaningful alternatives remain separate. Compound tweaks remain separate from their smaller constituent tweaks and should explicitly reference what they contain.

## Source hygiene

Static registry operations can be normalized into readable `.reg` definitions. A script that still performs runtime work remains a script. Obfuscated, unexplained or unsafe source code belongs in quarantine and is not part of the executable production database.
