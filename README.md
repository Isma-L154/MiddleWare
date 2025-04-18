# 🔐 JWT Authentication & Authorization Middleware

This side project is a simple yet effective **middleware** component designed to handle **user authentication and authorization using JSON Web Tokens (JWT)**. It is designed to be easily integrated into other projects by deploying it as a **NuGet package**.

Middleware plays a vital role in web applications by acting as a filter between the HTTP request and the core application logic. This middleware ensures that only authenticated and authorized users can access protected routes or resources.

---

## 🧩 What is Middleware?

A **middleware** is a function or component that processes HTTP requests and/or responses within the application pipeline. It's typically used to:

- Authenticate and authorize users
- Log incoming and outgoing traffic
- Handle errors or exceptions
- Modify requests or responses
- Manage headers, sessions, or CORS

---

## ✅ Purpose of This Middleware

The main purpose of this middleware is to provide **basic but secure access control** through JWT-based authentication. Specifically, it:

- 🔐 **Authenticates**: Verifies the validity of a JWT sent in the request (usually in the Authorization header).
- 🔓 **Authorizes**: Checks whether the user has the required role or permissions to access a specific route or resource.

---

## 🔧 How It Works

1. The client sends a request with a JWT in the `Authorization` header (e.g., `Bearer <token>`).
2. The middleware:
   - Verifies the token's validity and signature.
   - Decodes the token to extract user data (e.g., ID, roles).
   - Checks if the user has permission to access the endpoint.
3. If the token is invalid or permissions are insufficient, the request is rejected with the appropriate HTTP status (`401 Unauthorized` or `403 Forbidden`).

---

## 🛠️ Technologies Used

- **JSON Web Tokens (JWT)** for token generation and validation
- Written in **[C# , .NET 8.0]**
- Lightweight, reusable, and easily integrated into any route-based app
- Deployed as a **NuGet package** for easy integration into other projects

---

## 📦 Deployment via NuGet

This middleware is packaged and deployed as a **NuGet package**, making it easy to integrate into any **C# .NET Core** project. To install the middleware package in your project:
