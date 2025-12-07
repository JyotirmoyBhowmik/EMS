# Contributing to Enterprise SCADA System

Thank you for your interest in contributing! This document provides guidelines for contributing to the project.

## Code of Conduct

- Be respectful and inclusive
- Focus on constructive feedback
- Help others learn and grow

## How to Contribute

### Reporting Bugs

1. Check if the bug has already been reported in Issues
2. Create a new issue with:
   - Clear title and description
   - Steps to reproduce
   - Expected vs actual behavior
   - Environment details (OS, Docker version, etc.)
   - Logs if applicable

### Suggesting Enhancements

1. Check if the enhancement has been suggested
2. Create an issue describing:
   - Use case and benefits
   - Proposed implementation (if any)
   - Potential drawbacks

### Pull Requests

1. **Fork the repository**
2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Make your changes**
   - Follow coding standards (see below)
   - Add tests for new functionality
   - Update documentation

4. **Test your changes**
   ```bash
   # Backend tests
   dotnet test

   # Frontend tests
   npm run test

   # Integration tests
   docker-compose up -d
   # Verify all services are healthy
   ```

5. **Commit with clear messages**
   ```bash
   git commit -m "feat: add new alarm notification type"
   git commit -m "fix: resolve database connection timeout"
   git commit -m "docs: update API documentation"
   ```

   Use conventional commits:
   - `feat:` new feature
   - `fix:` bug fix
   - `docs:` documentation
   - `style:` formatting
   - `refactor:` code restructuring
   - `test:` adding tests
   - `chore:` maintenance

6. **Push to your fork**
   ```bash
   git push origin feature/your-feature-name
   ```

7. **Create Pull Request**
   - Provide clear description
   - Reference related issues
   - Include screenshots for UI changes

## Coding Standards

### C# (.NET)

- Follow Microsoft's C# coding conventions
- Use meaningful variable names
- Add XML documentation comments for public APIs
- Use async/await for I/O operations
- Handle exceptions appropriately

```csharp
/// <summary>
/// Retrieves a tag by its unique identifier
/// </summary>
/// <param name="tagId">The tag identifier</param>
/// <returns>Tag entity or null if not found</returns>
public async Task<Tag?> GetTagByIdAsync(int tagId)
{
    return await _dbContext.Tags
        .Include(t => t.AlarmRules)
        .FirstOrDefaultAsync(t => t.Id == tagId);
}
```

### TypeScript/React

- Use functional components with hooks
- TypeScript strict mode enabled
- Props should have defined interfaces
- Use meaningful component names

```typescript
interface TagValueCardProps {
  tag: Tag;
  onUpdate?: (value: number) => void;
}

export const TagValueCard: React.FC<TagValueCardProps> = ({ tag, onUpdate }) => {
  // Component implementation
};
```

### Database

- Use migrations for schema changes
- Add appropriate indexes
- Include comments for complex queries

```sql
-- Create index for frequently queried site/device combinations
CREATE INDEX idx_tags_site_device ON tags(site, device)
WHERE is_enabled = true;
```

## Testing Requirements

### Unit Tests

- Minimum 70% code coverage for new code
- Test happy path and error cases
- Use descriptive test names

```csharp
[Fact]
public async Task CreateTag_WithDuplicateName_ShouldThrowException()
{
    // Arrange
    var tag1 = new Tag { Name = "DUPLICATE.TAG", DataType = "Float" };
    var tag2 = new Tag { Name = "DUPLICATE.TAG", DataType = "Float" };
    
    await _service.CreateTagAsync(tag1);

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => _service.CreateTagAsync(tag2)
    );
}
```

### Integration Tests

- Test service-to-service communication
- Verify database interactions
- Test message queue flows

## Documentation

- Update README.md for user-facing changes
- Update API documentation (Swagger/OpenAPI)
- Add comments for complex logic
- Update administrator guide for configuration changes

## Review Process

1. **Automated Checks**: CI/CD pipeline must pass
   - Build succeeds
   - Tests pass
   - No security vulnerabilities
   - Code coverage meets threshold

2. **Code Review**: At least one maintainer review
   - Code quality and standards
   - Test coverage
   - Documentation
   - Performance implications

3. **Testing**: Manual testing for significant changes
   - Verify functionality
   - Check for regressions
   - Test edge cases

## Release Process

1. Version follows Semantic Versioning (semver)
   - MAJOR: Breaking changes
   - MINOR: New features (backward compatible)
   - PATCH: Bug fixes

2. Changelog updated with changes
3. Tag release in Git
4. Update Docker images
5. Deploy to staging for validation
6. Deploy to production

## Questions?

- Check existing documentation in `docs/`
- Search closed issues
- Ask in discussions
- Contact: jyotirmoy.bhowmik@gmail.com

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.

Thank you for contributing! 🎉
