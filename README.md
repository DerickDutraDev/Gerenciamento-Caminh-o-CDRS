# 🚚 Sistema de Gerenciamento de Caminhões - TruckYard

![Status](https://img.shields.io/badge/Status-Concluído-success)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![Tests](https://img.shields.io/badge/Tests-Passing-green)

Sistema desenvolvido para otimizar e gerenciar o fluxo logístico, focando no controle de entrada e saída de veículos e gestão de rotas.

## 📋 Funcionalidades

- **Controle de Pátio**: Monitoramento em tempo real da disponibilidade.
- **Gerenciamento de Rotas**: Criação de rotas apenas para caminhões disponíveis.
- **Gestão Fiscal**: Associação de NFEs às rotas.
- **API Documentada**: Swagger integrado para exploração dos endpoints.

## 🚀 Como Rodar o Projeto

### Pré-requisitos
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Instalação e Execução

1. Clone o repositório:
   ```bash
   https://github.com/DerickDutraDev/Gerenciamento-Caminh-o-CDRS.git
   cd truck-yard
   ```

2. Restaure as dependências:
   ```bash
   dotnet restore
   ```

3. Execute o projeto:
   ```bash
   dotnet run --project TruckYard/TruckYard.csproj
   ```

4. Acesse a documentação da API:
   - Abra o navegador em `http://localhost:5000/swagger` (ou a porta indicada no terminal).

## 🧪 Como Rodar os Testes

Este projeto inclui testes unitários utilizando xUnit e Entity Framework Core InMemory.

```bash
dotnet test
```

## 🛠️ Tecnologias

- **C# .NET 9.0**
- **ASP.NET Core Web API**
- **Entity Framework Core (SQLite)**
- **xUnit** (Testes)
- **HTML/CSS/JS** (Frontend básico)

## 👨‍💻 Autor

Desenvolvido por **Derick Dutra**.
