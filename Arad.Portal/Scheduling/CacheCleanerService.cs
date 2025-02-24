using Arad.Portal.Helpers.UI;
using System.Threading;

namespace Arad.Portal.Scheduling;

public class CacheCleanerService(SharedRuntimeData provider)
{
    private Timer _timer;

    public void StartTimer()
    {
        TimerCallback cb = new(DictionaryReview);
        _timer = new(cb, null, 1000, 20 * 1000 * 60);
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private void DictionaryReview(object state)
    {
        provider.DeleteAllUnusedData();
    }
}