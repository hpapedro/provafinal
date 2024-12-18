using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using provasub.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDataContext>();

builder.Services.AddCors(options =>{
    options.AddPolicy("AllowAllOrigins", builder =>{
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.MapGet("/", () => "ProvaSub");

//Cadastrar Aluno
app.MapPost("/api/aluno/cadastrar", async (Aluno aluno, AppDataContext context) =>
{
    context.Alunos.Add(aluno);
    await context.SaveChangesAsync();
    return Results.Created($"/api/aluno/{aluno.Id}", aluno);
});

//Listar Alunos
app.MapGet("/api/aluno/listar", async (AppDataContext context)=>{
    return await context.Alunos.ToListAsync();
});

//Buscar Aluno
app.MapGet("/api/aluno/{id}", async (AppDataContext context)=>{
    return await context.Alunos.ToListAsync();
});


//Cadasrar IMC
app.MapPost("/api/imc/cadastrar", async (HttpContext context, AppDataContext db) =>
{
    var imc = await context.Request.ReadFromJsonAsync<IMC>();
    
    if (imc == null)
    {
        return Results.BadRequest("Dados inválidos.");
    }

    // Verifica se o Aluno existe
    var aluno = await db.Alunos.FindAsync(imc.AlunoId);
    if (aluno == null)
    {
        return Results.NotFound($"Aluno com ID {imc.AlunoId} não encontrado.");
    }

    // Calcula o IMC
    var resultadoImc = imc.Peso / (imc.Altura * imc.Altura);
    imc.Classificacao = CalcularClassificacao(resultadoImc); // Método para calcular a classificação
    imc.ImcCalculado = resultadoImc; // Armazena o valor do IMC calculado

    // Adiciona o IMC ao banco de dados
    db.IMCs.Add(imc);
    await db.SaveChangesAsync();

    return Results.Created($"/api/imc/{imc.Id}", imc);
});

// Método para calcular a classificação do IMC
string CalcularClassificacao(double imc)
{
    if (imc < 18.5)
    {
        return "Magreza";
    }
    else if (imc >= 18.5 && imc < 25)
    {
        return "Norma";
    }
    else if (imc >= 25 && imc < 30)
    {
        return "Sobrepeso";
    }
    else if (imc >= 30 && imc < 40)
    {
        return "Obesidade";
    }
    else
    {
        return "Obesidade Grave";
    }
}

app.MapGet("/api/imc/listar", async (AppDataContext context)=>
{
    return await context.IMCs.ToListAsync();
});

// Atualizar IMC
app.MapPut("/api/imc/atualizar/{id}", async ([FromServices] AppDataContext db, [FromBody] IMC imcAlterado, [FromRoute] int id) =>
{
    // Encontra o IMC existente pelo ID
    var imcExistente = await db.IMCs.FindAsync(id);

    if (imcExistente == null)
    {
        return Results.NotFound($"IMC com ID {id} não encontrado.");
    }

    // Verifica se o Aluno existe
    var aluno = await db.Alunos.FindAsync(imcAlterado.AlunoId);
    if (aluno == null)
    {
        return Results.NotFound($"Aluno com ID {imcAlterado.AlunoId} não encontrado.");
    }

    // Atualiza os dados do IMC
    imcExistente.Peso = imcAlterado.Peso;
    imcExistente.Altura = imcAlterado.Altura;
    imcExistente.Classificacao = CalcularClassificacao(imcAlterado.Peso / (imcAlterado.Altura * imcAlterado.Altura)); // Atualiza a classificação
    imcExistente.ImcCalculado = imcAlterado.Peso / (imcAlterado.Altura * imcAlterado.Altura); // Atualiza o IMC calculado

    // Atualiza o Aluno no IMC
    imcExistente.AlunoId = imcAlterado.AlunoId; // Certifique-se de atualizar o AlunoId aqui

    // Atualiza no banco de dados
    db.IMCs.Update(imcExistente);
    await db.SaveChangesAsync();

    return Results.Ok(imcExistente); // Retorna o IMC atualizado
});





app.Run();