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
            _logger.LogError(ex, "Error updating worktime row.");
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
                _logger.LogError(ex, "Error deleting worktime row.");
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
            _logger.LogError(ex, "Error inserting worktime row.");
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
            _logger.LogError(ex, "Error creating worktime row.");
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
    
    private void ClearPendingProjecttimeInsertions() =>
        _projecttimesToInsert.Clear();

    private void RemovePendingProjecttimeInsertion(ProjecttimeModel projecttime) =>
        _projecttimesToInsert.RemoveAll(p => p.ProjecttimeId == projecttime.ProjecttimeId);

    private async Task EditProjecttimeRow(ProjecttimeModel projecttime)
    {
        if (!_projecttimeGrid.IsValid) return;

        ClearPendingProjecttimeInsertions();
        await _projecttimeGrid.EditRow(projecttime);
    }

    private async Task OnUpdateProjecttimeRow(ProjecttimeModel projecttime)
    {
        RemovePendingProjecttimeInsertion(projecttime);
        try
        {
            if (!await ProjecttimeService.UpdateProjecttimeAsync(projecttime))
            {
                _projecttimeGrid.CancelEditRow(projecttime);
                NotificationService.Notify(NotificationSeverity.Error, "Update of project time failed.",
                    duration: 4000);
            }
        }
        catch (Exception ex)
        {
            _projecttimeGrid.CancelEditRow(projecttime);
            NotificationService.Notify(NotificationSeverity.Error, "Update of project time failed.", duration: 4000);
            _logger.LogError(ex, "Error updating project time row.");
        }
    }

    private async Task SaveProjecttimeRow(ProjecttimeModel projecttime)
    {
        if (projecttime.ProjectId == Guid.Empty)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Please select a project.", duration: 4000);
            return;
        }
        await _projecttimeGrid.UpdateRow(projecttime);
    }

    private void CancelProjecttimeEdit(ProjecttimeModel projecttime)
    {
        RemovePendingProjecttimeInsertion(projecttime);
        _projecttimeGrid.CancelEditRow(projecttime);
    }

    private async Task DeleteProjecttimeRow(ProjecttimeModel projecttime)
    {
        RemovePendingProjecttimeInsertion(projecttime);

        var projecttimeIdExists = _projecttimes.Any(p => p.ProjecttimeId == projecttime.ProjecttimeId && p.ProjecttimeId != Guid.Empty);
        if (projecttimeIdExists)
        {
            try
            {
                if (!await ProjecttimeService.DeleteProjecttimeAsync(projecttime.ProjecttimeId))
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Deletion of project time failed.", duration: 4000);
                    return;
                }

                _projecttimes.RemoveAll(p => p.ProjecttimeId == projecttime.ProjecttimeId);
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Deletion of project time failed.", duration: 4000);
                _logger.LogError(ex, "Error deleting project time row.");
                return;
            }
        }
        else
        {
            _projecttimeGrid.CancelEditRow(projecttime);
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
        catch (Exception ex)
        {
            _projecttimesToInsert.RemoveAll(p => p.ProjecttimeId == newProjecttime.ProjecttimeId);
            NotificationService.Notify(NotificationSeverity.Error, "Insertion of project time failed.", duration: 4000);
            _logger.LogError(ex, "Error inserting project time row.");
        }
    }

    private async Task OnCreateProjecttimeRow(ProjecttimeModel projecttime)
    {
        if (Workday is null) return;

        projecttime.WorkdayId = Workday.WorkdayId;
        try
        {
            var createdProjecttimeId = await ProjecttimeService.CreateProjecttimeAsync(projecttime);
            var createdProjecttime = await ProjecttimeService.GetProjecttimeAsync(createdProjecttimeId);
            
            //Workday.Projecttimes.Add(createdProjecttime);

            if (createdProjecttime is not null) _projecttimes.Add(createdProjecttime);
            _projecttimes = _projecttimes
                .OrderBy(x => _projects
                    .FirstOrDefault(p => p.ProjectId == x.ProjectId)?.Name)
                .ToList();
        }
        catch (Exception ex)
        {
            NotificationService.Notify(NotificationSeverity.Error, "Create of project time failed.", duration: 4000);
            _logger.LogError(ex, "Error creating project time row.");
        }
        finally
        {
            _projecttimesToInsert.RemoveAll(p => p.ProjecttimeId == projecttime.ProjecttimeId);
            await _projecttimeGrid.Reload();
        }
    }
}