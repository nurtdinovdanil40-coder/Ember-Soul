using Microsoft.Xna.Framework;

namespace MonoGameLibrary.Input;

public class InputManager
{

    public KeyboardInfo Keyboard { get; private set; }


    public MouseInfo Mouse { get; private set; }

    public InputManager()
    {
        Keyboard = new KeyboardInfo();
        Mouse = new MouseInfo();
    }

    public void Update(GameTime gameTime)
    {
        Keyboard.Update();
        Mouse.Update();
    }

    
}

