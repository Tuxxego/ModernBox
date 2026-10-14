namespace ModernBoxM2Rewrite
{
    /// Cross-feature entry points the tab buttons call. The space workstream fills these in.
    internal static class M2Hooks
    {
        /// Star Map button ("galaxy").
        internal static void OpenStarMap()
        {
            M2Space.OpenStarMap();
        }
    }

    /// Placeholder until the space port lands.
    internal static partial class M2Space
    {
        static partial void OpenStarMapImpl(ref bool handled);

        internal static void OpenStarMap()
        {
            bool handled = false;
            OpenStarMapImpl(ref handled);
        }
    }
}
