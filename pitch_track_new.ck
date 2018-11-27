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
FFT fft2;
// synthesis
TriOsc s => JCRev r => dac;

// set reverb mix
.05 => r.mix;
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
FFT_SIZE => fft2.size;
Windowing.hamming( fft.size() ) => fft.window => fft2.window;
// find sample rate
second / samp => float srate;
0 => int rmsCount;


// *********************************************************************************
// ***************** SETUP: QUANTIZATION AND PLAYBACK VARIABLES ********************
// *********************************************************************************

// input temporal values driving quantization
60 => float beatsPerMin; //tempo
4 => float beatsPerMeasure; //meter x/4
4 => float divsPerBeat; //4 - 16th note quant, 2 - 8th note quant, etc...
60 => float secPerMin;

// sample and duration calculations
(beatsPerMeasure*divsPerBeat) $ int => int divsPerMeasure;
((secPerMin * srate)/(divsPerBeat * beatsPerMin)) => float samplesPerDiv; //samples per smallest note divdiv
(secPerMin)/(beatsPerMin*divsPerBeat) => float divDur; //duration in seconds of smallest note divdiv
Math.round(samplesPerDiv/(fft.size()*HOP_SIZE)) $ int => int numFramesPerDiv;

//initialize storage arrays
float freqArr[divsPerMeasure][numFramesPerDiv]; //to hold measure frequencies
for (0 => int i; i<freqArr.size(); i++) 
    for (0 => int j; j<freqArr[0].size(); j++)
        0 => freqArr[i][j];

//to hold measure keynums for all FFT frames
float keynumArr[divsPerMeasure][numFramesPerDiv];
for (0 => int i; i<freqArr.size(); i++) 
    for (0 => int j; j<freqArr[0].size(); j++)
        0 => freqArr[i][j];
 
float midiArr[divsPerMeasure]; // to hold midi note keynums
for (0 => int i; i<midiArr.size(); i++) 
    0 => midiArr[i];

  
// *********************************************************************************
// ***************************** EXECUTION STEPS (MAIN) ****************************
// *********************************************************************************

// 1 measure click-track countdown
for (0 => int i; i<beatsPerMeasure; i++)
{
    <<< (i+1) >>>; 
    (divsPerBeat*divDur)::second => now;
}

// "record" adc input for 1 measure
recordADC_F0();

// freq->keynum results
convertF02KeyNum_AllFrames();

// get likely keynum results
computeMostLikelyKeyNum();

// *********************************************************************************
// ************************** HELPER FUNCTIONS (ACTUAL PROCESSING) *****************
// *********************************************************************************

fun void recordADC_F0() 
{
    0 => float tmpF0; 
    for (0=>int i; i<freqArr.size(); i++)
    {
        for (0=>int j; j<freqArr[0].size(); j++)
        {
            extractF0() => tmpF0;
            tmpF0 => freqArr[i][j]; tmpF0 => s.freq;
            (fft.size()*HOP_SIZE)::samp => now;
        }
        //print beat
        if(i%4==0) <<<i/4 + 1>>>;
    } 
}

// ******************************* extractF0() *********************************
fun float extractF0()
{   
    // if signal is above noise floor, extract the freq
    if(flagAboveRMSThresh())
    {
        //return getF0viaSpectrumMax(); 
        return getF0viaCepstrum();  
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
        //<<<thisScaledRMS>>>;
        rmsCount++;
        //<<<rmsCount>>>;
        return 1;
    }
    else 
        return 0;
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
    // take IFFT of 1024-point frame and put result into cepstrum array
    ifft.transform(logVals);
    ifft.samples(c);
    
    
    //find peak of cepstrum
    0 => float max; 0 => int quefrency;
    for( 15 => int i; i < c.size(); i++ ) //ignore filter bins, keep source
    {
        Math.fabs(c[i]) => float abs_c;
        if( abs_c > max )
        {
            abs_c => max;
            i => quefrency;
        }
    }
    //<<< quefrency >>>;
    
    // convert to to frequency
    (srate / (quefrency $ float) ) => float target_freq;
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
    (where $ float) / fft.size() * srate => float target_freq;
    // set gain
    (max / .8) => float target_gain;
    return target_freq;
}

// ********************************** convertF02KeyNum_AllFrames() ****************************

fun void convertF02KeyNum_AllFrames()
{
    for (0 => int i; i<freqArr.size(); i++)
    {
        for (0=>int j; j<freqArr[0].size(); j++)
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
    for (0 => int i; i<keynumArr.size(); i++)
    {
        for (0=>int j; j<keynumArr[0].size(); j++)
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
