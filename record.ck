// chuck this with other shreds to record to file

// arguments: rec:<filename>

// get name
me.arg(0) => string filename;
if( filename.length() == 0 ) "new_loop_rec_" + ((now$string) + ".wav" => filename;

// pull samples from the dac
dac => Gain g => WvOut w => blackhole;
// this is the output file name
filename => w.wavFilename;
<<<"writing to file:", "'" + w.filename() + "'">>>;
// any gain you want for the output
.8 => g.gain;

// temporary workaround to automatically close file on remove-shred
null @=> w;

// infinite time loop...
// ctrl-c will stop it, or modify to desired duration
while( true ) 1::second => now;