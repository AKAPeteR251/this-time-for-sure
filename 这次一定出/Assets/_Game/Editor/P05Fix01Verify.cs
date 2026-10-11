using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ThisTime.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[InitializeOnLoad]
public static class P05Fix01Verify
{
    private const string Flag="P05.Fix01.Play";
    private static bool running;
    private static EditorWindow view;
    private static readonly List<string> checks=new List<string>();
    private static string Reports{get{return Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName,"Docs/Production/P05_FIX01_Verification");}}
    static P05Fix01Verify(){EditorApplication.update+=Poll;EditorApplication.playModeStateChanged+=Changed;}
    private static void Poll()
    {
        if(running){view.Repaint();EditorApplication.QueuePlayerLoopUpdate();}
        string request=Path.Combine(Reports,"verify.request");
        if(!File.Exists(request)||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        File.Delete(request);Verify();
    }
    [MenuItem("Tools/这次一定出/程序/P05-FIX01/验证单抽卡框比例")]
    public static void Verify()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorSceneManager.GetActiveScene().isDirty){Write("blocked","先停止Play并保存场景。");return;}
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/Main.unity");SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();
    }
    private static void Changed(PlayModeStateChange mode)
    {
        if(mode!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Flag,false))return;
        SessionState.SetBool(Flag,false);running=true;checks.Clear();Application.runInBackground=true;
        view=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
        var ui=UnityEngine.Object.FindObjectOfType<PrototypeGame>();Write("running","Main单抽2:3/详情返回/三窗口/十连布局");ui.StartCoroutine(Guard(Check(ui)));
    }
    private static T Get<T>(PrototypeGame ui,string field){return (T)typeof(PrototypeGame).GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(ui);}
    private static IEnumerator Check(PrototypeGame ui)
    {
        yield return null;yield return null;
        var state=Get<IPrototypeUIAdapter>(ui,"game");
        ui.RequestSummon(1);Require(state.Currency==30,"签约前不扣费");ui.SkipSigning();
        while(ui.FlowStage==SummonFlowStage.AutoSigning)yield return null;
        ui.SkipPresentation();yield return null;yield return null;
        Require(state.Currency==29&&ui.CommittedSummonRequests==1,"真实单抽唯一Draw");
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(1600,1000)})
        {
            typeof(U01Setup).GetMethod("Resize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size.x,size.y});yield return null;yield return null;
            foreach(string id in new[]{"R002","SSR001"})
            {
                var pull=new PrototypePull{Card=state.Cards.Single(c=>c.Id==id),IsNew=true};ui.PlayBatchForDebug(new List<PrototypePull>{pull});ui.SkipPresentation();
                yield return null;yield return null;Canvas.ForceUpdateCanvases();
                var container=Get<RectTransform>(ui,"summaryCards");var tile=container.Find("SingleResultSlot/"+id).GetComponent<RectTransform>();
                Require(Mathf.Abs(tile.rect.width/tile.rect.height-2f/3f)<.001f,size+":"+id+"卡框2:3");
                Require(Mathf.Abs(tile.position.x-container.position.x)<1,size+":"+id+"居中");
                yield return Capture(size.x+"x"+size.y+"_"+id+"_single");
                var e=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=tile.TransformPoint(tile.rect.center)};
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);Require(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==tile.GetComponent<Button>(),"卡片实际射线命中:"+id);
                ExecuteEvents.Execute(tile.gameObject,e,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(tile.gameObject,e,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(tile.gameObject,e,ExecuteEvents.pointerClickHandler);
                Require(Get<string>(ui,"currentScreen")=="Detail","卡片点击详情");ui.HandleEscape();yield return null;yield return null;
                Require(Get<string>(ui,"currentScreen")=="Summary","详情返回原结果");
            }
        }
        typeof(U01Setup).GetMethod("Resize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{1280,720});yield return null;yield return null;
        ui.RequestSummon(10);ui.SkipSigning();while(ui.FlowStage==SummonFlowStage.AutoSigning)yield return null;ui.SkipPresentation();yield return null;yield return null;
        Require(state.Currency==19&&ui.CommittedSummonRequests==2&&ui.PresentationBatch.Count==10,"真实十连及唯一Draw");
        var cards=Get<RectTransform>(ui,"summaryCards");Require(cards.childCount==10&&cards.Find("SingleResultSlot")==null,"十连仍为十张原布局");
        for(int i=0;i<10;i++){var tile=cards.GetChild(i).GetComponent<RectTransform>();Require(Mathf.Abs(tile.anchorMax.x-tile.anchorMin.x-.18f)<.001f,"十连原列宽:"+i);}
        yield return Capture("1280x720_ten_unchanged");
        File.WriteAllText(Path.Combine(Reports,"checks.json"),JsonUtility.ToJson(new Checks{items=checks.ToArray(),currency=state.Currency,commits=ui.CommittedSummonRequests},true));
        Finish("passed","Main Play通过：单抽三窗口2:3、缺图/冰法师、点击详情返回、真实单抽十连与十连原布局。");
    }
    private static IEnumerator Capture(string name){Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Reports,name+".png"));yield return null;yield return null;}
    private static IEnumerator Guard(IEnumerator task)
    {while(true){bool move=false;Exception error=null;try{move=task.MoveNext();}catch(Exception e){error=e;}if(error!=null){Finish("failed",error.ToString());yield break;}if(!move)yield break;var nested=task.Current as IEnumerator;if(nested!=null){yield return Guard(nested);if(!running)yield break;}else yield return task.Current;}}
    private static void Require(bool ok,string text){if(!ok)throw new InvalidOperationException(text);checks.Add(text);}
    private static void Finish(string state,string result){running=false;Write(state,result);EditorApplication.delayCall+=()=>EditorApplication.ExitPlaymode();}
    private static void Write(string state,string result){Directory.CreateDirectory(Reports);File.WriteAllText(Path.Combine(Reports,"result.json"),JsonUtility.ToJson(new Report{stage=state,result=result,at=DateTime.Now.ToString("o")},true));}
    [Serializable]private sealed class Report{public string stage,result,at;}
    [Serializable]private sealed class Checks{public string[] items;public int currency,commits;}
}
