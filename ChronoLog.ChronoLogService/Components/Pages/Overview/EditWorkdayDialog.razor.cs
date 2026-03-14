using ChronoLog.Core.Models.DisplayObjects;
using Radzen;
using Radzen.Blazor;

namespace ChronoLog.ChronoLogService.Components.Pages.Overview;

public partial class EditWorkdayDialog
{
    private RadzenDataGrid<WorktimeModel> _worktimeGrid = new();
    private List<WorktimeModel> _worktimes = [];
    private readonly List<WorktimeModel> _worktimesToInsert = [];

    private RadzenDataGrid<TimeEntryModel> _timeEntryGrid = new();
    private List<TimeEntryModel> _timeEntries = [];
    private readonly List<TimeEntryModel> _timeEntriesToInsert = [];

    private void ClearPendingWorktimeInsertions() =>
        _worktimesToInsert.Clear();

    private void RemovePendingWorktimeInsertion(WorktimeModel worktime) =>
        _worktimesToInsert.RemoveAll(w => w.WorktimeId == worktime.WorktimeId);
    
    private async Task SaveWorktimeRow(WorktimeModel worktime) =>
        await _worktimeGrid.UpdateRow(worktime);

    private async Task EditWorktimeRow(WorktimeModel worktime)
    {
        if (!_worktimeGrid.IsValid) return;

        ClearPendingWorktimeInsertions();
        await _worktimeGrid.EditRow(worktime);
    }

    private async Task OnUpdateWorktimeRow(WorktimeModel worktime)
    {
        RemovePendingWorktimeInsertion(worktime);
        
        if (IsOverlappingWithExistingWorktimes(worktime))
        {
            _worktimeGrid.CancelEditRow(worktime);
            NotificationService.Notify(NotificationSeverity.Error, "The specified worktime overlaps with existing worktimes.", duration: 4000);
            return;
        }

        try
        {
            var updatedWorktime = await WorktimeService.UpdateWorktimeAsync(worktime);
            if (!updatedWorktime)
            {
                _worktimeGrid.CancelEditRow(worktime);
                NotificationService.Notify(NotificationSeverity.Error, "Update of worktime failed.", duration: 4000);
            }
        }
        catch (Exception ex)
        {
            _worktimeGrid.CancelEditRow(worktime);
            NotificationService.Notify(NotificationSeverity.Error, "Update of worktime failed.", duration: 4000);
            Logger?.LogError(ex, "Error updating worktime row.");
        }
    }

    private void CancelWorktimeEdit(WorktimeModel worktime)
    {
        RemovePendingWorktimeInsertion(worktime);
        _worktimeGrid.CancelEditRow(worktime);
    }

    private async Task DeleteWorktimeRow(WorktimeModel worktime)
    {
        RemovePendingWorktimeInsertion(worktime);

        var worktimeIdExists = _worktimes.Any(w => w.WorktimeId == worktime.WorktimeId && w.WorktimeId != Guid.Empty);
        if (worktimeIdExists)
        {
            try
            {
                if (!await WorktimeService.DeleteWorktimeAsync(worktime.WorktimeId))
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Deletion of worktime failed.",
                        duration: 4000);
                    return;
                }

                _worktimes.RemoveAll(w => w.WorktimeId == worktime.WorktimeId);
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Deletion of worktime failed.", duration: 4000);
                Logger?.LogError(ex, "Error deleting worktime row.");
                return;
            }
        }
        else
        {
            _worktimeGrid.CancelEditRow(worktime);
        }
        
        await _worktimeGrid.Reload();
    }

    private async Task InsertWorktimeRow()
    {
        if (!_worktimeGrid.IsValid) return;

        ClearPendingWorktimeInsertions();
        var newWorktime = new WorktimeModel();
        try
        {
            _worktimesToInsert.Add(newWorktime);
            await _worktimeGrid.InsertRow(newWorktime);
        }
        catch (Exception ex)
        {
            _worktimesToInsert.RemoveAll(w => w.WorktimeId == newWorktime.WorktimeId);
            NotificationService.Notify(NotificationSeverity.Error, "Insertion of worktime failed.", duration: 4000);
            Logger?.LogError(ex, "Error inserting worktime row.");
        }
    }

    private async Task OnCreateWorktimeRow(WorktimeModel worktime)
    {
        if (Workday is null) return;
        
        if (IsOverlappingWithExistingWorktimes(worktime))
        {
            _worktimesToInsert.RemoveAll(w => w.WorktimeId == worktime.WorktimeId);
            _worktimeGrid.CancelEditRow(worktime);
            NotificationService.Notify(NotificationSeverity.Error, "The specified worktime overlaps with existing worktimes.", duration: 4000);
            await _worktimeGrid.Reload();
            return;
        }

        worktime.WorkdayId = Workday.WorkdayId;
        try
        {
            var createdWorktimeId = await WorktimeService.CreateWorktimeAsync(worktime);
            var createdWorktime = await WorktimeService.GetWorktimeAsync(createdWorktimeId);
            
            if (createdWorktime is not null) _worktimes.Add(createdWorktime);
        }
        catch (Exception ex)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Create of worktime failed.", duration: 4000);
            Logger?.LogError(ex, "Error creating worktime row.");
        }
        finally
        {
            _worktimesToInsert.RemoveAll(w => w.WorktimeId == worktime.WorktimeId);
            await _worktimeGrid.Reload();
        }
    }
    
    private bool IsOverlappingWithExistingWorktimes(WorktimeModel worktimeCandidate)
    {
        if (worktimeCandidate.EndTime is null)
            return false;

        var start = worktimeCandidate.StartTime;
        var end = worktimeCandidate.EndTime;
        
        var existingWorktimes = _worktimes
            .Concat(_worktimesToInsert)
            .Where(w => w.WorktimeId != worktimeCandidate.WorktimeId);

        foreach (var worktime in existingWorktimes)
        {
            if (worktime.StartTime == default || worktime.EndTime == null) continue;
            
            if (!(end <= worktime.StartTime || start >= worktime.EndTime))
                return true;
        }

        return false;
    }
    
    private void ClearPendingTimeEntryInsertions() =>
        _timeEntriesToInsert.Clear();

    private void RemovePendingTimeEntryInsertion(TimeEntryModel timeEntry) =>
        _timeEntriesToInsert.RemoveAll(p => p.TimeEntryId == timeEntry.TimeEntryId);

    private async Task EditTimeEntryRow(TimeEntryModel timeEntry)
    {
        if (!_timeEntryGrid.IsValid) return;

        ClearPendingTimeEntryInsertions();
        await _timeEntryGrid.EditRow(timeEntry);
    }

    private async Task OnUpdateTimeEntryRow(TimeEntryModel timeEntry)
    {
        RemovePendingTimeEntryInsertion(timeEntry);
        try
        {
            if (!await TimeEntryService.UpdateTimeEntryAsync(timeEntry))
            {
                _timeEntryGrid.CancelEditRow(timeEntry);
                NotificationService.Notify(NotificationSeverity.Error, "Update of time entry failed.",
                    duration: 4000);
            }
        }
        catch (Exception ex)
        {
            _timeEntryGrid.CancelEditRow(timeEntry);
            NotificationService.Notify(NotificationSeverity.Error, "Update of time entry failed.", duration: 4000);
            Logger?.LogError(ex, "Error updating time entry row.");
        }
    }

    private async Task SaveTimeEntryRow(TimeEntryModel timeEntry)
    {
        if (timeEntry.ProjectId == Guid.Empty)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Please select a project.", duration: 4000);
            return;
        }
        await _timeEntryGrid.UpdateRow(timeEntry);
    }

    private void CancelTimeEntryEdit(TimeEntryModel timeEntry)
    {
        RemovePendingTimeEntryInsertion(timeEntry);
        _timeEntryGrid.CancelEditRow(timeEntry);
    }

    private async Task DeleteTimeEntryRow(TimeEntryModel timeEntry)
    {
        RemovePendingTimeEntryInsertion(timeEntry);

        var timeEntryIdExists = _timeEntries.Any(p => p.TimeEntryId == timeEntry.TimeEntryId && p.TimeEntryId != Guid.Empty);
        if (timeEntryIdExists)
        {
            try
            {
                if (!await TimeEntryService.DeleteTimeEntryAsync(timeEntry.TimeEntryId))
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Deletion of time entry failed.", duration: 4000);
                    return;
                }

                _timeEntries.RemoveAll(p => p.TimeEntryId == timeEntry.TimeEntryId);
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Deletion of time entry failed.", duration: 4000);
                Logger?.LogError(ex, "Error deleting time entry row.");
                return;
            }
        }
        else
        {
            _timeEntryGrid.CancelEditRow(timeEntry);
        }
        
        await _timeEntryGrid.Reload();
    }

    private async Task InsertTimeEntryRow()
    {
        if (!_timeEntryGrid.IsValid) return;

        ClearPendingTimeEntryInsertions();
        var newTimeEntry = new TimeEntryModel();
        try
        {
            _timeEntriesToInsert.Add(newTimeEntry);
            await _timeEntryGrid.InsertRow(newTimeEntry);
        }
        catch (Exception ex)
        {
            _timeEntriesToInsert.RemoveAll(p => p.TimeEntryId == newTimeEntry.TimeEntryId);
            NotificationService.Notify(NotificationSeverity.Error, "Insertion of time entry failed.", duration: 4000);
            Logger?.LogError(ex, "Error inserting time entry row.");
        }
    }

    private async Task OnCreateTimeEntryRow(TimeEntryModel timeEntry)
    {
        if (Workday is null) return;

        timeEntry.WorkdayId = Workday.WorkdayId;
        try
        {
            var createdTimeEntryId = await TimeEntryService.CreateTimeEntryAsync(timeEntry);
            var createdTimeEntry = await TimeEntryService.GetTimeEntryAsync(createdTimeEntryId);

            if (createdTimeEntry is not null) _timeEntries.Add(createdTimeEntry);
            _timeEntries = _timeEntries
                .OrderBy(x => _projects
                    .FirstOrDefault(p => p.ProjectId == x.ProjectId)?.Name)
                .ToList();
        }
        catch (Exception ex)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Create of time entry failed.", duration: 4000);
            Logger?.LogError(ex, "Error creating time entry row.");
        }
        finally
        {
            _timeEntriesToInsert.RemoveAll(p => p.TimeEntryId == timeEntry.TimeEntryId);
            await _timeEntryGrid.Reload();
        }
    }
}