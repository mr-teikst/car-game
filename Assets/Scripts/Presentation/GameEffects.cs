using UnityEngine;

public class GameEffects : MonoBehaviour
{
    [SerializeField] Light underglow;
    [SerializeField] ParticleSystem sparks, fireworks;
    [SerializeField] AudioSource engine, music, effects;
    [SerializeField] AudioClip engineClip, musicClip, crashClip, fireworkClip, coinClip, fuelClip;
    float fireworkTimer;
    public void Initialize()
    {
        if (!engineClip) engineClip = Tone(65,.5f,true); if (!musicClip) musicClip = Music();
        if (!crashClip) crashClip = CrashSound(); if (!fireworkClip) fireworkClip = FireworkSound();
        if (!coinClip) coinClip = Tone(880,.12f,false); if (!fuelClip) fuelClip = Tone(440,.18f,false);
        engine.clip=engineClip; engine.loop=true; engine.volume=.12f; music.clip=musicClip; music.loop=true; music.volume=.14f; music.Play(); effects.volume=.55f;
    }
    public void StartRun() { engine.Play(); }
    public void PauseEngine() { engine.Pause(); }
    public void ResumeEngine() { engine.UnPause(); }
    public void StopEngine() { engine.Stop(); }
    public void Tick(float dt, Transform player, float speed)
    {
        if (underglow) underglow.intensity=2+speed*.16f; engine.pitch=.8f+speed/60f;
        fireworkTimer-=dt;
        if (fireworks && fireworkTimer<=0) {
            fireworkTimer=Random.Range(5f,10f);
            fireworks.transform.position=player.position+new Vector3(Random.Range(-8f,8f),Random.Range(12f,16f),Random.Range(32f,42f));
            Color[] colors = { new Color(1f,.68f,.12f), new Color(.18f,.9f,1f), new Color(.95f,.27f,.75f), new Color(.5f,1f,.32f), new Color(.5f,.55f,1f) };
            int choice=Random.Range(0,colors.Length);
            var main=fireworks.main;
            main.startColor=new ParticleSystem.MinMaxGradient(colors[choice],colors[(choice+1)%colors.Length]);
            main.startSize=.75f;
            main.startLifetime=1.8f;
            main.startSpeed=8f;
            fireworks.Clear(true);
            fireworks.Play(true);
            fireworks.Emit(140);
            effects.PlayOneShot(fireworkClip,.65f);
        }
    }
    void Burst(Vector3 pos, Color color) { if (!sparks) return; sparks.transform.position=pos+Vector3.up; var main=sparks.main; main.startColor=color; sparks.Emit(30); }
    public void PlayHit(Vector3 p) { effects.PlayOneShot(crashClip,1f); Burst(p,new Color(1,.35f,.05f)); }
    public void PlayCoin(Vector3 p) { effects.PlayOneShot(coinClip); Burst(p,Color.yellow); }
    public void PlayFuel(Vector3 p) { effects.PlayOneShot(fuelClip); Burst(p,Color.green); }
    static AudioClip CrashSound()
    {
        const int rate=22050;
        int length=(int)(rate*.55f);
        var samples=new float[length]; var noise=new System.Random(1682);
        for (int i=0;i<length;i++) {
            float t=(float)i/rate;
            float rumble=Mathf.Sin(2*Mathf.PI*(95f-65f*t)*t);
            float crunch=(float)(noise.NextDouble()*2-1);
            float envelope=Mathf.Exp(-t*8f);
            samples[i]=Mathf.Clamp(envelope*(.32f*rumble+.42f*crunch),-.9f,.9f);
        }
        var clip=AudioClip.Create("Car crash",length,1,rate,false); clip.SetData(samples,0); return clip;
    }
    static AudioClip FireworkSound()
    {
        const int rate=22050;
        int length=(int)(rate*.7f);
        var samples=new float[length]; var noise=new System.Random(2701);
        for (int i=0;i<length;i++) {
            float t=(float)i/rate;
            float blast=(float)(noise.NextDouble()*2-1)*Mathf.Exp(-t*11f);
            float low=Mathf.Sin(2*Mathf.PI*(130f-85f*t)*t)*Mathf.Exp(-t*5f);
            float crackle=t>.12f ? (float)(noise.NextDouble()*2-1)*Mathf.Exp(-(t-.12f)*6f)*.22f : 0f;
            samples[i]=Mathf.Clamp(.48f*blast+.32f*low+crackle,-.9f,.9f);
        }
        var clip=AudioClip.Create("Firework burst",length,1,rate,false); clip.SetData(samples,0); return clip;
    }
    static AudioClip Tone(float frequency,float duration,bool loop)
    {
        int rate=22050,n=(int)(rate*duration); var data=new float[n];
        for(int i=0;i<n;i++) { float t=(float)i/rate; float envelope=loop?1:Mathf.Exp(-t*16); data[i]=envelope*(Mathf.Sin(2*Mathf.PI*frequency*t)*.55f+Mathf.Sin(2*Mathf.PI*frequency*2*t)*.2f); }
        var clip=AudioClip.Create("Generated tone",n,1,rate,false); clip.SetData(data,0); return clip;
    }
    static AudioClip Music()
    {
        int rate=22050,n=rate*8; var data=new float[n]; float[] notes={110,130.81f,164.81f,146.83f};
        for(int i=0;i<n;i++) { float t=(float)i/rate, beat=t%.5f, f=notes[(int)(t/2)%4]; data[i]=.25f*Mathf.Sin(2*Mathf.PI*f*t)*Mathf.Exp(-beat*3)+.12f*Mathf.Sin(2*Mathf.PI*f*2*t)*Mathf.Exp(-beat*6); }
        var clip=AudioClip.Create("Generated music",n,1,rate,false); clip.SetData(data,0); return clip;
    }
}
