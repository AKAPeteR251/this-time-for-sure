using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ThisTime.Game;

[InitializeOnLoad]
public static class P31SummaryRarityVerify
{
    public static string Reports {get{return Path.Combine(Directory.GetParent(Application.dataPath).Parent.FullName,"Docs/Production/P31_SUMMARY_RARITY_Verification");}}
    private const string Flag="ThisTime.P31SummaryRarity";
    private static bool testing;private static double started;private static EditorWindow gameView;
    private static readonly List<string> checks=new List<string>();
    static P31SummaryRarityVerify(){EditorApplication.playModeStateChanged+=Changed;EditorApplication.update+=Update;}
    public static void Verify()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorSceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("停止Play并保存编辑后验证");
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/Main.unity");SessionState.SetBool(Flag,true);Write("starting","Main实际Play");EditorApplication.EnterPlaymode();
    }
    private static void Update()
    {
        string request=Path.Combine(Reports,"P31.request");if(!EditorApplication.isCompiling&&!EditorApplication.isUpdating&&File.Exists(request)){File.Delete(request);try{Verify();}catch(Exception e){Write("failed",e.ToString());}}
        if(!testing)return;if(EditorApplication.isPlaying){gameView.Repaint();EditorApplication.QueuePlayerLoopUpdate();}
        if(EditorApplication.timeSinceStartup-started>180)Complete("FAIL: 超时");
    }
    private static void Changed(PlayModeStateChange change)
    {
        if(change!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Flag,false))return;
        SessionState.SetBool(Flag,false);testing=true;started=EditorApplication.timeSinceStartup;checks.Clear();
        Application.runInBackground=true;EditorApplication.isPaused=false;Resize(1280,720);
        gameView=EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
        var ui=UnityEngine.Object.FindObjectOfType<PrototypeGame>();ui.StartCoroutine(Guard(Check(ui)));
    }
    private static IEnumerator Check(PrototypeGame ui)
    {
        yield return null;yield return null;
        var state=Get<IPrototypeUIAdapter>(ui,"game");
        Require(ui.theme.HasCharacterCards,"Main实际引用F10主题");
        var batch=new List<PrototypePull>();foreach(string id in new[]{"R001","SR003","SSR001","UR001","SR002","R002","SR001","SR004","SSR002","R003"})batch.Add(new PrototypePull{Card=state.Cards.Find(c=>c.Id==id),IsNew=batch.Count<4});
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(1600,1000)})
        {
            Resize(size.x,size.y);yield return null;yield return null;ui.PlayBatchForDebug(batch);yield return null;ui.SkipPresentation();yield return null;yield return null;Canvas.ForceUpdateCanvases();
            var summary=Page(ui,"Summary");var cards=summary.GetComponentsInChildren<AspectRatioFitter>().Where(f=>f.gameObject.GetComponent<Image>()!=null&&f.transform.Find("ThumbnailBorder")!=null).ToArray();
            Require(cards.Length==10,"十连卡面数量:"+size);
            foreach(var card in cards)
            {
                var rect=(RectTransform)card.transform;Require(Mathf.Abs(rect.rect.width/rect.rect.height- .90f)<.002f,"头像小卡比例:"+card.name+":"+size);
                var pull=batch.Single(p=>p.Card.Id==card.name);Require(card.transform.Find("SummaryNew")!=null==pull.IsNew,"真实NEW标记:"+card.name);
                foreach(var layer in card.GetComponentsInChildren<SummaryTileGraphic>()) Require(layer.GetComponent<CanvasRenderer>()!=null && layer.GetComponent<CanvasRenderer>().GetMesh()!=null && layer.GetComponent<CanvasRenderer>().GetMesh().vertexCount==4,"小卡图层实际绘制:"+layer.name);
                Require(card.transform.Find("SummaryRarity").GetComponent<Text>().text==pull.Card.Rarity.ToString(),"真实档位:"+card.name);
                Require(!card.GetComponentsInChildren<Text>().Any(t=>t.text==pull.Card.Name),"结算不展示名字:"+card.name);
            }
            Require(!summary.GetComponentsInChildren<Transform>().Any(t=>t.name=="NewRibbon"||t.name=="TopTrim"),"删除旧卡面黑块红贴纸");
            var ur=summary.GetComponentsInChildren<SummaryRainbowFrame>().Single();
            Require(ur.GetComponent<CanvasRenderer>().GetMesh().vertexCount==256,"UR彩色边框实际绘制");
            float phase=ur.Phase,pulse=ur.Pulse;yield return new WaitForSecondsRealtime(.23f);
            Require(ur.Phase!=phase&&Mathf.Abs(ur.Pulse-pulse)>.01f,"UR渐变流动及闪烁");
            foreach(var card in cards)
            {
                var label=card.transform.Find("SummaryRarity").GetComponent<Text>();
                var mesh=label.GetComponent<CanvasRenderer>().GetMesh();var v=mesh.vertices;
                float minX=float.MaxValue,maxX=float.MinValue,minY=float.MaxValue,maxY=float.MinValue;
                foreach(var point in v){minX=Mathf.Min(minX,point.x);maxX=Mathf.Max(maxX,point.x);minY=Mathf.Min(minY,point.y);maxY=Mathf.Max(maxY,point.y);}
                Vector2 centre=label.rectTransform.anchoredPosition+new Vector2((minX+maxX)*.5f,(minY+maxY)*.5f);
                Require(centre.magnitude<2,"可见字体轮廓居中:"+card.name);
            }
            yield return Capture(size.x+"x"+size.y+"_ten_cards");
        }
        Resize(1280,720);yield return null;ui.PlayBatchForDebug(new List<PrototypePull>{batch[1]});yield return null;ui.SkipPresentation();yield return null;yield return Capture("1280x720_sr_single");
        var summaryCard=Page(ui,"Summary").GetComponentsInChildren<Button>().Single(b=>b.name=="SR003");Click(summaryCard);yield return null;
        Require(Get<string>(ui,"currentScreen")=="Detail","点击卡面进入正确详情");yield return Capture("1280x720_sr_detail");ui.HandleEscape();yield return null;
        ui.RequestSummon(10);yield return new WaitForSecondsRealtime(1.1f);Click(Page(ui,"Signing").GetComponentsInChildren<Button>().Single(b=>b.GetComponentInChildren<Text>().text=="跳过"));
        float deadline=Time.realtimeSinceStartup+3;while(ui.CommittedSummonRequests==0&&Time.realtimeSinceStartup<deadline)yield return null;
        Require(ui.CommittedSummonRequests==1&&state.Currency==20&&state.TotalPulls==10,"真实随机十连唯一Draw与扣费");var same=ui.PresentationBatch.ToArray();ui.SkipPresentation();yield return null;
        Require(ui.FlowStage==SummonFlowStage.Summary&&same.SequenceEqual(ui.PresentationBatch),"演出跳过保留同批");yield return Capture("1280x720_random_ten");
        Click(Page(ui,"Summary").GetComponentsInChildren<Button>().Single(b=>b.GetComponentInChildren<Text>()!=null&&b.GetComponentInChildren<Text>().text.Contains("图鉴")));yield return null;yield return Capture("1280x720_collection");
        Require(Get<string>(ui,"currentScreen")=="Collection","图鉴切换正常");
        var owned=Page(ui,"Collection").GetComponentsInChildren<Button>().First(b=>b.transform.Find("RarityFrame")!=null);Click(owned);yield return null;
        Require(Get<string>(ui,"currentScreen")=="Detail","图鉴已拥有卡面点击详情");
        File.WriteAllText(Path.Combine(Reports,"checks.json"),JsonUtility.ToJson(new CheckReport{checks=checks.ToArray(),currency=state.Currency,totalPulls=state.TotalPulls,commits=ui.CommittedSummonRequests},true));
        Complete("PASS: UR渐变闪烁/四档字体轮廓居中/三窗口/输入与唯一Draw；视觉待验收。");
    }
    private static void Click(Button button)
    {
        Canvas.ForceUpdateCanvases();Vector3[] corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);
        var e=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,(corners[0]+corners[2])*.5f)};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);Require(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==button,"实际UI射线命中:"+button.name);
        ExecuteEvents.Execute(button.gameObject,e,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(button.gameObject,e,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(button.gameObject,e,ExecuteEvents.pointerClickHandler);
    }
    private static T Get<T>(PrototypeGame ui,string name){return(T)typeof(PrototypeGame).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(ui);}
    private static GameObject Page(PrototypeGame ui,string name){return Get<Dictionary<string,GameObject>>(ui,"screens")[name];}
    private static void Resize(int w,int h){typeof(U01Setup).GetMethod("Resize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{w,h});}
    private static IEnumerator Capture(string name){yield return null;yield return null;Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(Reports,name+".png"));yield return null;yield return null;}
    private static IEnumerator Guard(IEnumerator routine){while(true){bool moved=false;Exception error=null;try{moved=routine.MoveNext();}catch(Exception e){error=e;}if(error!=null){Complete("FAIL: "+error);yield break;}if(!moved)yield break;var nested=routine.Current as IEnumerator;if(nested!=null){yield return Guard(nested);if(!testing)yield break;}else yield return routine.Current;}}
    private static void Require(bool good,string label){if(!good)throw new InvalidOperationException(label);checks.Add(label);}
    private static void Complete(string result){testing=false;Write(result.StartsWith("PASS")?"passed":"failed",result);File.Copy(Path.Combine(Reports,"runtime_status.json"),Path.Combine(Reports,"final_result.json"),true);EditorApplication.delayCall+=()=>EditorApplication.ExitPlaymode();}
    private static void Write(string stage,string result){Directory.CreateDirectory(Reports);File.WriteAllText(Path.Combine(Reports,"runtime_status.json"),JsonUtility.ToJson(new Report{stage=stage,result=result,atUtc=DateTime.UtcNow.ToString("o"),unityVersion=Application.unityVersion},true));Debug.Log("[P31] "+result);}
    [Serializable]private sealed class Report{public string stage,result,atUtc,unityVersion;}
    [Serializable]private sealed class CheckReport{public string[] checks;public int currency,totalPulls,commits;}
}
