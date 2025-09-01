using Cysharp.Threading.Tasks;

namespace GameCore.BuffSystem
{
    public interface IDebuffable
    {
        UniTask ApplyDebuff(Debuff debuff);
    }
}
