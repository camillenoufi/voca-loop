//--
// name: pitch_track_new.ck
// desc: cepstrum-based and silence-gate pitch-tracking
//
// author: Camille Noufi
//   date: Nov 2018
//--
 

// ******** PITCH DETECTION VARIABLE SETUP ***********
// analysis
adc => PoleZero dcblock => FFT fft =^ RMS rms => blackhole;
// synthesis
TriOsc s => JCRev r => dac;

// set reverb mix
.05 => r.mix;
// set to block DC
.99 => dcblock.blockZero;

// set FFT params
1024 => int fft_size;
0.5 => float hop_size;
fft_size => fft.size;
Windowing.hamming( fft.size() ) => fft.window;
// find sample rate
second / samp => float srate;


// ******** QUANTIZATION AND PLAYBACK VARIABLE SETUP ***********
// input temporal values driving quantization
60 => float beatsPerMin; //tempo
4 => float beatsPerMeasure; //meter x/4
4 => float subsPerBeat; //4 - 16th note quant, 2 - 8th note quant, etc...
60 => float secPerMin;

// sample and duration calculations
(beatsPerMeasure*subsPerBeat) $ int => int subsPerMeasure;
((secPerMin * srate)/(subsPerBeat * beatsPerMin)) => float samplesPerSub; //samples per smallest note subdiv
(secPerMin)/(beatsPerMin*subsPerBeat) => float subDur; //duration in seconds of smallest note subdiv
Math.floor(samplesPerSub/(fft.size()*hop_size)) $ int => int numFramesPerSub;

//initialize storage arrays
float freqArr[subsPerMeasure][numFramesPerSub]; //to hold measure frequencies
for (0 => int i; i<freqArr.size(); i++) 
    for (0 => int j; j<freqArr[0].size(); j++)
        0 => freqArr[i][j];  

float midiArr[subsPerMeasure]; // to hold midi note keynums
for (0 => int i; i<freqArr.size(); i++) 
    0 => midiArr[i];

  

// ******** EXECUTION LOOP ***********
<<<"1">>>; 1::second => now;
<<<"2">>>; 1::second => now;
<<<"3">>>; 1::second => now;
<<<"go">>>; 1::second => now;

for (0 => int i; i<freqArr.size(); i++)
{
    for (0 => int j; j<freqArr[0].size(); j++)
    {
        extractF0() @=> float targetArr[]; //[freq (Hz), gain (0-1)], both are floats
        // advance time
        (fft.size()*hop_size)::samp => now;
    }    
} 
      




// ******** HELPER FUNCTIONS (ACTUAL PROCESSING) ***********

fun float[] extractF0()
{
    // to hold frame's fft results
    fft.size()/2 => int cepst_size;
    float logVals[cepst_size];
    complex cepst_coeffs[cepst_size];
    float target_freq, target_gain;
    
        
    // if signal is above noise floor, extract the freq
    if(flagAboveRMSThresh())
    {
        fft.upchuck() @=> UAnaBlob fftVals; 
        // take log of mag(FFT)
        0 => float max; int where;
        for( int i; i < fftVals.fvals().size(); i++ )
        {
            //take the log of the squared magnitude
            //Math.log(Math.pow(fftVals.fvals()[i],2)) => logVals[i];
            
            // compare
            if( fftVals.fvals()[i] > max )
            {
                // save
                fftVals.fvals()[i] => max;
                i => where;
            }
                    
        }
        
        //take FFT again
        // set freq
        (where $ float) / fft.size() * srate => target_freq;
        // set gain
        (max / .8) => target_gain;
        
        return [target_freq, target_gain];  
    }
    //otherwise, set f0 and gain to 0
    else
    {
        0.0 => target_freq;
        0.0 => target_gain;
    }
  
}


fun int flagAboveRMSThresh() // adjust hardcoded values depending on space/mic setup
{
    // upchuck: take fft then rms
    rms.upchuck() @=> UAnaBlob rmsVal;
    // print out RMS
    1000*rmsVal.fval(0) => float thisScaledRMS;      
    
    if(thisScaledRMS >= 0.5) 
    {
        //<<<thisScaledRMS>>>;
        return 1;
    }
    else 
        return 0;
}
