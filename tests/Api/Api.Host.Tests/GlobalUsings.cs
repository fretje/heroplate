global using FluentAssertions;
global using Tests.Shared;
global using Xunit;
global using Xunit.Extensions.Ordering;

[assembly: TestCaseOrderer("Xunit.Extensions.Ordering.TestCaseOrderer", "Xunit.Extensions.Ordering")]
[assembly: TestCollectionOrderer("Xunit.Extensions.Ordering.CollectionOrderer", "Xunit.Extensions.Ordering")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1033:Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture", Justification = "DbResetFixture is not needed in the constructor", Scope = "namespaceanddescendants", Target = "~N:Api.Host.Tests")]