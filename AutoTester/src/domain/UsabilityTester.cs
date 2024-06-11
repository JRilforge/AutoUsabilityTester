using Microsoft.Playwright;

namespace AutoTester.domain;

public class UsabilityTester
{
    public IPlaywright playwright { set; get; }
    public IBrowser browser { set; get; }
    public Guid _guid { get; }

    public UsabilityTester(Guid guid)
    {
        this._guid = guid;
    }
}