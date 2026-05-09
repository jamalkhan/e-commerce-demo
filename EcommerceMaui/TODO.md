# EcommerceMaui — Parity TODO

Tracks features where `EcommerceMaui` is out of core feature parity with `EcommerceMvc` and `EcommerceSpa`.

When parity is intact, the **Open parity gaps** section is empty.

## Open parity gaps

### 2026-05-09: Account / authentication UI

- Status in this project: **missing**
- Implemented in: `EcommerceMvc`, `EcommerceSpa`
- Missing in: `EcommerceMaui`
- To reach parity:
  1. Implement deep linking so the app can be launched from a `https://sandbox.mvc.jamal.com/account/reset-password?token=...` (or custom-scheme) URL with the token preserved.
     - iOS: configure Associated Domains + `applinks:`
     - Android: intent filters with `<data android:scheme="..." />` + assetlinks.json
     - Mac Catalyst: `LSApplicationCategoryType` + URL scheme registration
     - Windows: package URI activation
  2. Add Account ViewModels + Pages: Register, Login, ForgotPassword, ResetPassword, OAuthCallback.
  3. Add `IAuthApiClient` (mirror of MVC's) and wire to `EcommerceApiClient`/bleak.Api.Rest.
  4. Persist session token in `SecureStorage` (MAUI's secure key/value store) instead of localStorage/cookie.
  5. Update `EcommerceApiClient` to attach `Authorization: Bearer {token}` when a session is present.
