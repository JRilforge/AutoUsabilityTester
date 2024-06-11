using AutoTester.domain;
using Microsoft.Playwright;

namespace AutoTester.util;

public class PlayWrightUtils
{
    public static async void executeInteractions(NextInteractions nextInteractions, IPage page)
    {
        foreach (var step in nextInteractions.instructions)
        {
            switch (step.command)
            {
                case "click":
                {
                    // Perform actions on the page (e.g., click a button)
                    await page.ClickAsync(step.selector);
                    break;
                }
                case "type":
                {
                    await page.FillAsync(step.selector, step.text);
                    break;
                }
            }
        }
    }
}