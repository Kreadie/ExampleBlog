using BlogTest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder();
string connection = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddServices(connection);
var app = builder.Build();
app.AddMiddleware();


async void onDbDeletion(HttpContext context)
{
    await context.SignOutAsync("Cookies");
    string path = Environment.CurrentDirectory + "\\wwwroot\\image\\user";
    string[] files = Directory.GetFiles(path);

    foreach (var f in files)
    {
        if (f != path + "\\_.png")
            File.Delete(f);
    } 
}

app.Use(async (context, next) =>
{
    var db = context.RequestServices.GetService<ApplicationContext>();
    if (db?.Users.ToList().Count == 0)
        onDbDeletion(context);

    await next.Invoke();

    if(context.Response.StatusCode == 404)
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync("wwwroot/notfound.html");
    }
});

app.MapGet("/login", async (HttpContext context) =>
{
    if (!context.User.Identity!.IsAuthenticated)
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync("wwwroot/login.html");
    }
    else
    {
        context.Response.Redirect("profile.html");
    }
});

app.MapPost("/login", async (User userData, HttpContext context, ApplicationContext db) =>
{
    string? name     = userData.Name;
    string? password = userData.Password;

    var user = db.Users.ToList().FirstOrDefault(u => u.Password == password && u.Name == name);

    if (user is null)
        return Results.Unauthorized();

    var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Name) };
    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(claimsIdentity));
    return Results.Ok();
});

app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});

app.MapGet("/register", async (HttpContext context) =>
{
    if(context.User.Identity!.IsAuthenticated)
    {
        context.Response.Redirect("profile.html");
        return;
    }
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync("wwwroot/register.html");

});

app.MapPost("/register", async (User user, HttpContext context,ApplicationContext db) =>
{
    if (user.Name == "" || user.Password == "")
        return null;
    var existingUser = db.Users.ToList().FirstOrDefault(u => user.Name == u.Name) ?? null;

    if(existingUser != null)
        return null;

    user.Guid = Guid.NewGuid();
    await db.Users.AddAsync(user);
    await db.SaveChangesAsync();
    return user;
});

app.MapGet("/userdata", async (HttpContext context, ApplicationContext db) =>
{
    if (context.User.Identity!.IsAuthenticated)
    {
        var userClaim = context.User.FindFirstValue(ClaimTypes.Name);
        var user = db.Users.ToList().FirstOrDefault(us => us.Name == userClaim);
        await context.Response.WriteAsJsonAsync(user);
    }
});

app.MapGet("/notes/{offset:int}", async (int offset, HttpContext context, ApplicationContext db) =>
{
    var test = db.Notes.FromSql($"SELECT * FROM Notes ORDER BY PublishingDate DESC OFFSET {offset} ROWS FETCH NEXT 7 ROWS ONLY").ToList();
    await context.Response.WriteAsJsonAsync(test);
});

app.MapGet("/user/pics", async (HttpContext context, ApplicationContext db) =>
{
    var userPics = db.Users.Select(user => user.ImagePath).ToList();
    await context.Response.WriteAsJsonAsync(userPics);
});

app.MapPost("/profile/picture", async (HttpContext context, ApplicationContext db) =>
{
    var files = context.Request.Form.Files;
    var userClaim = context.User.FindFirstValue(ClaimTypes.Name);

    var user = db.Users.FirstOrDefault(us => us.Name == userClaim);
    user!.ImagePath = $"wwwroot/image/user/{user!.Name}.png";

    foreach(var file in files)
    {
        using (var fileStream = new FileStream(user.ImagePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
    }
    db.Users.Update(user);
    await db.SaveChangesAsync();
});
 
app.MapPost("/notes", async (Note noteText, HttpContext context, ApplicationContext db) =>
{
    if(context.User.Identity!.IsAuthenticated && noteText.Text != "")
    {
        var userClaim = context.User.FindFirstValue(ClaimTypes.Name);
        var user = db.Users.ToList().FirstOrDefault(us => us.Name == userClaim);
        Note note = new Note { Guid = Guid.NewGuid(), Text = noteText.Text,
                               UserName = user!.Name, UserGuid = user!.Guid,
                               PublishingDate = DateTime.Now};
        user.Notes.Add(note);
        await db.Notes.AddAsync(note);
        await db.SaveChangesAsync();
    } 
    else
        context.Response.StatusCode = 401;

});

app.MapDelete("/notes/delete/{guid:guid}", async (Guid guid, HttpContext context,  ApplicationContext db) =>
{
    if(!context.User.Identity!.IsAuthenticated)
    {
        context.Response.StatusCode = 401;
        return;
    }

    var claim = context.User.FindFirstValue(ClaimTypes.Name);
    var user = db.Users.ToList().FirstOrDefault(us => us.Name == claim);
    Note found = db.Notes.ToList().FirstOrDefault(note => note.Guid == guid)!;

    if (found.UserGuid != user?.Guid)
    {
        context.Response.StatusCode = 403;
        return;
    }

    user.Notes.Remove(found);
    db.Notes.Remove(found);
    await db.SaveChangesAsync();
});

app.MapPost("/notes/rating/{type}", async (Note note, string type, HttpContext context, ApplicationContext db) =>
{
    if(!context.User.Identity!.IsAuthenticated)
    {
        context.Response.StatusCode = 401;
        return;
    }
    var claim = context.User.FindFirstValue(ClaimTypes.Name);
    var user = db.Users.ToList().FirstOrDefault(us => us.Name == claim);

    Note found = db.Notes.ToList().FirstOrDefault(n => n.Guid == note.Guid)!;
    bool containsYes = found!.WhoLiked.Contains(user!.Name);
    bool containsNo = found!.WhoDisliked.Contains(user!.Name);

    if(containsYes)
    {
        found.WhoLiked.Remove(user.Name);
        if (type == "-")
        {
            found.WhoDisliked.Add(user.Name);
        }
        found.Rating = found.WhoLiked.Count() - found.WhoDisliked.Count();
        db.Notes.Update(found);
        await db.SaveChangesAsync();
    } else if (containsNo)
    {
        found.WhoDisliked.Remove(user.Name);
        if (type == "+")
        {
            found.WhoLiked.Add(user.Name);
        }
        found.Rating = found.WhoLiked.Count() - found.WhoDisliked.Count();
        db.Notes.Update(found);
        await db.SaveChangesAsync();
    } else
    {
        if (type == "+")
        {
            found.WhoLiked.Add(user.Name);
        }
        else if (type == "-")
        {
            found.WhoDisliked.Add(user.Name);
        }
        found.Rating = found.WhoLiked.Count() - found.WhoDisliked.Count();
        db.Notes.Update(found);
        await db.SaveChangesAsync();
    }

    await context.Response.WriteAsJsonAsync(note.Rating);

});

app.MapGet("/islogin", (HttpContext context, ApplicationContext db) =>
{
    if (!context.User.Identity!.IsAuthenticated)
        context.Response.StatusCode = 401;
});

app.MapGet("/check-who/", async (HttpContext context, ApplicationContext db) =>
{
    if (!context.User.Identity!.IsAuthenticated)
        context.Response.StatusCode = 401;
    var claim = context.User.FindFirstValue(ClaimTypes.Name);
    await context.Response.WriteAsJsonAsync(claim);
});

app.Run();
//TODO: добавить параметр страниц к строке запроса (?)
//TODO: добавлять оценку без перезагрузки страницы