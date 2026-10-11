using UnityEngine;
using UnityEngine.EventSystems;
namespace ThisTime.Game
{
    // Down 属于哪个阶段，由它自己持有；水晶的 Up 不得穿透到角色。
    public sealed class RevealPointerGate : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
    {
        public PrototypeGame owner;
        private SummonFlowStage pressedStage;
        private int pressedIndex;
        public void OnPointerDown(PointerEventData e)
        {
            if(e.button!=PointerEventData.InputButton.Left)return;
            pressedStage=owner.FlowStage;pressedIndex=owner.RevealIndex;
            if(pressedStage==SummonFlowStage.CrystalCharging&&owner.CrystalView.Contains(e.position,e.pressEventCamera))owner.HitCrystal();
        }
        public void OnPointerClick(PointerEventData e)
        {
            if(e.button!=PointerEventData.InputButton.Left)return;
            if(pressedStage==SummonFlowStage.CrystalCharging||pressedStage==SummonFlowStage.CrystalBreaking)return;
            if(pressedStage==owner.FlowStage&&pressedIndex==owner.RevealIndex)owner.AdvancePresentation();
        }
    }
}
