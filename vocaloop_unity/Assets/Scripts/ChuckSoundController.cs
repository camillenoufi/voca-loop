using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChuckSoundController : MonoBehaviour {

	//Chuck Metronome
	private ChuckSubInstance myChuckTempo;
    private ChuckFloatSyncer myMetronomeSyncer;
    ChuckEventListener myMetronomeNotifier;
	public static bool globalBeatFlag = false; 
	bool sporkWaiting = false;
	
	//Chuck Instrument
	private ChuckSubInstance myChuck;
    private ChuckFloatSyncer myTempoSyncer;
	private ChuckFloatSyncer myMeterSyncer;
	private ChuckIntSyncer myInstrumentSyncer;
	ChuckEventListener myBeatNotifier;
	
	// Use this for initialization
	void Start () {
        myChuck = GetComponent<ChuckSubInstance>();
		SetChuckVars();
        StartChuckMetronome(myChuckTempo);
        RunChuckInstrument();
	}

	void SetChuckVars()
	{
        //metronome
        myChuckTempo = GetComponent<ChuckSubInstance>();
        myMetronomeSyncer = gameObject.AddComponent<ChuckFloatSyncer>();
        myMetronomeSyncer.SyncFloat(myChuckTempo, "BEATS_PER_MIN"); //current instance of chuck is determining pos value
		//instruments
		myTempoSyncer = gameObject.AddComponent<ChuckFloatSyncer>();
        myTempoSyncer.SyncFloat(myChuck, "BEATS_PER_MIN"); //current instance of chuck is determining pos value
        myMeterSyncer = gameObject.AddComponent<ChuckFloatSyncer>();
        myMeterSyncer.SyncFloat(myChuck, "BEATS_PER_MEAS"); //current instance of chuck is determining pos value
        myInstrumentSyncer = gameObject.AddComponent<ChuckIntSyncer>();
		myInstrumentSyncer.SyncInt(myChuck, "instrument");
    }

    void Update()
    {
        myMetronomeSyncer.SetNewValue(main.currentTempo);
		myTempoSyncer.SetNewValue(main.currentTempo);
		myMeterSyncer.SetNewValue((float)main.currentMeter);
		if (main.adcFlag && main.currentInstrument!="" && !sporkWaiting) {
            StartCoroutine(SyncLoopWithMetronome());
		}
    }

    private IEnumerator SyncLoopWithMetronome()
    {
		sporkWaiting = true;
		yield return new WaitUntil(() => globalBeatFlag==true);
        myInstrumentSyncer.SetNewValue(main.instrumentDict[main.currentInstrument]);
        myChuck.BroadcastEvent("sporkTheLoop");
        main.adcFlag = false;
		sporkWaiting = false;
        
    }

    void StartChuckMetronome(ChuckSubInstance myChuckTempo)
    {
        // instantiate Chuck Pitch Tracking code
        myChuckTempo.RunCode(@"

			80 => global float BEATS_PER_MIN;
			global Event mtrNotifier;
			while(true) 
			{
				mtrNotifier.broadcast();
				(60/BEATS_PER_MIN)::second => now;
			}
		");

        myMetronomeNotifier = gameObject.AddComponent<ChuckEventListener>();
        myMetronomeNotifier.ListenForEvent(myChuckTempo, "mtrNotifier", BroadcastBeat);
    }

	void BroadcastBeat()
	{
		globalBeatFlag = true;
        StartCoroutine(StopBroadcast());
	}

    private IEnumerator StopBroadcast()
    {
        yield return new WaitForSeconds(1/16*60/main.currentTempo);
		globalBeatFlag = false;
    }
	
	void RunChuckInstrument()
    {
        myChuck.RunCode(@"
			//--
			// name: pitch_track_looper.ck
			// desc: adc looper (via cepstrum and RMS-tresholded pitch-tracking) with various playback tones
			//
			// author: Camille Noufi
			//   date: Nov 2018
			//--

			// See ""Execution(Main)"" section for start of run-time code
			// SETUP modifies pitch tracking and quantization variables
			// HELPER FUNCTIONS do the work

			// *********************************************************************************
			// ******************* CONSTANT SYNCER VARIABLES ****************************
			// *********************************************************************************

			// constant (input) temporal values driving quantization
			global Event beatNotifier;
			80 => global float BEATS_PER_MIN; //tempo
			8 => global float BEATS_PER_MEAS; //meter x/4
			4 => float DIVS_PER_BEAT; //4 - 16th note quant, 2 - 8th note quant, etc...

			// *********************************************************************************
			// ******************* SETUP: INSTRUMENTS ******************************************
			// *********************************************************************************

			1 => global int instrument;
			global Event sporkTheLoop;
			
			// connect for synths
			JCRev r => dac;
			0.1 => r.mix;
			
			// DRUM
			me.dir() + ""/kick.wav"" => string drumfile;
			if( me.args() ) me.arg(0) => drumfile; 
			SndBuf kick => dac;
			0 => kick.gain;
			drumfile => kick.read; 

			me.dir() + ""/snare.wav"" => string snarefile;
			if( me.args() ) me.arg(0) => snarefile; 
			SndBuf snare => dac;
			snarefile => snare.read; 


			// *********************************************************************************
			// ******************* SETUP: PITCH DETECTION VARIABLES ****************************
			// *********************************************************************************

			// analysis
			adc => PoleZero dcblock => BPF bpf => FFT fft =^ RMS rms => blackhole;
			FFT fft2 => blackhole;
			// set to block DC
			.99 => dcblock.blockZero;

			// set BPF params
			100 => float fL;
			1200 => float fH;
			Math.sqrt(fL * fH) => float fC;
			fC / (fH - fL) => float Q;
			bpf.set(fC, Q);


			// set FFT params
			1024 => int FFT_SIZE;
			0.5 => float HOP_SIZE;
			FFT_SIZE => fft.size;
			Windowing.hamming(fft.size()) => fft.window;
			// find sample rate
			second / samp => float SRATE;


			// ***************** SETUP: QUANTIZATION AND PLAYBACK VARIABLES ********************

			// sample and duration calculations
			60 => float SEC_PER_MIN;
			(BEATS_PER_MEAS * DIVS_PER_BEAT) $ int => int divsPerMeasure;
			(SEC_PER_MIN * SRATE) / (DIVS_PER_BEAT * BEATS_PER_MIN) => float samplesPerDiv; //samples per smallest note div
			Math.round(samplesPerDiv / (FFT_SIZE * HOP_SIZE)) $ int => int numFramesPerDiv;
			//<<< numFramesPerDiv >>>;
			SEC_PER_MIN / (BEATS_PER_MIN * DIVS_PER_BEAT) => float divDur; //duration in seconds of smallest note div

			//initialize storage arrays:
			//to hold measure frequencies
			float freqArr[divsPerMeasure][numFramesPerDiv]; 
			for (0 => int i; i<freqArr.size(); i++) 
				for (0 => int j; j<freqArr[0].size(); j++)
					0 => freqArr[i][j];
			//to hold midi keynums for all FFT frames
			float keynumArr[divsPerMeasure][numFramesPerDiv];
			for (0 => int i; i<keynumArr.size(); i++) 
				for (0 => int j; j<keynumArr[0].size(); j++)
					0 => keynumArr[i][j];
			// to hold per-note midi keynums
			int midiArr[divsPerMeasure]; 
			for (0 => int i; i<midiArr.size(); i++) 
				0 => midiArr[i];


			// *********************************************************************************
			// ***************************** EXECUTION (MAIN) **********************************
			// *********************************************************************************

			while(true) 
			{
				sporkTheLoop => now;
				spork ~ execute(instrument);
			}

			fun void execute(int type)
			{
				<<< ""Record Instrument"", type >>>;

				// 1 measure click-track countdown
				clickTrackCountdown();

				// record adc input for 1 measure
				recordADC_F0();

				// freq->keynum results
				convertF02KeyNum_AllFrames();

				// get likely keynum for each note
				computeMostLikelyKeyNum(type);

				// play back results in a loop
				playbackLoopGo(type);

			}



			// *********************************************************************************
			// ************************** HELPER FUNCTIONS (ACTUAL PROCESSING) *****************
			// *********************************************************************************


			// ******************************* clickTrackCountdown() *********************************
			fun void clickTrackCountdown()
			{
				//<<<""countdown: "",BEATS_PER_MEAS$int,"" / 4"">>>;
				for (0 => int i; i < BEATS_PER_MEAS; i++)
				{
					beatNotifier.broadcast();
					<<< (i + 1) >>>;
					(DIVS_PER_BEAT * divDur)::second => now;
				}
			}

			// ******************************* recordADC_F0() *********************************
			fun void recordADC_F0()
			{
				// for all notes in measure
				0 => float tmpF0;
				for (0=>int i; i < freqArr.size(); i++)
				{
					spork ~ recordNote(i);
					signalBeat(i);
					divDur::second => now;
				}
			}

			fun void recordNote(int i)
			{
				//for all buffer frames in note
				for (0=>int j; j<freqArr[0].size(); j++)
				{
					extractF0() => freqArr[i][j];
					(FFT_SIZE*HOP_SIZE)::samp => now;
				}
			}

			// ******************************* extractF0() *********************************
			fun float extractF0()
			{
				// if signal is above noise floor, extract the freq
				if (flagAboveRMSThresh())
				{
					return getF0viaSpectrumMax();
					//return getF0viaCepstrum();  
				}
				else
					return 0.0;
			}

			// ********************************** flagAboveRMSThresh() ******************************
			fun int flagAboveRMSThresh() // adjust hardcoded values depending on space/mic setup
			{
				// upchuck: take fft then rms
				rms.upchuck();
				1000 * rms.fval(0) => float thisScaledRMS;
				//<<<thisScaledRMS>>>;

				if (thisScaledRMS >= 0.1)
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}

			// ********************************** getF0viaCepstrum() *******************************
			// cepstrum-based pitch: f0 = scaled_reciprocal(idx(max(IFFT(log(mag^2(FFT(audio_samples)))))))
			fun float getF0viaCepstrum()
			{
				// to hold frame's fft results
				FFT_SIZE/2 => int CEPST_SIZE;
				float logVals[CEPST_SIZE];
				complex c[CEPST_SIZE];
				float cr[CEPST_SIZE];
				
				//take the log of the squared magnitude of the fft (already computed to get rms)
				0 => float this_fval;
				for( 0 => int i; i < fft.fvals().size(); i++ )
				{
					fft.fval(i) => this_fval;
					Math.pow(this_fval,2) => this_fval; //square it
					Math.log(this_fval) => this_fval; //take the log
					this_fval => logVals[i];
				}    
				
				// take re(FFT) of 1024-point frame and put result into cepstrum array
				fft2.transform(logVals);
				fft2.spectrum(c);
				
				for( 0 => int i; i < c.size(); i++ )
				{
					c[i].re => cr[i];
				}  
				
				//find peak of cepstrum
				0 => float max; 0 => float abs_c; 0 => int qi; 
				for( 10 => int i; i < cr.size(); i++ ) // indices of sung freq quefrencies 100-1200hz
				{
					cr[i] => abs_c;
					if( abs_c > max )
					{
						abs_c => max;
						i => qi;
					}
				}
				//<<< quefrency >>>;
				
				// convert to to frequency
				SRATE / qi$float  => float target_freq;
				//<<<target_freq>>>;
				return target_freq;
			}

			// *********************************** getF0viaSpectrumMax *****************************
			fun float getF0viaSpectrumMax()
			{
				0 => float max; int where;
				for (0 => int i; i < fft.fvals().size(); i++)
				{
					// compare
					if (fft.fval(i) > max)
					{
						// save
						fft.fval(i) => max;
						i => where;
					}
				}

				// set freq
				where$float / fft.size() * SRATE => float target_freq;
				return target_freq;
			}

			// ********************************** convertF02KeyNum_AllFrames() ****************************
			fun void convertF02KeyNum_AllFrames()
			{
				for (0 => int i; i < freqArr.size(); i++)
				{
					for (0 => int j; j < freqArr[0].size(); j++)
					{
						freqArr[i][j] => float tmp;
						if (tmp >= fL)
							Math.round(Std.ftom(tmp)) => keynumArr[i][j];
					else
						0.0 => keynumArr[i][j];
						//<<<keynumArr[i][j]>>>;
					}
				}
			}

			// *********************************** computeMostLikelyKeyNum() **********************
			fun void computeMostLikelyKeyNum(int type)
			{
				12 => int octave;
				13 => int ninth;
				
				//for each note in measure
				for (0 => int i; i<keynumArr.size(); i++)
				{ 
					
					// count occurences of each keynum value
					float histogram[128]; 
					0 => float mode; 0 => int this_keynum;
					for (0 => int j; j < keynumArr[0].size(); j++) {
						keynumArr[i][j]$int => this_keynum;
						if (this_keynum<15) 
							0 => this_keynum;
						histogram[this_keynum] + 1 => histogram[this_keynum];
						Math.max(mode, histogram[this_keynum]) => mode;
					}
					
					// select mode - this is the midi pitch for the note
					for (0 => int j; j < histogram.size(); j++) {
						if (histogram[j] == mode) 
						{
							j => midiArr[i];
						}
					}        
				}
				
				// perform drum beat corrections
				if(type == 1 || type == 2) 
				{
					for (1 => int i; i < midiArr.size(); i++) {
						if (midiArr[i-1] != 0) {
							0 => midiArr[i];
						}
					}
				}
				// perform pitch corrections
				else 
				{
					for (1 => int i; i < midiArr.size(); i++) {
						//unison/octave doubling correction
						if (midiArr[i] == (midiArr[i-1] + octave)) {
							midiArr[i-1] => midiArr[i]; 
						}
						// octave doubling correction following interval change
						else if ((midiArr[i-1] != 0) && (midiArr[i] >= (midiArr[i-1] + ninth))) {
							(midiArr[i] - octave) => midiArr[i];
						}
						else if (midiArr[i] <= (midiArr[i-1] - ninth)) {
							(midiArr[i] + octave) => midiArr[i];
						}
						if(midiArr[i]<15)
						{
							0 => midiArr[i];
						}
						//<<<midiArr[i]>>>;
					} 
				}
			}

			// *********************************** playbackLoopGo() **********************
			fun void playbackLoopGo(int type)
			{
				int thisMidiArr[divsPerMeasure]; // to hold midi note keynums
				for (0 => int i; i < midiArr.size(); i++)
					midiArr[i] => thisMidiArr[i];
				while (true)
					playSynthesizedMeasure(type, thisMidiArr);
			}

			fun void playSynthesizedMeasure(int type, int midiArr[])
			{

				divDur::second => dur T;
				// SYNTH INSTRUMENTS
				if (type==5) {
					TriOsc t => ADSR e => r;       
					0.5 => t.gain;
					for (0 => int i; i<midiArr.size(); i++)
					{
						Std.mtof(midiArr[i]) => t.freq;            
						spork ~ play(i, T, e, midiArr, [0.2,0.1,0.9,0.0], [0.0,0.0,0.9,0.0], [0.0,0.0,0.9,0.2]); //ADSR onset, sustain, last
						T => now;
					}
					0.0 => t.gain;
					t =< e =< r;    
				}
				else if (type==4) {
					SinOsc s => ADSR e => r;      
					1 => s.gain;
					for (0 => int i; i<midiArr.size(); i++)
					{
						Std.mtof(midiArr[i]) => s.freq;
						spork ~ play(i, T, e, midiArr, [0.2,0.1,0.9,0.0], [0.0,0.0,0.9,0.0], [0.0,0.0,0.9,0.3]); //ADSR onset, sustain, last
						T => now;
					}
					0.0 => s.gain; 
					s =< e =< r;    
				}
				else if (type==3) {
					SawOsc w => ADSR e => r;       
					0.1 => w.gain;
					for (0 => int i; i<midiArr.size(); i++)
					{
						Std.mtof(midiArr[i]) => w.freq;
						spork ~ play(i, T, e, midiArr, [0.2,0.1,0.9,0.0], [0.0,0.0,0.9,0.0], [0.0,0.0,0.9,0.2]); //ADSR onset, sustain, last
						T => now;
					}
					0.0 => w.gain; 
					w =< e =< r;    
				}
				// DRUMS
				else if (type==1) {
					for (0 => int i; i<midiArr.size(); i++)
					{
						if(midiArr[i]>10) 
						{
							0 => kick.pos;
							1 => kick.gain;
							1 => kick.rate;
						}
						T => now;
					}
				}
				else if (type==2) {
					for (0 => int i; i<midiArr.size(); i++)
					{
						if(midiArr[i]>10) 
						{
							0 => snare.pos;
							0.8 => snare.gain;
							1 => snare.rate;
						}
						T => now;
					}
				}
			}

			fun void play(int i, dur T, ADSR e, int midiArr[], float on[], float sus[], float last[]) 
			{
				//onset of note
				if ( (i > 0 && (midiArr[i] != midiArr[i-1])) || i==0) {
					e.set( (on[0]*divDur)::second, (on[1]*divDur)::second, on[2], (on[3]*divDur)::second );  //(a,d,s height % of freq,r)                
				}
				//last part of sustained note or last note in measure
				else if ( ((i<midiArr.size()-1) && (midiArr[i+1] != midiArr[i])) || i==(midiArr.size()-1) ) {
					e.set( (last[0]*divDur)::second, (last[1]*divDur)::second, last[2], (last[3]*divDur)::second );  //(a,d,s height % of freq,r)    
				}
				//sustained note
				else {
					e.set( (sus[0]*divDur)::second, (sus[1]*divDur)::second, sus[2], (sus[3]*divDur)::second );  //(a,d,s height % of freq,r)    
				}            
				e.keyOn();// press the key
				T - e.releaseTime() => now; // play/wait until beginning of release
				e.keyOff(); //release the key
				e.releaseTime() => now; // wait until release is done
			}


			// *********************************** signalBeat() **********************
			fun void signalBeat(int i)
			{
				if (i % DIVS_PER_BEAT == 0) 
				{
					beatNotifier.broadcast();
					<<< ""recording beat:"", (i / DIVS_PER_BEAT + 1)$int >>>;
				}
			}
		");

    	myBeatNotifier = gameObject.AddComponent<ChuckEventListener>();
        myBeatNotifier.ListenForEvent(myChuck, "beatNotifier", DisplayBeat);

    }

    void DisplayBeat()
    {
		main.beatFlag = true;
    }
}
