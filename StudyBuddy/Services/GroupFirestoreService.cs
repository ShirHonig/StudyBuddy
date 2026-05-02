using System.Net.Http.Json;
using System.Text.Json;

namespace StudyBuddy.Services;

public class GroupFirestoreService
{
    private const string ProjectId = "studdybuddy-app522";
    private const string ApiKey    = "AIzaSyDRqnwE4RRuEmJOJaXWY4_mVhrX5g9Rl80";

    // Top-level groups collection — visible to all users
    private const string BaseUrl = $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents/groups";

    private static readonly HttpClient Http = new();
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.General);

    private static HttpRequestMessage AuthedRequest(HttpMethod method, string url)
    {
        var req = new HttpRequestMessage(method, url);
        if (!string.IsNullOrEmpty(UserSession.IdToken))
            req.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", UserSession.IdToken);
        return req;
    }

    // ── Create ────────────────────────────────────────────────────────────────

    public async Task<string> CreateGroupAsync(GroupItem group)
    {
        var body    = ToRestDoc(group, creatorUid: UserSession.Uid);
        var content = JsonContent.Create(body, options: JsonOpts);
        var req     = AuthedRequest(HttpMethod.Post, $"{BaseUrl}?key={ApiKey}");
        req.Content = content;
        var response = await Http.SendAsync(req);
        var json     = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Firebase {(int)response.StatusCode}: {json[..Math.Min(200,json.Length)]}");

        using var doc = JsonDocument.Parse(json);
        var name = doc.RootElement.GetProperty("name").GetString() ?? "";
        return name.Split('/').Last();
    }

    // ── Get all groups ────────────────────────────────────────────────────────

    public async Task<List<GroupItem>> GetAllGroupsAsync()
    {
        var req      = AuthedRequest(HttpMethod.Get, $"{BaseUrl}?key={ApiKey}");
        var response = await Http.SendAsync(req);
        var json     = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Firebase GET ALL {(int)response.StatusCode}: {json[..Math.Min(150,json.Length)]}");

        var groups = new List<GroupItem>();
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("documents", out var docs))
            foreach (var d in docs.EnumerateArray())
                groups.Add(FromRestDoc(d));
        return groups;
    }

    // ── Get groups I'm a member of ────────────────────────────────────────────

    public async Task<List<GroupItem>> GetMyGroupsAsync()
    {
        // Fetch all groups, then filter client-side for ones where current user is a member
        var all = await GetAllGroupsAsync();
        return all.Where(g => g.MembersList.Any(m => m.Uid == UserSession.Uid)).ToList();
    }

    // ── Join (read → add uid → patch) ─────────────────────────────────────────

    public async Task JoinGroupAsync(GroupItem group)
    {
        var docUrl = $"{BaseUrl}/{group.Id}";

        // 1. Read current members
        var getReq  = AuthedRequest(HttpMethod.Get, $"{docUrl}?key={ApiKey}");
        var getResp = await Http.SendAsync(getReq);
        var getJson = await getResp.Content.ReadAsStringAsync();
        if (!getResp.IsSuccessStatusCode)
            throw new Exception($"Firebase GET {(int)getResp.StatusCode}: {getJson[..Math.Min(150,getJson.Length)]}");

        var members = new List<MemberInfo>();
        using (var doc = JsonDocument.Parse(getJson))
        {
            if (doc.RootElement.TryGetProperty("fields", out var fields) &&
                fields.TryGetProperty("MembersList", out var membersField) &&
                membersField.TryGetProperty("arrayValue", out var arr) &&
                arr.TryGetProperty("values", out var vals))
            {
                foreach (var v in vals.EnumerateArray())
                {
                    if (v.TryGetProperty("mapValue", out var mapVal) &&
                        mapVal.TryGetProperty("fields", out var mFields))
                    {
                        members.Add(new MemberInfo
                        {
                            Uid   = GetStr(mFields, "Uid")   ?? "",
                            Email = GetStr(mFields, "Email") ?? "",
                            Name  = GetStr(mFields, "Name")  ?? ""
                        });
                    }
                }
            }
        }

        if (members.Any(m => m.Uid == UserSession.Uid)) return; // already a member

        // Add current user
        members.Add(new MemberInfo
        {
            Uid   = UserSession.Uid,
            Email = UserSession.Email,
            Name  = UserSession.FullName
        });

        // 2. Patch the MembersList field
        var patch = new
        {
            fields = new
            {
                MembersList = new
                {
                    arrayValue = new
                    {
                        values = members.Select(m => new
                        {
                            mapValue = new
                            {
                                fields = new
                                {
                                    Uid   = new { stringValue = m.Uid },
                                    Email = new { stringValue = m.Email },
                                    Name  = new { stringValue = m.Name }
                                }
                            }
                        }).ToArray()
                    }
                }
            }
        };

        var patchUrl = $"{docUrl}?updateMask.fieldPaths=MembersList&key={ApiKey}";
        var content  = JsonContent.Create(patch, options: JsonOpts);
        var req      = AuthedRequest(HttpMethod.Patch, patchUrl);
        req.Content  = content;
        var resp     = await Http.SendAsync(req);
        if (!resp.IsSuccessStatusCode)
        {
            var errJson = await resp.Content.ReadAsStringAsync();
            throw new Exception($"Firebase JOIN {(int)resp.StatusCode}: {errJson[..Math.Min(150,errJson.Length)]}");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static object ToRestDoc(GroupItem g, string creatorUid) => new
    {
        fields = new
        {
            Subject         = new { stringValue = g.Subject },
            SubjectColorHex = new { stringValue = g.SubjectColorHex },
            Title           = new { stringValue = g.Title },
            Description     = new { stringValue = g.Description },
            CreatorUid      = new { stringValue = creatorUid },
            CreatorEmail    = new { stringValue = UserSession.Email },
            CreatorInitial  = new { stringValue = g.CreatorInitial },
            CreatorName     = new { stringValue = g.CreatorName },
            NextMeeting     = new { stringValue = g.NextMeeting.ToString("o") },
            MembersList     = new
            {
                arrayValue = new
                {
                    values = new[]
                    {
                        new
                        {
                            mapValue = new
                            {
                                fields = new
                                {
                                    Uid   = new { stringValue = creatorUid },
                                    Email = new { stringValue = UserSession.Email },
                                    Name  = new { stringValue = g.CreatorName }
                                }
                            }
                        }
                    }
                }
            }
        }
    };

    private static GroupItem FromRestDoc(JsonElement doc)
    {
        var g = new GroupItem();

        if (doc.TryGetProperty("name", out var n))
            g.Id = n.GetString()?.Split('/').Last() ?? "";

        if (!doc.TryGetProperty("fields", out var f)) return g;

        g.Subject        = GetStr(f, "Subject")        ?? "";
        g.SubjectColorHex= GetStr(f, "SubjectColorHex")?? "#737785";
        g.Title          = GetStr(f, "Title")          ?? "";
        g.Description    = GetStr(f, "Description")    ?? "";
        g.CreatorUid     = GetStr(f, "CreatorUid")     ?? "";
        g.CreatorEmail   = GetStr(f, "CreatorEmail")   ?? "";
        g.CreatorInitial = GetStr(f, "CreatorInitial") ?? "?";
        g.CreatorName    = GetStr(f, "CreatorName")    ?? "";

        var meetingStr = GetStr(f, "NextMeeting");
        if (DateTime.TryParse(meetingStr, out var dt))
            g.NextMeeting = dt;

        // Avatar colors derived from subject color
        g.AvatarBg = Color.FromArgb(g.SubjectColorHex).WithAlpha(0.25f);
        g.AvatarFg = Color.FromArgb(g.SubjectColorHex);

        // MembersList array (new format with MemberInfo objects)
        if (f.TryGetProperty("MembersList", out var memList) &&
            memList.TryGetProperty("arrayValue", out var arr) &&
            arr.TryGetProperty("values", out var vals))
        {
            foreach (var v in vals.EnumerateArray())
            {
                if (v.TryGetProperty("mapValue", out var mapVal) &&
                    mapVal.TryGetProperty("fields", out var mFields))
                {
                    var member = new MemberInfo
                    {
                        Uid   = GetStr(mFields, "Uid")   ?? "",
                        Email = GetStr(mFields, "Email") ?? "",
                        Name  = GetStr(mFields, "Name")  ?? ""
                    };
                    member.AvatarBg = g.AvatarBg;
                    member.AvatarFg = g.AvatarFg;
                    g.MembersList.Add(member);
                }
            }
        }

        // Backward compat: old Members array (just UIDs)
        if (g.MembersList.Count == 0 && f.TryGetProperty("Members", out var oldMem) &&
            oldMem.TryGetProperty("arrayValue", out var oldArr) &&
            oldArr.TryGetProperty("values", out var oldVals))
        {
            foreach (var v in oldVals.EnumerateArray())
                if (v.TryGetProperty("stringValue", out var sv))
                    g.MembersList.Add(new MemberInfo { Uid = sv.GetString() ?? "" });
        }

        return g;
    }

    private static string? GetStr(JsonElement f, string key)
    {
        if (f.TryGetProperty(key, out var p) && p.TryGetProperty("stringValue", out var v))
            return v.GetString();
        return null;
    }
}
