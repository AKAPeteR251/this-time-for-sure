using System;
using System.Collections.Generic;

namespace ThisTime.Game
{
    public enum CardRarity { R, SR, SSR, UR }

    public sealed class PrototypeCard
    {
        public string Id;
        public string Name;
        public string Title;
        public CardRarity Rarity;

        public PrototypeCard(string id, string name, string title, CardRarity rarity)
        {
            Id = id;
            Name = name;
            Title = title;
            Rarity = rarity;
        }
    }

    public sealed class PrototypePull
    {
        public PrototypeCard Card;
        public bool IsNew;
    }

    // 首轮只保留内存状态，让抽卡与界面先跑通。
    public sealed class PrototypeGacha
    {
        private readonly Random random = new Random();
        private readonly Dictionary<string, int> collection = new Dictionary<string, int>();

        public int Currency { get; private set; } = 30;
        public int TotalPulls { get; private set; }
        public readonly List<PrototypeCard> Cards = new List<PrototypeCard>
        {
            new PrototypeCard("R001", "星魔法少女", "星辉契约者", CardRarity.R),
            new PrototypeCard("R002", "风铃", "街角奏者", CardRarity.R),
            new PrototypeCard("R003", "米洛", "巡灯人", CardRarity.R),
            new PrototypeCard("R004", "洛恩", "星图学徒", CardRarity.R),
            new PrototypeCard("R005", "浅汐", "月港领航员", CardRarity.R),
            new PrototypeCard("SR001", "光牧师", "圣光祈愿者", CardRarity.SR),
            new PrototypeCard("SR002", "风精灵弓箭手", "风林游侠", CardRarity.SR),
            new PrototypeCard("SR003", "雷巫女", "雷霆祈愿者", CardRarity.SR),
            new PrototypeCard("SR004", "弦月", "镜海歌者", CardRarity.SR),
            new PrototypeCard("SSR001", "冰法师", "冰城守望者", CardRarity.SSR),
            new PrototypeCard("SSR002", "星澜", "长夜引路人", CardRarity.SSR),
            new PrototypeCard("UR001", "火女战士", "炽焰先锋", CardRarity.UR)
        };

        public void Refill() { Currency += 10; }

        public int CountOf(string characterId)
        {
            int count;
            return collection.TryGetValue(characterId, out count) ? count : 0;
        }

        public int CollectedCharacters { get { return collection.Count; } }

        public List<PrototypePull> Draw(int count)
        {
            Currency -= count;
            TotalPulls += count;
            var results = new List<PrototypePull>();
            for (int i = 0; i < count; i++)
            {
                int roll = random.Next(100);
                CardRarity rarity = roll < 70 ? CardRarity.R
                    : roll < 94 ? CardRarity.SR
                    : roll < 99 ? CardRarity.SSR : CardRarity.UR;
                List<PrototypeCard> pool = Cards.FindAll(card => card.Rarity == rarity);
                PrototypeCard selected = pool[random.Next(pool.Count)];
                int owned = CountOf(selected.Id);
                results.Add(new PrototypePull { Card = selected, IsNew = owned == 0 });
                collection[selected.Id] = owned + 1;
            }
            return results;
        }
    }
}
