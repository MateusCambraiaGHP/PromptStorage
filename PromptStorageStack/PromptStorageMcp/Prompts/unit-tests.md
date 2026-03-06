---
name: unit-tests
description: Template for generating unit tests following AAA pattern with xUnit, Moq, and centralized mock classes in a Data folder
tags: testing, xunit, unit-tests, tdd, moq, mocks
---

# Unit Test Generation Rules

## Framework & Libraries
- Use **xUnit** as the test framework
- Use **Moq** for mocking dependencies
- Use **FluentAssertions** for assertions when available, otherwise use `Assert`

## Project Folder Structure

Organize the test project with a `Data/` folder for mocks and mirror the source project structure for tests:

```
Tests/
├── Data/
│   ├── Repositories/
│   │   ├── MockCustomerRepository.cs
│   │   └── MockOrderRepository.cs
│   ├── Services/
│   │   ├── MockCacheService.cs
│   │   └── MockMapper.cs
│   └── UnitOfWork/
│       └── MockUnitOfWork.cs
├── Application/
│   └── Features/
│       └── Customers/
│           ├── Commands/
│           │   └── CreateCustomerCommandTests.cs
│           └── Queries/
│               └── GetCustomerQueryTests.cs
└── Domain/
    └── Entities/
        └── CustomerTests.cs
```

## Mock Classes (Data Folder)

Create **one mock class per interface** in the `Data/` folder. Each mock class:
- Has a static `GetMock()` method returning a configured `Mock<T>`
- Contains private static methods to provide fake data
- Pre-configures common setups so tests stay clean

```csharp
namespace ProjectName.Tests.Data.Repositories
{
    public class MockCustomerRepository
    {
        public static Mock<ICustomerRepository> GetMock()
        {
            var mock = new Mock<ICustomerRepository>();

            mock.Setup(c => c.Create(It.IsAny<Customer>()));
            mock.Setup(r => r.GetAsync(null, null, null))
                .ReturnsAsync((Expression<Func<Customer, bool>> filter,
                                Func<IQueryable<Customer>, IOrderedQueryable<Customer>> orderBy,
                                Expression<Func<Customer, object>>[] includes) =>
                {
                    var customers = CustomersMock();
                    return customers.ToList();
                });

            return mock;
        }

        private static List<Customer> CustomersMock()
            => new()
            {
                // Add realistic fake data here
            };
    }
}
```

## Naming Convention
- Test class: `{ClassName}Tests`
- Test method: `Should{Action}_{Entity}_{Scenario}_Return{Result}`
- Example: `ShouldCreate_Customer_ValidData_ReturnSucess`

## Test Class Structure

Inject all mocks via the **constructor** using the centralized mock classes:

```csharp
namespace ProjectName.Tests.Application.Features.Customers.Commands
{
    public class CreateCustomerCommandTests
    {
        private readonly Mock<ICustomerRepository> _customerRepository;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<ICacheService> _cacheService;

        public CreateCustomerCommandTests()
        {
            _customerRepository = MockCustomerRepository.GetMock();
            _mapper = MockMapper.GetMock();
            _unitOfWork = MockUnitOfWork.GetMock();
            _cacheService = MockCacheService.GetMock();
        }

        [Fact]
        public async Task ShouldCreate_Customer_ValidData_ReturnSucess()
        {
            //Arrange
            var commandDto = new CreateCustomerDto { Name = "Test" };

            //Act
            var handler = new CreateCustomerCommandHandler(
                _customerRepository.Object,
                _mapper.Object,
                _unitOfWork.Object,
                _cacheService.Object);
            var result = await handler.Handle(command, CancellationToken.None);

            //Assert
            result.Should().NotBeNull();
        }
    }
}
```

## AAA Comments
- Use only short inline comments: `//Arrange`, `//Act`, `//Assert`
- Do NOT use verbose comments like `// Arrange - Set up test data`

## Coverage Requirements
- Happy path (valid input, expected behavior)
- Edge cases (null, empty, boundary values)
- Error cases (exceptions, invalid input)
- At least 3 test methods per public method

## Rules
- Each test class tests ONE class (handler, service, etc.) only
- Create one mock class per interface in the `Data/` folder — reuse across all tests
- Initialize all mocks in the test class constructor, not in each test method
- Use `[Theory]` with `[InlineData]` for parameterized tests when testing multiple inputs
- Never use `Thread.Sleep` — use async patterns
- Use descriptive variable names: `expectedTotal`, `invalidInput`, not `x`, `y`
- Mirror the source project namespace in the test project namespace
