using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;

namespace BtcTransmuter.Controllers
{
    [Route("[controller]")]
    public class TestController: Controller
    {
        [AllowAnonymous]
        [HttpPost("")]
        public async Task<IActionResult> Test([FromBody]GetAutoCompleteRequest request)
        {
            var assemblies = new[]
            {
                typeof(string).Assembly,
                typeof(IList<string>).Assembly,
                Assembly.Load("Microsoft.CodeAnalysis"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp"),
                Assembly.Load("Microsoft.CodeAnalysis.Features"),
                Assembly.Load("Microsoft.CodeAnalysis.CSharp.Features"),
            };

            var partTypes = MefHostServices.DefaultAssemblies.Concat(assemblies)
                .Distinct()
                .SelectMany(x => x.GetTypes())
                .ToArray();

            var compositionContext = new ContainerConfiguration()
                .WithParts(partTypes)
                .CreateContainer();

            MefHostServices host = MefHostServices.Create(compositionContext);
            var workspace = new AdhocWorkspace(host);

//            var variableCode = "";
//            foreach (var variable in request.Variables)
//            {
//                variableCode = $"{variable.Value} {variable.Key};";
//            }

//            var scriptCode = $"{variableCode} var x={request.Command}";
            var pos = request.Pos.GetValueOrDefault(request.Command.Length );
//            pos = (scriptCode.Length - request.Command.Length) + pos;
            var scriptCode = request.Command;
            var compilationOptions = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                usings: new[] { "System" });

            var scriptProjectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), "Script",
                    "Script", LanguageNames.CSharp, isSubmission: true)
                .WithMetadataReferences(new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                })
                .WithCompilationOptions(compilationOptions);

            var scriptProject = workspace.AddProject(scriptProjectInfo);
            var scriptDocumentInfo = DocumentInfo.Create(
                DocumentId.CreateNewId(scriptProject.Id), "Script",
                sourceCodeKind: SourceCodeKind.Script,
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(scriptCode), VersionStamp.Create())));
            var scriptDocument = workspace.AddDocument(scriptDocumentInfo);

            var completionService = CompletionService.GetService(scriptDocument);
            var results = await completionService.GetCompletionsAsync(scriptDocument, pos);

            return Json(results.Items.Where(item => item.Tags.Contains("Public")).Select(item => new
            {
                Text = item.DisplayText,
                Props = item.Properties.Select(pair => $"{pair.Key}:{pair.Value}"),
                Tags = item.Tags
            }));
        }
    }

    public class GetAutoCompleteRequest
    {
        public string Command { get; set; }

        public Dictionary<string, string>  Variables{ get; set; }
        public int? Pos { get; set; } 
    }
}