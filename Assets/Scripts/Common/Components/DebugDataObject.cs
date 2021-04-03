using Common.DataModels.Debug;
using UnityEngine;

namespace Common.Components
{
    public class DebugDataObject : MonoBehaviour
    {
        public delegate void DebugDataEvent(DebugData debugData);
        public static event DebugDataEvent DebugDataCollected;

        private Driver _driver => GetComponent<Driver>();

        public void Update()
        {
            if(_driver == null)
            {
                return;
            }

            DebugDataCollected?.Invoke(_driver.CollectDebugData());
        }
    }
}