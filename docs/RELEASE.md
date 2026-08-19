# Farla release checklist

## Alpha / internal

- Windows x64 build succeeds.
- Standalone executable artifact exists.
- Installer definition exists.
- SHA-256 checksum is generated.
- First-run wizard works from an empty `%LOCALAPPDATA%\Farla` profile.
- Read-only analysis works without admin rights where Windows allows it.
- Every recommended tweak is audited and executable.
- Apply creates rollback state before changing values.
- Full rollback works.
- Diagnostics remain read-only.
- Game session data is local and user-controlled.
- Copilot observations do not automatically change the system.

## Public beta

- Add reproducible installer builds.
- Add crash/support log export.
- Add update channel and changelog display.
- Add remote licensing service.
- Add stable payment and refund flow.
- Test fresh install, upgrade, uninstall, and rollback on clean Windows machines.
- Run a larger audited tweak corpus through the same provenance pipeline.

## Stable commercial release

- Authenticode-sign the executable and installer with a publisher certificate.
- Sign release artifacts during CI using repository secrets or a dedicated signing service.
- Never ship the signing certificate or private key in the repository.
- Publish a public privacy policy and terms matching the actual product behavior.
- Publish a support process and refund policy.
- Make all paid feature gates server-verifiable without shipping a master secret in the client.
- Provide a clean uninstall that leaves user data only when explicitly requested.

## Recommended CI secrets for signing

```text
WINDOWS_SIGNING_PFX_BASE64
WINDOWS_SIGNING_PASSWORD
WINDOWS_TIMESTAMP_URL
```

Signing is intentionally not enabled in the current Alpha because there is no release certificate configured.
