using Toolbelt.Blazor.Extensions.DependencyInjection;

var appBuilder = WebApplication.CreateBuilder(args);
appBuilder.Services.AddRazorPages();
appBuilder.Services.AddServerSideBlazor();
appBuilder.Services.AddSpeechSynthesis(options =>
{
    options.DisableClientScriptAutoInjection = true;
});

var app = appBuilder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await app.RunAsync();
