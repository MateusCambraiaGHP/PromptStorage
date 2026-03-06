---
name: application-layer
description: Rules for project layered architecture — Application layer (handlers, validators, models), Infrastructure layer (external services, messaging, caching), and Data layer (entities, repositories, EF Core, database access)
tags: application, handlers, fluent-validation, base-response, base-request, models, validators, business-rules, infrastructure, external-services, data, repositories, entities, ef-core, database
---

# Application Layer

## Overview
ALWAYS create an **Application** layer (folder) that stores:
- **Handlers** — handle requests and contain business rules
- **Models** — request and response DTOs
- **Validators** — FluentValidation validators for requests

## Folder Structure

```
Application/
├── Models/
│   ├── Base/
│   │   ├── BaseHandlerRequest.cs
│   │   └── BaseHandlerResponse.cs
│   ├── Reservations/
│   │   ├── CreateReservationRequest.cs
│   │   └── CreateReservationResponse.cs
│   └── Customers/
│       ├── CreateCustomerRequest.cs
│       └── CreateCustomerResponse.cs
├── Handlers/
│   ├── Reservations/
│   │   ├── ICreateReservationHandler.cs
│   │   └── CreateReservationHandler.cs
│   └── Customers/
│       ├── ICreateCustomerHandler.cs
│       └── CreateCustomerHandler.cs
└── Validators/
    ├── Reservations/
    │   └── CreateReservationRequestValidator.cs
    └── Customers/
        └── CreateCustomerRequestValidator.cs
```

---

# Base Models

## BaseHandlerResponse\<T\>

Generic response model used by ALL handlers. Contains success status, message, validation errors, and the typed data:

```csharp
public class BaseHandlerResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<string> ValidationErrors { get; set; }
    public T Data { get; set; }

    public BaseHandlerResponse()
    {
        Success = true;
        ValidationErrors = new List<string>();
    }

    public BaseHandlerResponse(T data, string message = "")
    {
        Success = true;
        Data = data;
        Message = message;
        ValidationErrors = new List<string>();
    }

    public BaseHandlerResponse(bool success, string message = "")
    {
        Success = success;
        Message = message;
        ValidationErrors = new List<string>();
    }
}
```

## BaseHandlerRequest

Base class for all requests (extend as needed):

```csharp
public abstract class BaseHandlerRequest
{
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}
```

---

# Handlers

Handlers contain the business logic. They validate the request using FluentValidation, process the business rule, and return a `BaseHandlerResponse<T>`:

```csharp
public class CreateReservationHandler : ICreateReservationHandler
{
    private readonly IReservationRepository _repository;
    private readonly IValidator<CreateReservationRequest> _validator;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateReservationHandler> _logger;

    public CreateReservationHandler(
        IReservationRepository repository,
        IValidator<CreateReservationRequest> validator,
        IMapper mapper,
        ILogger<CreateReservationHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseHandlerResponse<CreateReservationResponse>> HandleAsync(
        CreateReservationRequest request)
    {
        var response = new BaseHandlerResponse<CreateReservationResponse>();

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            response.Success = false;
            response.ValidationErrors = validationResult.Errors
                .Select(e => e.ErrorMessage).ToList();
            return response;
        }

        var entity = _mapper.Map<Reservation>(request);
        await _repository.CreateAsync(entity);

        response.Data = _mapper.Map<CreateReservationResponse>(entity);
        response.Message = "Reservation created successfully.";

        return response;
    }
}
```

---

# Validation

Use **FluentValidation** for ALL request DTOs:

```csharp
public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.GuestName)
            .NotEmpty().WithMessage("Guest name is required.")
            .MaximumLength(100).WithMessage("Guest name must not exceed 100 characters.");

        RuleFor(x => x.CheckInDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Check-in date must be in the future.");

        RuleFor(x => x.CheckOutDate)
            .GreaterThan(x => x.CheckInDate).WithMessage("Check-out must be after check-in.");
    }
}
```

---

# Application Layer Rules
- ALWAYS validate in the handler before processing business logic
- ALWAYS return `BaseHandlerResponse<T>` from handlers — never throw exceptions for validation
- ALWAYS populate `ValidationErrors` list when validation fails
- ALWAYS include a descriptive `Message` on success responses
- Each handler has its own interface (e.g., `ICreateReservationHandler`)
- Inject dependencies via constructor (repository, validator, mapper, logger)
- Log business operations at the handler level

---

# Infrastructure Layer

## Overview
The **Infrastructure** layer handles all **external concerns** — anything that is NOT core business logic:
- External API calls (HTTP clients, third-party services)
- Messaging (queues, event buses, SignalR)
- Caching (Redis, MemoryCache)
- Email / SMS / notification services
- File storage (Azure Blob, S3, local disk)
- Authentication providers (OAuth, JWT token services)

## Folder Structure

```
Infrastructure/
├── ExternalServices/
│   ├── PaymentGateway/
│   │   ├── IPaymentGatewayService.cs
│   │   └── PaymentGatewayService.cs
│   └── EmailService/
│       ├── IEmailService.cs
│       └── EmailService.cs
├── Caching/
│   ├── ICacheService.cs
│   └── RedisCacheService.cs
├── Messaging/
│   ├── IMessagePublisher.cs
│   └── RabbitMqPublisher.cs
└── Identity/
    ├── ITokenService.cs
    └── JwtTokenService.cs
```

## External Service Pattern

Every external service MUST have:
- An **interface** defined in Infrastructure (or Application, if handlers depend on it)
- A **concrete implementation** in Infrastructure
- Proper **error handling** — external calls can fail, always wrap with try/catch
- **Logging** on every external call

```csharp
public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentGatewayService> _logger;

    public PaymentGatewayService(
        HttpClient httpClient,
        ILogger<PaymentGatewayService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Processing payment for order {OrderId}", request.OrderId);

            var response = await _httpClient.PostAsJsonAsync("/api/payments", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<PaymentResult>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Payment gateway failed for order {OrderId}", request.OrderId);
            throw;
        }
    }
}
```

## Cache Service Pattern

```csharp
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IDistributedCache cache,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var cached = await _cache.GetStringAsync(key);
        if (cached is null) return default;

        return JsonSerializer.Deserialize<T>(cached);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
        };

        var json = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, json, options);
    }
}
```

## Infrastructure Layer Rules
- ALWAYS define an interface for every external service
- ALWAYS wrap external calls with try/catch and proper logging
- NEVER put business logic in Infrastructure — only integration/communication logic
- Register HttpClient services using `IHttpClientFactory` / `AddHttpClient<T>()`
- Use `IOptions<T>` for external service configuration (URLs, API keys, timeouts)
- NEVER hardcode URLs, keys, or secrets — use configuration/environment variables

---

# Data Layer

## Overview
The **Data** layer handles all **database concerns**:
- **Entities** — domain models mapped to database tables
- **Repositories** — data access abstraction over EF Core
- **DbContext** — Entity Framework Core database context
- **Configurations** — EF Core Fluent API entity configurations
- **Migrations** — database schema versioning

## Folder Structure

```
Data/
├── Context/
│   └── AppDbContext.cs
├── Entities/
│   ├── BaseEntity.cs
│   ├── Customer.cs
│   ├── Reservation.cs
│   └── Order.cs
├── Repositories/
│   ├── Base/
│   │   ├── IBaseRepository.cs
│   │   └── BaseRepository.cs
│   ├── ICustomerRepository.cs
│   ├── CustomerRepository.cs
│   ├── IReservationRepository.cs
│   └── ReservationRepository.cs
├── Configurations/
│   ├── CustomerConfiguration.cs
│   └── ReservationConfiguration.cs
├── UnitOfWork/
│   ├── IUnitOfWork.cs
│   └── UnitOfWork.cs
└── Migrations/
    └── (auto-generated)
```

## Base Entity

All entities inherit from a base class with common fields:

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
```

## Base Repository

Generic repository with common CRUD operations:

```csharp
public interface IBaseRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        params Expression<Func<T, object>>[] includes);
    Task CreateAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.Where(e => e.IsActive).ToListAsync();

    public async Task<IEnumerable<T>> GetAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        if (filter is not null)
            query = query.Where(filter);

        foreach (var include in includes)
            query = query.Include(include);

        if (orderBy is not null)
            query = orderBy(query);

        return await query.ToListAsync();
    }

    public async Task CreateAsync(T entity)
        => await _dbSet.AddAsync(entity);

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Delete(T entity)
    {
        entity.IsActive = false;
        _dbSet.Update(entity);
    }
}
```

## Specific Repositories

Extend the base repository for entity-specific queries:

```csharp
public interface ICustomerRepository : IBaseRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email);
}

public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context) { }

    public async Task<Customer?> GetByEmailAsync(string email)
        => await _dbSet.FirstOrDefaultAsync(c => c.Email == email && c.IsActive);
}
```

## Unit of Work

Coordinate multiple repository operations in a single transaction:

```csharp
public interface IUnitOfWork : IDisposable
{
    ICustomerRepository Customers { get; }
    IReservationRepository Reservations { get; }
    Task<int> CommitAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public ICustomerRepository Customers { get; }
    public IReservationRepository Reservations { get; }

    public UnitOfWork(
        AppDbContext context,
        ICustomerRepository customers,
        IReservationRepository reservations)
    {
        _context = context;
        Customers = customers;
        Reservations = reservations;
    }

    public async Task<int> CommitAsync()
        => await _context.SaveChangesAsync();

    public void Dispose()
        => _context.Dispose();
}
```

## Entity Configuration (Fluent API)

Use Fluent API for entity configurations — never data annotations on entities:

```csharp
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(c => c.Email)
            .IsUnique();
    }
}
```

## Data Layer Rules
- ALWAYS use a `BaseEntity` with `Id`, `CreatedAt`, `UpdatedAt`, `IsActive`
- ALWAYS use a generic `BaseRepository<T>` — avoid repeating CRUD logic
- ALWAYS use `IUnitOfWork` to coordinate saves across multiple repositories
- ALWAYS use Fluent API (`IEntityTypeConfiguration<T>`) for entity config — never data annotations
- ALWAYS use soft delete (`IsActive = false`) — never hard delete
- NEVER expose `DbContext` directly to handlers — always go through repositories
- NEVER put business logic in repositories — only data access logic
- Use `AsNoTracking()` for read-only queries to improve performance

---

# Full Project Structure (All Layers)

```
ProjectName/
├── Controllers/
│   ├── BaseController.cs
│   ├── CustomersController.cs
│   └── ReservationsController.cs
├── Application/
│   ├── Models/
│   │   ├── Base/
│   │   │   ├── BaseHandlerRequest.cs
│   │   │   └── BaseHandlerResponse.cs
│   │   └── Reservations/
│   │       ├── CreateReservationRequest.cs
│   │       └── CreateReservationResponse.cs
│   ├── Handlers/
│   │   └── Reservations/
│   │       ├── ICreateReservationHandler.cs
│   │       └── CreateReservationHandler.cs
│   └── Validators/
│       └── Reservations/
│           └── CreateReservationRequestValidator.cs
├── Infrastructure/
│   ├── ExternalServices/
│   ├── Caching/
│   └── Messaging/
├── Data/
│   ├── Context/
│   │   └── AppDbContext.cs
│   ├── Entities/
│   │   ├── BaseEntity.cs
│   │   └── Customer.cs
│   ├── Repositories/
│   │   ├── Base/
│   │   │   ├── IBaseRepository.cs
│   │   │   └── BaseRepository.cs
│   │   ├── ICustomerRepository.cs
│   │   └── CustomerRepository.cs
│   ├── Configurations/
│   │   └── CustomerConfiguration.cs
│   ├── UnitOfWork/
│   │   ├── IUnitOfWork.cs
│   │   └── UnitOfWork.cs
│   └── Migrations/
└── Program.cs
```
