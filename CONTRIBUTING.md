# Contributing to ShapeSpecs

Thank you for considering contributing to ShapeSpecs! This document provides guidelines for contributing to the project.

## Code of Conduct

Please be respectful and professional in all interactions related to this project.

## Development Setup

### Prerequisites

- Visual Studio 2019 or newer with:
  - .NET Desktop Development workload
  - Office/SharePoint Development workload
- Microsoft Visio 2016 or newer (32-bit or 64-bit)
- .NET Framework 4.7.2 or newer

### Getting Started

1. Fork the repository
2. Clone your fork locally
3. Open `ShapeSpecs.sln` in Visual Studio
4. Restore NuGet packages
5. Build the solution

## Coding Standards

### General Guidelines

- Follow C# naming conventions (PascalCase for public members, camelCase for private fields with underscore prefix)
- Write XML documentation comments for all public classes, methods, and properties
- Keep methods focused and concise (Single Responsibility Principle)
- Use meaningful variable and method names
- Avoid magic numbers - use named constants

### Design Principles

Follow YAGNI + SOLID + DRY + KISS:

- **YAGNI** (You Aren't Gonna Need It): Don't add functionality until it's needed
- **SOLID**: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **DRY** (Don't Repeat Yourself): Avoid code duplication
- **KISS** (Keep It Simple, Stupid): Prefer simple solutions over complex ones

### Code Style

- Use 4 spaces for indentation (not tabs)
- Place opening braces on the same line for methods and control structures
- Use `var` when the type is obvious from the right side
- Prefer explicit types when it improves readability
- Limit line length to 120 characters where practical

### Error Handling

- Use specific exception types
- Provide meaningful error messages
- Log errors appropriately
- Don't swallow exceptions unless you have a good reason

## Testing

### Unit Tests

- Write unit tests for all new functionality
- Maintain or improve code coverage
- Use NUnit framework
- Follow Arrange-Act-Assert pattern
- Use descriptive test names that explain what is being tested

### Running Tests

```bash
# In Visual Studio
Test > Run All Tests

# Or use Test Explorer
View > Test Explorer
```

## Submitting Changes

### Pull Request Process

1. Create a feature branch from `main`
2. Make your changes
3. Write or update tests as needed
4. Update documentation if required
5. Ensure all tests pass
6. Commit your changes with clear, descriptive commit messages
7. Push to your fork
8. Submit a pull request to the main repository

### Commit Messages

- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor to..." not "Moves cursor to...")
- Limit the first line to 72 characters
- Reference issues and pull requests when relevant

Examples:
```
Add async file copy support to FileHelper

Fix duplicate ribbon initialization bug

Update StorageService error handling to distinguish error types
```

### Pull Request Guidelines

- Provide a clear description of the changes
- Reference any related issues
- Include screenshots for UI changes
- Ensure CI builds pass
- Request review from maintainers

## Project Structure

```
ShapeSpecs/
├── ShapeSpecs.Core/          - Business logic and data models
│   ├── Models/               - Data models
│   ├── Services/             - Core services
│   └── Utilities/            - Helper classes
├── ShapeSpecs.UI/            - User interface components
│   ├── Forms/                - UI forms
│   └── Ribbon/               - Ribbon UI
├── ShapeSpecs.Addin/         - VSTO integration
└── ShapeSpecs.Core.Tests/    - Unit tests
```

## Architecture Decisions

- The Core layer has no UI dependencies
- Services use dependency injection
- Storage uses JSON for metadata serialization
- File attachments are stored externally with relative paths

## Questions?

If you have questions about contributing, please:

1. Check existing documentation
2. Search closed issues for similar questions
3. Open a new issue with the "question" label

## License

By contributing to ShapeSpecs, you agree that your contributions will be licensed under the MIT License.
