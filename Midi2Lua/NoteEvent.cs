using System;
using System.Collections.Generic;

namespace MidiToLua {
    public class NoteEvent {
        public string note;
        public int position;
        public string instrument;

        public NoteEvent(int track, string note, int position) {
            this.note = note;
            this.position = position;
            instrument = Instruments.GetInstrument(track, note);
        }

        public override string ToString() {
            return note + ", " + position + ", " + instrument;
        }
    }
}
