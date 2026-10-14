namespace TCG.Foundation.Editor
{
    public static class ExpandedChecks
    {
        public static void Run()
        {
            KeywordExpansionChecks.Run();CommanderPresentationChecks.Run();TroopPresentationChecks.Run();ExpandedArtChecks.Run();MulticolorCostChecks.Run();
            UnityEngine.Debug.Log("EXPANSION TARGETED CHECKS PASSED");
        }
    }
}
