# Passo a Passo para Configurar a API e o Frontend

## Para buildar a API

### Pré-requisitos
- .NET 8.0
- PostgreSQL

### Passos

1. Configure a string de conexão no arquivo `appsettings.json` com os dados básicos de login do PostgreSQL.

2. Configure o `applicationUrl` no arquivo `Properties/launchsettings.json` com a porta desejada. Exemplo:
    Na linha do HTTPS e do HTTP da parte profiles (deixar http mesmo para o https, visto da falta do certificado):
    ```json
    "applicationUrl": "http://localhost:7207;http://localhost:5259"
    ```

    > **Observação**: Se alterar aqui, também será necessário mudar a URL no `api.ts` do frontend.

3. No terminal (CMD), dentro da pasta do projeto `CambioAPI`, execute os comandos:

    ```bash
    dotnet tool install --global dotnet-ef --version 8.0
    dotnet ef database update -p (caminho completo pro CambioAPI.csproj)
    ```

4. Depois, execute o projeto com:

    ```bash
    dotnet run --launch-profile https
    ```

---


## Para configurar o Frontend

### Pré-requisitos
- API configurada e rodando
- Node.js e npm instalados

### Passos

1. Navegue até a pasta do projeto frontend.

2. Instale as dependências com:

    ```bash
    npm install --legacy-peer-deps
    ```

3. Execute o projeto com:

    ```bash
    npm run dev
    ```

4. Acesse no navegador o endereço exibido no terminal.  
   Normalmente será `http://localhost:3000`, a menos que essa porta esteja em uso.

---

Com isso, a API e o frontend estarão rodando localmente!
Para prosseguir, crie um usuário, para o acesso do primeiro usuário master, é necessário transformar o usuário em master diretamente no postgreSQL. (propriedade IsMaster na tabela Users).
