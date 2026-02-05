using WebRemote.Models;
using WindowsInput;

namespace WindowsInputRemote
{
    public class WindowsControl : IWebRemoteControl
    {
        public WindowsControl()
        {
            _input = new InputSimulator();
        }

        internal InputSimulator _input = new();


        public void Dispose()
        {

        }
  
        public void SendKey(int keycode)
        {
            switch (keycode)
            {
                case 14:   // BackSpace
                    _input.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    break;

                case 28:   // Return
                    _input.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    break;

                case 87:   // F11
                    _input.Keyboard.KeyPress(VirtualKeyCode.F11);
                    break;

                case 113:  // XF86AudioMute
                    _input.Keyboard.KeyPress(VirtualKeyCode.VOLUME_MUTE);
                    break;

                case 114:  // XF86AudioLowerVolume
                    _input.Keyboard.KeyPress(VirtualKeyCode.VOLUME_DOWN);
                    break;

                case 115:  // XF86AudioRaiseVolume
                    _input.Keyboard.KeyPress(VirtualKeyCode.VOLUME_UP);
                    break;

                case 133:  // XF86Copy
                    _input.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                    break;

                case 135:  // XF86Paste
                    _input.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
                    break;

                case 150:  // XF86WWW
                    _input.Keyboard.KeyPress(VirtualKeyCode.BROWSER_HOME);
                    break;

                case 163:  // XF86AudioNext
                    _input.Keyboard.KeyPress(VirtualKeyCode.MEDIA_NEXT_TRACK);
                    break;

                case 164:  // XF86AudioPlay
                    _input.Keyboard.KeyPress(VirtualKeyCode.MEDIA_PLAY_PAUSE);
                    break;

                case 165:  // XF86AudioPrev
                    _input.Keyboard.KeyPress(VirtualKeyCode.MEDIA_PREV_TRACK);
                    break;

                case 226:  // XF86AudioMedia
                    _input.Keyboard.KeyPress(VirtualKeyCode.LAUNCH_MEDIA_SELECT);
                    break;

                default:
                    return;
            }
        }


        public void TypeText(string text)
        {

            _input.Keyboard.TextEntry(text);
        }


            public void MoveBy(int x, int y)
        {
            _input.Mouse.MoveMouseBy(x, y);
        }

        public void SendClick(int button)
        {
            switch (button)
            {
                case 1:
                    _input.Mouse.LeftButtonClick();
                    break;
                case 2:
                    _input.Mouse.MiddleButtonClick();
                    break;
                case 3:
                    _input.Mouse.RightButtonClick();
                    break;
                default: return;
            }
        }
    }

}
