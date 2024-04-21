namespace Utilities.Launcher;

public interface IRunnable
{
    void Run();

    static void Run<T>() where T: IRunnable, new()
    {
        new T().Run();
    }
}
