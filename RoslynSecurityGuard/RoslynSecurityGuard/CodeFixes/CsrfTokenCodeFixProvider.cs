﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSecurityGuard.Analyzers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RoslynSecurityGuard.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InsecureCookieCodeFixProvider)), Shared]
    public class CsrfTokenCodeFixProvider : CodeFixProvider
    {
        private const string CreateAnnotationTitle = "Add [ValidateAntiForgeryToken] validation";
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CsrfTokenAnalyzer.DiagnosticId);


        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();


            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CreateAnnotationTitle,
                    createChangedDocument: c => AddAnnotation(context.Document, diagnostic, c),
                    equivalenceKey: CreateAnnotationTitle),
                diagnostic);

        }


        private async Task<Document> AddAnnotation(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var annotationsHttp = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent as AttributeListSyntax;
            var methodDeclaration = annotationsHttp.Parent as MethodDeclarationSyntax;

            var annotationValidate = SF.AttributeList()
                    .AddAttributes(SF.Attribute(SF.IdentifierName("ValidateAntiForgeryToken")))
                    .WithLeadingTrivia(annotationsHttp.GetLeadingTrivia()
                        .Insert(0, SF.ElasticEndOfLine(Environment.NewLine))
                    );

            var nodes = new List<SyntaxNode>();
            nodes.Add(annotationValidate);

            var newRoot = root.InsertNodesAfter(annotationsHttp, nodes);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
