using UnityEngine;
using UnityEngine.UI;
namespace ThisTime.Game
{
    // Preserve the authored UR frame; tint its highlights gently without changing the portrait.
    [RequireComponent(typeof(Image))]
    public sealed class CardIridescence : MonoBehaviour
    {
        public float speed = .12f;
        private Image image;
        private void Awake() { image = GetComponent<Image>(); }
        private void Update()
        {
            var hue = Color.HSVToRGB(Mathf.Repeat(Time.unscaledTime * speed,1),.32f,1);
            image.color = Color.Lerp(Color.white,hue,.65f);
        }
    }
}
