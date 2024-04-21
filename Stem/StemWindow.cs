using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace Stem;

public class StemWindow : GameWindow
{
    internal StemWindow(IStemInstance stemInstance, GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings)
    {
        StemInstance = stemInstance;
    }

    internal IStemInstance StemInstance { get; private init; }

    private const double MAX_DT = 0.1;

    protected override void OnLoad()
    {
        // TODO: This stuff should be in rules!
        GL.DepthFunc(DepthFunction.Lequal);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        CenterWindow();

        base.OnLoad();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        StemInstance.ModifyState((store, state) =>
        {
            state.Set(KeyboardState);
            state.Set(MouseState);
        });

        if (!paused || KeyboardState.IsKeyPressed(Keys.Space) || (KeyboardState.IsKeyDown(Keys.Space) && KeyboardState.IsKeyDown(Keys.LeftShift)))
        {
            StemInstance.ExecuteUpdateRules(Math.Min(args.Time, MAX_DT));
        }

        base.OnUpdateFrame(args);
    }

    public bool paused = false;

    public float magnification = 1f;

    public Vector2 center = new(0, 0);

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        StemInstance.ModifyState((store, state) =>
        {
            state.Set(KeyboardState);
            state.Set(MouseState);
        });

        // TODO: Move this junk into some built-in engine rules
        if (KeyboardState.IsKeyDown(Keys.Escape) && KeyboardState.IsKeyDown(Keys.LeftShift))
        {
            Close();
            return;
        }
        if (KeyboardState.IsKeyDown(Keys.LeftControl))
        {
            if (KeyboardState.IsKeyDown(Keys.KeyPadAdd) || MouseState.ScrollDelta.Y > 0)
            {
                magnification *= 1.1f;
            }
            if (KeyboardState.IsKeyDown(Keys.KeyPadSubtract) || MouseState.ScrollDelta.Y < 0)
            {
                magnification *= 1f / 1.1f;
            }
            if (KeyboardState.IsKeyDown(Keys.KeyPad0))
            {
                magnification = 1f;
            }
            if (IsKeyPressed(Keys.Enter))
            {
                paused = !paused;
            }
        }

        if (KeyboardState.IsKeyDown(Keys.F1))
        {
            var moveSpeed = KeyboardState.IsKeyDown(Keys.F2) ? 10 : 1;
            if (KeyboardState.IsKeyDown(Keys.Left))
            {
                Bounds = Bounds.Translated(new() { X = -moveSpeed, Y = 0 });
            }
            if (KeyboardState.IsKeyDown(Keys.Right))
            {
                Bounds = Bounds.Translated(new() { X = moveSpeed, Y = 0 });
            }
            if (KeyboardState.IsKeyDown(Keys.Up))
            {
                Bounds = Bounds.Translated(new() { X = 0, Y = -moveSpeed });
            }
            if (KeyboardState.IsKeyDown(Keys.Down))
            {
                Bounds = Bounds.Translated(new() { X = 0, Y = moveSpeed });
            }
        }

        
        if (MouseState.IsButtonDown(MouseButton.Middle))
        {
            if (MouseState.WasButtonDown(MouseButton.Middle))
            {
                if (KeyboardState.IsKeyDown(Keys.F1))
                {
                    __boundsChange = __boundsChange is null ? 
                        new Vector2i((int) Math.Floor(MouseState.Delta.X), (int) Math.Floor(MouseState.Delta.Y))
                        : new Vector2i((int) Math.Floor(MouseState.Delta.X), (int) Math.Floor(MouseState.Delta.Y)) + __boundsChange;
                    Bounds = Bounds.Translated(__boundsChange.Value);
                } else
                {
                    center -= new Vector2(MouseState.Delta.X, -MouseState.Delta.Y) / magnification;
                }
            }
        } else
        {
            __boundsChange = null;
        }

        StemInstance.ExecuteRenderRules(Math.Min(args.Time, MAX_DT));

        SwapBuffers();

        base.OnRenderFrame(args);
    }

    Vector2i? __boundsChange = null;

    protected override void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, e.Width, e.Height);
        base.OnResize(e);
    }
}
