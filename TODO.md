# E-commerce Demo — Punch List & Scratchpad

Roadmap of features needed to make the three storefront apps (`EcommerceMaui`, `EcommerceMvc`, `EcommerceSpa`) commercially viable, plus things that make them competitive.

> This file is the **shared scratchpad / roadmap**. It is *not* the place to record parity gaps between the three apps — those go in each project's own `TODO.md` (`EcommerceMaui/TODO.md`, etc.). Items here are parity-tracked (should land in all three apps) unless explicitly tagged platform-specific.

Check items off as they ship. Promote items to actual work by carving a vertical slice and implementing across all three apps.

---

## Phase 0 — Backend foundation

Nothing else matters until this is in place; today there's an in-memory `ProductStore` and login is a `?uname=` query string.

- [x] Real product database (Postgres / SQL Server) — persistent products, categories, inventory, prices
- [ ] Real authentication — password hashing, sessions or JWTs, password reset, OAuth (Google / Apple / Microsoft)
- [ ] Persisted user accounts (id, email, profile)
- [ ] Environment-based config (dev / staging / prod) and secrets management

## Phase 1 — Catalog & discovery

- [ ] Categories / departments — products belong to categories; browse by category
- [ ] Filters — price range, category, brand, in-stock
- [ ] Sorting — price, newest, popularity, rating
- [ ] Pagination or infinite scroll
- [ ] Search upgrades — autocomplete/suggestions, typo tolerance, search analytics
- [ ] Product detail upgrades — multiple images, gallery, zoom, variants (size/color), specifications, stock status, related products
- [ ] Recently viewed strip on home / PDP

## Phase 2 — Cart & checkout

Biggest gap today; there's no concept of buying anything.

- [ ] Cart — add / remove / update quantity; persisted across sessions for logged-in users; merge anonymous cart on login
- [ ] Mini-cart in header (count badge, hover/tap preview)
- [ ] Coupon / promo codes
- [ ] Estimated tax & shipping in cart
- [ ] Checkout flow — shipping address → shipping method → payment → review → confirmation
- [ ] Guest checkout
- [ ] Address book (saved shipping / billing addresses)
- [ ] Payment integration — Stripe / Braintree; saved payment methods
- [ ] Order confirmation page + email
- [ ] Order persistence — order history, status, line items

## Phase 3 — Account & order management

- [ ] Account dashboard (profile, addresses, payment methods, preferences)
- [ ] Order history with detail view
- [ ] Order tracking / shipping status
- [ ] Returns / refunds workflow
- [ ] Email & marketing preferences
- [ ] Wishlist / save for later

## Phase 4 — Trust & social proof

- [ ] Product reviews & ratings (write, display, aggregate)
- [ ] Q&A on PDP
- [ ] Stock urgency indicators ("only 3 left")
- [ ] Trust badges (secure checkout, return policy)
- [ ] Real Privacy Policy, Terms of Service, Shipping Policy, Returns Policy pages

## Phase 5 — Personalization & marketing (Zeta sweet spot)

Where the three-stack demo earns its keep — show the same personalized experience across MVC / SPA / Maui.

- [ ] Behavioral tracking parity across all three apps *(today only MVC has Zync — parity gap if kept in scope)*
- [ ] Identity resolution — tie anonymous browsing to logged-in user across devices
- [ ] Personalized recommendations — homepage, PDP "you might also like," cart cross-sell
- [ ] Personalized search & merchandising
- [ ] Abandoned cart email / push
- [ ] Email signup / newsletter with preferences
- [ ] Loyalty / rewards — points, tiers, redemption
- [ ] Promotions engine — sale banners, time-boxed deals, segment-targeted offers
- [ ] A/B testing harness — feature-flag-driven experiments with conversion tracking

## Phase 6 — Operational maturity

- [ ] SEO — meta tags, OG tags, structured data (Product / Offer / Review), sitemap.xml, robots.txt (mostly MVC / SPA)
- [ ] Image optimization & CDN
- [ ] Caching (page, fragment, API)
- [ ] Performance budgets, Core Web Vitals monitoring
- [ ] Accessibility (WCAG 2.1 AA) audit pass
- [ ] Error monitoring (Sentry / App Insights)
- [ ] Structured logging + metrics + traces
- [ ] CI/CD pipeline with preview environments
- [ ] Feature flags

## Phase 7 — Compliance & admin

- [ ] GDPR / CCPA consent banner with granular cookie controls
- [ ] "Do not sell my data" workflow
- [ ] Admin / catalog management UI (CRUD products, categories, inventory, orders)
- [ ] Order fulfillment workflow (mark shipped, tracking number, status emails)
- [ ] Customer support — contact form, optional live chat

---

## Platform-specific opportunities (NOT parity-tracked)

Each app gets to flex on its strengths.

### Maui
- [ ] Apple Pay / Google Pay native
- [ ] Push notifications
- [ ] Biometric login (Face ID / Touch ID / Windows Hello)
- [ ] Barcode scan / camera lookup
- [ ] Offline catalog cache
- [ ] Native share sheet

### MVC
- [ ] Server-side rendering tuned for SEO
- [ ] Edge caching
- [ ] ASP.NET output caching
- [ ] Full-page SSR personalization

### SPA
- [ ] Client-side route prefetching
- [ ] Optimistic UI
- [ ] Service-worker offline
- [ ] Instant search-as-you-type

---

## Suggested build order

For a demo (not a real shop), pick a vertical slice that tells one story end-to-end:

1. **Phase 0** — non-negotiable foundation
2. **Cart + checkout MVP** from Phase 2 (DB-backed cart, Stripe test mode, order confirmation)
3. **Account + order history** from Phase 3
4. Jump to **Phase 5 personalization** — that's where the three-stack demo is uniquely interesting
