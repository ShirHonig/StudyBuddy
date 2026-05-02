using System.Net.Http.Json;
using System.Text.Json;
using Plugin.Firebase.Firestore;
using StudyBuddy.Models;

namespace StudyBuddy.Services;

public class TaskFirestoreService
{
    private const string ProjectId = "studdybuddy-app522";
    private const string ApiKey    = "AIzaSyDRqnwE4RRuEmJOJaXWY4_mVhrX5g9Rl80";
    private static string BaseUrl =>
        $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents/users/{UserSession.Uid}/tasks";

    private static readonly HttpClient Http = new();
    private static readonly System.Text.Json.JsonSerializerOptions JsonOpts =
        new(System.Text.Json.JsonSerializerDefaults.General); // PascalCase, no camelCase

    private static HttpRequestMessage AuthedRequest(HttpMethod method, string url)
    {
        var req = new HttpRequestMessage(method, url);
        if (!string.IsNullOrEmpty(UserSession.IdToken))
            req.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", UserSession.IdToken);
        return req;
    }

    // ── REST: Write ──────────────────────────────────────────────────────────

    public async Task<string> AddTaskAsync(TaskItem task)
    {
        var body = ToRestDocument(task);
        var content = JsonContent.Create(body, options: JsonOpts);
        var req = AuthedRequest(HttpMethod.Post, $"{BaseUrl}?key={ApiKey}");
        req.Content = content;
        var response = await Http.SendAsync(req);
        var json = await response.Content.ReadAsStringAsync();
        System.Diagnostics.Debug.WriteLine($"[Firebase REST] POST status={response.StatusCode} body={json}");
        response.EnsureSuccessStatusCode();

        // Extract the auto-generated document ID from the response name field
        // name = "projects/.../documents/users/default-user/tasks/{id}"
        using var doc = JsonDocument.Parse(json);
        var name = doc.RootElement.GetProperty("name").GetString() ?? string.Empty;
        return name.Split('/').Last();
    }

    public async Task UpdateTaskAsync(TaskItem task)
    {
        if (string.IsNullOrWhiteSpace(task.Id)) return;

        var url = $"{BaseUrl}/{task.Id}?key={ApiKey}";
        var body = ToRestDocument(task);
        var content = JsonContent.Create(body, options: JsonOpts);
        var req = AuthedRequest(HttpMethod.Patch, url);
        req.Content = content;
        var response = await Http.SendAsync(req);
        System.Diagnostics.Debug.WriteLine($"[Firebase REST] PATCH status={response.StatusCode}");
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTaskAsync(string taskId)
    {
        if (string.IsNullOrWhiteSpace(taskId)) return;

        var url = $"{BaseUrl}/{taskId}?key={ApiKey}";
        var req = AuthedRequest(HttpMethod.Delete, url);
        var response = await Http.SendAsync(req);
        System.Diagnostics.Debug.WriteLine($"[Firebase REST] DELETE status={response.StatusCode}");
    }

    // ── REST: Read ───────────────────────────────────────────────────────────

    public async Task<List<TaskItem>> GetTasksAsync()
    {
        var req = AuthedRequest(HttpMethod.Get, $"{BaseUrl}?key={ApiKey}");
        var response = await Http.SendAsync(req);
        var json = await response.Content.ReadAsStringAsync();
        System.Diagnostics.Debug.WriteLine($"[Firebase REST] GET status={response.StatusCode}");

        if (!response.IsSuccessStatusCode) return [];

        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("documents", out var documents))
            return [];

        var tasks = new List<TaskItem>();
        foreach (var document in documents.EnumerateArray())
        {
            var item = FromRestDocument(document);
            tasks.Add(item);
        }
        return tasks;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static object ToRestDocument(TaskItem task) => new
    {
        fields = new
        {
            Title      = new { stringValue  = task.Title       ?? "" },
            Description= new { stringValue  = task.Description ?? "" },
            Category   = new { stringValue  = task.Category    ?? "" },
            CategoryColor = new { stringValue = task.CategoryColor ?? "" },
            Date       = new { stringValue  = task.Date        ?? "" },
            TeacherName= new { stringValue  = task.TeacherName ?? "" },
            Status     = new { stringValue  = task.Status      ?? "" },
            Priority   = new { stringValue  = task.Priority    ?? "" },
            IsCompleted= new { booleanValue = task.IsCompleted },
            IsUrgent   = new { booleanValue = task.IsUrgent    }
        }
    };

    private static TaskItem FromRestDocument(JsonElement doc)
    {
        var item = new TaskItem();

        // Extract document ID from name
        if (doc.TryGetProperty("name", out var nameProp))
            item.Id = nameProp.GetString()?.Split('/').Last() ?? "";

        if (!doc.TryGetProperty("fields", out var fields))
            return item;

        // Try PascalCase first (new documents), fall back to camelCase (old documents)
        item.Title        = GetString(fields, "Title")        ?? GetString(fields, "title");
        item.Description  = GetString(fields, "Description")  ?? GetString(fields, "description");
        item.Category     = GetString(fields, "Category")     ?? GetString(fields, "category");
        item.CategoryColor= GetString(fields, "CategoryColor")?? GetString(fields, "categoryColor");
        item.Date         = GetString(fields, "Date")         ?? GetString(fields, "date");
        item.TeacherName  = GetString(fields, "TeacherName")  ?? GetString(fields, "teacherName");
        item.Status       = GetString(fields, "Status")       ?? GetString(fields, "status");
        item.Priority     = GetString(fields, "Priority")     ?? GetString(fields, "priority");
        item.IsCompleted  = GetBool(fields, "IsCompleted")    || GetBool(fields, "isCompleted");
        item.IsUrgent     = GetBool(fields, "IsUrgent")       || GetBool(fields, "isUrgent");

        return item;
    }

    private static string? GetString(JsonElement fields, string key)
    {
        if (fields.TryGetProperty(key, out var f) && f.TryGetProperty("stringValue", out var v))
            return v.GetString();
        return null;
    }

    private static bool GetBool(JsonElement fields, string key)
    {
        if (fields.TryGetProperty(key, out var f) && f.TryGetProperty("booleanValue", out var v))
            return v.GetBoolean();
        return false;
    }
}
