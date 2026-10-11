"""Package timestamped actual Main Play frames with that session's Unity listener PCM."""
from pathlib import Path
import json, subprocess, hashlib, wave
import numpy as np
import av

HERE=Path(__file__).resolve().parent
FFMPEG='E:/FormatFactory/ffmpeg.exe'
reports=[]
for folder in HERE.glob('*_capture'):
    report_path=folder/'capture.json'
    if not report_path.exists(): continue
    meta=json.loads(report_path.read_text(encoding='utf-8-sig'))
    if not meta['frameTimes'] or meta['samples']<=0 or meta['peak']<=.001:
        raise RuntimeError(f'No real image/audio evidence: {folder.name}')
    frames=meta['frameTimes']
    concat=['ffconcat version 1.0']
    for i, when in enumerate(frames):
        concat += [f"file '{i:05d}.jpg'",f"duration {(frames[i+1]-when) if i+1<len(frames) else max(.02,meta['duration']-when):.8f}"]
    concat += [f"file '{len(frames)-1:05d}.jpg'"]
    (folder/'frames.ffconcat').write_text('\n'.join(concat)+'\n',encoding='utf-8')
    output=HERE/(folder.name.removesuffix('_capture')+'.mp4')
    delay=max(0,round(meta['audioStart']*1000))
    command=[FFMPEG,'-y','-hide_banner','-loglevel','error','-f','concat','-safe','0','-i',str(folder/'frames.ffconcat'),
        '-i',str(folder/'unity_listener_mix.wav'),'-af',f'adelay={delay}:all=1,apad',
        '-c:v','libx264','-preset','fast','-crf','20','-pix_fmt','yuv420p','-fps_mode','vfr',
        '-c:a','aac','-b:a','192k','-t',str(meta['duration']),'-movflags','+faststart',str(output)]
    subprocess.run(command,check=True)
    with wave.open(str(folder/'unity_listener_mix.wav'),'rb') as reader:
        values=np.frombuffer(reader.readframes(reader.getnframes()),np.int16).astype(float)/32768
        audio_seconds=reader.getnframes()/reader.getframerate()
        audio_rate=reader.getframerate()
    container=av.open(str(output))
    streams=[dict(type=s.type,codec=s.codec_context.name,width=getattr(s.codec_context,'width',None),height=getattr(s.codec_context,'height',None)) for s in container.streams]
    reports.append(dict(file=output.name,sha256=hashlib.sha256(output.read_bytes()).hexdigest(),bytes=output.stat().st_size,
        actualCaptureSeconds=meta['duration'],muxSeconds=container.duration/1e6,frames=len(frames),
        averageCaptureFps=len(frames)/meta['duration'],audioSeconds=audio_seconds,audioStart=meta['audioStart'],audioRate=audio_rate,
        peak=float(np.max(np.abs(values))),rms=float(np.sqrt(np.mean(values**2))),clippedSamples=int(np.sum(np.abs(values)>=.999)),
        streams=streams,source='Actual Unity Main Play frames + same session OnAudioFilterRead listener mix; no reconstructed audio track'))
    container.close()
(HERE/'clip_manifest.json').write_text(json.dumps(reports,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(reports,ensure_ascii=False,indent=2))
