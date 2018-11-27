//--
// name: pitch_track_new.ck
// desc: adc pitch-tracker (via cepstrum and RMS-treshold)
//
// author: Camille Noufi
//   date: Nov 2018
//--
 
// *********************************************************************************
// ******************* SETUP: PITCH DETECTION VARIABLES ****************************
// *********************************************************************************

// analysis
adc => PoleZero dcblock => BPF bpf => FFT fft =^ RMS rms => blackhole;
IFFT ifft => blackhole;
// set to block DC
.99 => dcblock.blockZero;

// set BPF params
100 => float fL;
1200 => float fH;
Math.sqrt(fL*fH) => float fC;
fC/(fH-fL) => float Q;
<<<Q>>>;
bpf.set(fC,Q);


// set FFT params
1024 => int FFT_SIZE;
0.5 => float HOP_SIZE;
FFT_SIZE => fft.size;
Windowing.hamming( fft.size() ) => fft.window;
// find sample rate
second / samp => float srate;
0 => int rmsCount;


// *********************************************************************************
// ***************** SETUP: QUANTIZATION AND PLAYBACK VARIABLES ********************
// *********************************************************************************

// constant (input) temporal values driving quantization
60 => float BEATS_PER_MIN; //tempo
4 => float BEATS_PER_MEAS; //meter x/4
4 => float DIVS_PER_BEAT; //4 - 16th note quant, 2 - 8th note quant, etc...
60 => float SEC_PER_MIN;

// sample and duration calculations
(BEATS_PER_MEAS*DIVS_PER_BEAT) $ int => int divsPerMeasure;
(SEC_PER_MIN * srate) / (DIVS_PER_BEAT * BEATS_PER_MIN) => float samplesPerDiv; //samples per smallest note div
Math.round( samplesPerDiv / (FFT_SIZE*HOP_SIZE) ) $ int => int numFramesPerDiv;
SEC_PER_MIN / (BEATS_PER_MIN * DIVS_PER_BEAT) => float divDur; //duration in seconds of smallest note div

//initialize storage arrays
float freqArr[divsPerMeasure][numFramesPerDiv]; //to hold measure frequencies
for (0 => int i; i<freqArr.size(); i++) 
    for (0 => int j; j<freqArr[0].size(); j++)
        0 => freqArr[i][j];

//to hold measure keynums for all FFT frames
float keynumArr[divsPerMeasure][numFramesPerDiv];
for (0 => int i; i<keynumArr.size(); i++) 
    for (0 => int j; j<keynumArr[0].size(); j++)
        0 => keynumArr[i][j];

int midiArr[divsPerMeasure]; // to hold midi note keynums
for (0 => int i; i<midiArr.size(); i++) 
    0 => midiArr[i];


// *********************************************************************************
// ***************************** EXECUTION (MAIN) **********************************
// *********************************************************************************


fun void execute(int type)
{
    <<<"record instrument", type>>>;
        
    // 1 measure click-track countdown
    clickTrackCountdown();
    
    // "record" adc input for 1 measure
    recordADC_F0();
    
    // freq->keynum results
    convertF02KeyNum_AllFrames();
    
    // get likely keynum for each note
    computeMostLikelyKeyNum();
    
    // play back results in a loop
    while (true) 
    {
        playSynthesizedMeasure(type);
    }
}


spork ~ execute(1);
(4*BEATS_PER_MEAS*DIVS_PER_BEAT*divDur)::second => now;
spork ~ execute(2);
(4*BEATS_PER_MEAS*DIVS_PER_BEAT*divDur)::second => now;
spork ~ execute(3);



// *********************************************************************************
// ************************** HELPER FUNCTIONS (ACTUAL PROCESSING) *****************
// *********************************************************************************


fun void clickTrackCountdown()
{
    <<<"countdown:">>>;
    for (0 => int i; i<BEATS_PER_MEAS; i++)
    {
        <<< (i+1) >>>; 
        (DIVS_PER_BEAT*divDur)::second => now;
    }
}

fun void recordADC_F0() 
{
    <<<"go:">>>;  
      
    // for all notes in measure
    0 => float tmpF0; 
    for (0=>int i; i<freqArr.size(); i++)
    {
        //for all buffer frames in note
        for (0=>int j; j<freqArr[0].size(); j++)
        {
            extractF0() => tmpF0;
            tmpF0 => freqArr[i][j];
            (FFT_SIZE*HOP_SIZE)::samp => now;
        }
        //print beat
        if(i%BEATS_PER_MEAS==0) <<<(i/BEATS_PER_MEAS + 1)$int>>>;
    } 
}

// ******************************* extractF0() *********************************
fun float extractF0()
{   
    // if signal is above noise floor, extract the freq
    if(flagAboveRMSThresh())
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
    // print out RMS
    1000*rms.fval(0) => float thisScaledRMS;      
    //<<<thisScaledRMS>>>;
    
    if(thisScaledRMS >= 0.1) 
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
    complex logVals[CEPST_SIZE];
    float c[CEPST_SIZE];
    
    //take the log of the squared magnitude of the fft (already computed to get rms)
    0 => float this_fval;
    for( 0 => int i; i < fft.fvals().size(); i++ )
    {
        fft.fval(i) => this_fval;
        Math.pow(this_fval,2) => this_fval; //square it
        Math.log(this_fval) => this_fval; //take the log
        this_fval$complex => logVals[i];  //cast to complex for IFFT UAna input
    }    
    // take IFFT of 102BEATS_PER_MEAS-point frame and put result into cepstrum array
    ifft.transform(logVals);
    ifft.samples(c);
    
    //find peak of cepstrum
    0 => float max; 0 => float abs_c; 0 => int quefrency; 
    for( 15 => int i; i < c.size(); i++ ) //start at 15 to ignore filter, keep source
    {
        Math.fabs(c[i]) => abs_c;
        if( abs_c > max )
        {
            abs_c => max;
            i => quefrency;
        }
    }
    //<<< quefrency >>>;
    
    // convert to to frequency
    srate / quefrency$float  => float target_freq;
    return target_freq;
}

// *********************************** getF0viaSpectrumMax *****************************

//use for comparison to cepstrum
fun float getF0viaSpectrumMax()
{
    0 => float max; int where;
    for( 0 => int i; i < fft.fvals().size(); i++ )
    {
        // compare
        if( fft.fval(i) > max )
        {
            // save
            fft.fval(i) => max;
            i => where;
        }
    }
    
    // set freq
    where$float / fft.size() * srate => float target_freq;
    return target_freq;
}

// ********************************** convertF02KeyNum_AllFrames() ****************************

fun void convertF02KeyNum_AllFrames()
{
    for (0 => int i; i<freqArr.size(); i++)
    {
        for (0 => int j; j<freqArr[0].size(); j++)
        {
            freqArr[i][j] => float tmp;
            if (tmp>=fL)
                Math.round(Std.ftom(tmp)) => keynumArr[i][j];
            else
                0.0 => keynumArr[i][j];
            //<<<keynumArr[i][j]>>>;
        } 
    }  
}

// *********************************** computeMostLikelyKeyNum() **********************

fun void computeMostLikelyKeyNum()
{
    //for each note in measure
    for (0 => int i; i<keynumArr.size(); i++)
    { 
        
        // count occurences of each keynum value
        float histogram[128]; 
        0 => float mode; 0 => int this_keynum;
        for (0 => int j; j < keynumArr[0].size(); j++) {
            keynumArr[i][j]$int => this_keynum;
            if (this_keynum<0) 
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
}

// *********************************** playSynthesizedMeasure() **********************

fun void playSynthesizedMeasure(int type)
{
    
    if (type==1) {
        TriOsc t => JCRev r => dac;
        .05 => r.mix;        
        0.6 => t.gain;
        for (0 => int i; i<midiArr.size(); i++)
        {
            Std.mtof(midiArr[i]) => t.freq;
            //if(i%BEATS_PER_MEAS==0) <<<i/BEATS_PER_MEAS + 1>>>; //print beat
            divDur::second => now;
        }
        0.0 => t.gain;    
    }
    else if (type==2) {
        SinOsc s => JCRev r => dac;
        .1 => r.mix;        
        0.6 => s.gain;
        for (0 => int i; i<midiArr.size(); i++)
        {
            Std.mtof(midiArr[i]) => s.freq;
            //if(i%BEATS_PER_MEAS==0) <<<i/BEATS_PER_MEAS + 1>>>; //print beat
            divDur::second => now;
        }
        0.0 => s.gain;     
    }
    else {
        SawOsc w => JCRev r => dac;
        .2 => r.mix;        
        0.6 => w.gain;
        for (0 => int i; i<midiArr.size(); i++)
        {
            Std.mtof(midiArr[i]) => w.freq;
            //if(i%BEATS_PER_MEAS==0) <<<i/BEATS_PER_MEAS + 1>>>; //print beat
            divDur::second => now;
        }
        0.0 => w.gain;     
    }    
    
    
}
