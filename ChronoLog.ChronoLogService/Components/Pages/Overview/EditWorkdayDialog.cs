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

        worktime.WorkdayId = Workday.WorkdayId;
        var createdWorktimeId = await WorktimeService.CreateWorktimeAsync(worktime);
        var createdWorktime = await WorktimeService.GetWorktimeByIdAsync(createdWorktimeId);

        if (createdWorktime != null) _worktimes.Add(createdWorktime);
        _worktimesToInsert.Remove(worktime);
        await _worktimeGrid.Reload();
    }

    private void ClearPendingProjecttimeInsertions()
    {
        _projecttimesToInsert.Clear();
    }

    private void RemovePendingProjecttimeInsertion(ProjecttimeModel project)
    {
        _projecttimesToInsert.Remove(project);
    }

    private async Task EditProjecttimeRow(ProjecttimeModel project)
    {
        if (!_projecttimeGrid.IsValid) return;

        ClearPendingProjecttimeInsertions();
        await _projecttimeGrid.EditRow(project);
    }

    private async Task OnUpdateProjecttimeRow(ProjecttimeModel project)
    {
        RemovePendingProjecttimeInsertion(project);
        if (!await ProjecttimeService.UpdateProjecttimeAsync(project))
        {
            _projecttimeGrid.CancelEditRow(project);
            NotificationService.Notify(NotificationSeverity.Error, "Update of projecttime failed.", duration: 4000);
        }
    }

    private async Task SaveProjecttimeRow(ProjecttimeModel project)
    {
        await _projecttimeGrid.UpdateRow(project);
    }

    private void CancelProjecttimeEdit(ProjecttimeModel project)
    {
        RemovePendingProjecttimeInsertion(project);
        _projecttimeGrid.CancelEditRow(project);
    }

    private async Task DeleteProjecttimeRow(ProjecttimeModel project)
    {
        RemovePendingProjecttimeInsertion(project);

        if (_projecttimes.Contains(project))
        {
            if (!await ProjecttimeService.DeleteProjecttimeAsync(project.ProjecttimeId))
            {
                NotificationService.Notify(NotificationSeverity.Error, "Deletion of projecttime failed.",
                    duration: 4000);
                return;
            }

            _projecttimes.Remove(project);
        }
        else
        {
            _projecttimeGrid.CancelEditRow(project);
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
            NotificationService.Notify(NotificationSeverity.Error, "Insertion of projecttime failed.", duration: 4000);
        }
    }

    private async Task OnCreateProjecttimeRow(ProjecttimeModel project)
    {
        if (Workday is null) return;

        Workday.Projecttimes.Add(project);

        project.WorkdayId = Workday.WorkdayId;
        var createdProjecttimeId = await ProjecttimeService.CreateProjecttimeAsync(project);
        var createdProjecttime = await ProjecttimeService.GetProjecttimeAsync(createdProjecttimeId);

        if (createdProjecttime != null) _projecttimes.Add(createdProjecttime);
        _projecttimesToInsert.Remove(project);
        await _projecttimeGrid.Reload();
    }
}