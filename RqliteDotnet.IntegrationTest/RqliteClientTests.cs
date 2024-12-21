using System.Runtime.InteropServices.JavaScript;
using System.Text.RegularExpressions;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using NUnit.Framework;

namespace RqliteDotnet.IntegrationTest;

public class RqliteClientTests
{
    private const int Port = 4001;
    private IContainer _container;
    private HttpClient _httpClient;

    [SetUp]
    public async Task Setup()
    {
        _container = new ContainerBuilder()
            .WithImage("rqlite/rqlite:8.32.7")
            .WithPortBinding(Port, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(Port)))
            .Build();
        
        await _container.StartAsync().ConfigureAwait(false);
        var url = $"http://{_container.Hostname}:{_container.GetMappedPublicPort(Port)}";
        _httpClient = new HttpClient() { BaseAddress = new Uri(url) };
        var content =
            new StringContent("[ \"CREATE TABLE foo (id INTEGER NOT NULL PRIMARY KEY, name TEXT, age INTEGER)\" ]");
        await _httpClient.PostAsync("/db/execute?timings", content);
    }
    
    [Test]
    public async Task RqliteClientPing_Works()
    {
        var url = $"http://{_container.Hostname}:{_container.GetMappedPublicPort(Port)}";
        var client = new RqliteClient(url);

        var version = await client.Ping();
        
        Assert.That(version, Is.Not.Empty);
        Assert.That(Regex.IsMatch(version, @"v\d+.\d+\.*")); //v8.10.1
    }

    [Test]
    public async Task RqliteClient_CanGetInsertData()
    {
        var url = $"http://{_container.Hostname}:{_container.GetMappedPublicPort(Port)}";
        var client = new RqliteClient(url);

        var result = await client.Execute("insert into foo(name, age) VALUES(\\\"john\\\", 42)");
        Assert.That(result!.Results!.Count, Is.EqualTo(1));
        Assert.That(result!.Results[0].RowsAffected, Is.EqualTo(1));
    }
}