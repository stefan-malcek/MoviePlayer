namespace MoviePlayer.Services.Interfaces
{
    public interface ICameraService
    {
        bool IsRunning { get; }
        void Run();
        void Cancel();
    }
}
