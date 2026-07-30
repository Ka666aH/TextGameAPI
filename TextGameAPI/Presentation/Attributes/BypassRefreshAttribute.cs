namespace TextGame.Presentation.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class BypassRefreshAttribute : Attribute { }
}