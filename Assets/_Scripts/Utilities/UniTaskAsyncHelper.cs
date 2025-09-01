using System.Threading;
using Cysharp.Threading.Tasks;

namespace _Utilities
{
    public static class UniTaskAsyncHelper
    {
        public delegate bool ConditionHandler();

        public static async UniTask WaitWhile(ConditionHandler condition, int pollingRateMs = 50, bool ignoreTimeScale = false,
            CancellationToken token = default)
        {
            while (condition.Invoke())
            {
                await UniTask.Delay(pollingRateMs, cancellationToken: token, ignoreTimeScale: ignoreTimeScale);
            }
        }

        public static async UniTask WaitUntil(ConditionHandler condition, int pollingRateMs = 50, bool ignoreTimeScale = false,
            CancellationToken token = default)
        {
            while (!condition.Invoke())
            {
                await UniTask.Delay(pollingRateMs, cancellationToken: token, ignoreTimeScale: ignoreTimeScale);
            }
        }

        public static async UniTask WaitWhileOrTimeout(ConditionHandler condition, int pollingRateMs = 50, bool ignoreTimeScale = false,
            int timeoutMs = 5000, CancellationToken token = default)
        {
            await UniTask.WhenAny(WaitWhile(condition, pollingRateMs, ignoreTimeScale, token),
                UniTask.Delay(timeoutMs, cancellationToken: token));
        }

        public static UniTask GetCancellationTokenTask(CancellationToken cancelToken)
        {
            return UniTask.Run(() => { cancelToken.WaitHandle.WaitOne(); },
                cancellationToken: cancelToken);
        }
    }
}