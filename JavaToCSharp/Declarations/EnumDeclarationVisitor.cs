﻿using System;
using System.Collections.Generic;
using com.github.javaparser.ast;
using com.github.javaparser.ast.body;
using JavaToCSharp.Comments;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JavaToCSharp.Declarations
{
    public class EnumDeclarationVisitor : BodyDeclarationVisitor<EnumDeclaration>
    {
        public override MemberDeclarationSyntax VisitForClass(ConversionContext context, ClassDeclarationSyntax classSyntax, EnumDeclaration enumDecl)
        {
            var name = enumDecl.getName();

            var members = enumDecl.getMembers().ToList<BodyDeclaration>();

            var entries = enumDecl.getEntries().ToList<EnumConstantDeclaration>();
            var memberSyntaxes = new List<EnumMemberDeclarationSyntax>();

            foreach (var entry in entries)
            {
                // TODO: support "equals" value
                var memberSyntax = SyntaxFactory.EnumMemberDeclaration(entry.getName());

                var memberComment = entry.getComment();
                if (memberComment != null)
                {
                    var trivia = CommentVisitor.VisitComment(context, memberComment);
                    memberSyntax = memberSyntax.WithLeadingTrivia(trivia);
                }

                memberSyntaxes.Add(memberSyntax);
            }

            if (members != null && members.Count > 0)
                context.Options.Warning("Members found in enum " + name + " will not be ported. Check for correctness.", enumDecl.getBegin().line);

            var enumSyntax = SyntaxFactory.EnumDeclaration(name)
                .AddMembers(memberSyntaxes.ToArray());

            var mods = enumDecl.getModifiers();

            if (mods.HasFlag(Modifier.PRIVATE))
                enumSyntax = enumSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
            if (mods.HasFlag(Modifier.PROTECTED))
                enumSyntax = enumSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
            if (mods.HasFlag(Modifier.PUBLIC))
                enumSyntax = enumSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var comment = enumDecl.getComment();
            if (comment != null)
            {
                var trivia = CommentVisitor.VisitComment(context, comment);
                enumSyntax = enumSyntax.WithLeadingTrivia(trivia);
            }

            return enumSyntax;
        }

        public override MemberDeclarationSyntax VisitForInterface(ConversionContext context, InterfaceDeclarationSyntax interfaceSyntax, EnumDeclaration declaration)
        {
            throw new NotImplementedException("Need to implement diversion of nested enums in interfaces to non-nested.");
        }
    }
}
