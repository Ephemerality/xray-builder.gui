using System;
using System.Threading;
using System.Threading.Tasks;

namespace XRayBuilderGUI.UI.Preview
{
    public interface IPreviewForm : IDisposable
    {
        Task Populate(string filePath, CancellationToken cancellationToken);
        void ShowDialog();
    }
}