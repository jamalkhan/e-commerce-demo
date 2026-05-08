# E-Commerce Demo

## Prerequisites

You need dotnet and Caddy. You'll also need to map the three sandbox hostnames to localhost in your hosts file.

### Hosts file

The demo uses three hostnames, all routed locally through Caddy:

| Hostname | Service | Local port |
| --- | --- | --- |
| sandbox.api.jamal.com | EcommerceApi | 5111 |
| sandbox.mvc.jamal.com | EcommerceMvc | 5112 |
| sandbox.spa.jamal.com | EcommerceSpa  | 5113 |

#### macOS / Linux

Add the following line to `/etc/hosts` (requires sudo):

```
127.0.0.1   sandbox.api.jamal.com sandbox.mvc.jamal.com sandbox.spa.jamal.com
```

#### Windows

Edit `C:\Windows\System32\drivers\etc\hosts` as Administrator and add the same line:

```
127.0.0.1   sandbox.api.jamal.com sandbox.mvc.jamal.com sandbox.spa.jamal.com
```

> Note: the hosts file does not support wildcards on either OS. If you start adding many subdomains, look at `dnsmasq` + `/etc/resolver/jamal.com` on macOS, or Acrylic DNS Proxy on Windows for wildcard support.

### dotnet

- EcommerceApi listens on http://localhost:5111
- EcommerceMvc listens on http://localhost:5112
- EcommerceSpa (Vite dev server) listens on http://localhost:5113

### Caddy

Caddy is a proxy server fronting all three apps with HTTPS. Each `sandbox.*.jamal.com` host listens on `:443` and forwards to the matching local port above.

https://caddyserver.com/docs/install

For Mac users...

brew install caddy

Then run caddy using the repo-root `Caddyfile`.

Caddy uses a local CA.

The first time:

macOS will prompt you

Trust the certificate in Keychain Access

Restart your browser

After that, you’re good.

Why Caddy is nice...

🔒 Automatic HTTPS (Let’s Encrypt) — zero config
🔁 Reverse proxy to Kestrel (localhost:5000)
🔄 Automatic certificate renewal
📦 Single config file (Caddyfile)
🧠 Sensible defaults (no TLS pain like nginx)

## Running

Run the following two processes concurrently (likely from two separate terminals)

dotnet run
sudo caddy run

## Aspire AppHost

For the cleanest local orchestration, run the Aspire AppHost instead:

dotnet run --project EcommerceAppHost/EcommerceAppHost.csproj

This starts:

- EcommerceApi
- EcommerceMvc
- EcommerceSpa (via the Vite dev server)

The AppHost gives you a single dashboard to inspect endpoints, logs, and startup order. You no longer need EcommerceMvc to manually launch EcommerceApi.

## Notes

This mirrors a production deployment, is cross platform compatible (sorry instructions here are for Mac only...). Both Caddy and EcommerceMvc should run on Windows and Linux, but i've not tested it.

choco install caddy

or

sudo apt install -y debian-keyring debian-archive-keyring apt-transport-https
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/gpg.key' | sudo gpg --dearmor -o /usr/share/keyrings/caddy-stable-archive-keyring.gpg
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/debian.deb.txt' | sudo tee /etc/apt/sources.list.d/caddy-stable.list
sudo apt update
sudo apt install caddy
