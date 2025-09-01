using Cysharp.Threading.Tasks;

namespace GameCore.BuffSystem
{
    public interface IBuffable
    {
        UniTask ApplyBuff(Buff buff);
    }
}
