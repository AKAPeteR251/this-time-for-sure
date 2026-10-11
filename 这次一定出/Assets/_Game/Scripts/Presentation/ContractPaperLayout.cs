using UnityEngine;
using UnityEngine.UI;

namespace ThisTime.Game
{
    // F09 的纸、英文和动态签名共享同一等比画布；美术坐标已转换为左下原点。
    public static class ContractPaperLayout
    {
        public static Color BackgroundShade {get{return new Color(.015f,.025f,.07f,.93f);}}
        public static bool Available(P04FeelConfig c) { return c != null && c.contractPaper != null && c.contractEnglishLettering != null; }
        public static RectTransform Fit(RectTransform slot, string name, P04FeelConfig c)
        {
            var paper = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            paper.SetParent(slot, false);
            var fit = paper.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectRatio = c.contractAspect; fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            Layer(paper, "Paper", c.contractPaper, new Vector4(0,0,1,1));
            Layer(paper, "EnglishLettering", c.contractEnglishLettering, new Vector4(0,0,1,1));
            return paper;
        }
        public static Image Layer(Transform parent, string name, Sprite sprite, Vector4 rect)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            r.SetParent(parent, false); Place(r, rect);
            var image = r.gameObject.AddComponent<Image>(); image.sprite = sprite;
            image.color = Color.white; image.raycastTarget = false;
            return image;
        }
        public static void Place(RectTransform r, Vector4 rect)
        { r.anchorMin = new Vector2(rect.x,rect.y); r.anchorMax = new Vector2(rect.z,rect.w); r.offsetMin = r.offsetMax = Vector2.zero; }
    }
}
