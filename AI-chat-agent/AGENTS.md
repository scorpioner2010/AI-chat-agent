# AI Chat Agent Project Rules

1. The application is a universal AI chat agent.
2. Website-specific logic must be implemented through platform adapters.
3. Domain must not depend on UI, databases, browser automation, or AI SDKs.
4. WPF code-behind must contain only UI-specific logic.
5. Business logic must be covered by unit tests.
6. Asynchronous I/O methods must accept CancellationToken.
7. AI responses will later use structured JSON and local validation.
8. Do not store passwords, API keys, browser sessions, or local databases in Git.
9. Do not implement CAPTCHA bypass, fingerprint spoofing, ban evasion, or mass account creation.
10. Do not add packages that are not required for this task.
