//using OpenTK.Windowing.Desktop;
//using OpenTK.Windowing.GraphicsLibraryFramework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace Sandbox.Launcher;

public unsafe static class GLFWHelper
{
    //public static IGLFWGraphicsContext SharedContext { get; set; }

    //private static unsafe Window* WindowPtr { get; set; }

    public unsafe static void LoadSharedContext()
    {
        //GLFW.Init();
        //
        //var settings = NativeWindowSettings.Default;
        //
        //var monitor = settings.CurrentMonitor.ToUnsafePtr<OpenTK.Windowing.GraphicsLibraryFramework.Monitor>();
        //
        //GLFW.WindowHint(WindowHintBool.Resizable, value: true);
        //
        //GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGlApi);
        //
        //GLFW.WindowHint(WindowHintInt.ContextVersionMajor, settings.APIVersion.Major);
        //GLFW.WindowHint(WindowHintInt.ContextVersionMinor, settings.APIVersion.Minor);
        //GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Any);
        //
        //var videoMode = GLFW.GetVideoMode(monitor);
        //
        //GLFW.WindowHint(WindowHintInt.RefreshRate, videoMode->RefreshRate);
        //
        //WindowPtr = GLFW.CreateWindow(100, 100, "GLFWHelper Window", monitor, (Window*) (void*) IntPtr.Zero);
        //
        //SharedContext = new GLFWGraphicsContext(WindowPtr);
    }
}
