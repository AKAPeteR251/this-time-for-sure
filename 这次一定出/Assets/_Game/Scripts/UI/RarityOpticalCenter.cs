using UnityEngine;
using UnityEngine.UI;
namespace ThisTime.Game
{
    [RequireComponent(typeof(Text))]
    public sealed class RarityOpticalCenter : MonoBehaviour
    {
        private Text text;
        private void Awake(){text=GetComponent<Text>();}
        private void LateUpdate()
        {
            var mesh=text.canvasRenderer.GetMesh();
            if(mesh==null||mesh.vertexCount==0)return;
            Vector3 centre=mesh.bounds.center;
            text.rectTransform.anchoredPosition=-new Vector2(centre.x,centre.y);
        }
    }
}
