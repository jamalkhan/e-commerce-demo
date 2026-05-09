import { useEffect, useMemo, useState } from "react";

const apiBaseUrl = __API_BASE_URL__;

const SESSION_STORAGE_KEY = "ecommerce_session";

function loadSession() {
  try {
    const raw = window.localStorage.getItem(SESSION_STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

function saveSession(session) {
  window.localStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(session));
  window.dispatchEvent(new Event("ecommerce-session-changed"));
}

function clearSession() {
  window.localStorage.removeItem(SESSION_STORAGE_KEY);
  window.dispatchEvent(new Event("ecommerce-session-changed"));
}

function useSession() {
  const [session, setSession] = useState(loadSession());
  useEffect(() => {
    const handle = () => setSession(loadSession());
    window.addEventListener("ecommerce-session-changed", handle);
    window.addEventListener("storage", handle);
    return () => {
      window.removeEventListener("ecommerce-session-changed", handle);
      window.removeEventListener("storage", handle);
    };
  }, []);
  return session;
}

function normalizePath(pathname) {
  return pathname.replace(/\/+$/, "") || "/";
}

function parseLocation() {
  return {
    pathname: normalizePath(window.location.pathname),
    searchParams: new URLSearchParams(window.location.search)
  };
}

async function apiRequest(path, options = {}) {
  const session = loadSession();
  const headers = {
    "Content-Type": "application/json",
    ...(options.headers ?? {})
  };
  if (session?.token && !headers.Authorization) {
    headers.Authorization = `Bearer ${session.token}`;
  }

  const response = await fetch(`${apiBaseUrl}${path}`, {
    headers,
    ...options
  });

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? await response.json()
    : null;

  if (!response.ok) {
    const message = body?.message ?? `Request failed with status ${response.status}.`;
    throw new Error(message);
  }

  return body;
}

function formatCurrency(value) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD"
  }).format(value);
}

function navigate(to) {
  window.history.pushState({}, "", to);
  window.dispatchEvent(new PopStateEvent("popstate"));
}

function Link({ href, className, children }) {
  return (
    <a
      href={href}
      className={className ?? ""}
      onClick={(event) => {
        if (
          event.defaultPrevented ||
          event.button !== 0 ||
          event.metaKey ||
          event.ctrlKey ||
          event.shiftKey ||
          event.altKey
        ) {
          return;
        }

        event.preventDefault();
        navigate(href);
      }}
    >
      {children}
    </a>
  );
}

function AppShell({ children }) {
  const session = useSession();

  const handleLogout = async (event) => {
    event.preventDefault();
    try {
      await apiRequest("/api/auth/logout", { method: "POST" });
    } catch {
      // best-effort
    }
    clearSession();
    navigate("/");
  };

  return (
    <div className="page-shell">
      <header className="site-header">
        <div className="brand-row">
          <Link href="/" className="brand-mark">EcommerceSpa</Link>
          <nav className="main-nav">
            <Link href="/">Home</Link>
            <Link href="/products">Products</Link>
            <Link href="/privacy">Privacy</Link>
          </nav>
          <nav className="auth-nav">
            {session ? (
              <>
                <span>{session.user?.name}</span>
                <a href="#" onClick={handleLogout}>Log out</a>
              </>
            ) : (
              <>
                <Link href="/login">Log in</Link>
                <Link href="/register">Sign up</Link>
              </>
            )}
          </nav>
        </div>
      </header>
      <main className="main-content">{children}</main>
    </div>
  );
}

function StatusCard({ title, message, actionHref, actionLabel }) {
  return (
    <section className="status-card">
      <h2>{title}</h2>
      <p>{message}</p>
      {actionHref && actionLabel ? (
        <Link href={actionHref} className="button secondary">
          {actionLabel}
        </Link>
      ) : null}
    </section>
  );
}

function ProductList({ products }) {
  return (
    <div className="product-grid">
      {products.map((product) => (
        <article key={product.id} className="product-card">
          <p className="eyebrow">Featured Product</p>
          <h3>{product.name}</h3>
          <p>{product.description}</p>
          <div className="product-card-footer">
            <strong>{formatCurrency(product.price)}</strong>
            <Link href={`/products/${product.id}`} className="button secondary">
              View Details
            </Link>
          </div>
        </article>
      ))}
    </div>
  );
}

function usePageTitle(title) {
  useEffect(() => {
    document.title = `${title} | EcommerceSpa`;
  }, [title]);
}

function HomePage() {
  usePageTitle("Home");

  const [query, setQuery] = useState("");

  return (
    <section className="hero">
      <div>
        <p className="eyebrow">Single-page storefront</p>
        <h1>Browse products without full page reloads.</h1>
        <p className="lede">
          This React client keeps navigation in-page while talking to the new
          EcommerceApi backend.
        </p>
      </div>
      <div className="panel-grid">
        <section className="panel">
          <h2>Sign in</h2>
          <p>Manage your account and orders.</p>
          <Link href="/login" className="button">Log in</Link>
          <Link href="/register" className="button secondary">Create account</Link>
        </section>
        <section className="panel">
          <h2>Search Products</h2>
          <form
            onSubmit={(event) => {
              event.preventDefault();
              navigate(`/search?q=${encodeURIComponent(query)}`);
            }}
          >
            <label htmlFor="search">Product name</label>
            <input
              id="search"
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              placeholder="Enter product name"
            />
            <button className="button" type="submit">Search</button>
          </form>
        </section>
      </div>
    </section>
  );
}

function PrivacyPage() {
  usePageTitle("Privacy");

  return (
    <section className="panel prose">
      <p className="eyebrow">Privacy Policy</p>
      <h1>Privacy Policy</h1>
      <p>Use this page to detail your site's privacy policy.</p>
    </section>
  );
}

function ProductsPage() {
  usePageTitle("Products");

  const [state, setState] = useState({
    loading: true,
    error: "",
    products: []
  });

  useEffect(() => {
    let active = true;

    apiRequest("/api/products")
      .then((products) => {
        if (active) {
          setState({ loading: false, error: "", products });
        }
      })
      .catch((error) => {
        if (active) {
          setState({ loading: false, error: error.message, products: [] });
        }
      });

    return () => {
      active = false;
    };
  }, []);

  if (state.loading) {
    return <StatusCard title="Loading products" message="Fetching the catalog from EcommerceApi." />;
  }

  if (state.error) {
    return (
      <StatusCard
        title="Catalog unavailable"
        message={state.error}
        actionHref="/"
        actionLabel="Back Home"
      />
    );
  }

  return (
    <section className="panel">
      <div className="section-heading">
        <div>
          <p className="eyebrow">Catalog</p>
          <h1>Products</h1>
        </div>
      </div>
      <ProductList products={state.products} />
    </section>
  );
}

function ProductDetailsPage({ productId }) {
  usePageTitle("Product Details");

  const [state, setState] = useState({
    loading: true,
    error: "",
    product: null
  });

  useEffect(() => {
    let active = true;

    apiRequest(`/api/products/${productId}`)
      .then((product) => {
        if (active) {
          setState({ loading: false, error: "", product });
        }
      })
      .catch((error) => {
        if (active) {
          setState({ loading: false, error: error.message, product: null });
        }
      });

    return () => {
      active = false;
    };
  }, [productId]);

  if (state.loading) {
    return <StatusCard title="Loading product" message="Fetching product details from EcommerceApi." />;
  }

  if (state.error || !state.product) {
    return (
      <StatusCard
        title="Product not found"
        message={state.error || "We couldn't find that product."}
        actionHref="/products"
        actionLabel="Back to Products"
      />
    );
  }

  const { product } = state;

  return (
    <section className="panel prose">
      <p className="eyebrow">Product Details</p>
      <h1>{product.name}</h1>
      <p>{product.description}</p>
      <p><strong>Price:</strong> {formatCurrency(product.price)}</p>
      <Link href="/products" className="button secondary">Back to Products</Link>
    </section>
  );
}

function SearchPage({ searchParams }) {
  const query = searchParams.get("q") ?? "";

  usePageTitle("Search");

  const [state, setState] = useState({
    loading: true,
    error: "",
    payload: null
  });

  useEffect(() => {
    let active = true;

    apiRequest(`/api/search?q=${encodeURIComponent(query)}`)
      .then((payload) => {
        if (active) {
          setState({ loading: false, error: "", payload });
        }
      })
      .catch((error) => {
        if (active) {
          setState({ loading: false, error: error.message, payload: null });
        }
      });

    return () => {
      active = false;
    };
  }, [query]);

  if (state.loading) {
    return <StatusCard title="Searching" message="Looking for matching products." />;
  }

  if (state.error || !state.payload) {
    return (
      <StatusCard
        title="Search unavailable"
        message={state.error || "Something went wrong while searching."}
        actionHref="/"
        actionLabel="Back Home"
      />
    );
  }

  return (
    <section className="panel">
      <div className="section-heading">
        <div>
          <p className="eyebrow">Search</p>
          <h1>Results</h1>
        </div>
        <Link href="/" className="button secondary">New Search</Link>
      </div>
      {state.payload.message ? (
        <p className="info-banner">{state.payload.message}</p>
      ) : null}
      {state.payload.results.length > 0 ? (
        <ProductList products={state.payload.results} />
      ) : null}
    </section>
  );
}

function LoginPage() {
  usePageTitle("Login");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setSubmitting(true);
    try {
      const session = await apiRequest("/api/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password })
      });
      saveSession(session);
      navigate("/");
    } catch (err) {
      setError(err.message || "Could not log in.");
    } finally {
      setSubmitting(false);
    }
  };

  const oauthReturn = `${window.location.origin}/oauth-callback`;

  return (
    <section className="panel prose">
      <p className="eyebrow">Account</p>
      <h1>Log in</h1>
      {error ? <p className="info-banner">{error}</p> : null}
      <form onSubmit={handleSubmit}>
        <label htmlFor="email">Email</label>
        <input id="email" type="email" autoComplete="email" required
          value={email} onChange={(e) => setEmail(e.target.value)} />
        <label htmlFor="password">Password</label>
        <input id="password" type="password" autoComplete="current-password" required
          value={password} onChange={(e) => setPassword(e.target.value)} />
        <button className="button" type="submit" disabled={submitting}>
          {submitting ? "Logging in..." : "Log in"}
        </button>
      </form>
      <p>
        <Link href="/forgot-password">Forgot your password?</Link>
        {" · "}
        <Link href="/register">Create an account</Link>
      </p>
      <hr />
      <p>Or sign in with:</p>
      <a className="button secondary"
         href={`${apiBaseUrl}/api/auth/oauth/google?returnUrl=${encodeURIComponent(oauthReturn)}`}>Google</a>
      <a className="button secondary"
         href={`${apiBaseUrl}/api/auth/oauth/facebook?returnUrl=${encodeURIComponent(oauthReturn)}`}>Facebook</a>
    </section>
  );
}

function RegisterPage() {
  usePageTitle("Sign up");
  const [email, setEmail] = useState("");
  const [name, setName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setSubmitting(true);
    try {
      const session = await apiRequest("/api/auth/register", {
        method: "POST",
        body: JSON.stringify({ email, name, password })
      });
      saveSession(session);
      navigate("/");
    } catch (err) {
      setError(err.message || "Could not create account.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <section className="panel prose">
      <p className="eyebrow">Account</p>
      <h1>Create an account</h1>
      {error ? <p className="info-banner">{error}</p> : null}
      <form onSubmit={handleSubmit}>
        <label htmlFor="email">Email</label>
        <input id="email" type="email" autoComplete="email" required
          value={email} onChange={(e) => setEmail(e.target.value)} />
        <label htmlFor="name">Name</label>
        <input id="name" type="text" autoComplete="name" required
          value={name} onChange={(e) => setName(e.target.value)} />
        <label htmlFor="password">Password (min 8 characters)</label>
        <input id="password" type="password" autoComplete="new-password" required minLength={8}
          value={password} onChange={(e) => setPassword(e.target.value)} />
        <button className="button" type="submit" disabled={submitting}>
          {submitting ? "Creating..." : "Create account"}
        </button>
      </form>
      <p><Link href="/login">Already have an account?</Link></p>
    </section>
  );
}

function ForgotPasswordPage() {
  usePageTitle("Forgot password");
  const [email, setEmail] = useState("");
  const [submitted, setSubmitted] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();
    try {
      await apiRequest("/api/auth/forgot-password", {
        method: "POST",
        body: JSON.stringify({ email })
      });
    } catch {
      // intentional: do not reveal whether the email exists
    } finally {
      setSubmitted(true);
    }
  };

  if (submitted) {
    return (
      <StatusCard
        title="Check your email"
        message="If that email is registered, a reset link is on its way. The link expires in one hour."
        actionHref="/login"
        actionLabel="Back to login"
      />
    );
  }

  return (
    <section className="panel prose">
      <p className="eyebrow">Account</p>
      <h1>Forgot your password?</h1>
      <form onSubmit={handleSubmit}>
        <label htmlFor="email">Email</label>
        <input id="email" type="email" autoComplete="email" required
          value={email} onChange={(e) => setEmail(e.target.value)} />
        <button className="button" type="submit">Send reset link</button>
      </form>
      <p><Link href="/login">Back to login</Link></p>
    </section>
  );
}

function ResetPasswordPage({ searchParams }) {
  usePageTitle("Reset password");
  const initialToken = searchParams.get("token") ?? "";
  const [token, setToken] = useState(initialToken);
  const [newPassword, setNewPassword] = useState("");
  const [error, setError] = useState("");
  const [done, setDone] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setSubmitting(true);
    try {
      await apiRequest("/api/auth/reset-password", {
        method: "POST",
        body: JSON.stringify({ token, newPassword })
      });
      setDone(true);
    } catch (err) {
      setError(err.message || "Could not reset password.");
    } finally {
      setSubmitting(false);
    }
  };

  if (done) {
    return (
      <StatusCard
        title="Password updated"
        message="Your password has been reset. You can now log in."
        actionHref="/login"
        actionLabel="Log in"
      />
    );
  }

  return (
    <section className="panel prose">
      <p className="eyebrow">Account</p>
      <h1>Choose a new password</h1>
      {error ? <p className="info-banner">{error}</p> : null}
      <form onSubmit={handleSubmit}>
        <input type="hidden" value={token} onChange={(e) => setToken(e.target.value)} />
        <label htmlFor="newPassword">New password (min 8 characters)</label>
        <input id="newPassword" type="password" autoComplete="new-password" required minLength={8}
          value={newPassword} onChange={(e) => setNewPassword(e.target.value)} />
        <button className="button" type="submit" disabled={submitting}>
          {submitting ? "Saving..." : "Save new password"}
        </button>
      </form>
    </section>
  );
}

function OAuthCallbackPage({ searchParams }) {
  usePageTitle("Signing you in");
  const [error, setError] = useState("");

  useEffect(() => {
    const token = searchParams.get("token");
    if (!token) {
      setError("OAuth sign-in did not return a session token.");
      return;
    }

    let cancelled = false;
    apiRequest("/api/auth/me", {
      headers: { Authorization: `Bearer ${token}` }
    })
      .then((user) => {
        if (cancelled) return;
        saveSession({
          token,
          expiresAt: new Date(Date.now() + 30 * 60 * 1000).toISOString(),
          user
        });
        navigate("/");
      })
      .catch((err) => {
        if (cancelled) return;
        setError(err.message || "Failed to complete OAuth sign-in.");
      });

    return () => {
      cancelled = true;
    };
  }, [searchParams]);

  if (error) {
    return (
      <StatusCard
        title="OAuth sign-in failed"
        message={error}
        actionHref="/login"
        actionLabel="Back to login"
      />
    );
  }

  return <StatusCard title="Signing you in" message="Completing OAuth sign-in." />;
}

function NotFoundPage() {
  usePageTitle("Not Found");

  return (
    <StatusCard
      title="Page not found"
      message="The page you requested doesn't exist in the SPA router."
      actionHref="/"
      actionLabel="Back Home"
    />
  );
}

export default function App() {
  const [location, setLocation] = useState(parseLocation());

  useEffect(() => {
    const handleRouteChange = () => {
      setLocation(parseLocation());
      window.scrollTo({ top: 0, behavior: "smooth" });
    };

    window.addEventListener("popstate", handleRouteChange);
    return () => window.removeEventListener("popstate", handleRouteChange);
  }, []);

  const route = useMemo(() => {
    const productMatch = location.pathname.match(/^\/products\/(\d+)$/);

    if (location.pathname === "/") {
      return <HomePage />;
    }

    if (location.pathname === "/privacy") {
      return <PrivacyPage />;
    }

    if (location.pathname === "/products") {
      return <ProductsPage />;
    }

    if (productMatch) {
      return <ProductDetailsPage productId={productMatch[1]} />;
    }

    if (location.pathname === "/search") {
      return <SearchPage searchParams={location.searchParams} />;
    }

    if (location.pathname === "/login") {
      return <LoginPage />;
    }

    if (location.pathname === "/register") {
      return <RegisterPage />;
    }

    if (location.pathname === "/forgot-password") {
      return <ForgotPasswordPage />;
    }

    if (location.pathname === "/reset-password") {
      return <ResetPasswordPage searchParams={location.searchParams} />;
    }

    if (location.pathname === "/oauth-callback") {
      return <OAuthCallbackPage searchParams={location.searchParams} />;
    }

    return <NotFoundPage />;
  }, [location]);

  return <AppShell>{route}</AppShell>;
}
