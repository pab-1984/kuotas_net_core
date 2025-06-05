# Etapa 1: Compilación
# Usar la imagen del SDK de .NET para compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copiar los archivos .csproj y restaurar dependencias primero
# Esto aprovecha el cache de capas de Docker si los proyectos no cambian
COPY NuGet.Config ./
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto del código fuente de la aplicación
COPY . ./

# Publicar la aplicación en modo Release
RUN dotnet publish -c Release -o out

# Etapa 2: Entorno de Ejecución
# Usar la imagen de ASP.NET Core Runtime, que es más ligera que el SDK
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime-env
WORKDIR /app

# Copiar la salida de la publicación de la etapa de compilación
COPY --from=build-env /app/out .

# Exponer el puerto en el que la aplicación escuchará dentro del contenedor
# ASP.NET Core por defecto escucha en el puerto 8080 (HTTP) y 8081 (HTTPS) dentro de los contenedores
# o en el puerto 80 si no se especifica Kestrel.
# Si tu appsettings.json o Program.cs especifica un puerto diferente para Kestrel, ajústalo aquí.
# Comúnmente, para HTTP es 80 o 8080. Para HTTPS es 443 o 8081.
EXPOSE 8080
EXPOSE 8081

# Establecer el punto de entrada para ejecutar la aplicación cuando el contenedor se inicie
# Kuotasmig.Core.dll será el nombre de tu ensamblado principal
ENTRYPOINT ["dotnet", "Kuotasmig.Core.dll"]