using System;
namespace MidiToLua {
    public class Section {
        public int samples, sampleBegin, sampleEnd, midiBegin, midiEnd;
        public double multiplier;

        public Section(int sampleBegin, int sampleEnd, int midiBegin, int midiEnd, double multiplier) {
            samples = sampleEnd - sampleBegin;
            this.sampleBegin = sampleBegin;
            this.sampleEnd = sampleEnd;
            this.midiBegin = midiBegin;
            this.midiEnd = midiEnd;
            this.multiplier = multiplier;
        }
    }
}
