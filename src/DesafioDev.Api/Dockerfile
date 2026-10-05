# Build a partir da raiz do repositório (contexto "."): a API depende de src/DesafioDev.Core,
# src/DesafioDev.Infra e de data/*.json, que ficam fora da pasta deste projeto.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Só os .csproj primeiro: o restore vira uma camada própria e só é refeito quando
# mudar alguma dependência, não a cada alteração de código.
COPY src/DesafioDev.Core/DesafioDev.Core.csproj src/DesafioDev.Core/
COPY src/DesafioDev.Infra/DesafioDev.Infra.csproj src/DesafioDev.Infra/
COPY src/DesafioDev.Api/DesafioDev.Api.csproj src/DesafioDev.Api/
RUN dotnet restore src/DesafioDev.Api/DesafioDev.Api.csproj

COPY src/DesafioDev.Core/ src/DesafioDev.Core/
COPY src/DesafioDev.Infra/ src/DesafioDev.Infra/
COPY src/DesafioDev.Api/ src/DesafioDev.Api/
COPY data/ data/
RUN dotnet publish src/DesafioDev.Api/DesafioDev.Api.csproj \
      -c Release -o /app/publish --no-restore -p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Diretório do banco criado na imagem já com o dono certo: quando o Docker monta um volume
# nomeado vazio aqui, ele herda esse dono e o usuário sem privilégio consegue gravar.
RUN mkdir -p /app/dados && chown "$APP_UID" /app/dados

COPY --from=build /app/publish .

# Usuário sem privilégio que já vem nas imagens do .NET 8.
USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ConnectionStrings__Estoque="Data Source=/app/dados/estoque.db"
EXPOSE 8080

ENTRYPOINT ["dotnet", "DesafioDev.Api.dll"]
