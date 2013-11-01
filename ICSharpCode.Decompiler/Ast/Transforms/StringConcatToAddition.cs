using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace ICSharpCode.Decompiler.Ast.Transforms
{
	class StringConcatToAddition : IAstTransform
	{
		public void Run(AstNode compilationUnit)
		{
			foreach (var inv in compilationUnit.Descendants.OfType<InvocationExpression>())
			{
				var target = inv.Target;
				var methodName = GetMethodName(target);
				if (methodName != "Concat")
					continue;

				var typeRef = target.FirstChild as TypeReferenceExpression;
				if (typeRef == null)
					continue;

				var primType = typeRef.Type as PrimitiveType;
				if (primType == null || primType.KnownTypeCode != KnownTypeCode.String)
					continue;

				//Handle string.Concat(str, str1, ...)
				var args = inv.Arguments.ToList();
				foreach (var node in args)
					node.Remove();

				//Handle string.Concat(new [] {str, str1})
				var arr = args.FirstOrDefault() as ArrayCreateExpression;
				if (arr != null)
				{
					if (args.Count > 1)
						throw new InvalidOperationException("Expecting only 1 array passed to Concat.");

					args = arr.Initializer.Elements.ToList();
					foreach (var node in args)
						node.Remove();
				}

				var newNode = args
					.Skip(1)
					.Aggregate(args.First(), (s, e) => new BinaryOperatorExpression(s, BinaryOperatorType.Add, e));

				inv.ReplaceWith(newNode);
			}
		}

		static string GetMethodName(Expression invocationTarget)
		{
			if (invocationTarget is IdentifierExpression)
				return ((IdentifierExpression)invocationTarget).Identifier;
			if (invocationTarget is MemberReferenceExpression)
				return ((MemberReferenceExpression)invocationTarget).MemberName;

			throw new InvalidOperationException("Unexpected expression type: " + invocationTarget.GetType());
		}
	}
}