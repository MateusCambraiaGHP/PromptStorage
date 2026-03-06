---
name: api-endpoints
description: Template for creating REST API endpoints using ASP.NET Core controllers with base controller, application layer handlers, FluentValidation, and generic base request/response models
tags: api, rest, controllers, endpoints, aspnet, handlers, fluent-validation, crud, actions
---

# API Endpoint Generation Rules

## Style
- Use **Controllers** (not minimal APIs)
- Controller receives the request, logs it, sends it to a **handler**, and returns the response
- Always return **explicit HTTP status codes** with clear error messages (`200`, `400`, `404`, `500`, etc.)

## Two Types of Endpoints

### 1. Action Endpoints — for business rules
Use when the endpoint involves business logic, validations, or orchestration:

```csharp
[HttpPost("process-payment")]
public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
```

### 2. CRUD Endpoints — for simple data operations
Use for straightforward Create, Read, Update, Delete with no complex logic:

```csharp
[HttpGet]
[HttpGet("{id:int}")]
[HttpPost]
[HttpPut("{id:int}")]
[HttpDelete("{id:int}")]
```

## Base Controller

ALWAYS create a `BaseController` that all controllers inherit from. It defines the base route with API versioning and lowercase routes:

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected readonly ILogger _logger;

    protected BaseController(ILogger logger)
    {
        _logger = logger;
    }

    protected IActionResult HandleResponse<T>(BaseHandlerResponse<T> response)
    {
        if (!response.Success)
        {
            if (response.ValidationErrors?.Count > 0)
                return BadRequest(response);

            return StatusCode(500, response);
        }

        return Ok(response);
    }
}
```

## Controller Structure

Controllers receive the request, log, delegate to a handler, and return:

```csharp
[Route("api/v1/[controller]")]
public class ReservationsController : BaseController
{
    private readonly ICreateReservationHandler _createReservationHandler;

    public ReservationsController(
        ILogger<ReservationsController> logger,
        ICreateReservationHandler createReservationHandler)
        : base(logger)
    {
        _createReservationHandler = createReservationHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(BaseHandlerResponse<CreateReservationResponse>), 200)]
    [ProducesResponseType(typeof(BaseHandlerResponse<CreateReservationResponse>), 400)]
    [ProducesResponseType(typeof(BaseHandlerResponse<CreateReservationResponse>), 500)]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequest request)
    {
        _logger.LogInformation("Creating reservation for {GuestName}", request.GuestName);

        var response = await _createReservationHandler.HandleAsync(request);

        return HandleResponse(response);
    }
}
```

## Route Rules
- All routes must be **lowercase**: `api/v1/reservations`, `api/v1/customers`
- Use plural nouns for resource names: `reservations`, `orders`, `customers`
- Use kebab-case for multi-word action endpoints: `process-payment`, `cancel-order`
- Always include API version: `api/v1/`

---

# Response Patterns

| Status Code | When to Use |
|-------------|-------------|
| `200 OK` | Successful GET, PUT, or action endpoint |
| `201 Created` | Successful POST (resource created) |
| `204 NoContent` | Successful DELETE |
| `400 BadRequest` | Validation errors — return `ValidationErrors` list |
| `404 NotFound` | Resource not found |
| `409 Conflict` | Duplicate resource |
| `500 InternalServerError` | Unhandled errors — return `Success = false` with message |

## Rules
- ALWAYS return explicit status codes — never let exceptions bubble up without a response
- ALWAYS include a descriptive `Message` in the response
- Use `ProducesResponseType` attributes on all endpoints to document possible responses
- Use cancellation tokens on all async endpoints
- Log at the controller level (request received)
- Controllers delegate ALL business logic to handlers in the Application layer (see `application-layer` prompt)
