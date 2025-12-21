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

        public IWebRemoteKeyboard Keyboard => new WindowsKeyboard(_input);

        public IWebRemoteMouse Mouse => new WindowsMouse(_input);

        public void Dispose()
        {
            
        }
    }



    public class WindowsKeyboard : IWebRemoteKeyboard
    {
        public WindowsKeyboard(InputSimulator input)
        {
            _input = input;
        }

        internal InputSimulator _input;

        public void SendKey(string keySequence)
        {
            switch (keySequence)
            {
                case "Return":
                    _input.Keyboard.KeyPress(VirtualKeyCode.RETURN); break;
                case "BackSpace":
                    _input.Keyboard.KeyPress(VirtualKeyCode.BACK); break;
                case "XF86AudioPlay":
                    _input.Keyboard.KeyPress(VirtualKeyCode.MEDIA_PLAY_PAUSE); break;
                case "XF86AudioPrev":
                    _input.Keyboard.KeyPress(VirtualKeyCode.MEDIA_PREV_TRACK); break;
                case "XF86AudioNext":
                    _input.Keyboard.KeyPress(VirtualKeyCode.MEDIA_NEXT_TRACK); break;
                case "XF86AudioMute":
                    _input.Keyboard.KeyPress(VirtualKeyCode.VOLUME_MUTE); break;
                case "XF86AudioLowerVolume":
                    _input.Keyboard.KeyPress(VirtualKeyCode.VOLUME_DOWN); break;
                case "XF86AudioRaiseVolume":
                    _input.Keyboard.KeyPress(VirtualKeyCode.VOLUME_UP); break;
                case "XF86WWW":
                    _input.Keyboard.KeyPress(VirtualKeyCode.BROWSER_HOME); break;
                case "XF86Copy":
                    _input.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C); break;
                case "XF86AudioMedia":
                    _input.Keyboard.KeyPress(VirtualKeyCode.LAUNCH_MEDIA_SELECT); break;
                case "XF86Paste":
                    _input.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V); break;
                case "F11":
                    _input.Keyboard.KeyPress(VirtualKeyCode.F11); break;
                default: return;
            }
        }

        public void TypeText(string text)
        {

            _input.Keyboard.TextEntry(text);
        }

 
    }


   

    public class WindowsMouse : IWebRemoteMouse
    {

        public WindowsMouse(InputSimulator input)
        {
            _input = input;
        }

        internal InputSimulator _input;

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
