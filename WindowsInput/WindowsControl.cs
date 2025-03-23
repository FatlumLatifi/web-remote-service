

using System.Windows.Input;
using WebRemote.Models;
using WindowsInput;
using WindowsInput.Native;

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
                    _input.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    break;
                case "BackSpace":
                    _input.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    break;
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
