using System;
using Android.Views;

namespace xnano.Droid
{
    public interface IGamepadEventHandler
    {
        void OnButtonEvent(Keycode keyCode, KeyEvent e, bool pressed);
        void OnMotionEvent(MotionEvent e);
    }
}
