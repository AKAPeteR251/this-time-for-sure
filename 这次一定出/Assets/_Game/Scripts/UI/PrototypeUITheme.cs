using UnityEngine;

namespace ThisTime.Game
{
    [CreateAssetMenu(menuName = "这次一定出/UI主题", fileName = "PrototypeUITheme")]
    public sealed class PrototypeUITheme : ScriptableObject
    {
        public Color background = new Color(0.055f, 0.065f, 0.085f);
        public Color panel = new Color(0.105f, 0.12f, 0.15f);
        public Color button = new Color(0.20f, 0.24f, 0.29f);
        public Color primary = new Color(0.76f, 0.65f, 0.43f);
        public Color text = new Color(0.94f, 0.93f, 0.89f);
        public Color muted = new Color(0.64f, 0.68f, 0.72f);
        public Color rare = new Color(0.39f, 0.64f, 0.79f);
        public Color superRare = new Color(0.64f, 0.49f, 0.83f);
        public Color ssr = new Color(0.86f, 0.71f, 0.40f);
        public Color ur = new Color(0.95f, 0.80f, 0.52f);
        [Header("U02-ART · 独立美术引用")]
        public Sprite summonBackground;
        public Sprite summonCharacter;
        public Sprite buttonNormal;
        public Sprite buttonPrimary;
        public Sprite informationPanel;
        public Sprite tooltipPanel;
        public Sprite frameR;
        public Sprite frameSR;
        public Sprite frameSSR;
        public Sprite frameUR;
        [Header("F10 角色卡面 · 共用1024×1536坐标")]
        public Sprite cardFrameR, cardFrameSR, cardFrameSSR, cardFrameUR;
        public Sprite cardBadgeR, cardBadgeSR, cardBadgeSSR, cardBadgeUR;
        public Sprite cardNewMask, cardNewFill, cardNameShade, cardNameOrnament;
        public float cardNewSpeed = .36f, cardNewBlink = .35f, cardUrSpeed = .12f;
        public bool HasCharacterCards { get { return cardFrameR != null && cardBadgeR != null; } }
        public Sprite CardFrame(CardRarity rarity) { return rarity == CardRarity.UR ? cardFrameUR : rarity == CardRarity.SSR ? cardFrameSSR : rarity == CardRarity.SR ? cardFrameSR : cardFrameR; }
        public Sprite CardBadge(CardRarity rarity) { return rarity == CardRarity.UR ? cardBadgeUR : rarity == CardRarity.SSR ? cardBadgeSSR : rarity == CardRarity.SR ? cardBadgeSR : cardBadgeR; }
        public Sprite currencyIcon;
        public Sprite summonIcon;
        public Sprite collectionIcon;
        public Sprite elementIce;
        public Sprite elementLight;
        public Sprite elementWind;
        public Sprite elementFire;
        public Sprite elementThunder;
        public Sprite sparkleBlue;
        public Sprite sparkleGold;
        [Header("总览人物裁切 · 左下原点归一化")]
        public Rect summaryPortraitCrop = new Rect(0.175f, 0.50f, 0.65f, 0.50f);
        [Header("结算头像裁切 · 左下原点归一化")]
        public Rect summaryFaceCrop = new Rect(.18f,.56f,.64f,.42f);
        public float summaryUrSpeed = .35f, summaryUrBlink = .32f;
        public float summaryAuraStrength=.75f, summaryAuraWidth=.28f;
        // 每档外画布不同；以下为归一化内窗 (x, y, width, height)，左下原点。
        public Rect windowR = new Rect(0.12f, 0.14f, 0.76f, 0.74f);
        public Rect windowSR = new Rect(0.12f, 0.14f, 0.76f, 0.74f);
        public Rect windowSSR = new Rect(0.15f, 0.18f, 0.70f, 0.65f);
        public Rect windowUR = new Rect(0.15f, 0.18f, 0.70f, 0.65f);

        public Sprite Frame(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.SR: return frameSR;
                case CardRarity.SSR: return frameSSR;
                case CardRarity.UR: return frameUR;
                default: return frameR;
            }
        }

        public Sprite ElementIcon(string id)
        {
            switch (id)
            {
                case "SSR001": return elementIce;
                case "SR001": return elementLight;
                case "SR002": return elementWind;
                case "UR001": return elementFire;
                case "SR003": return elementThunder;
                case "R001": return currencyIcon;
                default: return null;
            }
        }

        public Rect Window(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.SR: return windowSR;
                case CardRarity.SSR: return windowSSR;
                case CardRarity.UR: return windowUR;
                default: return windowR;
            }
        }

        public Color Rarity(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.SR: return superRare;
                case CardRarity.SSR: return ssr;
                case CardRarity.UR: return ur;
                default: return rare;
            }
        }
    }
}
