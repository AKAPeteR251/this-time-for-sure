using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThisTime.Game
{
    // 演出音源只消费时间线；不计算抽卡，不访问系统录音设备。
    public sealed class SummonAudio : MonoBehaviour
    {
        [Serializable] public sealed class CueEvidence
        { public string cue; public float at, phaseSeconds; public double dsp; }
        public readonly List<CueEvidence> Evidence = new List<CueEvidence>();
        private P04FeelConfig config;
        private AudioSource voice, bed, charge, effects, ui;
        private Coroutine fade;
        private float phaseStart;
        private float lastCrystalSound=-100;
        private bool carryCrystalBreak;
        public void BeginPrelude()
        {
            StopImmediate();phaseStart=Time.realtimeSinceStartup;
            double start=AudioSettings.dspTime+.025;
            Schedule(voice,config.voiceFirst,config.voiceVolume,1,start,"voice_first");
            Schedule(bed,config.ceremonyBed,config.bedVolume,1,start,"prelude_bed");
        }
        public void SecondDeclaration(){voice.Stop();Schedule(voice,config.voiceSecond,config.voiceVolume,1,AudioSettings.dspTime+.01,"voice_second");}
        public void CrystalWait()
        {
            StopImmediate();bed.clip=config.ceremonyBed;bed.loop=true;bed.volume=config.masterVolume*config.bedVolume*.28f;bed.Play();Record("crystal_wait");
        }
        public void CrystalHit(float energy)
        {
            if(Time.unscaledTime-lastCrystalSound<.095f)return;
            lastCrystalSound=Time.unscaledTime;effects.pitch=Mathf.Lerp(.85f,1.3f,energy);
            effects.PlayOneShot(config.rReveal,config.masterVolume*config.effectVolume*.26f);Record("crystal_hit");
        }
        public void FinalDeclaration()
        {
            bed.volume=config.masterVolume*config.bedVolume*.15f;effects.pitch=1;
            Schedule(voice,config.voiceFinal,config.voiceVolume,1,AudioSettings.dspTime+.01,"voice_final");
        }
        public void CrystalBreak(){effects.pitch=1;GateBurst();carryCrystalBreak=true;}
        public bool NarrationPlaying { get { return voice != null && voice.isPlaying; } }
        public bool ChargePlaying { get { return charge != null && charge.isPlaying; } }
        public bool CeremonyBedPlaying { get { return bed != null && bed.isPlaying; } }
        public void Configure(P04FeelConfig value)
        {
            config = value;
            voice = Source("Narration"); bed = Source("CeremonyBed"); charge = Source("CardCharge");
            effects = Source("Impacts"); ui = Source("ConfirmSkip");
            if (FindObjectOfType<AudioListener>() == null)
            {
                Camera camera = Camera.main ?? FindObjectOfType<Camera>();
                if (camera != null) camera.gameObject.AddComponent<AudioListener>();
                else gameObject.AddComponent<AudioListener>();
            }
        }
        private AudioSource Source(string name)
        {
            var holder = new GameObject("P04Audio_" + name); holder.transform.SetParent(transform, false);
            var source = holder.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend = 0;
            return source;
        }
        public void BeginCeremony(float seconds)
        {
            StopImmediate(); phaseStart = Time.realtimeSinceStartup;
            if (config == null) return;
            float pitch = config.referenceSeconds / Mathf.Max(0.1f, seconds);
            double start = AudioSettings.dspTime + 0.025;
            Schedule(voice, config.narration, config.voiceVolume, pitch, start, "narration");
            Schedule(bed, config.ceremonyBed, config.bedVolume, pitch, start, "ceremony_bed");
        }
        private void Schedule(AudioSource source, AudioClip clip, float gain, float pitch, double when, string cue)
        {
            if (clip == null) return;
            source.clip = clip; source.pitch = pitch; source.volume = config.masterVolume * gain;
            source.PlayScheduled(when); Record(cue, when);
        }
        public void SealImpact() { OneShot(config == null ? null : config.sealSound, "seal_impact"); }
        public void GateBurst() { OneShot(config == null ? null : config.gateSound, "gate_burst"); }
        public void BeginCard(CardRarity rarity)
        {
            if(carryCrystalBreak){carryCrystalBreak=false;voice.Stop();bed.Stop();bed.loop=false;charge.Stop();}
            else StopImmediate();
            phaseStart = Time.realtimeSinceStartup;
            if (config == null) return;
            AudioClip clip = rarity == CardRarity.UR ? config.urCharge : rarity == CardRarity.SSR ? config.ssrCharge : null;
            if (clip != null)
            { charge.clip = clip; charge.volume = config.masterVolume * config.effectVolume * 0.65f; charge.pitch = 1; charge.Play(); Record("charge_" + rarity); }
        }
        public void CardImpact(CardRarity rarity)
        {
            if (config == null) return;
            AudioClip clip = rarity == CardRarity.UR ? config.urReveal : rarity == CardRarity.SSR ? config.ssrReveal
                : rarity == CardRarity.SR ? config.srReveal : config.rReveal;
            charge.Stop(); OneShot(clip, "reveal_" + rarity);
        }
        private void OneShot(AudioClip clip, string cue)
        {
            if (clip == null || config == null) return;
            effects.PlayOneShot(clip, config.masterVolume * config.effectVolume); Record(cue);
        }
        public void FinishPhase()
        {
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(FadePhase());
        }
        private IEnumerator FadePhase()
        {
            float duration = config == null ? .075f : config.stopFadeSeconds;
            float v = voice.volume, b = bed.volume, c = charge.volume, e = effects.volume;
            for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float p = 1 - elapsed / duration; voice.volume = v * p; bed.volume = b * p;
                charge.volume = c * p; effects.volume = e * p; yield return null;
            }
            voice.Stop(); bed.Stop(); charge.Stop(); effects.Stop(); effects.volume = 1; fade = null;
        }
        public void Confirm()
        { if (config != null && config.confirm != null) { ui.PlayOneShot(config.confirm, config.masterVolume * .5f); Record("confirm"); } }
        public void Skip()
        {
            FinishPhase();
            if (config != null && config.skip != null) { ui.PlayOneShot(config.skip, config.masterVolume * .5f); Record("skip"); }
        }
        private void StopImmediate()
        {
            carryCrystalBreak=false;
            if (fade != null) StopCoroutine(fade); fade = null;
            if (voice == null) return;
            voice.Stop(); bed.Stop(); bed.loop=false; charge.Stop(); effects.Stop(); effects.volume = 1; effects.pitch=1;
        }
        private void Record(string cue, double dsp = -1)
        { Evidence.Add(new CueEvidence { cue = cue, at = Time.realtimeSinceStartup, phaseSeconds = Time.realtimeSinceStartup - phaseStart, dsp = dsp < 0 ? AudioSettings.dspTime : dsp }); }
        private void OnDisable() { StopImmediate(); if (ui != null) ui.Stop(); }
    }
}
