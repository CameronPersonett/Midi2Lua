using System;
using System.Collections.Generic;
using System.IO;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace MidiToLua {
    class Program {
        private static string path;

        private static MidiFile midi;

        private static Song song;

        private static List<string> lua;

        static void Main(string[] args) {
            Console.WriteLine("Enter the absolute path to your midi files.");
            path = Console.ReadLine();

            Directory.CreateDirectory(path + "/lua");

            string[] files = Directory.GetFiles(path);

            for(int i = 0; i < files.Length; i++) {
                string[] split = files[i].Split("/");
                string name = split[split.Length - 1];

                if(name.Substring(name.Length - 4, 4).Equals(".mid")) {
                    midi = MidiFile.Read(files[i]);

                    song = new Song(name);

                    BuildSong();

                    BuildNoteEvents();

                    BuildScript();

                    WriteScript();

                    Console.WriteLine("Wrote song: " + name);
                }
            }

            Console.WriteLine("Finished writing Lua scripts.");

            Console.ReadKey();
        }

        private static void BuildSong() {
            int duration = (int)midi.GetDuration<MidiTimeSpan>();

            TempoMap tempoMap = midi.GetTempoMap();

            int bpm = (int)tempoMap.GetTempoAtTime(new MidiTimeSpan()).BeatsPerMinute;

            int previousLoc = 0;
            int sampleBegin = 0;
            int sampleEnd;
            int midiBegin = 0;
            int midiEnd;
            int curSectionTicks;
            double curSectionSecs;
            double multiplier;
            double seconds = 0;
            Section sec;

            foreach(ValueChange<Tempo> tempoChange in tempoMap.GetTempoChanges()) {
                MidiTimeSpan curSpan = new MidiTimeSpan(tempoChange.Time);
                int curTime = (int)tempoChange.Time;

                curSectionSecs = ((60d / (double)bpm) * (((double)curTime - (double)previousLoc) / 480d));
                curSectionTicks = (int)(curSectionSecs * 20);

                if(curSectionTicks > 0) {
                    sampleEnd = curSectionTicks + sampleBegin;
                    midiEnd = curTime;
                    multiplier = ((double)curTime - (double)previousLoc) / (double)curSectionTicks;
                    sec = new Section(sampleBegin, sampleEnd, midiBegin, midiEnd, multiplier);
                    song.AddSection(sec);
                    sampleBegin = sampleEnd;
                }

                midiBegin = previousLoc = curTime;
                bpm = (int)tempoMap.GetTempoAtTime(curSpan).BeatsPerMinute;
            }

            curSectionSecs = seconds + ((60d / (double)bpm) * (((double)duration - (double)previousLoc) / 480d));
            curSectionTicks = (int)(curSectionSecs * 20);
            multiplier = (double)(((double)duration - (double)previousLoc) / (double)curSectionTicks);
            sampleEnd = curSectionTicks + sampleBegin;
            midiEnd = duration;
            sec = new Section(sampleBegin, sampleEnd, midiBegin, midiEnd, multiplier);
            song.AddSection(sec);
        }

        private static void BuildNoteEvents() {
            foreach(Chord chord in midi.GetChords()) {
                foreach(Note note in chord.Notes) {
                    int channel = chord.Channel;
                    string mtNote = note.GetMusicTheoryNote().ToString();
                    int position = (int)chord.Time;
                    song.AddNoteEvent(new NoteEvent(channel, mtNote, position));
                }
            }
        }

        private static void BuildScript() {
            lua = new List<string>();

            lua.Add("song = {}");
            lua.Add("song.name = \'" + song.name + "\'");
            lua.Add("song.samples = {}");
            lua.Add("");

            lua.Add("for i = 1, " + song.totalSamples + ", 1 do");
            lua.Add("    song.samples[i] = {}");
            lua.Add("    song.samples[i].noteEvents = {}");
            lua.Add("end");
            lua.Add("");

            for(int i = 0; i < song.noteEvents.Count; i++) {
                lua.Add("song.samples[" + (GetSampleNumber(song.noteEvents[i].position) + 1) +
                    "].noteEvents[#song.samples[" + (GetSampleNumber(song.noteEvents[i].position) + 1) +
                    "].noteEvents+1] = { note='" + song.noteEvents[i].note +
                    "', instrument='" + song.noteEvents[i].instrument + "' }");
            }
        }

        private static void WriteScript() {
            File.WriteAllLines(path + "/lua/" + song.name + ".lua", lua);
        }

        private static int GetSampleNumber(int location) {
            for(int i = 0; i < song.sections.Count; i++) {
                if(location >= song.sections[i].midiBegin &&
                    location <= song.sections[i].midiEnd) {
                    int relLoc = location - song.sections[i].midiBegin;
                    int relEnd = song.sections[i].midiEnd - song.sections[i].midiBegin;
                    return (int)(((double)relLoc / (double)relEnd) * (double)song.sections[i].samples) + song.sections[i].sampleBegin;
                }
            } return -1;
        }
    }
}