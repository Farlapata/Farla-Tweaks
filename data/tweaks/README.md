# Farla Tweak Database

This directory is the canonical source for Farla's optimization definitions.

A tweak is not just a registry command. Each definition should describe:

- what it changes
- why it changes it
- risk level
- restart requirements
- dependencies
- conflicts
- exact before/after state
- rollback information
- source/provenance

Raw `.reg` files and scripts should not be copied into the product blindly. They must be audited, deduplicated, normalized and represented as structured definitions first.

## Safety rule

A definition without enough information to safely explain and reverse its change does not belong in the production database.
