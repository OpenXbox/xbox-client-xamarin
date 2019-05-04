using System;
using Android.Content;
using Android.Runtime;
using Android.Hardware.Input;
using Android.Views;
using System.Collections.Generic;

using SmartGlass.Nano;

namespace xnano.Droid.Gamestream
{
    public class InputHandler : IDisposable
    {
        private bool _disposed;

        private readonly NanoClient _nanoClient;

        public InputHandler(NanoClient nano)
        {
            _nanoClient = nano;
        }

        public bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            return false;
        }

        public bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            return false;
        }

        public bool OnGenericMotionEvent(MotionEvent e)
        {
            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: Cleanup
                }

                _disposed = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }
    }
}