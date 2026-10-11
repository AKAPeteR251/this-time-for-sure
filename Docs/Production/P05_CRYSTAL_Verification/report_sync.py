from pathlib import Path
import json,wave
out=Path(r'D:/vibe/这次一定出/Docs/Production/P05_CRYSTAL_Verification')
r=json.loads((out/'checks.json').read_text(encoding='utf-8-sig'));pairs=[];last=None
for e in r['audioCues']:
 if e['cue']=='voice_final':last=e
 if e['cue']=='gate_burst' and last:
  pairs.append({'finalVoiceAt':last['at'],'breakAt':e['at'],'secondsToBreak':e['at']-last['at']});last=None
report={'method':'Actual Unity timeline cue records; not hardware speaker latency','finalLineToBurst':pairs,'checks':len(r['checks']),'currency':r['currency'],'pulls':r['totalPulls'],'commits':r['commits']}
(out/'audio_sync.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(report,ensure_ascii=False,indent=2))
