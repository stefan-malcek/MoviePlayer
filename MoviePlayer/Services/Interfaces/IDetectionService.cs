namespace MoviePlayer.Services.Interfaces
{
    public interface IDetectionService
    {
        bool IsObjectDetected { get; }
        void SetFilter(string filter);
    }
}
