using System;
using System.Diagnostics;
using System.Windows.Input;
using Android.Views;

using Xamarin.Forms;

using SmartGlass.Nano;
using SmartGlass.Nano.Packets;

namespace xnano.Droid.Gamestream
{
    public class InputHandler : IGamepadEventHandler, IDisposable
    {
        private bool _disposed;

        private readonly NanoClient _nanoClient;

        InputButtons _buttons;
        InputAnalogue _analog;
        InputExtension _extension;

        ICommand SendFrameCommand;

        public InputHandler(NanoClient nano)
        {
            _nanoClient = nano;

            _buttons = new InputButtons();
            _analog = new InputAnalogue();
            _extension = new InputExtension
            {
                Unknown1 = 1
            };

            SendFrameCommand = new Command(async () =>
            {
                if (_nanoClient.Input == null)
                {
                    Debug.WriteLine("SendFrame: InputChannel not ready");
                    return;
                }

                await _nanoClient.Input.SendInputFrame(DateTime.Now,
                    _buttons, _analog, _extension);
            });
        }

        bool HandleButtonPress(Keycode keyCode, bool pressed)
        {
            if (!InputHelper.ButtonMap.TryGetValue(keyCode, out NanoGamepadButton button))
                return false;

            _buttons.ToggleButton(button, pressed);
            SendFrameCommand.Execute(null);

            return true;
        }

        public void OnButtonEvent(Keycode keyCode, KeyEvent e, bool pressed)
        {
            HandleButtonPress(keyCode, pressed);
        }

        public void OnMotionEvent(MotionEvent e)
        {
            return;
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