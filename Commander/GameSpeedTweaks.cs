using Kingmaker;
using UnityEngine;

namespace Commander
{
    internal static class GameSpeedTweaks
    {
        private static bool _wasActive, _isOn;

        public static void OnGUI()
        {
            var isActive = Event.current.keyCode == KeyCode.F;

            if (isActive && !_wasActive)
            {
                Toggle();
            }

            _wasActive = isActive;
        }

        private static void Toggle()
        {
            if (_isOn)
            {
                TurnOff();
                _isOn = false;
            }
            else
            {
                TurnOn(); 
                _isOn = true;
            }
        }

        private static void TurnOn()
        {
            Game.Instance.TimeController.DebugTimeScale = 0.5f;
        }

        private static void TurnOff()
        {
            Game.Instance.TimeController.DebugTimeScale = 1f;
        }
    }
}
