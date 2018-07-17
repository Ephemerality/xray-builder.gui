namespace XRayBuilderGUI
{
    public interface IProgressBar
    {
        void Add(int value);
        void Set(int value);
        void SetMax(int max);
        void Set(int value, int max);
    }
}