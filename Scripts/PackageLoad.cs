using UnityEngine;

namespace UPRProfiler
{
    public class PackageLoad : MonoBehaviour
    {
        public static bool useLua = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void OnStartGame()
        {
            GameObject uprGameObject = new GameObject("UPRProfiler", typeof(InnerPackageS))
            {
                name = "UPRProfiler",
                hideFlags = HideFlags.HideAndDontSave,
            };
            DontDestroyOnLoad(uprGameObject);
            NetworkServer.ConnectTcpPort(56000);
            Debug.Log("[UPRProfiler] PackageLoad OnStartGame");
        }
    }
}

