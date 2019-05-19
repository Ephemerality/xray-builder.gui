using System.Threading;
using System.Threading.Tasks;

namespace XRayBuilderGUI.UI.Preview
{
    public interface IPreviewForm
    {
        Task Populate(string filePath, CancellationToken cancellationToken);
        void ShowDialog();
    }
}