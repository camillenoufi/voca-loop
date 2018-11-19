//--
// name: pitch_track_new.ck
// desc: cepstrum-based and silence-gate pitch-tracking
//
// author: Camille Noufi
//   date: Nov 2018
//--

// ******** VARIABLE SETUP ***********
// analysis
adc => PoleZero dcblock => FFT fft => blackhole;
// synthesis
SinOsc s => JCRev r => dac;

// set reverb mix
.05 => r.mix;
// set to block DC
.99 => dcblock.blockZero;


// set FFT params
1024 => fft.size;
// window
Windowing.hamming( fft.size() ) => fft.window;
// to hold frame's fft results
UAnaBlob fftVals;
// find sample rate
second / samp => float srate;

float target_freq, target_gain;



// ******** EXECUTION LOOP ***********
while( true )
{
    // take fft
    fft.upchuck() @=> fftVals;
    
    getFundFreq();
    
    // hop
    (fft.size()/2)::samp => now;
    target_freq => s.freq;
    <<<target_freq>>>;
    <<<s.freq>>>;
}



// ******** HELPER FUNCTIONS (ACTUAL PROCESSING) ***********

fun void getFundFreq()
{
    // if signal is above noise floor, extract the freq
    if(determineSNRThresh() == 1)
    {
        // find peak
        0 => float max; int where;
        for( int i; i < fftVals.fvals().size(); i++ )
        {
            // compare max
            if( fftVals.fvals()[i] > max ) {
                fftVals.fvals()[i] => max;
                i => where;
            }
                   
        }
        
        // set freq
        (where $ float) / fft.size() * srate => target_freq;
        // set gain
        (max / .8) => target_gain;
    }
    //otherwise, set f0 and gain to 0
    else
    {
        0.0 => target_freq;
        0.0 => target_gain;
    }
    
    
}



fun int determineSNRThresh()
{
    return 1;
}
