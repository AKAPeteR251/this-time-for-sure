using System.Collections.Generic;

namespace ThisTime.Game
{
    // UI 使用已有共享模型。正式规则到位后只替换适配器。
    public interface IPrototypeUIAdapter
    {
        List<PrototypeCard> Cards { get; }
        int Currency { get; }
        int TotalPulls { get; }
        int CollectedCharacters { get; }
        int CountOf(string id);
        List<PrototypePull> Draw(int count);
        void Refill();
        string RulesDescription { get; }
    }

    public sealed class MemoryPrototypeUIAdapter : IPrototypeUIAdapter
    {
        private readonly PrototypeGacha source = new PrototypeGacha();
        public List<PrototypeCard> Cards { get { return source.Cards; } }
        public int Currency { get { return source.Currency; } }
        public int TotalPulls { get { return source.TotalPulls; } }
        public int CollectedCharacters { get { return source.CollectedCharacters; } }
        public int CountOf(string id) { return source.CountOf(id); }
        public List<PrototypePull> Draw(int count) { return source.Draw(count); }
        public void Refill() { source.Refill(); }
        public string RulesDescription { get { return
            "当前原型 · 基础随机\n\nR 70%    /    SR 24%    /    SSR 5%    /    UR 1%\n\n"
            + "先决定稀有度，同稀有度角色等概率。\n单次消耗 1 份资源，十次消耗 10 份。\n免费补充每次 +10，无等待。\n\n"
            + "保底、新手赠礼、共鸣与玄学尚未接入。\n收藏仅保留在本次运行，退出后重置。\n\nSSR / UR 预览不消耗资源，也不计入收藏。"; } }
    }

    // 独立 UI 场景固定序列，无随机计算，不进入正式收藏。
    public sealed class FixedPrototypeUIAdapter : IPrototypeUIAdapter
    {
        private readonly List<PrototypeCard> cards = new PrototypeGacha().Cards;
        private readonly Dictionary<string, int> owned = new Dictionary<string, int>();
        public List<PrototypeCard> Cards { get { return cards; } }
        public int Currency { get; private set; } = 30;
        public int TotalPulls { get; private set; }
        public int CollectedCharacters { get { return owned.Count; } }
        public int CountOf(string id) { int n; return owned.TryGetValue(id, out n) ? n : 0; }
        public void Refill() { Currency += 10; }
        public List<PrototypePull> Draw(int count)
        {
            var result = new List<PrototypePull>();
            for (int i = 0; i < count; i++)
            {
                PrototypeCard card = cards[(TotalPulls + i) % cards.Count];
                int n = CountOf(card.Id);
                result.Add(new PrototypePull { Card = card, IsNew = n == 0 });
                owned[card.Id] = n + 1;
            }
            Currency -= count;
            TotalPulls += count;
            return result;
        }
        public string RulesDescription { get { return
            "UI MOCK · 固定数据\n\n按现有角色表顺序返回结果，不使用随机概率。\n\n"
            + "资源与收藏只存在于此 UI 场景。\n用于检查 NEW、重复卡、未收录与资源不足。\n\n"
            + "保底、赠礼、共鸣与存档未接入。\n正式运行请关闭 Use Fixed Mock。"; } }
    }
}
