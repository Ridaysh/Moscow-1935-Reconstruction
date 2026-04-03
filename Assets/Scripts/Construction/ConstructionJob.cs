using System;
using UnityEngine;

[Serializable]
public enum ConstructionJobStatus
{
    InProgress,
    Completed,
    Cancelled
}

[Serializable]
public class ConstructionJob
{
    [SerializeField] private string jobId;
    [SerializeField] private ProjectData project;
    [SerializeField] private GameZone zone;
    [SerializeField] private int startYear;
    [SerializeField] private int startMonth;
    [SerializeField] private int monthsRemaining;
    [SerializeField] private int totalMonths;
    [SerializeField] private ConstructionJobStatus status = ConstructionJobStatus.InProgress;

    public ConstructionJob(ProjectData project, GameZone zone, int startYear, int startMonth)
    {
        this.project = project;
        this.zone = zone;
        this.startYear = startYear;
        this.startMonth = startMonth;
        totalMonths = Mathf.Max(1, project != null ? project.DurationMonths : 1);
        monthsRemaining = totalMonths;
        jobId = Guid.NewGuid().ToString("N");
    }

    public string JobId => jobId;
    public ProjectData Project => project;
    public GameZone Zone => zone;
    public int StartYear => startYear;
    public int StartMonth => startMonth;
    public int MonthsRemaining => monthsRemaining;
    public int TotalMonths => totalMonths;
    public ConstructionJobStatus Status => status;
    public bool IsComplete => status == ConstructionJobStatus.Completed;

    public float Progress01 =>
        totalMonths > 0 ? 1f - Mathf.Clamp01((float)monthsRemaining / totalMonths) : 0f;

    public int GetRemainingDaysEstimate()
    {
        if (status != ConstructionJobStatus.InProgress || monthsRemaining <= 0)
        {
            return 0;
        }

        int year = TimeSystem.CurrentYear;
        int month = TimeSystem.CurrentMonth;
        int days = TimeSystem.GetRemainingDaysInCurrentMonth();

        for (int i = 1; i < monthsRemaining; i++)
        {
            IncrementMonth(ref year, ref month);
            days += TimeSystem.GetDaysInMonthValue(month, year);
        }

        return Mathf.Max(0, days);
    }

    public void AdvanceMonth()
    {
        if (status != ConstructionJobStatus.InProgress)
        {
            return;
        }

        monthsRemaining = Mathf.Max(0, monthsRemaining - 1);
        if (monthsRemaining == 0)
        {
            status = ConstructionJobStatus.Completed;
        }
    }

    public void Cancel()
    {
        if (status == ConstructionJobStatus.Completed)
        {
            return;
        }

        status = ConstructionJobStatus.Cancelled;
    }

    private static void IncrementMonth(ref int year, ref int month)
    {
        month++;
        if (month <= 12)
        {
            return;
        }

        month = 1;
        year++;
    }
}
