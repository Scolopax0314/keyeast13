using UnityEngine;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab;
    public float noteSpeed = 5f;

    private List<(int note, float time, float duration)> scheduledNotes = new();

    void Update()
    {
        float currentTime = Time.time;

        for (int i = scheduledNotes.Count - 1; i >= 0; i--)
        {
            var (note, time, duration) = scheduledNotes[i];
            if (currentTime >= time)
            {
                SpawnNote(note, duration);
                scheduledNotes.RemoveAt(i);
            }
        }
    }

    public void ScheduleNote(int noteNumber, float startTime, float duration)
    {
        scheduledNotes.Add((noteNumber, startTime, duration));
    }

    private void SpawnNote(int noteNumber, float duration)
    {
        float x = GetXFromNoteNumber(noteNumber);
        Vector3 spawnPos = new Vector3(x, 10f, 0);
        GameObject note = Instantiate(notePrefab, spawnPos, Quaternion.identity);

        note.transform.localScale = new Vector3(0.8f, duration * noteSpeed, 0.2f);
        note.AddComponent<FallDown>().speed = noteSpeed;
    }

    private float GetXFromNoteNumber(int noteNumber)
    {
        return (noteNumber - 60) * 0.9f; // Middle C = 0
    }
}
