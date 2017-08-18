# Mongoizer
> Please support this project by simply putting a Github star. Share this library with friends on Twitter and everywhere else you can.

**`Mongoizer`** is a lightweight library to use [MongoDB] with .NET, allowing basic CRUD operations by implementing the repository pattern on the top of [MongoDB .NET driver].

**NOTE**: This project is in experimental stage now, functionality is subject to slightly change.

You can have a look at **unit tests** for demo usage. Meanwhile, usage instructions will be provided on further releases.

> Built with `.NET Framework v4.6.2`, solution currently supports `MongoDB v3.4`.

## Prerequisites
Packages in this project depend on
- [MongoDB.Driver v2.4.4](https://www.nuget.org/packages/MongoDB.Driver)
- [MongoDB.Driver.Core v2.4.4](https://www.nuget.org/packages/MongoDB.Driver.Core)
- [MongoDB.Bson v2.4.4](https://www.nuget.org/packages/MongoDB.Bson)

> Older versions contain outdated dependencies, might produce errors.

## Getting started
### Installation
You can install **`Mongoizer`** by running following commands in the Package Manager Console
```
Install-Package Mongoizer.Domain -Pre
Install-Package Mongoizer.Core -Pre
```

### Solution architecture
The solution consists of 3 projects
```
Mongoizer Solution
├─ Mongoizer.Domain
│  - Shared domain classes
│  
├─ Mongoizer.Core
│  - Core functionality
│  
└─ Mongoizer.Tests
   - Unit tests, mocks
```

### Running tests
Simply clone this repository and run the tests using `Unit Test Explorer`.

**NOTE**: You should provide the connection settings at the `app.config` before running the tests.

## Contributing
If you want to file a bug, contribute some code, or improve documentation, please read up on the following contribution guidelines:
- [Issue guidelines](CONTRIBUTING.md#submit)
- [Contributing guidelines](CONTRIBUTING.md)
- [Coding rules](CONTRIBUTING.md#rules)
- [ChangeLog](CHANGELOG.md)

## License
The MIT License (MIT)

Copyright (c) 2017 [Burak Tasci]

[MongoDB]: https://www.mongodb.com/
[MongoDB .NET driver]: https://github.com/mongodb/mongo-csharp-driver
[Burak Tasci]: http://www.buraktasci.com
