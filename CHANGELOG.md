# Change Log
All notable changes to this project will be documented in this file.

## v1.0.0-rc.3 - 2017-08-11
### Fixed
- Resolved accept arguments of type `TKey` on `Update`/`Delete` methods (closes [#5](https://github.com/fulls1z3/mongoizer/issue/5))

## v1.0.0-rc.2 - 2017-07-31
### Added
- Added `findOptions` to find/search methods (closes [#4](https://github.com/fulls1z3/mongoizer/issue/4))

### Fixed
- Resolved `connection string` is checked after client initialization (closes [#3](https://github.com/fulls1z3/mongoizer/issue/3))
- Resolved serialization issue from BsonType `ObjectId` to `string` (closes [#2](https://github.com/fulls1z3/mongoizer/issue/2))

### Changed
- Changed `collection` member access level to `public` (closes [#1](https://github.com/fulls1z3/mongoizer/issue/1))

## v1.0.0-beta.1 - 2017-07-13
- Initial release
