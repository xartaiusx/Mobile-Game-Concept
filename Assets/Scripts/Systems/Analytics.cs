using UnityEngine;
using Game.Rhythm;

namespace Game.Systems
{
    /// <summary>
    /// Minimal analytics hook. Replace Debug.Log with your telemetry backend later.
    /// </summary>
    public class Analytics : MonoBehaviour
    {
        public static void LogBeat(RhythmGrade grade, int comboLength)
        {
            Debug.Log($"Analytics Beat grade={grade} combo={comboLength}");
        }

        public static void LogSessionStart()
        {
            Debug.Log("Analytics Session Start");
        }

        public static void LogSessionEnd()
        {
            Debug.Log("Analytics Session End");
        }
    }
}
