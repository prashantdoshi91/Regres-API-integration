# ReqresIntegration

A .NET 9 class library that integrates with the public [reqres.in](https://reqres.in) API to fetch and display user data. 
It demonstrates best practices in API consumption, async programming, error handling, optional caching, and testability.

---

## 🚀 Features

- ✅ Asynchronous API client using `HttpClientFactory`
- ✅ Supports paginated user fetching
- ✅ Internal DTO mapping
- ✅ Service abstraction with interface
- ✅ Clean architecture separation (Core, Infrastructure, Console)
- ✅ Optional in-memory caching with configurable expiration
- ✅ Unit testing with xUnit + Moq

---

## 🗂️ Project Structure

ReqresIntegration.sln
│
├── ReqresIntegration.Core # Models and interfaces
├── ReqresIntegration.Infrastructure # API client, service logic, caching
├── ReqresIntegration.ConsoleDemo # Example usage console app
└── ReqresIntegration.Tests # Unit tests



---

## ⚙️ Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download)
- Git (if cloning)
- Optional: Visual Studio, Rider, or VS Code

---

## 🛠️ Setup & Run

### 1. Clone the Repository

git clone <your-github-url>
cd ReqresIntegration

### 2. Restore Packages

dotnet restore

### 3. Run Console Demo

cd ReqresIntegration.ConsoleDemo
dotnet run

You should see a list of users printed from the Reqres API.



## 🧪 Run Tests

dotnet test ReqresIntegration.Tests


## 🔁 Configuration

appsettings.json (used by the console demo):

{
  "ReqresApi": {
    "BaseUrl": "https://reqres.in/api/",
    "ApiKey": "reqres-free-v1"
  }
}


## 🧠 Design Decisions

HttpClientFactory is used for better resource management and testability.

Clean separation of concerns: DTOs in Core, logic in Infrastructure.

Async/await is used throughout for modern non-blocking IO.

Optional caching via IMemoryCache improves performance for repeated calls.

Extensible for adding Polly retry logic or API auth headers later.


## 🏆 Bonus Features (Optional)

In-memory caching with CachedExternalUserService

Configurable base URL using IOptions<T>

Fully testable and mockable components

Designed to be plugged into larger systems (e.g., ASP.NET Core app)

