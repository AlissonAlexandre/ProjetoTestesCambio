using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CambioAPI.Configuration
{
    public static class SwaggerConfig
    {
        public static void ConfigureSwagger(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Câmbio API",
                Version = "v1",
                Description = "API para operações de câmbio",
                Contact = new OpenApiContact
                {
                    Name = "Suporte",
                    Email = "suporte@cambioapi.com"
                }
            });

            
            options.DocumentFilter<SwaggerDocumentFilter>();
        }
    }

    public class SwaggerDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var responses = new Dictionary<string, OpenApiResponse>
            {
                {"400", new OpenApiResponse {Description = "Requisição inválida - Verifique os dados enviados"}},
                {"401", new OpenApiResponse {Description = "Não autorizado - Autenticação necessária"}},
                {"403", new OpenApiResponse {Description = "Proibido - Você não tem permissão para acessar este recurso"}},
                {"404", new OpenApiResponse {Description = "Não encontrado - O recurso solicitado não existe"}},
                {"500", new OpenApiResponse {Description = "Erro interno do servidor - Entre em contato com o suporte"}}
            };

            foreach (var path in swaggerDoc.Paths.Values)
            {
                foreach (var operation in path.Operations.Values)
                {
                    foreach (var response in responses)
                    {
                        if (!operation.Responses.ContainsKey(response.Key))
                        {
                            operation.Responses.Add(response.Key, response.Value);
                        }
                    }
                }
            }
        }
    }
} 