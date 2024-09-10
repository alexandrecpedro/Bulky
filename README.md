<div align = "center">
  <h1> Fina </h1>
</div>
<br>

<div align = 'center' justify-content = 'space-around' >
  <img width="1604" alt="Fina - Desktop" src="./project/screens/screen1.png">
</div>
<br>
<div align = 'center' justify-content = 'space-around' >
  <img width="1604" alt="Fina - Registration" src="./project/screens/screen2.png">
</div>
<br>
<div align = 'center' justify-content = 'space-around' >
  <img width="1604" alt="Fina - Documentation" src="./project/screens/screen3.png">
</div>
<br>
<br>

<p></p>

<p align="center">
 <a href="#theproject">The Project</a> •
 <a href="#target">Target</a> •
 <a href="#technologies">Technologies</a> •
 <a href="#route">Route</a> •
 <a href="#howtouse">How to Use</a>
</p>
<br>

<div id="theproject">
<h2> 📓 The Project </h2>
<p> Fina platform for personal financial control </p>
</div>

<div id="target">
<h2> 💡 Target </h2>
Development of a Fina, a personal financial control platform, at Full Stack Journey 2024, by Balta.io
</div>
<br>

<div id="technologies">
<h2> 🛠 Technologies </h2>
The following tools were used in building the project:<br><br>

|                       Type                       |           Tools           |            References             |
| :----------------------------------------------: | :-----------------------: | :-------------------------------: |
|                       IDE                        |          VS CODE          |  https://code.visualstudio.com/     |
|         Programming Language (Frontend)          |           BLAZOR          |  https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor                  |
|             Browser fonts (Frontend)             |        GOOGLE FONTS       |       https://fonts.google.com/                  |
|           Graphic components (Frontend)          |         MUDBLAZOR         |    https://mudblazor.com/                  |
| Low-level binary code executed in web (Frontend) |        WEB ASSEMBLY       |     https://webassembly.org/      |
|          Programming Language (Backend)          |           DOTNET          |  https://dotnet.microsoft.com/en-us/                  |
|                Database (Backend)                |        MSSQL SERVER       | https://www.microsoft.com/en-us/sql-server/sql-server-2022            |
|          DotNET ORM (Backend, Database)          |     ENTITY FRAMEWORK      |     https://learn.microsoft.com/en-us/ef/      |
|    API Documentation & Design Tools (Backend)    |          SWAGGER          |        https://swagger.io/                  |
| Open source API development ecosystem (Testing)  |          POSTMAN          |      https://www.postman.com/        |
<br>

<div align = 'center'>
  <h3>Backend | API</h3>
  <img height =' 100px ' src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/csharp/csharp-original.svg" />
  <img height =' 100px ' left='80px' src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/dot-net/dot-net-original.svg" />
  <img height =' 100px ' left='80px' src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/swagger/swagger-original.svg" />
  <br>
  <h3>Testing</h3>
  <img width =' 100px ' src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/postman/postman-original.svg" />
  <br>
  <h3>Database</h3>
  <img height =' 100px ' src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/microsoftsqlserver/microsoftsqlserver-original.svg" />
  <br>
  <h3>IDE</h3>
  <img height =' 100px ' src="https://cdn.jsdelivr.net/gh/devicons/devicon/icons/vscode/vscode-original.svg" />
  <br>
  <h3>Frontend</h3>
  <img width =' 100px ' src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/blazor/blazor-original.svg" />
  <img width =' 100px ' left='80px' src="https://cdn.jsdelivr.net/gh/devicons/devicon@latest/icons/wasm/wasm-original.svg" />
  <br>
</div>
<br/>

<div id="route">
<h2> 🔎 Route </h2>
  <ol>
    <li &nbsp;>Part 1 - Backend, Minimal APIs and Entity Framework<br/>
      <ul &nbsp;>
        <li &nbsp;>Install VS Code (IDE)</li>
        <li>Create a new project folder: mkdir Fina</li>
        <li>Enter the project: cd Fina</li>
        <li>Create a new solution: dotnet new sln</li>
        <li &nbsp;><b>Core project</b>
          <ul>
            <li>Create a new project: dotnet new classlib -o Fina.Core</li>
            <li>Add to solution: dotnet sln add ./Fina.Core</li>
            <li>Enter project: cd Fina.Core</li>
            <!-- Guid uses 128 bits - it could be slower and consumes more resources (not indexed on db) -->
            <li>Create Models, Enums, Request and Responses</li>
            <li>Use Constructors and JsonConstructor, JsonIgnore</li>
            <li>Setup the request handlers (interfaces): Handlers</li>
          </ul>
        </li>
        <li &nbsp;><b>Backend project</b>
          <ul>
            <li>Create a new project: dotnet new web -o Fina.Api</li>
            <li>Add to solution: dotnet sln add ./Fina.Api</li>
            <li>Enter project: cd Fina.Api</li>
            <li>References Core project: dotnet add reference ../Fina.Core</li>
          </ul>
        </li>
        <li &nbsp;><b>Frontend project</b>
          <ul>
            <li>Create a new project: dotnet new blazorwasm -o Fina.Web --pwa</li>
            <!-- Runs only on browser -->
            <li>Add to solution: dotnet sln add ./Fina.Web</li>
            <li>Enter project: cd Fina.Web</li>
            <li>References Core project: dotnet add reference ../Fina.Core</li>
          </ul>
        </li>
        <li>Return to root folder: cd .. </li>
        <li>Delete any files created from building/compiling the project: dotnet clean</li>
        <li>Compile all references: dotnet build</li>
      </ul>
    </li>
    <br>
    <li &nbsp;>Part 2 - Frontend, Blazor, WebAssembly and PWA<br/>
      <ul &nbsp;>
        <li &nbsp;><b>Core project</b>
          <ul>
            <li>Common share folder: Common
              <ol>
                <li>DateTime converter methods</li>
              </ol>
            </li>
            <li>Set url variables: Configuration.cs
              <ol>
                <li>Frontend project: FrontendUrl variable</li>
                <li>Backend project: BackendUrl variable</li>
              </ol>
            </li>
          </ul>
        </li>
        <li &nbsp;><b>Backend project</b>
          <ul>
            <li>Enter backend project: cd Fina.Api</li>
            <li>Add Entity Framework and SQL Server:
              dotnet add package Microsoft.EntityFrameworkCore.SqlServer
            </li>
            <!-- Automatically generates DB (no need create table, drop, etc) -->
            <li>Add Design: dotnet add package Microsoft.EntityFrameworkCore.Design</li>
            <!-- To install EF tools: dotnet tool install ef-tools -->
            <!-- To update EF tools: dotnet tool update --global dotnet-ef -->
            <li>Add documentation packages:
              <ol>
                <li>OpenAPI: dotnet add package Microsoft.AspNetCore.OpenApi</li>
                <li>Swashbuckle (Swagger): dotnet add package Swashbuckle.AspNetCore</li>
              </ol>
            </li>
            <li>Data access
              <ol>Data folder and files
                <li>Mappings: CategoryMapping, TransactionMapping</li>
                <li>Data Context (DB configurations): AppDbContext</li>
              </ol>
            </li>
            <li>Migrations
              <ol>
                <li>Generate: dotnet ef migrations add v1</li>
                <li>Apply to DB: dotnet ef database update</li>
              </ol>
            </li>
            <li>Handlers (Business Logic): CategoryHandler, TransactionHandler</li>
            <li>API settings: ApiConfiguration</li>
            <li>Common folder and files : Common.Api
              <ol>
                <li>Interface: IEndpoint</li>
                <li>Extension methods
                  <ul>
                    <li>AppExtension</li>
                    <li>BuildExtension</li>
                  </ul>
                </li>
              </ol>
            </li>
            <li>Endpoints folder and files
              <ol>
                <li>Categories
                  <ul>
                    <li>Create category: CreateCategoryEndpoint</li>
                    <li>Delete category: DeleteCategoryEndpoint</li>
                    <li>Get category by id: GetCategoryByIdEndpoint</li>
                    <li>Get all categories: GetAllCategoriesEndpoint</li>
                    <li>Update category: UpdateCategoryEndpoint</li>
                  </ul>
                </li>
                <li>Transactions
                  <ul>
                    <li>Create transaction: CreateTransactionEndpoint</li>
                    <li>Delete transaction: DeleteTransactionEndpoint</li>
                    <li>Get transaction by id: GetTransactionByIdEndpoint</li>
                    <li>Get transactions by period: GetTransactionsByPeriodEndpoint</li>
                    <li>Update transaction: UpdateTransactionEndpoint</li>
                  </ul>
                </li>
                <li>Endpoint configurations: Endpoint.cs</li>
              </ol>
            </li>
            <li>Apply Dependency Injection: Program.cs</li>
          </ul>
        </li>
        <li &nbsp;><b>Testing</b>
          <ul>
            <li>Test backend at SWAGGER: http://localhost:5259/swagger/index.html</li>
            <li>Test backend at Postman</li>
          </ul>
        </li>
      </ul>
    <br>
    <li &nbsp;>Part 3 - Componentisation, MudBlazor, Publishing<br/>
      <ul &nbsp;> 
        <li &nbsp;><b>Backend project</b>
          <ul>
            <li>Set urls: appsettings.json
              <ol>
                <li>BackendUrl: http://localhost:5259</li>
                <li>FrontendUrl: http://localhost:5278</li>
              </ol>
            </li>
          </ul>
        </li>
        <li &nbsp;><b>Core project</b>
          <ul>
            <li>Set urls: Configuration.cs
              <ol>
                <li>BackendUrl: http://localhost:5259</li>
                <li>FrontendUrl: http://localhost:5278</li>
              </ol>
            </li>
          </ul>
        </li>
        <li &nbsp;><b>Frontend project</b>
          <ul>
            <li>Add packages
              <ol>
                <li>HTTP: dotnet add package Microsoft.Extensions.Http</li>
                <li>MudBlazor: dotnet add package MudBlazor</li>
              </ol>
            </li>
            <li>Modify initial structure
              <ol>
                <li>Remove some files from:
                  <ul>
                    <li>Layout</li>
                    <li>Pages</li>
                    <li>wwwroot</li>
                  </ul>
                </li>
                <li>Adapt Layout
                  <ul>
                    <li>Name: Layout -> Layouts</li>
                    <li>Apply MudBlazor template: MainLayout.razor</li>
                  </ul>
                </li>
              </ol>
            </li>
            <li>Adapt files
              <ol>
                <li>_Imports.razor
                  <ul>
                    <li>@using Fina.Web.Layout -> @using Fina.Web.Layouts</li>
                    <li>Add MudBlazor: @using MudBlazor</li>
                  </ul>
                </li>
                <li>wwwroot/index.html
                  <ul>
                    <li>Title: Fina</li>
                    <li>Remove Bootstrap reference</li>
                    <li>Add links for Google Fonts: Raleway</li>
                    <li>Add MudBlazor script
                      ```html
                      <script src="_content/MudBlazor/MudBlazor.min.js"></script>
                      ```
                    </li>
                  </ul>
                </li>
                <li>Program.cs
                  <ul>
                    <li>Add MudBlazor settings
                      ```
                      <!-- On top of code -->
                      using MudBlazor.Services;
                      <!-- Below "builder.RootComponents.Add<HeadOutlet>("head::after");" -->
                      builder.Services.AddMudServices()
                      ```
                    </li>
                  </ul>
                </li>
              </ol>
            </li>
            <li>Web settings
              <ol>
                <li>WebConfiguration.cs</li>
                <li>Program.cs
                  <ul>
                    <li>Add HttpClient settings
                      ```
                      <!-- Below "builder.Services.AddMudServices();" -->
                      builder.Services
                        .AddHttpClient(
                          WebConfiguration.HttpClientName,
                          opt =>
                          {
                            opt.BaseAddress = new Uri(Configuration.BackendUrl);
                          });
                      ```
                    </li>
                  </ul>
                </li>
              </ol>
            </li>
            <li>Handlers
              <ol>
                <li>CategoryHandler</li>
                <li>Program.cs
                  <ul>
                    <li>Add HttpCLient settings
                      ```csharp
                      <!-- Above "await builder.Build().RunAsync();" -->
                      builder.Services.AddTransient<ICategoryHandler, CategoryHandler>();
                      ```
                    </li>
                  </ul>
                </li>
              </ol>
            </li>
            <li>Pages
              <ol>
                <li>Categories
                  <ul>
                    <li>Create: Create.razor && Create.razor.cs</li>
                    <li>GetAll: GetAll.razor && GetAll.razor.cs</li>
                  </ul>
                </li>
              </ol>
            </li>
          </ul>
        </li>
      </ul>
    </li>
    <br>
  </ol>
</div>

<div id="howtouse">
<h2>🧪 How to use</h2>
  <ol &nbsp;>
    <li &nbsp;>Set the development environment at you local computer</li>
    <li &nbsp;>Clone the repository
      <ul>
        <li>git clone https://github.com/alexandrecpedro/financial-control-app</li>
      </ul>
    </li>
    <li &nbsp;>Build the project:
      <ul>
        <li>dotnet build</li>
      </ul>
    </li>
    <li &nbsp;>Run the project:
      <ul>
        <li>dotnet run</li>
      </ul>
    </li>
    <li><b>Testing</b>
      <ul>
        <li &nbsp;><u>Using Swagger documentation</u>
          <ol>
            <li &nbsp;>Enter the directory
              <ul>
                <li>cd Fina.Api</li>
              </ul>
            </li>
            <li &nbsp;>Build the project
              <ul>
                <li>dotnet build</li>
              </ul>
            </li>
            <li &nbsp;>Run
              <ul>
                <li>dotnet run</li>
                <li>Open in Browser: http://localhost:5259/swagger/index.html</li>
              </ul>
            </li>
          </ol>
        </li>
        <li &nbsp;><u>Frontend</u>
          <ol>
            <li &nbsp;>Enter the directory
              <ul>
                <li>cd Fina.Web</li>
              </ul>
            </li>
            <li &nbsp;>Build the project
              <ul>
                <li>dotnet build</li>
              </ul>
            </li>
            <li &nbsp;>Run
              <ul>
                <li>dotnet run</li>
              </ul>
            </li>
          </ol>
        </li>
      </ul>
    </li>
  </ol>
</div>
