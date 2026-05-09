# EcommerceMvc — Parity TODO

Tracks features where `EcommerceMvc` is out of core feature parity with `EcommerceMaui` and `EcommerceSpa`.

When parity is intact, the **Open parity gaps** section is empty.

## Open parity gaps

### 2026-05-09: Account / authentication UI (parity note)

- Status in this project: **implemented**
- Implemented in: `EcommerceMvc`, `EcommerceSpa`
- Missing in: `EcommerceMaui`
- Note: MAUI lacks the Account UI because it requires deep linking work. MVC's implementation is complete; this entry exists only to flag the cross-project gap until MAUI catches up. See `EcommerceMaui/TODO.md` for the work required there.
