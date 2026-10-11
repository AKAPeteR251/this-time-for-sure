"""P04: local SAPI narration + original synthesized cues, no network or third-party music."""
from pathlib import Path
import json, subprocess, wave, hashlib
import numpy as np

ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parent
OUT = ROOT / '这次一定出/Assets/_Game/Audio/P04'
FFMPEG = Path('E:/FormatFactory/ffmpeg.exe')
SR = 44100
rng = np.random.default_rng(7100804)

def read(path):
    with wave.open(str(path), 'rb') as w:
        samples = np.frombuffer(w.readframes(w.getnframes()), np.int16).astype(np.float64) / 32768
        return samples, w.getframerate()

def save(name, data):
    data = np.clip(data, -.98, .98)
    with wave.open(str(OUT / name), 'wb') as w:
        w.setparams((1, 2, SR, len(data), 'NONE', 'not compressed'))
        w.writeframes((data * 32767).astype('<i2').tobytes())

def t(seconds): return np.arange(round(seconds * SR)) / SR
def envelope(x, attack=.01, release=.1):
    return np.minimum(1, x/attack) * np.minimum(1, (x[-1]-x)/release)
def bell(seconds, base, strength=.45):
    x = t(seconds)
    y = sum(np.sin(2*np.pi*base*h*x) * np.exp(-x*(2+h*.8)) / h**1.7 for h in [1, 2, 2.76, 4.1])
    return y * strength * np.minimum(1, x/.003)
def impact(seconds, base, strength):
    x = t(seconds)
    sweep = np.sin(2*np.pi*(base*x + 75*(1-np.exp(-8*x))/8)) * np.exp(-x*9)
    noise = rng.normal(0, 1, len(x)) * np.exp(-x*22)
    return (sweep*.72 + noise*.13) * strength * np.minimum(1, x/.002)
def chord(seconds, frequencies, strength=.3):
    x=t(seconds)
    return sum(np.sin(2*np.pi*f*x)*np.exp(-x*2.7) for f in frequencies)/len(frequencies)*strength*envelope(x,.012,.12)

OUT.mkdir(parents=True, exist_ok=True)
voice = np.zeros(round(6.8 * SR))
segments=[]
for index, (start, target) in enumerate([(0.06,1.6),(1.75,2.45),(5.0,1.55)]):
    source=HERE/f'voice_rate3_{index}.wav'
    raw, rate=read(source)
    voiced=np.flatnonzero(np.abs(raw)>.006)
    first=max(0,voiced[0]-round(.035*rate)); last=min(len(raw),voiced[-1]+round(.07*rate))
    trimmed=raw[first:last]
    tmp=HERE/f'voice_trim_{index}.wav'
    with wave.open(str(tmp),'wb') as w:
        w.setparams((1,2,rate,len(trimmed),'NONE','not compressed'));w.writeframes((trimmed*32767).astype('<i2').tobytes())
    tempo=len(trimmed)/rate/target
    prepared=HERE/f'voice_fitted_{index}.wav'
    subprocess.run([str(FFMPEG),'-y','-hide_banner','-loglevel','error','-i',str(tmp),'-af',f'atempo={tempo:.8f},highpass=f=100,lowpass=f=9500','-ar',str(SR),str(prepared)],check=True)
    fitted,_=read(prepared)
    fitted=fitted/(max(np.max(np.abs(fitted)),.001))*.64
    # A short, quiet chapel reflection preserves consonant intelligibility.
    delayed=np.zeros(len(fitted)+round(.09*SR));delayed[:len(fitted)]=fitted
    delayed[round(.09*SR):round(.09*SR)+len(fitted)]+=fitted*.10
    at=round(start*SR); end=min(len(voice),at+len(delayed));voice[at:end]+=delayed[:end-at]
    segments.append(dict(index=index,start=start,rawSeconds=len(raw)/rate,trimmedSeconds=len(trimmed)/rate,tempo=tempo,processedSeconds=len(fitted)/SR))
save('sacred_narration.wav',voice)

x=t(6.8)
# Harmonic vowel-like pad, low pulses, ascending scan. This is synthesis, not a recorded choir.
pad=sum(np.sin(2*np.pi*f*x+np.sin(x*.8)*.10)*(.4+.6*np.sin(np.minimum(x/4.2,1)*np.pi/2)) for f in [110,165,220,277.18,329.63,440])/6
pad*=.24
pad*=np.where(x<4.25,1,np.where(x<5.85,.13,np.exp(-(x-5.85)*4)))
scan=np.sin(2*np.pi*(170*x+80*x*x))*np.minimum(x/4.2,1)**3*.08
scan*=np.where(x<4.25,1,0)
bed=(pad+scan)*envelope(x,.22,.5)
save('ceremony_bed.wav',bed)
seal=impact(.8,64,.72)+bell(.8,392,.18)
gate=impact(.95,48,.68)+chord(.95,[220,329.63,440,659.25],.32)
save('seal_impact.wav',seal);save('gate_burst.wav',gate)
save('r_reveal.wav',bell(.25,880,.38))
save('sr_reveal.wav',chord(.42,[440,554.37,659.25],.54))
for tier, secs, base in [('ssr',1.0,196),('ur',1.5,146.83)]:
    z=t(secs);charge=np.sin(2*np.pi*(base*z+65*z*z))*.14*envelope(z,.25,.06)
    charge+=np.sin(2*np.pi*base*1.5*z)*.06*envelope(z,.2,.1)
    save(tier+'_charge.wav',charge)
save('ssr_reveal.wav',impact(.65,88,.60)+chord(.65,[392,493.88,587.33,784],.32))
save('ur_reveal.wav',impact(.9,53,.72)+chord(.9,[293.66,440,587.33,880],.35))
save('confirm.wav',chord(.15,[740,988],.3))
z=t(.13);save('skip.wav',rng.normal(0,1,len(z))*.085*envelope(z,.006,.10))

# Standalone listenable mix uses the same times and gain ratios as Main.
mix=voice*.8*.95+bed*.8*.32
for start,data in [(.72,seal),(5.85,gate)]:
    at=round(start*SR);end=min(len(mix),at+len(data));mix[at:end]+=data[:end-at]*.8*.65
save('ceremony_preview.wav',mix)
metadata=dict(engine='Windows SAPI',voice='Microsoft Huihui Desktop - Chinese (Simplified)',rate=3,
    status='Actual local TTS sample; synthetic desktop voice, artistic acceptance pending',
    text=['以此真名，缔结星约。','让群星回应，让命运见证——','星门，为您而开！'],
    waveformSecondLinePunctuation='SAPI raw uses a full stop; suspense pause is scheduled before the third line',
    sampleRate=SR,ceremonySeconds=6.8,segments=segments,
    originalEffects='Deterministic NumPy harmonic/noise synthesis; no third-party music',
    files=[dict(path=p.name,sha256=hashlib.sha256(p.read_bytes()).hexdigest(),seconds=len(read(p)[0])/SR,peak=float(np.max(np.abs(read(p)[0])))) for p in OUT.glob('*.wav')])
(HERE/'audio_manifest.json').write_text(json.dumps(metadata,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'segments':segments,'files':len(metadata['files'])},ensure_ascii=False))
