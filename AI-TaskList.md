# AI Task List

## Next Steps for QueryForge

1. **Implement Include System for Update Queries:**
   - Extend the `UpdateQuery` class to support an `Include` method similar to the `SelectQuery`.
   - Ensure that nested updates, such as updating related entities like `Inventory.Item`, are handled correctly.
   - Create unit tests to verify that related entities are updated when a parent entity, like `Player`, is updated.
    ``
   var updatePlayerQuery = new UpdateQuery<Player>(this.context, "Player", player)
     .Include("Inventory") 
      .Where("Id = 1");
     // this should update player , and every inventory record , but not touch the item table
   ``
2. **Enhance Query Capabilities:**
   - Add support for complex query conditions and joins in both `SelectQuery` and `UpdateQuery`.
   - Implement transaction support to ensure data integrity during batch operations.

3. **Improve Documentation and Examples:**
   - Expand the README with more detailed examples and advanced usage scenarios.
   - Ensure all public methods and classes are well-documented with XML comments.

4. **Refactor and Optimize Codebase:**
   - Review and refactor code for better readability and maintainability.
   - Remove any deprecated or unused code.
   - Implement caching mechanisms to improve query performance.

5. **Community Engagement:**
   - Set up guidelines for contributing to the project.
   - Encourage community feedback and feature requests.

6. **Testing and Quality Assurance:**
   - Increase test coverage to ensure reliability and stability.
   - Implement continuous integration and deployment pipelines.

7. **Bug Fixes and Error Handling:**
   - Address any known issues or bugs reported by users.
   - Improve error handling and logging for better debugging.

This roadmap will guide the development of QueryForge to enhance its functionality and usability. Contributions and feedback are welcome to help shape the future of the project.
