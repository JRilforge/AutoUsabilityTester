namespace AutoTester.domain;

/*{
    ""element"": ""****"",
    ""selector"": ""****"",
    ""bounds"": {
        ""x"": ""**x axis coordinate of the element**"",
        ""y"": ""**y axis coordinate of the element**"",
        ""width"": ""**width of the element**"",
        ""height"": ""**height of the element**""
    },
    ""click point"": {
        ""x"": ""**x axis coordinate to click**"",
        ""y"": ""**y axis coordinate to click**""
    },
    ""instructions"": [
        ""**Command for interaction with elements. E.g. click(<css selector|xpath>), type(<text to input>, <css selector|xpath>), etc.**""
    ],
    ""expecting"": ""**describe what you're expecting to see next**"",
    ""thoughts"": ""**detail your thought process (from picking the element to determining the next user interaction)**""
}*/
public class NextInteractions
{
    /**
     * element name and description
     */
    public string element { get; }
    
    /**
     * XPath or Css selector
     */
    public string selector { get; }
    
    /**
     * Element bounds
     */
    public BoundingBox bounds { get; }
    
    /**
     * Commands for interaction with this element
     */
    public IList<CommandInteraction> instructions { get; }

    public string expecting { get; }

    public string thoughts { get; }

    public bool completed { get; }

    public class BoundingBox
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }

    public class ClickPoint
    {
        public int x;
        public int y;
    }

    public class CommandInteraction
    {
        public string command;
        public string selector;
        
        /**
         * text to input into a form field
         */
        public string text;
    }
}