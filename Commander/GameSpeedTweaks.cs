using Kingmaker;
using UnityEngine;

namespace Commander
{
    internal static class GameSpeedTweaks
    {
        private static bool _wasActive, _isOn;

        public static void OnGUI()
        {
            if (Event.current == null) { return; }

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
            if (Game.Instance?.TimeController == null) { return; }

            Game.Instance.TimeController.DebugTimeScale = 0.5f;
        }

        private static void TurnOff()
        {
            if (Game.Instance?.TimeController == null) { return; }

            Game.Instance.TimeController.DebugTimeScale = 1f;
        }
    }
}
