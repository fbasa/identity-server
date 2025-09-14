# IDP 


PKCE (Proof Key for Code Exchange), pronounced “pixie,” is a security enhancement to the **OAuth 2.0 Authorization Code flow**, designed to protect public clients—like mobile apps and single-page applications—that **cannot securely store a client secret**.

### Why PKCE Exists
The traditional Authorization Code flow assumes the client can keep a secret. But in public apps:
- Anyone can decompile the app and extract secrets.
- Malicious apps can intercept redirect URIs and steal authorization codes.

PKCE solves this by **binding the authorization request to the token exchange**, making it nearly impossible for attackers to hijack the flow.

---

### 🚦 How PKCE Flow Works

Here’s a simplified breakdown:

1. **Client creates a `code_verifier`**: A random string known only to the app.
2. **Client generates a `code_challenge`**: A hashed version of the `code_verifier` (usually using SHA256).
3. **Authorization Request**: The app sends the `code_challenge` to the authorization server when requesting an authorization code.
4. **User logs in and grants access**.
5. **Token Request**: The app sends the `code_verifier` (not the challenge) along with the authorization code to get an access token.
6. **Server validates**: It hashes the `code_verifier` and compares it to the original `code_challenge`. If they match, the token is issued.

---

### Why It’s Secure
- Even if an attacker intercepts the authorization code, they **can’t exchange it for a token** without the original `code_verifier`.
- No client secret is needed, making it ideal for public clients.
- PKCE is now **mandatory in OAuth 2.1**.

---

### Real-World Analogy
Think of PKCE like a lock-and-key system:
- The app sends a lock (`code_challenge`) to the server.
- Later, it must present the matching key (`code_verifier`) to unlock the token.

