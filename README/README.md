# GatekeeperXMiddleware

GatekeeperX is a robust middleware for ASP.NET Core, designed to sanitize and protect incoming JSON requests against vulnerabilities like XSS and SQL Injection. It's fast, reusable, and built to shield your application from OWASP Top 10 threats while ensuring secure data validation.

## Features

- **Input Validation**: Automatically validates all incoming JSON payloads to ensure they are free from malicious inputs.
- **Protection against XSS**: Filters potentially harmful scripts from JSON inputs to prevent Cross-Site Scripting attacks.
- **Protection against SQL Injection**: Detects patterns associated with SQL Injection and blocks them.
- **Reusable Middleware**: Designed as a middleware, it can be easily integrated into any ASP.NET Core project.
- **Extensible**: Allows for future validation and sanitization logic to be added.
- **OWASP Top 10 Protection**: Currently focuses on injection attacks and Cross-Site Scripting (XSS), with plans to expand to cover other OWASP vulnerabilities.

## Requirements

- **.NET 8.0** or higher
- ASP.NET Core API

## Installation (NOT ready for installation yet):

To use **GatekeeperXMiddleware**, add the NuGet package `GatekeeperX` to your project:

```bash
dotnet add package GatekeeperX

