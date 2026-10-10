using UnityEditor;
using UnityEngine;

namespace TCG.Foundation.Editor
{
    // Shared entry point is owned by the coordinator; specialists own their suites.
    public static class CoordinationPilotChecks
    {
        [MenuItem("TCG/Base/Validar piloto de coordenacao")]
        public static void Run()
        {
            CollectionChecks.Run();
            NetworkChecks.Run();
            CoordinationRulesChecks.Run();
            CoordinationNetworkChecks.Run();
            Debug.Log("COORDINATION PILOT INTEGRATION PASSED");
        }
    }
}
