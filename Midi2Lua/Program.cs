﻿using System;
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

                if(split.Length == 1) {
                    split = files[i].Split("\\");
                }

                string name = split[split.Length - 1];

                if(name.Substring(name.Length - 4, 4).Equals(".mid")) {
                    midi = MidiFile.Read(files[i]);

                    song = new Song(name.Replace(".mid", ""));

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

            lua.Add("function add(sample, note, instrument)");
            lua.Add("    table.insert(song.samples[sample].noteEvents, " +
                "{ note = note, instrument = instrument })");
            lua.Add("end");
            lua.Add("");

            lua.Add("b='bass'");
            lua.Add("g='guitar'");
            lua.Add("f='flute'");
            lua.Add("e='bell'");
            lua.Add("a='bassDrum'");
            lua.Add("l='lowFloorTom'");
            lua.Add("h='highFloorTom'");
            lua.Add("o='lowTom'");
            lua.Add("m='lowMidTom'");
            lua.Add("d='highMidTom'");
            lua.Add("i='highTom'");
            lua.Add("s='snare'");
            lua.Add("t='hat'");
            lua.Add("u='unkown'");
            lua.Add("");

            lua.Add("aa='F#1'");
            lua.Add("ab='G1'");
            lua.Add("ac='G#1'");
            lua.Add("ad='A1'");
            lua.Add("ae='A#1'");
            lua.Add("af='B1'");
            lua.Add("ag='C2'");
            lua.Add("ah='C#2'");
            lua.Add("ai='D2'");
            lua.Add("aj='D#2'");
            lua.Add("ak='E2'");
            lua.Add("al='F2'");
            lua.Add("am='F#2'");
            lua.Add("an='G2'");
            lua.Add("ao='G#2'");
            lua.Add("ap='A2'");
            lua.Add("aq='A#2'");
            lua.Add("ar='B2'");
            lua.Add("as='C3'");
            lua.Add("at='C#3'");
            lua.Add("au='D3'");
            lua.Add("av='D#3'");
            lua.Add("aw='E3'");
            lua.Add("ax='F3'");
            lua.Add("ay='F#3'");
            lua.Add("az='G3'");
            lua.Add("ba='G#3'");
            lua.Add("bb='A3'");
            lua.Add("bc='A#3'");
            lua.Add("bd='B3'");
            lua.Add("be='C4'");
            lua.Add("bf='C#4'");
            lua.Add("bg='D4'");
            lua.Add("bh='D#4'");
            lua.Add("bi='E4'");
            lua.Add("bj='F4'");
            lua.Add("bk='F#4'");
            lua.Add("bl='G4'");
            lua.Add("bm='G#4'");
            lua.Add("bn='A4'");
            lua.Add("bo='A#4'");
            lua.Add("bp='B4'");
            lua.Add("bq='C5'");
            lua.Add("br='C#5'");
            lua.Add("bs='D5'");
            lua.Add("bt='D#5'");
            lua.Add("bu='E5'");
            lua.Add("bv='F5'");
            lua.Add("bw='F#5'");
            lua.Add("bx='G5'");
            lua.Add("by='G#5'");
            lua.Add("bz='A5'");
            lua.Add("ca='A#5'");
            lua.Add("cb='B5'");
            lua.Add("cc='C6'");
            lua.Add("cd='C#6'");
            lua.Add("ce='D6'");
            lua.Add("cf='D#6'");
            lua.Add("cg='E6'");
            lua.Add("ch='F6'");
            lua.Add("ci='F#6'");
            lua.Add("cj='G6'");
            lua.Add("ck='G#6'");
            lua.Add("cl='A6'");
            lua.Add("cm='A#6'");
            lua.Add("cn='B6'");
            lua.Add("co='C7'");
            lua.Add("cp='C#7'");
            lua.Add("cq='D7'");
            lua.Add("cr='D#7'");
            lua.Add("cs='E7'");
            lua.Add("ct='F7'");
            lua.Add("cu='F#7'");
            lua.Add("");

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
                lua.Add("add(" + (GetSampleNumber(song.noteEvents[i].position) + 1) + ", " +
                    song.noteEvents[i].GetNoteVar() + ", " +
                    song.noteEvents[i].GetInstrumentVar() + ")");
            } lua.Add("");

            lua.Add("packet = {}");
            lua.Add("packet.command = 'queue'");
            lua.Add("packet.song = song");
            lua.Add("rednet.broadcast(packet, 'JBPP')");
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