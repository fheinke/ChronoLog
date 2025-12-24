using ChronoLog.Core.Models.DisplayObjects;
using Radzen;
using Radzen.Blazor;

namespace ChronoLog.ChronoLogService.Components.Pages.Overview;

public partial class EditWorkdayDialog
{
    private RadzenDataGrid<WorktimeModel> _worktimeGrid = new();
    private List<WorktimeModel> _worktimes = [];
    private readonly List<WorktimeModel> _worktimesToInsert = [];

    private RadzenDataGrid<ProjecttimeModel> _projecttimeGrid = new();
    private List<ProjecttimeModel> _projecttimes = [];
    private readonly List<ProjecttimeModel> _projecttimesToInsert = [];

    private void ClearPendingWorktimeInsertions()
    {
        _worktimesToInsert.Clear();
    }

    private void RemovePendingWorktimeInsertion(WorktimeModel worktime)
    {
        _worktimesToInsert.Remove(worktime);
    }

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
        
        if (!await WorktimeService.UpdateWorktimeAsync(worktime))
        {
            _worktimeGrid.CancelEditRow(worktime);
            NotificationService.Notify(NotificationSeverity.Error, "Update of worktime failed.", duration: 4000);
        }
    }

    private async Task SaveWorktimeRow(WorktimeModel worktime)
    {
        await _worktimeGrid.UpdateRow(worktime);
    }

    private void CancelWorktimeEdit(WorktimeModel worktime)
    {
        RemovePendingWorktimeInsertion(worktime);
        _worktimeGrid.CancelEditRow(worktime);
    }

    private async Task DeleteWorktimeRow(WorktimeModel worktime)
    {
        RemovePendingWorktimeInsertion(worktime);

        if (_worktimes.Contains(worktime))
        {
            if (!await WorktimeService.DeleteWorktimeAsync(worktime.WorktimeId))
            {
                NotificationService.Notify(NotificationSeverity.Error, "Deletion of worktime failed.", duration: 4000);
                return;
            }

            _worktimes.Remove(worktime);
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
        catch
        {
            _worktimesToInsert.Remove(newWorktime);
            NotificationService.Notify(NotificationSeverity.Error, "Insertion of worktime failed.", duration: 4000);
        }
    }

    private async Task OnCreateWorktimeRow(WorktimeModel worktime)
    {
        if (Workday is null) return;
        
        if (IsOverlappingWithExistingWorktimes(worktime))
        {
            _worktimesToInsert.Remove(worktime);
            _worktimeGrid.CancelEditRow(worktime);
            NotificationService.Notify(NotificationSeverity.Error, "The specified worktime overlaps with existing worktimes.", duration: 4000);
            await _worktimeGrid.Reload();
            return;
        }

        worktime.WorkdayId = Workday.WorkdayId;
        var createdWorktimeId = await WorktimeService.CreateWorktimeAsync(worktime);
        var createdWorktime = await WorktimeService.GetWorktimeByIdAsync(createdWorktimeId);

        if (createdWorktime != null) _worktimes.Add(createdWorktime);
        _worktimesToInsert.Remove(worktime);
        await _worktimeGrid.Reload();
    }
    
    private bool IsOverlappingWithExistingWorktimes(WorktimeModel worktimeCandidate)
    {
        if (worktimeCandidate.EndTime == null)
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
            {
                return true;
            }
        }

        return false;
    }

    private void ClearPendingProjecttimeInsertions()
    {
        _projecttimesToInsert.Clear();
    }

    private void RemovePendingProjecttimeInsertion(ProjecttimeModel projettime)
    {
        _projecttimesToInsert.Remove(projettime);
    }

    private async Task EditProjecttimeRow(ProjecttimeModel projettime)
    {
        if (!_projecttimeGrid.IsValid) return;

        ClearPendingProjecttimeInsertions();
        await _projecttimeGrid.EditRow(projettime);
    }

    private async Task OnUpdateProjecttimeRow(ProjecttimeModel projettime)
    {
        RemovePendingProjecttimeInsertion(projettime);
        if (!await ProjecttimeService.UpdateProjecttimeAsync(projettime))
        {
            _projecttimeGrid.CancelEditRow(projettime);
            NotificationService.Notify(NotificationSeverity.Error, "Update of project time failed.", duration: 4000);
        }
    }

    private async Task SaveProjecttimeRow(ProjecttimeModel projettime)
    {
        if (projettime.ProjectId == Guid.Empty)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Please select a project.", duration: 4000);
            return;
        }
        await _projecttimeGrid.UpdateRow(projettime);
    }

    private void CancelProjecttimeEdit(ProjecttimeModel projettime)
    {
        RemovePendingProjecttimeInsertion(projettime);
        _projecttimeGrid.CancelEditRow(projettime);
    }

    private async Task DeleteProjecttimeRow(ProjecttimeModel projettime)
    {
        RemovePendingProjecttimeInsertion(projettime);

        if (_projecttimes.Contains(projettime))
        {
            if (!await ProjecttimeService.DeleteProjecttimeAsync(projettime.ProjecttimeId))
            {
                NotificationService.Notify(NotificationSeverity.Error, "Deletion of project time failed.",
                    duration: 4000);
                return;
            }

            _projecttimes.Remove(projettime);
        }
        else
        {
            _projecttimeGrid.CancelEditRow(projettime);
        }

        await _projecttimeGrid.Reload();
    }

    private async Task InsertProjecttimeRow()
    {
        if (!_projecttimeGrid.IsValid) return;

        ClearPendingProjecttimeInsertions();
        var newProjecttime = new ProjecttimeModel();
        try
        {
            _projecttimesToInsert.Add(newProjecttime);
            await _projecttimeGrid.InsertRow(newProjecttime);
        }
        catch
        {
            _projecttimesToInsert.Remove(newProjecttime);
            NotificationService.Notify(NotificationSeverity.Error, "Insertion of project time failed.", duration: 4000);
        }
    }

    private async Task OnCreateProjecttimeRow(ProjecttimeModel projettime)
    {
        if (Workday is null) return;

        Workday.Projecttimes.Add(projettime);

        projettime.WorkdayId = Workday.WorkdayId;
        var createdProjecttimeId = await ProjecttimeService.CreateProjecttimeAsync(projettime);
        var createdProjecttime = await ProjecttimeService.GetProjecttimeAsync(createdProjecttimeId);

        if (createdProjecttime != null) _projecttimes.Add(createdProjecttime);
        _projecttimesToInsert.Remove(projettime);
        await _projecttimeGrid.Reload();
    }
}