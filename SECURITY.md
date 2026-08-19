# Security Policy

Farla Tweaks is a system-modification product. Security and provenance are therefore treated as product features, not cleanup work.

## Shipped code rules

- No obfuscated executable payloads.
- No downloaded scripts or binaries at runtime without an explicit, documented source and integrity check.
- No hidden persistence mechanisms.
- No credential collection.
- Registry changes must be represented as structured data and pass audit review before recommendation.
- Every executable registry tweak must support rollback.
- Third-party utilities remain quarantined until source and behavior are understood.

## Tweak provenance

Every shipped tweak should have an audit status, source name, source URL when applicable, risk level, and audit notes.

`Quarantined` and `Rejected` entries must never reach the recommendation engine.

## Reporting

Security issues should be reported privately to the project maintainer before public disclosure. Include the affected version, reproduction steps, and any relevant logs without including personal data.
