# ChronoLog API Reference

## Overview

The ChronoLog API provides RESTful endpoints for managing time tracking, workdays, projects, and employee data. All endpoints return JSON responses and require authentication unless otherwise noted.

**Base URL**: `/api`

**Authentication**: All endpoints require authentication via Azure AD OAuth 2.0. Include the Bearer token in the Authorization header:
```
Authorization: Bearer {token}
```

If you want to obtain a token, you can use the `azure-cli` or any OAuth 2.0 compatible library to authenticate against your Azure AD tenant.
```bash
az login
az account get-access-token --resource "api://{YOUR_CLIENT_ID}"
```

---

## Table of Contents

1. [Employee Controller](#employee-controller)
2. [Workday Controller](#workday-controller)
3. [Worktime Controller](#worktime-controller)
4. [Project Controller](#project-controller)
5. [Projecttime Controller](#projecttime-controller)
6. [Common Response Codes](#common-response-codes)
7. [Data Models](#data-models)

---

## Employee Controller

Manages current employee data and settings.

**Base Route**: `/api/employee`

### GET /api/employee

Returns the current authenticated employee data.

**Authorization**: Required

**Response**: `200 OK`
```json
{
  "employeeId": "guid",
  "email": "string",
  "name": "string",
  "province": "Province enum",
  "isAdmin": "boolean",
  "isProjectManager": "boolean",
  "vacationDaysPerYear": "int",
  "dailyWorkingTimeInHours": "double",
  "overtimeCorrectionInHours": "double",
  "lastSeen": "datetime"
}
```

---

### GET /api/employee/absenceDays/currentYear

Returns absence days for the current employee for the current year.

**Authorization**: Required

**Response**: `200 OK`
```json
[
  {
    "date": "datetime",
    "type": "WorkdayType enum",
    "isAbsence": "boolean"
  }
]
```

---

### GET /api/employee/absenceDays/{year}

Returns absence days for the current employee for the specified year.

**Authorization**: Required

**Path Parameters**:
- `year` (int): The year to retrieve absence days for

**Response**: `200 OK`
```json
[
  {
    "date": "datetime",
    "type": "WorkdayType enum",
    "isAbsence": "boolean"
  }
]
```

---

### GET /api/employee/vacationDaysTaken/currentYear

Returns the number of vacation days taken by the current employee for the current year.

**Authorization**: Required

**Response**: `200 OK`
```json
5
```

---

### GET /api/employee/vacationDaysTaken/{year}

Returns the number of vacation days taken by the current employee for the specified year.

**Authorization**: Required

**Path Parameters**:
- `year` (int): The year to retrieve vacation days for

**Response**: `200 OK`
```json
5
```

---

### PATCH /api/employee

Updates the current employee settings.

**Authorization**: Required

**Request Body**:
```json
{
  "province": "Province enum (optional)",
  "vacationDaysPerYear": "int (optional)",
  "dailyWorkingTimeInHours": "double (optional)",
  "overtimeCorrectionInHours": "double (optional)"
}
```

**Response**: `204 No Content` on success

**Error Responses**:
- `400 Bad Request`: Failed to update employee settings

---

## Workday Controller

Manages workdays with CRUD operations.

**Base Route**: `/api/workday`

### POST /api/workday

Creates a new workday.

**Authorization**: Required

**Request Body**:
```json
{
  "date": "DateOnly",
  "type": "WorkdayType enum"
}
```

**Response**: `201 Created`
```json
{
  "workdayId": "guid",
  "employeeId": "guid",
  "date": "datetime",
  "type": "WorkdayType enum"
}
```

**Error Responses**:
- `400 Bad Request`: Failed to create workday

---

### GET /api/workday/types

Returns all available workday types with categorization.

**Authorization**: Required

**Response**: `200 OK`
```json
{
  "workdayTypes": [
    {
      "name": "string",
      "value": "int",
      "isWorkingDay": "boolean",
      "isNonWorkingDay": "boolean"
    }
  ],
  "workingDays": [
    // Same structure as workdayTypes, filtered for working days
  ],
  "nonWorkingDays": [
    // Same structure as workdayTypes, filtered for non-working days
  ]
}
```

---

### GET /api/workday

Returns all workdays for the current employee.

**Authorization**: Required

**Response**: `200 OK`
```json
[
  {
    "workdayId": "guid",
    "employeeId": "guid",
    "date": "datetime",
    "type": "WorkdayType enum",
    "worktimes": [
      {
        "worktimeId": "guid",
        "startTime": "TimeOnly",
        "endTime": "TimeOnly",
        "breakTime": "TimeSpan"
      }
    ],
    "projecttimes": [
      {
        "projecttimeId": "guid",
        "projectId": "guid",
        "timeSpent": "TimeSpan",
        "responseText": "string"
      }
    ]
  }
]
```

---

### GET /api/workday/startdate/{startDate}/enddate/{endDate}

Returns workdays within a specified date range.

**Authorization**: Required

**Path Parameters**:
- `startDate` (DateOnly): Start date (format: YYYY-MM-DD)
- `endDate` (DateOnly): End date (format: YYYY-MM-DD)

**Response**: `200 OK` - Same structure as GET /api/workday

---

### GET /api/workday/{workdayId}

Returns a specific workday by its ID.

**Authorization**: Required

**Path Parameters**:
- `workdayId` (guid): The workday ID

**Response**: `200 OK`
```json
{
  "workdayId": "guid",
  "employeeId": "guid",
  "date": "datetime",
  "type": "WorkdayType enum",
  "worktimes": [...],
  "projecttimes": [...]
}
```

**Error Responses**:
- `404 Not Found`: Workday with specified ID not found

---

### GET /api/workday/totalWorktime/{workdayId}

Returns the total worktime for a specific workday.

**Authorization**: Required

**Path Parameters**:
- `workdayId` (guid): The workday ID

**Response**: `200 OK`
```json
"08:30:00"
```

---

### GET /api/workday/totalOvertime

Returns the total overtime across all workdays for the current employee.

**Authorization**: Required

**Response**: `200 OK`
```json
12.5
```

---

### GET /api/workday/officeDays/{year}

Returns the total number of office days in a specified year.

**Authorization**: Required

**Path Parameters**:
- `year` (int): The year

**Response**: `200 OK`
```json
220
```

---

### GET /api/workday/officeDays/currentYear

Returns the total number of office days in the current year.

**Authorization**: Required

**Response**: `200 OK`
```json
220
```

---

### GET /api/workday/officeDays/startdate/{startDate}/enddate/{endDate}

Returns the total number of office days within a specified date range.

**Authorization**: Required

**Path Parameters**:
- `startDate` (DateOnly): Start date
- `endDate` (DateOnly): End date

**Response**: `200 OK`
```json
45
```

---

### PATCH /api/workday/{workdayId}

Updates an existing workday.

**Authorization**: Required

**Path Parameters**:
- `workdayId` (guid): The workday ID

**Request Body**:
```json
{
  "date": "DateOnly (optional)",
  "type": "WorkdayType enum (optional)"
}
```

**Response**: `204 No Content` on success

**Error Responses**:
- `404 Not Found`: Workday not found
- `400 Bad Request`: Failed to update workday

---

### DELETE /api/workday/{workdayId}

Deletes a workday by its ID.

**Authorization**: Required

**Path Parameters**:
- `workdayId` (guid): The workday ID

**Response**: `204 No Content` on success

**Error Responses**:
- `404 Not Found`: Workday not found
- `400 Bad Request`: Failed to delete workday

---

## Worktime Controller

Manages worktimes (start/end times and breaks) with CRUD operations.

**Base Route**: `/api/worktime`

### POST /api/worktime

Creates a new worktime entry.

**Authorization**: Required

**Request Body**:
```json
{
  "workdayId": "guid",
  "startTime": "TimeOnly",
  "endTime": "TimeOnly (optional)",
  "breakTime": "TimeSpan (optional)"
}
```

**Response**: `201 Created`
```json
{
  "worktimeId": "guid",
  "workdayId": "guid",
  "startTime": "TimeOnly",
  "endTime": "TimeOnly",
  "breakTime": "TimeSpan"
}
```

**Error Responses**:
- `400 Bad Request`: Failed to create worktime

---

### GET /api/worktime

Returns all worktimes for the current employee.

**Authorization**: Required

**Response**: `200 OK`
```json
[
  {
    "worktimeId": "guid",
    "workdayId": "guid",
    "startTime": "TimeOnly",
    "endTime": "TimeOnly",
    "breakTime": "TimeSpan"
  }
]
```

---

### GET /api/worktime/startdate/{startDate}/enddate/{endDate}

Returns worktimes within a date range.

**Authorization**: Required

**Path Parameters**:
- `startDate` (DateOnly): Start date
- `endDate` (DateOnly): End date

**Response**: `200 OK` - Same structure as GET /api/worktime

---

### GET /api/worktime/{worktimeId}

Returns a specific worktime by its ID.

**Authorization**: Required

**Path Parameters**:
- `worktimeId` (guid): The worktime ID

**Response**: `200 OK`
```json
{
  "worktimeId": "guid",
  "workdayId": "guid",
  "startTime": "TimeOnly",
  "endTime": "TimeOnly",
  "breakTime": "TimeSpan"
}
```

**Error Responses**:
- `404 Not Found`: Worktime not found

---

### GET /api/worktime/totalWorktime/startdate/{startDate}/enddate/{endDate}

Returns the total worktime within a date range.

**Authorization**: Required

**Path Parameters**:
- `startDate` (DateOnly): Start date
- `endDate` (DateOnly): End date

**Response**: `200 OK`
```json
"160:30:00"
```

---

### PATCH /api/worktime/{worktimeId}

Updates an existing worktime.

**Authorization**: Required

**Path Parameters**:
- `worktimeId` (guid): The worktime ID

**Request Body**:
```json
{
  "startTime": "TimeOnly (optional)",
  "endTime": "TimeOnly (optional)",
  "breakTime": "TimeSpan (optional)"
}
```

**Response**: `204 No Content` on success

**Error Responses**:
- `404 Not Found`: Worktime not found
- `400 Bad Request`: Failed to update worktime

---

### DELETE /api/worktime/{worktimeId}

Deletes a worktime by its ID.

**Authorization**: Required

**Path Parameters**:
- `worktimeId` (guid): The worktime ID

**Response**: `204 No Content` on success

**Error Responses**:
- `404 Not Found`: Worktime not found
- `400 Bad Request`: Failed to delete worktime

---

## Project Controller

Manages projects with CRUD operations. Requires admin or project manager privileges.

**Base Route**: `/api/project`

**Authorization**: Requires `ProjectManagement` policy (IsAdmin or IsProjectManager)

### POST /api/project

Creates a new project.

**Authorization**: ProjectManagement required

**Request Body**:
```json
{
  "name": "string",
  "description": "string (optional)",
  "responseObject": "string",
  "defaultResponseText": "string (optional)",
  "isDefault": "boolean (optional)"
}
```

**Response**: `201 Created`
```json
{
  "projectId": "guid",
  "name": "string",
  "description": "string",
  "responseObject": "string",
  "defaultResponseText": "string",
  "isDefault": "boolean"
}
```

**Error Responses**:
- `400 Bad Request`: Failed to create project
- `403 Forbidden`: Insufficient permissions

---

### GET /api/project

Returns all projects.

**Authorization**: ProjectManagement required

**Response**: `200 OK`
```json
[
  {
    "projectId": "guid",
    "name": "string",
    "description": "string",
    "responseObject": "string",
    "defaultResponseText": "string",
    "isDefault": "boolean"
  }
]
```

---

### GET /api/project/{projectId}

Returns a specific project by its ID.

**Authorization**: ProjectManagement required

**Path Parameters**:
- `projectId` (guid): The project ID

**Response**: `200 OK`
```json
{
  "projectId": "guid",
  "name": "string",
  "description": "string",
  "responseObject": "string",
  "defaultResponseText": "string",
  "isDefault": "boolean"
}
```

**Error Responses**:
- `404 Not Found`: Project not found

---

### PATCH /api/project/{projectId}

Updates an existing project.

**Authorization**: ProjectManagement required

**Path Parameters**:
- `projectId` (guid): The project ID

**Request Body**:
```json
{
  "name": "string (optional)",
  "description": "string (optional)",
  "responseObject": "string (optional)",
  "defaultResponseText": "string (optional)",
  "isDefault": "boolean (optional)"
}
```

**Response**: `204 No Content` on success

**Error Responses**:
- `404 Not Found`: Project not found
- `400 Bad Request`: Failed to update project

---

### DELETE /api/project/{projectId}

Deletes a project by its ID.

**Authorization**: ProjectManagement required

**Path Parameters**:
- `projectId` (guid): The project ID

**Response**: `204 No Content` on success

**Error Responses**:
- `404 Not Found`: Project not found
- `400 Bad Request`: Failed to delete project (may be the default project)

---

## Projecttime Controller

Manages project time entries with CRUD operations.

**Base Route**: `/api/projecttime`

### POST /api/projecttime

Creates a new projecttime entry.

**Authorization**: Required

**Request Body**:
```json
{
  "workdayId": "guid",
  "projectId": "guid (optional, defaults to default project)",
  "timeSpent": "TimeSpan",
  "responseText": "string (optional)"
}
```

**Response**: `201 Created`
```json
{
  "projecttimeId": "guid",
  "workdayId": "guid",
  "projectId": "guid",
  "timeSpent": "TimeSpan",
  "responseText": "string"
}
```

**Error Responses**:
- `400 Bad Request`: Failed to create projecttime

---

### GET /api/projecttime

Returns all projecttimes for the current employee.

**Authorization**: Required

**Response**: `200 OK`
```json
[
  {
    "projecttimeId": "guid",
    "workdayId": "guid",
    "projectId": "guid",
    "timeSpent": "TimeSpan",
    "responseText": "string"
  }
]
```

---

### GET /api/projecttime/{projecttimeIds}

Returns projecttimes by a comma-separated list of IDs.

**Authorization**: Required

**Path Parameters**:
- `projecttimeIds` (string): Comma-separated GUIDs (e.g., "guid1,guid2,guid3")

**Response**: `200 OK`
```json
[
  {
    "projecttimeId": "guid",
    "workdayId": "guid",
    "projectId": "guid",
    "timeSpent": "TimeSpan",
    "responseText": "string"
  }
]
```

**Error Responses**:
- `400 Bad Request`: No valid projecttime IDs provided
- `404 Not Found`: No projecttimes found

---

### GET /api/projecttime/startdate/{startDate}/enddate/{endDate}

Returns projecttimes within a date range.

**Authorization**: Required

**Path Parameters**:
- `startDate` (DateTime): Start date
- `endDate` (DateTime): End date

**Response**: `200 OK` - Same structure as GET /api/projecttime

---

### GET /api/projecttime/{projecttimeId}

Returns a specific projecttime by its ID.

**Authorization**: Required

**Path Parameters**:
- `projecttimeId` (guid): The projecttime ID

**Response**: `200 OK`
```json
{
  "projecttimeId": "guid",
  "workdayId": "guid",
  "projectId": "guid",
  "timeSpan": "TimeSpan",
  "responseText": "string"
}
```

**Error Responses**:
- `404 Not Found`: Projecttime not found

---

### PATCH /api/projecttime/{projecttimeId}

Updates an existing projecttime.

**Authorization**: Required

**Path Parameters**:
- `projecttimeId` (guid): The projecttime ID

**Request Body**:
```json
{
  "workdayId": "guid (optional)",
  "projectId": "guid (optional)",
  "timeSpent": "TimeSpan (optional)",
  "responseText": "string (optional)"
}
```

**Response**: `204 No Content` on success

**Error Responses**:
- `404 Not Found`: Projecttime not found
- `400 Bad Request`: Failed to update projecttime

---

### DELETE /api/projecttime/{projecttimeId}

Deletes a projecttime by its ID.

**Authorization**: Required

**Path Parameters**:
- `projecttimeId` (guid): The projecttime ID

**Response**: `204 No Content` on success

**Error Responses**:
- `404 Not Found`: Projecttime not found
- `400 Bad Request`: Failed to delete projecttime

---

## Common Response Codes

| Code | Description |
|------|-------------|
| `200 OK` | Request successful |
| `201 Created` | Resource successfully created |
| `204 No Content` | Request successful, no content to return (typically for updates/deletes) |
| `400 Bad Request` | Invalid request data or operation failed |
| `401 Unauthorized` | Authentication required or invalid token |
| `403 Forbidden` | Insufficient permissions for the requested operation |
| `404 Not Found` | Requested resource not found |
| `500 Internal Server Error` | Server error occurred |

---

## Data Models

### WorkdayType Enum

Represents the type of workday:
- `Homeoffice` - Work from home
- `Office` - Regular office day
- `Krankheitstag` - Sick leave
- `Urlaub` - Vacation day
- `Gleitzeittag` - Compensatory time off
- `Feiertag` - Public holiday
- `Dienstreise` - Business trip

### Province Enum

German provinces for calculating public holidays:
- `ALL` - All provinces
- `BW` - Baden-Wuerttemberg
- `BY` - Bayern
- `BE` - Berlin
- `BB` - Brandenburg
- `HB` - Bremen
- `HH` - Hamburg
- `HE` - Hessen
- `MV` - Mecklenburg-Vorpommern
- `NI` - Niedersachsen
- `NW` - Nordrhein-Westfalen
- `RP` - Rheinland-Pfalz
- `SL` - Saarland
- `SN` - Sachsen
- `ST` - Sachsen-Anhalt
- `SH` - Schleswig-Holstein
- `TH` - Thueringen

### Time Formats

- **DateTime**: ISO 8601 format (e.g., `2026-01-19T14:30:00Z`)
- **DateOnly**: ISO date format (e.g., `2026-01-19`)
- **TimeOnly**: 24-hour time format (e.g., `14:30:00`)
- **TimeSpan**: Duration format (e.g., `08:30:00` for 8 hours 30 minutes)

---

## Examples

### Creating a Workday

```bash
curl -X POST https://chronolog.example.com/api/workday \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "date": "2026-01-19",
    "type": "Office"
  }'
```

### Adding Worktime to a Workday

```bash
curl -X POST https://chronolog.example.com/api/worktime \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "workdayId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "startTime": "09:00:00",
    "endTime": "17:30:00",
    "breakTime": "00:30:00"
  }'
```

### Creating a Project Time Entry

```bash
curl -X POST https://chronolog.example.com/api/projecttime \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "workdayId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "projectId": "2ba85f64-1234-5678-b3fc-2c963f66afa6",
    "timeSpent": "04:00:00",
    "responseText": "Implemented new feature"
  }'
```

### Getting Workdays for a Date Range

```bash
curl -X GET "https://chronolog.example.com/api/workday/startdate/2026-01-01/enddate/2026-01-31" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Updating Employee Settings

```bash
curl -X PATCH https://chronolog.example.com/api/employee \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "province": "Bayern",
    "vacationDaysPerYear": 30,
    "dailyWorkingTimeInHours": 8.0
  }'
```

---

## Notes

- All timestamps are returned in UTC
- GUIDs are represented as strings in the format `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- Optional fields in request bodies can be omitted or set to `null`
- The API uses Azure AD for authentication - obtain a token from the configured Azure AD tenant
- Project management endpoints require the user to have `IsAdmin` or `IsProjectManager` set to `true` in their employee profile
