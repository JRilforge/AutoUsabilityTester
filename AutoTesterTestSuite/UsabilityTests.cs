using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace AutoTester;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class UsabilityTest : PageTest
{
    [Test]
    public async Task ChatboxLoadsCorrectly()
    {
        await Page.GotoAsync("http://localhost:5267/");

        await Expect(Page).ToHaveTitleAsync("Home");

        var heroTitle = Page.Locator("#title");
        var messenger = Page.Locator("#messenger");

        await Expect(heroTitle).ToHaveTextAsync("AutoTester");
        await Expect(messenger).ToBeVisibleAsync();
    }

    [Test]
    public async Task TaskSubmissionStartsWebsocketConnection()
    {
        
    }

    [Test]
    public async Task TaskIncorrectlySubmittedResultsInValidError()
    {
        
    }

    [Test]
    public async Task TaskSubmissionPerformsActionsAndReportsStates()
    {
        
    }

    [Test]
    public async Task GetStartedLink()
    {
        await Page.GotoAsync("https://playwright.dev");

        // Click the get started link.
        await Page.GetByRole(AriaRole.Link, new() { Name = "Get started" }).ClickAsync();

        // Expects page to have a heading with the name of Installation.
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Installation" })).ToBeVisibleAsync();
    } 
}