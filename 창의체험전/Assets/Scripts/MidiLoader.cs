using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using Unity.VisualScripting;

public class MidiLoader : MonoBehaviour
{
    public string midiFileName = "unity.mid";
    public NoteSpawner noteSpawner;

    void Start()
    {
        var midiPath = Application.dataPath + "/Midis/" + midiFileName;
        var midiFile = MidiFile.Read(midiPath);
        var tempoMap = midiFile.GetTempoMap();
        var notes = midiFile.GetNotes();

        foreach (var note in notes)
        {
            var metricStartTime = note.TimeAs<MetricTimeSpan>(tempoMap);
            var metricDuration = note.LengthAs<MetricTimeSpan>(tempoMap);

            double startTimeSec = metricStartTime.TotalMicroseconds / 1_000_000.0;
            double durationSec = metricDuration.TotalMicroseconds / 1_000_000.0;

            noteSpawner.ScheduleNote((int)note.NoteNumber, (float)startTimeSec, (float)durationSec);
        }
    }
}
