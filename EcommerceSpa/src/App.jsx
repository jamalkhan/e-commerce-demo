import { useEffect, useMemo, useState } from "react";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || "https://sandbox.api.jamal.com";

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
  const response = await fetch(`${apiBaseUrl}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...(options.headers ?? {})
    },
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

  const [userName, setUserName] = useState("");
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
          <h2>Log In</h2>
          <form
            onSubmit={(event) => {
              event.preventDefault();
              navigate(`/login?uname=${encodeURIComponent(userName)}`);
            }}
          >
            <label htmlFor="username">Username</label>
            <input
              id="username"
              value={userName}
              onChange={(event) => setUserName(event.target.value)}
              placeholder="Login"
            />
            <button className="button" type="submit">Log In</button>
          </form>
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

function LoginPage({ searchParams }) {
  const userName = searchParams.get("uname") ?? "";

  usePageTitle("Login");

  const [state, setState] = useState({
    loading: true,
    error: "",
    payload: null
  });

  useEffect(() => {
    let active = true;

    apiRequest("/api/login", {
      method: "POST",
      body: JSON.stringify({ userName })
    })
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
  }, [userName]);

  if (state.loading) {
    return <StatusCard title="Logging in" message="Contacting EcommerceApi." />;
  }

  if (state.error || !state.payload) {
    return (
      <StatusCard
        title="Login unavailable"
        message={state.error || "We couldn't complete the login request."}
        actionHref="/"
        actionLabel="Back Home"
      />
    );
  }

  return (
    <section className="panel prose">
      <p className="eyebrow">Account</p>
      <h1>Welcome</h1>
      <p>{state.payload.message}</p>
      <p><strong>{state.payload.userName}</strong></p>
      <Link href="/" className="button secondary">Back Home</Link>
    </section>
  );
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
      return <LoginPage searchParams={location.searchParams} />;
    }

    return <NotFoundPage />;
  }, [location]);

  return <AppShell>{route}</AppShell>;
}
