# Passo a Passo para Configurar a API e o Frontend

## Para buildar a API

### Pré-requisitos
- .NET 8.0
- PostgreSQL

### Passos

1. Configure a string de conexão no arquivo `appsettings.json` com os dados básicos de login do PostgreSQL.

2. No terminal (CMD), dentro da pasta do projeto `CambioAPI`, execute o comando:

    ```bash
    dotnet ef database update -p CambioAPI
    ```

3. Execute o projeto dentro da pasta `CambioAPI` com:

    ```bash
    dotnet run
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

Com isso, a API e o frontend estarão rodando localmente.
