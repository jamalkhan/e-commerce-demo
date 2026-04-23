# E-Commerce Demo

## Prerequisites

You need dotnet and Caddy. You'll also need to create an alias to sandbox.jamal.com

### alias

On Mac, add the following to /etc/hosts

127.0.0.1   sandbox.jamal.com

### dotnet

EcommerceMvc is a simple dotnet (10) Web / Mvc App that serves the web content on http://localhost:5112

### Caddy

Caddy is a proxy server. The dotnet Kestral server is configured to listen on http \ 5112. Caddy, listens on https://sandbox.jamal.com:443 and forwards traffic to http://localhost:5112

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
