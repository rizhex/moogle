using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MoogleEngine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

Moogle.data = new Data();
System.Console.WriteLine("data ok");

Moogle.vTFIDF = new DataTFIDF(Moogle.data.GetNormContent());
System.Console.WriteLine("tfidf ok");

Moogle.sgt = new Suggestion(Moogle.data.GetContent());
System.Console.WriteLine("sgt ok");

app.Run();
