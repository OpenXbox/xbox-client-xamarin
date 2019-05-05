using System;
using System.Collections.Generic;
using Android.Views;

using SmartGlass.Nano;

namespace xnano.Droid.Gamestream
{
    public static class InputHelper
    {
        public static Dictionary<Keycode, NanoGamepadButton> ButtonMap =
            new Dictionary<Keycode, NanoGamepadButton>()
            {
                {Keycode.ButtonA, NanoGamepadButton.A},
                {Keycode.ButtonB, NanoGamepadButton.B},
                {Keycode.ButtonX, NanoGamepadButton.X},
                {Keycode.ButtonY, NanoGamepadButton.Y},
                {Keycode.DpadUp, NanoGamepadButton.DPadUp},
                {Keycode.DpadDown, NanoGamepadButton.DPadDown},
                {Keycode.DpadLeft, NanoGamepadButton.DPadLeft},
                {Keycode.DpadRight, NanoGamepadButton.DPadRight},
                {Keycode.ButtonThumbl, NanoGamepadButton.LeftThumbstick},
                {Keycode.ButtonThumbr, NanoGamepadButton.RightThumbstick},
                {Keycode.ButtonL1, NanoGamepadButton.LeftShoulder},
                {Keycode.ButtonR1, NanoGamepadButton.RightShoulder},
                {Keycode.ButtonStart, NanoGamepadButton.Start},
                {Keycode.ButtonSelect, NanoGamepadButton.Back},
                {Keycode.ButtonMode, NanoGamepadButton.Guide}
            };

        // Source: https://developer.android.com/training/game-controllers/controller-input.html#joystick
        public static float GetCenteredAxis(MotionEvent e, InputDevice device, Axis axis, int historyPos)
        {
            InputDevice.MotionRange range = device.GetMotionRange(axis, e.Source);

            // A joystick at rest does not always report an absolute position of
            // (0,0). Use the getFlat() method to determine the range of values
            // bounding the joystick axis center.
            if (range != null)
            {
                float flat = range.Flat;
                float value = historyPos < 0
                              ? e.GetAxisValue(axis):
                                e.GetHistoricalAxisValue(axis, historyPos);

                // Ignore axis values that are within the 'flat' region of the
                // joystick axis center.
                if (Math.Abs(value) > flat)
                {
                    return value;
                }
            }
            return 0;
        }
    }
}
