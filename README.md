# LegacyOrderMgmt

LegacyOrderMgmt is a deliberately old-fashioned order management and invoicing system built on **ASP.NET Core 2.2** (EOL December 2019). It handles the full order lifecycle for Meridian Industrial Group: customer order intake (web UI + EDI XML), pricing and discount calculation, shipping carrier integration, invoicing, and sales reporting. It exists as a modernization demo — the app works, but carries a rich catalogue of outdated and risky patterns for a Copilot-guided upgrade to .NET 8.

## App purpose

- Accept orders from the internal sales UI and via EDI XML import from customer systems
- Apply tiered pricing and discount rules per customer
- Track order status through the full lifecycle: Draft → Confirmed → Processing → Shipped → Invoiced
- Integrate with shipping carriers (sync HTTP) and the warehouse pick system
- Generate invoices and email them via SMTP
- Produce sales reports as CSV

## Project layout

- `LegacyOrderMgmt.sln` — solution entry point
- `LegacyOrderMgmt.Core` — domain models, EF Core 2.2 DbContext, OrderService, PricingEngine
- `LegacyOrderMgmt.Web` — ASP.NET Core 2.2 MVC application with API-style endpoints, services, and startup
- `Database` — SQL Server LocalDB schema and seed scripts

## How it fits the application estate

LegacyOrderMgmt sits between **LegacyCRM** and **LegacyInventory** in the business flow:

```
LegacyCRM          LegacyOrderMgmt            LegacyInventory
─────────────    ───────────────────────    ─────────────────────
Customer won  →  Order created & priced  →  Stock allocated
Opportunity   →  Order confirmed         →  Pick/pack from warehouse
Account data  →  Invoice generated       →  Stock level updated
              →  Shipped + tracking      →
```

## Setup

1. Run `Database\CreateDatabase.sql` against LocalDB or SQL Server Express.
2. Run `Database\SeedData.sql` to load demo customers, products, pricing rules, orders, invoices, and shipments.
3. Update `LegacyOrderMgmt.Web\appsettings.json` connection string if needed.
4. Build: `dotnet build LegacyOrderMgmt.sln`
5. Run with IIS Express or `dotnet run` from `LegacyOrderMgmt.Web\` (requires ASP.NET Core 2.2 runtime).

## Seed data summary

- 5 customers (Aerotech Dynamics, Kronfeld Automotive, NordPower Energy, Castellan Manufacturing, Halcyon Distribution)
- 8 products across 4 categories
- 6 pricing rules (Standard / Silver / Gold / Platinum tiers)
- 5 orders in various statuses (Draft, Confirmed, Processing, Shipped, Invoiced)
- 2 invoices (1 paid, 1 pending)
- 2 shipments (1 delivered, 1 in transit)

## Intentional legacy patterns

### Framework & Hosting
1. **EOL target framework** — `netcoreapp2.2` in both `.csproj` files; ASP.NET Core 2.2 reached end of life in December 2019.
2. **`WebHost.CreateDefaultBuilder` hosting model** — `LegacyOrderMgmt.Web\Program.cs`; replaced by `WebApplication.CreateBuilder` in .NET 6+.
3. **`Startup.cs` with `ConfigureServices` / `Configure`** — `LegacyOrderMgmt.Web\Startup.cs`; replaced by minimal hosting model.
4. **`IHostingEnvironment`** — `LegacyOrderMgmt.Web\Startup.cs`, `HomeController.cs`; deprecated in ASP.NET Core 3.0, replaced by `IWebHostEnvironment`.
5. **`services.AddMvc()` + `app.UseMvc()`** — `LegacyOrderMgmt.Web\Startup.cs`; replaced by `AddControllersWithViews()` and endpoint routing.
6. **Verbose EF Core DI with `UseInternalServiceProvider`** — `LegacyOrderMgmt.Web\Startup.cs`; simplify to direct `AddDbContext`.

### Data Access
7. **No `AsNoTracking()` on read queries** — `LegacyOrderMgmt.Core\Services\OrderService.cs`; all list queries track entities unnecessarily.
8. **Synchronous EF Core queries** — `OrderService.cs`, `PricingEngine.cs`, `InvoiceService.cs`; `.ToList()`, `.FirstOrDefault()`, `.SaveChanges()` used throughout instead of async equivalents.
9. **Raw SQL string concatenation** — `OrderService.cs` (`ConfirmOrder`), `InvoiceController.cs` (`OverdueReport`), `InvoiceService.cs` (`MarkOverdueInvoices`), `ReportExportService.cs`; SQL injection risk.
10. **`Database.ExecuteSqlCommand`** — `OrderService.cs`, `InvoiceService.cs`; deprecated in EF Core 3.0+, replaced by `ExecuteSqlRaw` / `ExecuteSqlInterpolated`.
11. **Race-condition order number generation** — `OrderService.cs` (`GenerateOrderNumber`), `EdiImportService.cs`; uses `MAX(Id)` pattern without atomicity guarantees.
12. **No decimal precision configuration** — `OrderDbContext.cs`; currency columns rely on SQL Server defaults, causing potential rounding drift.

### HTTP & Integrations
13. **`new HttpClient()` anti-pattern** — `ShippingIntegrationService.cs`; instantiated directly inside methods, risking socket exhaustion under load. Should use `IHttpClientFactory`.
14. **Sync-over-async** — `ShippingIntegrationService.cs`; `.Result` on `PostAsync` and `GetAsync` blocks threads and risks deadlocks.
15. **`WebClient` usage** — `ShippingIntegrationService.cs` (`GetTrackingStatus`); `WebClient` is obsolete in modern .NET.
16. **`Thread.Sleep` retry delays** — `ShippingIntegrationService.cs`, `NotificationService.cs`; blocks the thread. Should use `Polly` with async retry policies.
17. **EDI XML import via `XmlSerializer`** — `EdiImportService.cs`; synchronous, reflection-heavy, no streaming, entire payload in memory. No JSON-over-HTTP alternative offered.
18. **Hardcoded EDI date format** — `EdiImportService.cs`; `DateTime.ParseExact` with `"yyyyMMdd"` format, silently falls back to `DateTime.Now` on parse failure.

### Notifications
19. **`SmtpClient`** — `NotificationService.cs`; obsolete, does not support modern authentication (OAuth, XOAUTH2). Replace with MailKit or a cloud mail service.
20. **Swallowed exceptions in notifications** — `NotificationService.cs`; all email failures caught and written to `Console.WriteLine` — no structured logging, no alerting.

### Caching & Concurrency
21. **`BinaryFormatter` for order state caching** — `OrderService.cs` (`CacheOrder`, `GetCachedOrder`); `BinaryFormatter` is a security vulnerability and was removed in .NET 7+.
22. **`[Serializable]` domain models** — `Order.cs`, `OrderLine.cs`, `Invoice.cs`; retained to support BinaryFormatter cache.
23. **`ReaderWriterLock` instead of `ReaderWriterLockSlim`** — `OrderService.cs`; the older `ReaderWriterLock` has higher overhead and worse fairness than `ReaderWriterLockSlim`.
24. **Static in-memory pricing rule cache with no invalidation** — `PricingEngine.cs`; 5-minute TTL with no mechanism to flush on rule change.
25. **`Thread.Sleep` in pricing cache load** — `PricingEngine.cs`; 50 ms artificial delay retained from development, blocks the calling thread.

### Configuration & Observability
26. **`IConfiguration["key"]` string indexer** — `ShippingIntegrationService.cs`, `NotificationService.cs`; no strongly-typed options, no validation at startup.
27. **`int.Parse` / `bool.Parse` on raw config strings** — `NotificationService.cs`; will throw `FormatException` if config value is missing or malformed.
28. **No health checks registered or mapped** — `Startup.cs`; intentionally omitted. App is invisible to Kubernetes liveness/readiness probes.
29. **`Console.WriteLine` for logging** — `NotificationService.cs`; mixed with no structured `ILogger` usage. No telemetry integration.
30. **Windows-only report file path** — `ReportExportService.cs`; hardcoded `C:\OrderReports\` path, preventing cross-platform or container deployment.

### Serialization & API Design
31. **`Newtonsoft.Json` throughout** — `CustomerOrderController.cs`, `ShippingIntegrationService.cs`; explicit `JsonConvert.SerializeObject` calls instead of controller-managed `System.Text.Json`.
32. **Inconsistent controller base** — `CustomerOrderController.cs`; inherits `Controller` (MVC) but returns JSON — should use `ControllerBase` with `[ApiController]`.
33. **`dynamic` payload binding** — `CustomerOrderController.cs` (`SubmitOrder`); `[FromBody] dynamic` bypasses model validation entirely.
34. **`DateTime.Now` throughout** — `Order.cs`, `OrderService.cs`, `PricingEngine.cs`, `InvoiceController.cs`, `InvoiceService.cs`, `EdiImportService.cs`; timezone-unaware timestamps cause incorrect date handling in multi-region deployments.
35. **`DataSet` / `DataTable` in reporting** — `ReportExportService.cs`; ADO.NET holdover; EF Core projections to DTOs would be idiomatic.

## What must change for a .NET 8 upgrade

- Upgrade both projects from `netcoreapp2.2` to `net8.0`.
- Replace `WebHost.CreateDefaultBuilder` + `Startup` with the minimal hosting model (`WebApplication.CreateBuilder`).
- Replace `IHostingEnvironment` with `IWebHostEnvironment`.
- Replace `AddMvc()` / `UseMvc()` with `AddControllersWithViews()` and endpoint routing.
- Simplify EF Core DI registration; remove `UseInternalServiceProvider`.
- Replace all synchronous EF Core calls with async equivalents (`ToListAsync`, `SaveChangesAsync`, etc.).
- Add `AsNoTracking()` to all read-only queries.
- Replace raw SQL string concatenation with parameterized queries (`FromSqlInterpolated`, `ExecuteSqlInterpolated`, or LINQ).
- Replace `BinaryFormatter` with `System.Text.Json` or a supported binary serializer (MessagePack, Protobuf).
- Remove `[Serializable]` from domain models.
- Replace `ReaderWriterLock` with `ReaderWriterLockSlim` or `ConcurrentDictionary`.
- Replace `new HttpClient()` with `IHttpClientFactory` (typed clients).
- Replace sync-over-async (`.Result`, `.GetAwaiter().GetResult()`) with full `async/await`.
- Replace `WebClient` with `HttpClient` via `IHttpClientFactory`.
- Replace `Thread.Sleep` retries with `Polly` async retry policies.
- Replace `SmtpClient` with MailKit or a cloud mail service.
- Introduce strongly-typed options (`IOptions<T>`) for all configuration sections.
- Register and map health check endpoints (`/health`, `/ready`).
- Replace `Console.WriteLine` with structured `ILogger<T>` logging.
- Replace `XmlSerializer` EDI import with a streaming or modern deserialization approach.
- Replace `dynamic` binding with a typed ViewModel + `[ApiController]` validation.
- Replace `Newtonsoft.Json` with `System.Text.Json`.
- Replace `DateTime.Now` with `DateTimeOffset.UtcNow` and add request localization middleware.
- Replace `DataSet`/`DataTable` report generation with EF Core projections + a CSV library.
- Fix race-condition order number generation using a database sequence or GUID.
- Remove Windows-only file path assumptions for cross-platform / container deployment.
