using System;
using Android.Views;

namespace xnano.Droid.Gamestream
{
    public static class InputHelper
    {
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
