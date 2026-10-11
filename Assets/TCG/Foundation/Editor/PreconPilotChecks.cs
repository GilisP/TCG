using UnityEngine;
namespace TCG.Foundation.Editor
{
    public static class PreconPilotChecks
    {
        public static void Run(){PreconLibraryChecks.Run();PreconUnitChecks.Run();PreconAuthorChecks.Run();PreconstructedDeckChecks.Run();PreconIntegrationChecks.Run();Debug.Log("PRECON MECHANICS CHECKS PASSED");}
    }
}
